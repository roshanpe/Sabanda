# Community Management Application — Mandatory Implementation Specification

**Scope:** Mandatory requirements only. Implement exactly what is specified. Do not add features not listed here.

---

## Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8, C#, minimal API or controller-based, layered architecture: API / Application / Domain / Infrastructure |
| ORM | Entity Framework Core 8, code-first migrations |
| Database | PostgreSQL |
| Frontend | React 18, TypeScript |
| Auth | JWT, issued by the .NET backend (no external identity provider) |
| QR display | `qrcode.react` |
| QR scanning | `@zxing/browser` |
| Email | SendGrid (SMTP abstracted behind `IEmailSender` interface) |
| Background jobs | Hangfire (in-process, PostgreSQL storage) |
| API prefix | `/api/v1/` on all routes |

---

## 1. Data Model

### 1.1 Tenant

```
Tenant
  Id              uuid            PK
  Name            varchar(200)    NOT NULL
  CreatedAt       timestamptz     NOT NULL DEFAULT now()
```

Every table below except `Tenant` itself carries a `TenantId uuid NOT NULL FK → Tenant.Id` column. All queries must filter by `TenantId` derived from the authenticated user's JWT claim `tenant_id`. No cross-tenant query must ever succeed.

### 1.2 Family

```
Family
  Id              uuid            PK
  TenantId        uuid            NOT NULL FK → Tenant.Id
  Name            varchar(200)    NOT NULL
  QrToken         text            NOT NULL UNIQUE
  QrTokenIssuedAt timestamptz     NOT NULL
  QrTokenExpiresAt timestamptz    NOT NULL
  CreatedAt       timestamptz     NOT NULL DEFAULT now()
```

A family must have at least one `Member` with `IsAdult = true` at creation time. Enforce this in the application layer before committing.

### 1.3 Member

```
Member
  Id              uuid            PK
  TenantId        uuid            NOT NULL FK → Tenant.Id
  FamilyId        uuid            NOT NULL FK → Family.Id
  FullName        varchar(300)    NOT NULL
  DateOfBirth     date            NOT NULL
  IsAdult         boolean         NOT NULL GENERATED ALWAYS AS (age(DateOfBirth) >= interval '18 years') STORED
  ContactEmail    varchar(320)    NULL      -- required when IsAdult = true, enforced in app layer
  ContactPhone    varchar(30)     NULL
  QrToken         text            NOT NULL UNIQUE
  QrTokenIssuedAt timestamptz     NOT NULL
  QrTokenExpiresAt timestamptz    NOT NULL
  ConsentGiven    boolean         NOT NULL DEFAULT false
  ConsentGivenBy  uuid            NULL FK → AppUser.Id  -- required when IsAdult = false
  ConsentGivenAt  timestamptz     NULL
  CreatedAt       timestamptz     NOT NULL DEFAULT now()
```

Rules enforced in the application layer:
- `IsAdult = false` → `ConsentGiven`, `ConsentGivenBy`, and `ConsentGivenAt` must all be set before the record is committed.
- `IsAdult = true` → `ContactEmail` must not be null.
- A member may belong to exactly one family. Enforce via FK; do not allow re-assignment.

### 1.4 AppUser

```
AppUser
  Id                  uuid            PK
  TenantId            uuid            NOT NULL FK → Tenant.Id
  MemberId            uuid            NULL FK → Member.Id  -- null for Administrator accounts not linked to a member
  FamilyId            uuid            NULL FK → Family.Id  -- set when role = PrimaryAccountHolder
  Email               varchar(320)    NOT NULL UNIQUE
  PasswordHash        text            NOT NULL
  Role                varchar(30)     NOT NULL  -- enum: Administrator | PrimaryAccountHolder | FamilyMember | ProgramCoordinator | EventCoordinator
  IsPrimaryHolder     boolean         NOT NULL DEFAULT false
  FailedLoginCount    int             NOT NULL DEFAULT 0
  LockedUntil         timestamptz     NULL
  MaxConcurrentSessions int           NOT NULL DEFAULT 1
  CreatedAt           timestamptz     NOT NULL DEFAULT now()
```

### 1.5 ActiveSession

```
ActiveSession
  Id          uuid            PK
  UserId      uuid            NOT NULL FK → AppUser.Id
  TokenJti    uuid            NOT NULL UNIQUE   -- JWT jti claim
  IssuedAt    timestamptz     NOT NULL
  ExpiresAt   timestamptz     NOT NULL
  RevokedAt   timestamptz     NULL
```

### 1.6 Membership

```
Membership
  Id              uuid            PK
  TenantId        uuid            NOT NULL FK → Tenant.Id
  Type            varchar(20)     NOT NULL  -- enum: Family | Program | Event
  FamilyId        uuid            NULL FK → Family.Id   -- required when Type = Family or Type = Event with family billing
  MemberId        uuid            NULL FK → Member.Id   -- required when Type = Program or Type = Event with individual billing
  StartDate       date            NOT NULL
  EndDate         date            NOT NULL
  PaymentStatus   varchar(20)     NOT NULL DEFAULT 'Initiated'  -- enum: Initiated | Pending | Completed | Failed | Refunded
  CreatedAt       timestamptz     NOT NULL DEFAULT now()
```

Overlap rule: no two `Membership` rows with the same `Type`, same `FamilyId` (or `MemberId`), and overlapping `[StartDate, EndDate]` ranges may exist. Enforce with a partial unique index or CHECK in the application layer before insert.

Valid `PaymentStatus` transitions:
- `Initiated` → `Pending` → `Completed`
- `Initiated` → `Failed`
- `Completed` → `Refunded`
- All other transitions are rejected with HTTP 422.

### 1.7 Program

```
Program
  Id              uuid            PK
  TenantId        uuid            NOT NULL FK → Tenant.Id
  Name            varchar(300)    NOT NULL
  MaxCapacity     int             NOT NULL
  CreatedAt       timestamptz     NOT NULL DEFAULT now()
```

### 1.8 ProgramEnrolment

```
ProgramEnrolment
  Id              uuid            PK
  TenantId        uuid            NOT NULL FK → Tenant.Id
  ProgramId       uuid            NOT NULL FK → Program.Id
  MemberId        uuid            NOT NULL FK → Member.Id
  Status          varchar(20)     NOT NULL  -- enum: Enrolled | Waitlisted | Cancelled
  WaitlistPosition int            NULL      -- set when Status = Waitlisted, insertion-ordered
  EnrolledAt      timestamptz     NOT NULL DEFAULT now()
```

Enrolment guard: before inserting with `Status = Enrolled`, confirm the member has an active `Membership` where `Type = Program` and `PaymentStatus = Completed` and today falls within `[StartDate, EndDate]`. If not, return HTTP 422 with `"Active program membership required"`.

Capacity guard: if enrolled count for the program equals `MaxCapacity`, set `Status = Waitlisted` and assign the next `WaitlistPosition`.

### 1.9 Event

```
Event
  Id              uuid            PK
  TenantId        uuid            NOT NULL FK → Tenant.Id
  Name            varchar(300)    NOT NULL
  EventDate       timestamptz     NOT NULL
  MaxCapacity     int             NOT NULL
  BillingBasis    varchar(20)     NOT NULL  -- enum: Family | Individual
  CreatedAt       timestamptz     NOT NULL DEFAULT now()
```

### 1.10 EventRegistration

```
EventRegistration
  Id              uuid            PK
  TenantId        uuid            NOT NULL FK → Tenant.Id
  EventId         uuid            NOT NULL FK → Event.Id
  FamilyId        uuid            NULL FK → Family.Id   -- set when Event.BillingBasis = Family
  MemberId        uuid            NULL FK → Member.Id   -- set when Event.BillingBasis = Individual
  Status          varchar(20)     NOT NULL  -- enum: Registered | Waitlisted | Cancelled
  WaitlistPosition int            NULL
  RegisteredAt    timestamptz     NOT NULL DEFAULT now()
```

Registration guard: requires an active `Membership` where `Type = Event` and `PaymentStatus = Completed`. Registration must be rejected if the requesting user's role is `FamilyMember` (individual login). Only `PrimaryAccountHolder` or `Administrator` may register for events.

### 1.11 AuditLog

```
AuditLog
  Id              uuid            PK
  TenantId        uuid            NOT NULL FK → Tenant.Id
  ActorUserId     uuid            NOT NULL FK → AppUser.Id
  ActorRole       varchar(30)     NOT NULL
  Action          varchar(100)    NOT NULL  -- e.g. "MinorDataRead", "Login", "LoginFailed", "RoleChanged"
  TargetEntityType varchar(100)   NULL      -- e.g. "Member"
  TargetEntityId  uuid            NULL
  Detail          jsonb           NULL      -- fields accessed, reason, etc.
  OccurredAt      timestamptz     NOT NULL DEFAULT now()
```

The `AuditLog` table must have no `UPDATE` or `DELETE` permissions granted to the application database user. Enforce append-only at the database permission level.

---

## 2. Authentication and Session Management

### 2.1 Login — `POST /api/v1/auth/login`

Request:
```json
{ "email": "string", "password": "string" }
```

Logic:
1. Look up `AppUser` by `email`. If not found, return HTTP 401 (do not reveal whether the email exists).
2. If `LockedUntil` is in the future, return HTTP 423 with `"Account locked"`.
3. Verify `PasswordHash`. If wrong, increment `FailedLoginCount`. If `FailedLoginCount >= 5`, set `LockedUntil = now() + 15 minutes` and return HTTP 423.
4. Reset `FailedLoginCount = 0`.
5. Count `ActiveSession` rows for this user where `ExpiresAt > now()` and `RevokedAt IS NULL`. If count equals `MaxConcurrentSessions`, return HTTP 409 with `"Session limit reached"`.
6. Issue a JWT with claims: `sub` (UserId), `role`, `tenant_id`, `family_id` (if applicable), `jti` (new UUID). Set expiry to 8 hours (configurable via `appsettings.json` key `Jwt:ExpiryHours`).
7. Insert an `ActiveSession` row with `TokenJti = jti`.
8. Write an `AuditLog` row with `Action = "Login"`.
9. Return HTTP 200 with the token.

### 2.2 Logout — `POST /api/v1/auth/logout`

Set `ActiveSession.RevokedAt = now()` for the current `jti`. Return HTTP 204.

### 2.3 JWT validation middleware

On every request to a protected route:
1. Validate signature, expiry, and required claims (`sub`, `role`, `tenant_id`, `jti`).
2. Look up `ActiveSession` by `jti`. If not found or `RevokedAt IS NOT NULL`, return HTTP 401.
3. Attach `TenantId`, `UserId`, and `Role` to the request context. All downstream queries must use `TenantId` from this context.

### 2.4 Failed login audit

Write an `AuditLog` row with `Action = "LoginFailed"` on every failed password check.

---

## 3. Role-Based Access Control

Apply these rules via policy attributes on every controller/endpoint. The middleware from §2.3 supplies the role claim.

| Role | Permitted actions |
|---|---|
| `Administrator` | All operations within the tenant. |
| `PrimaryAccountHolder` | Family CRUD, member CRUD within own family, event registration for own family, view own family's memberships and enrolments. |
| `FamilyMember` | Read own member profile, read own memberships and enrolments. Cannot register for events. Cannot view other members' data. |
| `ProgramCoordinator` | Read and update assigned programs only. Read enrolments for assigned programs. |
| `EventCoordinator` | Read and update assigned events only. Read registrations for assigned events. |

Any request that does not match the permitted actions for the authenticated role must return HTTP 403. Role checks must run in the API layer, not the application or domain layer.

---

## 4. Family and Member API

### 4.1 Create family — `POST /api/v1/families`

Roles: `Administrator`, `PrimaryAccountHolder`

Request:
```json
{
  "name": "string",
  "primaryHolder": {
    "fullName": "string",
    "dateOfBirth": "date",
    "contactEmail": "string",
    "contactPhone": "string"
  }
}
```

Logic:
1. Validate `primaryHolder.dateOfBirth` produces `IsAdult = true`. If not, return HTTP 422 with `"Family must have at least one adult"`.
2. Create `Family` record with a new QR token (see §5).
3. Create `Member` record for the primary holder.
4. Create `AppUser` record with `Role = PrimaryAccountHolder`, `IsPrimaryHolder = true`, linked to the new member and family.
5. Return HTTP 201 with the created family including the primary holder's member ID.

### 4.2 Get family — `GET /api/v1/families/{familyId}`

Roles: `Administrator`, `PrimaryAccountHolder` (own family only)

Returns: family details, list of members (id, fullName, isAdult, dateOfBirth), QR token URL.

`FamilyMember` role may call `GET /api/v1/families/{familyId}/summary` which returns name and member count only — no individual member details.

### 4.3 Create member — `POST /api/v1/families/{familyId}/members`

Roles: `Administrator`, `PrimaryAccountHolder` (own family only)

Request:
```json
{
  "fullName": "string",
  "dateOfBirth": "date",
  "contactEmail": "string | null",
  "contactPhone": "string | null",
  "consent": {
    "givenBy": "userId",
    "givenAt": "timestamptz"
  } | null
}
```

Logic:
1. Compute `isAdult` from `dateOfBirth`.
2. If `isAdult = false`: `consent` must be present. Set `ConsentGiven = true`, `ConsentGivenBy`, `ConsentGivenAt`. If `consent` is absent, return HTTP 422 with `"Consent required for members under 18"`.
3. If `isAdult = true`: `contactEmail` must be present. If absent, return HTTP 422 with `"Contact email required for adult members"`.
4. Assign the member to this family. A member with an existing `FamilyId` may not be re-assigned; return HTTP 409.
5. Generate a QR token for the member (see §5).
6. Return HTTP 201.

### 4.4 Get member — `GET /api/v1/members/{memberId}`

Roles: `Administrator`, `PrimaryAccountHolder` (own family), `FamilyMember` (own record only)

If the member `IsAdult = false`, write an `AuditLog` row with `Action = "MinorDataRead"`, `TargetEntityType = "Member"`, `TargetEntityId = memberId`, `Detail = { "fields": ["all"] }` before returning the response.

---

## 5. QR Token System

### 5.1 Token format

QR tokens are JWTs signed with the backend's signing key. Claims:

```json
{
  "sub": "<familyId or memberId>",
  "type": "family | member",
  "tenant_id": "<tenantId>",
  "jti": "<uuid>",
  "iat": <issued_at_unix>,
  "exp": <expires_at_unix>
}
```

Token expiry default: 90 days. Configurable via `appsettings.json` key `QrToken:ExpiryDays`. Do not use the same signing key as the auth JWT; use a separate `QrToken:SigningKey` config value.

### 5.2 Token generation

Called internally when a Family or Member is created, and when rotation or regeneration is triggered.

1. Generate a new `jti` UUID.
2. Sign the JWT.
3. Write `QrToken`, `QrTokenIssuedAt`, `QrTokenExpiresAt` to the Family or Member row.
4. The previous token is immediately invalid; no grace period.

### 5.3 QR token lookup — `GET /api/v1/qr/lookup`

This endpoint must not be used for login. It returns profile data only.

Header: `Authorization: Bearer <auth-jwt>` (the scanning user's session token, not the QR token)  
Query param: `token=<qr-jwt>`

Logic:
1. Validate the QR JWT signature and expiry. If invalid or expired, return HTTP 401 with `"QR token invalid or expired"`.
2. Extract `sub`, `type`, and `tenant_id` from the QR JWT.
3. Confirm `tenant_id` matches the scanning user's tenant. If not, return HTTP 403.
4. If `type = "family"`: return family summary scoped to the scanning user's role.
5. If `type = "member"`: return member profile scoped to the scanning user's role. If `IsAdult = false`, write `AuditLog` with `Action = "MinorDataRead"`.

### 5.4 Admin token regeneration — `POST /api/v1/families/{familyId}/qr/regenerate` and `POST /api/v1/members/{memberId}/qr/regenerate`

Roles: `Administrator` only. Calls internal token generation (§5.2). Returns the new token. Logs `Action = "QrTokenRegenerated"` in `AuditLog`.

---

## 6. Membership API

### 6.1 Create membership — `POST /api/v1/memberships`

Roles: `Administrator`

Request:
```json
{
  "type": "Family | Program | Event",
  "familyId": "uuid | null",
  "memberId": "uuid | null",
  "startDate": "date",
  "endDate": "date",
  "billingBasis": "Family | Individual"
}
```

Validation:
- `type = Family` → `familyId` required, `memberId` null.
- `type = Program` → `memberId` required, `familyId` null.
- `type = Event` → either `familyId` or `memberId` required depending on `billingBasis`.
- Overlap check: query for existing memberships with the same `type` and matching `familyId`/`memberId` where `startDate <= newEndDate AND endDate >= newStartDate`. If found, return HTTP 409 with `"Overlapping membership exists"`.
- `startDate` must be before `endDate`. If not, return HTTP 422.

On success, create the record with `PaymentStatus = Initiated`. Return HTTP 201.

### 6.2 Update payment status — `PATCH /api/v1/memberships/{membershipId}/payment-status`

Roles: `Administrator`

Request: `{ "status": "Pending | Completed | Failed | Refunded" }`

Validate the transition against the allowed matrix in §1.6. If invalid, return HTTP 422 with `"Invalid payment status transition from X to Y"`. On success, update and return HTTP 200.

### 6.3 Get memberships — `GET /api/v1/memberships`

Query params: `familyId`, `memberId`, `type`, `paymentStatus`  
Roles: `Administrator` (any), `PrimaryAccountHolder` (own family/members only)

---

## 7. Program and Enrolment API

### 7.1 Create program — `POST /api/v1/programs`

Roles: `Administrator`

Request: `{ "name": "string", "maxCapacity": int }`

Returns HTTP 201 with the created program including `id`.

### 7.2 Get program — `GET /api/v1/programs/{programId}`

Roles: `Administrator`, `ProgramCoordinator` (assigned programs only)

### 7.3 Enrol in program — `POST /api/v1/programs/{programId}/enrolments`

Roles: `Administrator`, `PrimaryAccountHolder`

Request: `{ "memberId": "uuid" }`

Logic:
1. Confirm the member belongs to the requesting user's family (if `PrimaryAccountHolder`).
2. Check for active `Program` membership: `Type = Program`, `PaymentStatus = Completed`, today within `[StartDate, EndDate]`. If none, return HTTP 422 with `"Active program membership required"`.
3. Check existing enrolment: if an `Enrolled` or `Waitlisted` record already exists for this member and program, return HTTP 409.
4. Count `Enrolled` records for the program. If count < `MaxCapacity`, insert with `Status = Enrolled`. Otherwise insert with `Status = Waitlisted` and `WaitlistPosition = (max existing WaitlistPosition) + 1`.
5. Return HTTP 201 with status and waitlist position if applicable.

### 7.4 Cancel enrolment — `DELETE /api/v1/programs/{programId}/enrolments/{enrolmentId}`

Roles: `Administrator`, `PrimaryAccountHolder` (own family)

Set `Status = Cancelled`. If the cancelled record was `Enrolled`, promote the next `Waitlisted` record (lowest `WaitlistPosition`) to `Enrolled` and set its `WaitlistPosition = null`.

---

## 8. Event and Registration API

### 8.1 Create event — `POST /api/v1/events`

Roles: `Administrator`

Request: `{ "name": "string", "eventDate": "timestamptz", "maxCapacity": int, "billingBasis": "Family | Individual" }`

Returns HTTP 201.

### 8.2 Register for event — `POST /api/v1/events/{eventId}/registrations`

Roles: `Administrator`, `PrimaryAccountHolder`

`FamilyMember` role must receive HTTP 403 with `"Event registration requires a family account"`.

Request: `{ "familyId": "uuid | null", "memberId": "uuid | null" }`

Logic:
1. Confirm `billingBasis` alignment: if `Event.BillingBasis = Family` then `familyId` required; if `Individual` then `memberId` required.
2. Check active `Event` membership with `PaymentStatus = Completed`. If none, return HTTP 422 with `"Active event membership required"`.
3. Count `Registered` records. If count < `MaxCapacity`, insert `Status = Registered`. Otherwise insert `Status = Waitlisted` with position.
4. Return HTTP 201.

### 8.3 Cancel registration — `DELETE /api/v1/events/{eventId}/registrations/{registrationId}`

Roles: `Administrator`, `PrimaryAccountHolder` (own family)

Same waitlist promotion logic as §7.4.

---

## 9. QR Scanning UI (React)

### 9.1 QR display

Component: `<QrCode entityId={id} entityType="family|member" token={qrToken} />`

Renders the QR token as a scannable image using `qrcode.react`. Display the entity name and expiry date beneath the code. If the token's `exp` is within 7 days, show a warning: `"QR code expiring soon"`.

### 9.2 QR scanner

Component: `<QrScanner onResult={handleScan} />`

Uses `@zxing/browser` to access the device camera via `navigator.mediaDevices.getUserMedia`. Requires HTTPS. On successful decode, call `GET /api/v1/qr/lookup?token=<decoded>` with the session JWT. Display the returned profile scoped to the current user's role. On error (expired, invalid, wrong tenant), display the error message from the API. Do not fall back to manual ID entry.

---

## 10. Audit Log

### 10.1 Required audit events

The following actions must write an `AuditLog` row. No exceptions.

| Action value | Trigger |
|---|---|
| `Login` | Successful login |
| `LoginFailed` | Failed login attempt |
| `RoleChanged` | `AppUser.Role` updated |
| `MinorDataRead` | Any read of a `Member` record where `IsAdult = false` |
| `QrTokenRegenerated` | Admin-forced QR regeneration |

### 10.2 Append-only enforcement

The application database user must have `INSERT` only on `AuditLog`. No `UPDATE`, `DELETE`, or `TRUNCATE` grants. Include this in the EF Core migration as a raw SQL statement:

```sql
REVOKE UPDATE, DELETE, TRUNCATE ON "AuditLog" FROM <app_db_user>;
```

### 10.3 Read audit log — `GET /api/v1/audit-logs`

Roles: `Administrator` only  
Query params: `action`, `actorUserId`, `targetEntityId`, `from` (timestamptz), `to` (timestamptz), `page`, `pageSize` (max 100)  
Returns paginated results ordered by `OccurredAt DESC`.

---

## 11. Minor Data Protection

### 11.1 Consent enforcement

The `POST /api/v1/families/{familyId}/members` endpoint (§4.3) must reject any request to create a member with `IsAdult = false` if `consent` is not present and valid in the request body. Return HTTP 422. This check runs before any database write.

### 11.2 Audit on every minor data read

Any endpoint that returns a `Member` record where `IsAdult = false` must write an `AuditLog` row before returning the response. This applies to: `GET /api/v1/members/{memberId}`, `GET /api/v1/families/{familyId}` (for each minor in the response), and `GET /api/v1/qr/lookup` when the resolved entity is a minor member.

---

## 12. Security Requirements

### 12.1 Transport

Redirect all HTTP requests to HTTPS. Return HTTP 301. Configure `UseHttpsRedirection()` in the .NET pipeline. Do not serve any response over plain HTTP.

### 12.2 Encryption at rest

All columns containing personally identifiable data (`FullName`, `ContactEmail`, `ContactPhone`, `PasswordHash`, `Email`) must be stored in a PostgreSQL database with encryption at rest enabled at the infrastructure level. Document this as a deployment prerequisite. `PasswordHash` must use BCrypt with a minimum work factor of 12.

### 12.3 Rate limiting

Apply rate limiting using ASP.NET Core's built-in rate limiter (`AddRateLimiter`):

- `POST /api/v1/auth/login`: 10 requests per minute per IP address. Return HTTP 429 on breach.
- All other authenticated endpoints: 300 requests per minute per `UserId`. Return HTTP 429 on breach.

### 12.4 Input validation

Use `FluentValidation` for all request DTOs. Validate on every endpoint before any application logic runs. Return HTTP 400 with a structured error body on validation failure:

```json
{
  "errors": [
    { "field": "contactEmail", "message": "Contact email is required for adult members" }
  ]
}
```

### 12.5 JWT validation

Configure `AddJwtBearer` with:
- `ValidateIssuerSigningKey = true`
- `ValidateIssuer = true`
- `ValidateAudience = true`
- `ValidateLifetime = true`
- `ClockSkew = TimeSpan.Zero`

Reject any token with a missing or unrecognised `jti`, or with a revoked session record.

### 12.6 Data retention

A Hangfire recurring job must run nightly. It deletes (hard delete) all `Member`, `AppUser`, `Family`, `Membership`, `ProgramEnrolment`, and `EventRegistration` records where `CreatedAt < now() - retention_period`. The retention period is read from `appsettings.json` key `DataRetention:Years`, defaulting to `7`. `AuditLog` records are exempt from deletion and retained indefinitely.

---

## 13. Multi-Tenancy

Use a shared PostgreSQL database with `tenant_id` column isolation (not separate schemas or databases).

Implementation:
1. Create a global EF Core query filter on every `DbSet<T>` where `T` implements `ITenantScoped` (an interface with `Guid TenantId`). The filter: `.HasQueryFilter(e => e.TenantId == _tenantId)` where `_tenantId` is injected from the request context per §2.3.
2. All `INSERT` operations must set `TenantId` from the request context before calling `SaveChangesAsync`. Override `SaveChangesAsync` in `DbContext` to enforce this.
3. Write an integration test that asserts a query made with `tenantId = A` never returns rows belonging to `tenantId = B`.

---

## 14. API Conventions

All endpoints must conform to the following:

- Route prefix: `/api/v1/`
- Content-Type: `application/json`
- Error response shape (all non-2xx responses):
  ```json
  { "error": "string", "errors": [ { "field": "string", "message": "string" } ] }
  ```
- Pagination shape (all list endpoints):
  ```json
  { "data": [], "page": 1, "pageSize": 20, "totalCount": 100 }
  ```
- IDs are UUIDs in all request and response bodies.
- Timestamps are ISO 8601 with UTC offset in all responses.
- HTTP status codes: 200 (ok), 201 (created), 204 (no content), 400 (validation), 401 (unauthenticated), 403 (forbidden), 404 (not found), 409 (conflict), 422 (business rule violation), 429 (rate limited).

---

## 15. Acceptance Criteria

Each item below must pass before the implementation is considered complete.

| # | Criterion |
|---|---|
| AC-1 | A family cannot be created without at least one adult member. |
| AC-2 | A minor member cannot be created without a consent record. |
| AC-3 | Login with an incorrect password 5 times locks the account for 15 minutes. |
| AC-4 | A JWT from tenant A cannot retrieve data belonging to tenant B. |
| AC-5 | An expired QR token returns HTTP 401 from the lookup endpoint. |
| AC-6 | A QR token presented to the auth login endpoint returns HTTP 400 or 401 (it must not authenticate the user). |
| AC-7 | A `FamilyMember` role attempting event registration receives HTTP 403. |
| AC-8 | Enrolling a member without an active completed program membership returns HTTP 422. |
| AC-9 | Enrolment when a program is at capacity sets status to `Waitlisted`. |
| AC-10 | Cancelling an `Enrolled` record promotes the first `Waitlisted` record to `Enrolled`. |
| AC-11 | Every read of a minor's profile writes an `AuditLog` row with `Action = "MinorDataRead"`. |
| AC-12 | A direct `DELETE` or `UPDATE` on the `AuditLog` table by the application database user fails with a permission error. |
| AC-13 | An invalid payment status transition (e.g. `Completed` → `Initiated`) returns HTTP 422. |
| AC-14 | Overlapping membership creation for the same family/type returns HTTP 409. |
| AC-15 | All HTTP requests are redirected to HTTPS. |
| AC-16 | More than 10 login attempts per minute from the same IP returns HTTP 429. |
| AC-17 | A `ProgramCoordinator` cannot access a program they are not assigned to. |
| AC-18 | A member cannot be assigned to more than one family. |
| AC-19 | Data older than the configured retention period is deleted by the nightly job; `AuditLog` rows are not deleted. |

---

*End of mandatory implementation specification v1.0*
