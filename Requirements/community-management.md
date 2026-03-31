# Community Management Application
## Software Requirements Specification (SRS)

**Stack:** .NET backend · React frontend · Python AI service

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [User Roles](#2-user-roles)
3. [Account and Family Rules](#3-account-and-family-rules)
4. [Phase 1 — Foundation](#4-phase-1--foundation)
5. [Phase 2 — Member Management and QR System](#5-phase-2--member-management-and-qr-system)
6. [Phase 3 — Memberships, Programs, Events and Calendar](#6-phase-3--memberships-programs-events-and-calendar)
7. [Phase 4 — Notifications, Communications and Admin Operations](#7-phase-4--notifications-communications-and-admin-operations)
8. [Phase 5 — AI Query Service, Payment Scaffold and Performance](#8-phase-5--ai-query-service-payment-scaffold-and-performance)
9. [Non-Functional Requirements](#9-non-functional-requirements)
10. [Architecture Notes and Constraints](#10-architecture-notes-and-constraints)
11. [Future Enhancements](#11-future-enhancements)
12. [Requirements Classification](#12-requirements-classification)

---

## 1. Introduction

### 1.1 Purpose

This document defines the software requirements for a web-based Community Management Application. It is structured by implementation phase to support delivery planning. Each phase includes the functional requirements it delivers, the architectural decisions that must be made before work begins, and the acceptance gate that signals readiness to proceed.

### 1.2 Scope

The system manages family-based community operations including:

- Family registration and individual member profiles
- QR-based identification for lookup (not authentication)
- Membership subscriptions at family and individual level
- Program and event registration with capacity and waitlist management
- Calendar-based scheduling with role-based visibility
- AI-assisted administrative querying with safety controls
- Notifications, bulk communications, and audit logging
- Multi-tenant architecture supporting data isolation per community

### 1.3 Payment Provision Notice

A payment system shall be provisioned in the architecture. Payment integration is not implemented in the current phase. The system shall be designed to support future connection to an external payment gateway without structural rework.

---

## 2. User Roles

| Role | Description |
|---|---|
| Primary Account Holder | Single responsible guardian per family. Manages the family account and can create logins for members. |
| Family Member | Individual linked to a family. Can log in separately if enabled. Sees own subscriptions and basic family details only. |
| Administrator | Full system access across all tenants (or within a tenant as configured). |
| Program Coordinator | Scoped access to assigned programs only. |
| Event Coordinator | Scoped access to assigned events only. |

---

## 3. Account and Family Rules

- Each family has exactly one Primary Account Holder.
- The Primary Account Holder may create login accounts for family members.
- A family login can view all member details and manage the family.
- An individual member login can view only their own subscriptions and basic family details.
- A member cannot belong to more than one family.
- A member who is enabled for individual login cannot use that login to perform family-level actions such as event registration.

---

## 4. Phase 1 — Foundation

**Gate:** Multi-tenancy pattern agreed, base schema migrated, JWT auth flow working end-to-end across roles.

### 4.1 Decisions Required Before Work Begins

These decisions block all downstream implementation. They must be made and recorded before Phase 1 development starts.

- **Multi-tenancy strategy** — choose between shared schema with `tenant_id` column isolation, separate schemas per tenant, or separate databases per tenant. This choice affects every migration, every query, and every middleware layer.
- **API versioning scheme** — URL prefix `/api/v1/...` is specified. Confirm route conventions and whether a version shim or controller inheritance pattern will be used.
- **Authentication provider** — decide whether to implement JWT issuance natively in .NET or delegate to an identity service such as Keycloak or Azure AD B2C.
- **Email provider** — select a transactional email provider (Amazon SES or SendGrid recommended) to support notifications in Phase 4.

### 4.2 Functional Requirements

#### FR-1: Family creation

A family record must include at least one adult member at the time of creation.

#### FR-2: Unique identifiers

Every family and every individual member must be assigned a unique system-generated identifier at creation. These identifiers are immutable.

#### FR-3: JWT authentication

User login shall use JWT-based authentication. Tokens must include role and tenant claims. Sessions must expire after a configurable timeout. Account lockout must trigger after repeated failed login attempts. Configurable concurrent session limits must be enforced.

#### FR-4: Role-based access control

All API endpoints must enforce RBAC based on the authenticated user's role. Role checks must be applied at the API layer, not only in the UI. The five roles defined in Section 2 represent the complete set for this phase.

#### FR-5: Multi-tenant data isolation

All data reads and writes must be scoped to the authenticated user's tenant. Cross-tenant data access must be impossible by construction, not by convention. Tenant-level configuration settings must be supported.

#### FR-6: API versioning

All API routes shall use the `/api/v1/...` prefix. Backward compatibility must be maintained within a version. A deprecation policy must be defined before any breaking change is introduced in a future version.

### 4.3 Technical Deliverables

- .NET solution with layered architecture (API, Application, Domain, Infrastructure)
- Entity Framework Core data model covering Family, Member, User, and Tenant
- JWT middleware with role and tenant claim validation
- RBAC policy configuration
- React application scaffold with protected routing
- CI/CD pipeline skeleton with HTTPS enforcement and OWASP ZAP baseline scan

---

## 5. Phase 2 — Member Management and QR System

**Gate:** QR scan returns correct role-scoped profile; expired tokens are rejected; audit log entries are created for every minor data access.

### 5.1 Decisions Required Before Work Begins

- **QR token rotation strategy** — define the rotation interval and whether rotation is time-based, event-triggered, or admin-initiated.
- **Token storage** — decide whether QR tokens are stored in the primary database or a fast-access store. Note that caching infrastructure is excluded from the current phase; design the interface so a cache can be added in Phase 5 without changing the token service contract.
- **Minor data consent model** — define the consent capture and storage approach required by applicable privacy regulation (GDPR, COPPA, or equivalent) before any minor data is written.

### 5.2 Member Profile Requirements

#### FR-7: Member details

Every member record must store: full name, date of birth, and — for adult members — at least one contact method.

#### FR-8: Child validation

A member is classified as a child if their date of birth indicates they are under 18 years of age at the time of registration. The system must enforce this classification and must not allow a child to be registered as an adult.

#### FR-9: Adult attributes

Adult member profiles must support the following optional fields: occupation, one or more skills with a proficiency level (Basic, Proficient, or Expert), and business name.

### 5.3 QR Code Requirements

#### FR-10: QR token generation

One QR code shall be generated per family and one per individual member. Tokens shall use JWT format and must be opaque to the end user.

#### FR-11: QR usage constraint

QR codes must not be used for login or authentication. They may only be used to trigger an information lookup. This constraint must be enforced at the API level, not only by UI design.

#### FR-12: Token lifecycle

Tokens must support: configurable expiry, scheduled rotation, and administrator-forced regeneration. Expired and invalidated tokens must be rejected at the point of scan. The token service interface must be designed to accommodate a caching layer without interface changes.

#### FR-13: QR scanning interface

A web-based camera scanning interface must be provided. The interface must work in modern mobile and desktop browsers over HTTPS. After a successful scan, data visibility must be scoped to the scanning user's role per FR-4.

### 5.4 Privacy and Audit Requirements

#### FR-14: Minor data access logging

Every read of a minor's profile data must be recorded in the audit log with the accessing user's identity, role, timestamp, and the fields accessed.

#### FR-15: Consent requirement

Explicit consent must be captured and stored before any minor's personal data is recorded. The consent record must identify who provided consent and when.

### 5.5 Technical Deliverables

- Member profile CRUD API with adult/child classification enforcement
- QR token generation, rotation, and forced-regeneration endpoints
- Token expiry and rejection middleware
- React QR display component (using a library such as `qrcode.react`)
- React camera scanning component (using a library such as `zxing-js`)
- Immutable audit log service with minor-access event capture

---

## 6. Phase 3 — Memberships, Programs, Events and Calendar

**Gate:** End-to-end enrolment flow works with active membership guard; waitlist promotes correctly when a place opens; calendar reflects role-based filters; CSV import validates schema, reports errors, and supports partial success.

### 6.1 Decisions Required Before Work Begins

- **Waitlist promotion mechanism** — decide whether promotion from waitlist to enrolled is automatic (triggered when a place opens) or manual (coordinator-approved). This affects the event model and notification triggers.
- **Overlap detection scope** — clarify whether the no-overlapping-memberships rule applies per individual, per family, or both, and across which membership types.
- **Event payment granularity** — confirm the billing unit for events: family-level charge, individual-level charge, or configurable per event.

### 6.2 Membership Requirements

#### FR-16: Subscription types

The system must support three membership types:

| Type | Billing basis | Billing cycle |
|---|---|---|
| Family membership | Per family | Annual |
| Program membership | Per individual | Quarterly |
| Event registration | Per family or per individual | Per event |

#### FR-17: Membership constraints

No two memberships of the same type may overlap in time for the same family or individual. Early renewal must replace the existing membership from the renewal date forward. Gaps must not be created by early renewal.

#### FR-18: Payment lifecycle (current phase — CSV only)

Payment records must support the following status values: Initiated, Pending, Completed, Failed, and Refunded. In the current phase, payment status updates are applied via CSV upload by an administrator. The data model and service interface must be designed so that a payment gateway can be wired in Phase 5 without schema changes.

### 6.3 Program Requirements

#### FR-19: Enrollment guard

A member may only enroll in a program if they hold an active membership of the appropriate type. The enrolment API must validate this at request time.

#### FR-20: Capacity and wait list

Each program must have a configurable maximum capacity. When capacity is reached, further enrolment requests must be added to a waitlist. Waitlist position must be preserved in insertion order. When a place opens, the next waitlisted member must be offered the place per the mechanism agreed in Section 6.1.

#### FR-21: Schedule and attendance tracking

Each program must store a schedule (recurring or one-off sessions). Attendance must be recorded per session per member. Attendance records must be linkable to QR scan events from Phase 2.

### 6.4 Event Requirements

#### FR-22: Event registration guard

Event registration requires an active membership. Registration may only be performed under a family login, not an individual member login.

#### FR-23: Event capacity and wait list

Events must support the same capacity and wait list behaviour as programs (FR-20).

### 6.5 Calendar Requirements

#### FR-24: Calendar view

A calendar view must display programs and events. The view must be filterable by type (programs only, events only, or all). Visibility of items must be scoped to the authenticated user's role.

### 6.6 Coordinator Assignment

#### FR-25: Program and event coordinators

Administrators may assign a Program Coordinator to one or more programs and an Event Coordinator to one or more events. Assigned coordinators may access only their assigned entities.

### 6.7 Bulk Operations

#### FR-26: CSV bulk import

Administrators may upload CSV files to create or update memberships, programs, and events in bulk. The following constraints apply to all CSV imports:

- A strict column schema must be defined and published for each entity type.
- Required fields and data types must be validated before any record is written.
- Validation errors must be reported per row with a clear error message.
- A valid subset of rows must be committed even when other rows fail (partial success).
- A summary of committed and rejected rows must be returned to the administrator.

### 6.8 Attendance Management

#### FR-27: Attendance correction

Administrators may manually add, modify, or remove attendance records for any session. All manual corrections must be recorded in the audit log with the administrator's identity and reason.

### 6.9 Technical Deliverables

- Membership service with type enforcement, overlap detection, and renewal logic
- Program and event APIs with capacity, wait list, and schedule management
- Enrollment service with active-membership guard
- Attendance service with QR scan linkage and manual correction
- Calendar API with role-scoped filtering
- React calendar component
- CSV import pipeline with schema validation, error reporting, and partial-success handling

---

## 7. Phase 4 — Notifications, Communications and Admin Operations

**Gate:** Expiry reminders fire at the correct time; age-transition alerts trigger on the correct date; bulk email filters produce the correct recipient list; deregistration workflow creates a complete audit trail.

### 7.1 Decisions Required Before Work Begins

- **Background job infrastructure** — the SRS currently states no background services in the current phase. Notifications (FR-28, FR-29) and age-transition alerts (FR-30) require scheduled or triggered jobs. This constraint must be resolved before Phase 4 starts. Hangfire for .NET is the recommended approach as it runs in-process and does not require a separate service.
- **Age-transition check frequency** — decide whether the age-18 check runs on a nightly schedule or is event-driven at login time.
- **Deregistration SLA** — define the maximum time an administrator has to process a de-registration request, as this affects notification and escalation design.

### 7.2 Notification Requirements

#### FR-28: Automated notifications

The system must send automated notifications for the following events:

- Membership approaching expiry (configurable lead time, recommended: 30 days and 7 days)
- Upcoming program sessions (configurable lead time)
- Upcoming events (configurable lead time)

Notification delivery must use the email provider selected in Phase 1. All notification sends must be logged.

#### FR-29: Deregistration workflow

A member or primary account holder may request de-registration by email. On receipt, an administrator must review and approve or reject the request. On approval, the system must remove the member's program enrollments, event registrations, and login account. All steps must be recorded in the audit log.

#### FR-30: Age-transition alert

When a member's date of birth indicates they have turned 18, the system must trigger a notification to the primary account holder and to the administrator. The notification must indicate that the member's data classification and consent status should be reviewed.

### 7.3 Bulk Communication Requirements

#### FR-31: Targeted bulk email

Administrators may send bulk email to a filtered subset of members. Supported filter criteria:

- Age range
- Skill or skill level
- Enrolled program
- Membership status (active, expired, none)

Filters may be combined. The system must display the estimated recipient count before the email is sent. All bulk sends must be logged with the filter criteria used and the recipient count.

### 7.4 Technical Deliverables

- Notification service with configurable triggers and email dispatch
- Scheduled job infrastructure (Hangfire or equivalent, pending constraint resolution)
- Age-transition detection and alert logic
- Bulk email targeting engine with filter composition and preview
- Deregistration request intake, admin review interface, and automated cleanup
- Manual attendance correction with audit logging (if not completed in Phase 3)

---

## 8. Phase 5 — AI Query Service, Payment Scaffold and Performance

**Gate:** RAG query returns role-appropriate results; prompt injection tests pass; payment lifecycle status transitions are correct; load test reaches the 5,000 concurrent user target.

### 8.1 Decisions Required Before Work Begins

- **Vector store** — choose a vector database for the RAG pipeline. Options: pgvector (lowest operational overhead), Qdrant, or Weaviate.
- **Embedding model** — choose between OpenAI embedding, Cohere, or a self-hosted model.
- **RAG chunking strategy** — define how member, program, and event data will be chunked and indexed for retrieval.
- **Payment gateway** — select the target gateway for future integration. Stripe is recommended for its API maturity and webhook model.
- **Caching layer** — the 5,000 concurrent user target is very unlikely to be achievable without a caching layer. This constraint must be reviewed in light of Phase 3–4 load test results. Redis is recommended.

### 8.2 AI Query Requirements

#### FR-32: Natural language query interface

Administrators may query system data using natural language. The query service must use a retrieval-augmented generation (RAG) pipeline. The Python AI service must expose a well-defined API consumed by the .NET backend.

#### FR-33: AI safety controls

The query service must implement all of the following controls:

- **Query validation** — queries must be validated before being passed to the language model.
- **Role-based result filtering** — results returned to the user must be filtered to data the user is authorised to see, applied after retrieval and before response generation.
- **Prompt injection protection** — the system must detect and reject inputs that attempt to override system instructions or extract unauthorised data.
- **Query scope restriction** — queries must be restricted to the tenant's data only.
- **Rate limiting** — the number of queries per user per time window must be configurable and enforced.

### 8.3 Payment Scaffold Requirements

#### FR-34: Payment lifecycle schema

The payment data model must support the following status values: Initiated, Pending, Completed, Failed, and Refunded. Status transitions must be validated (for example, a Completed payment may not move directly to Initiated).

#### FR-35: Current phase payment updates

In the current phase, payment status is updated by administrator CSV upload. The import must validate status transition rules and reject invalid transitions with a clear error message.

#### FR-36: Future-ready payment interface

The payment service must expose an interface that a payment gateway web-hook handler can implement. Stripe's web-hook event model is the reference design. The interface must be defined and documented in this phase even though it will not be wired to a gateway until a future phase.

### 8.4 Performance Requirements

#### NFR-P1: Concurrent user target

The system must support 5,000 concurrent users. This target must be validated by a load test run against the complete system (all phases integrated). If the target is not met without caching, the caching constraint from Section 10 must be lifted and a Redis layer added.

### 8.5 Technical Deliverables

- Python AI service with RAG pipeline, query validation, and rate limiting
- Prompt injection detection layer
- Role-filtered result assembly in the .NET API
- Payment lifecycle schema and status transition enforcement
- CSV-based payment status update with transition validation
- Payment gateway integration interface (stubs only, no live gateway)
- Load testing harness and performance profiling report
- Redis caching layer (conditional on load test outcome)

---

## 9. Non-Functional Requirements

### 9.1 Security

| Requirement | Detail |
|---|---|
| Transport | HTTPS enforced on all endpoints. HTTP requests must be redirected or rejected. |
| Encryption at rest | All personally identifiable data must be encrypted at rest. |
| RBAC | Role checks enforced at the API layer on every request. |
| Rate limiting | All public and authenticated API endpoints must be rate-limited. |
| Input validation | All inputs must be validated and sanitized before processing. |
| JWT validation | All tokens must be validated for signature, expiry, and required claims on every request. |
| OWASP | Development must follow OWASP Top 10 mitigation. OWASP ZAP baseline scanning must be integrated into CI from Phase 1. |

### 9.2 Privacy

- Access to minor (under-18) profile data must be logged on every read (FR-14).
- Explicit consent must be obtained and recorded before any minor's data is stored (FR-15).
- Data retention is configurable per tenant. The default retention period is 7 years.

### 9.3 Audit Logging

The audit log must be immutable. Once written, audit records must not be modifiable or deletable except by automated retention policy enforcement. The following events must always be logged:

- Successful and failed login attempts
- Every read of a minor's data
- Role assignment and role changes
- Administrator overrides of any record
- Deregistration request, approval, and execution
- Bulk email sends (filter criteria and recipient count)
- Manual attendance corrections

### 9.4 Availability and Performance

- Target concurrent user load: 5,000 (validated in Phase 5)
- The system must be accessible from modern mobile and desktop browsers
- API response time targets must be defined and measured during Phase 5 load testing

### 9.5 Data Migration

Data migration in the current phase is limited to CSV import and export. No ETL tooling or direct database migration from external systems is in scope.

---

## 10. Architecture Notes and Constraints

### 10.1 Technology Stack

| Layer | Technology |
|---|---|
| Backend API | .NET (C#), layered architecture |
| Frontend | React |
| AI service | Python |
| Database | Relational (SQL Server or PostgreSQL) |
| ORM | Entity Framework Core |

### 10.2 Current Phase Constraints

The following are explicitly excluded from the current phase. These are constraints, not permanent decisions, and must be revisited as evidence from load testing becomes available.

- **No caching layer** — to be revisited in Phase 5 if the 5,000 concurrent user target is not met without it.
- **No background services** — must be resolved before Phase 4, as scheduled notifications cannot be delivered without job infrastructure.
- **No payment gateway** — payment life cycle is CSV-managed in the current phase. Gateway integration is a Phase 5 deliverable.
- **No mobile apps** — the system is web-based only. Responsive design for mobile browsers is required.

### 10.3 Modular Design

All services must be designed for future extensibility. Dependencies between services must be expressed through interfaces, not concrete implementations. This applies particularly to the payment service, notification service, and QR token service, all of which have known future integration points.

### 10.4 API Design

- All routes use the `/api/v1/...` prefix
- Backward compatibility must be maintained within a version
- A formal deprecation notice must precede any breaking change in a future version
- All endpoints must return consistent error response shapes

---

## 11. Future Enhancements

The following items are out of scope for the current delivery but must be kept in mind during design to avoid blocking future work.

| Enhancement | Dependency |
|---|---|
| Payment gateway integration (Stripe) | Payment service interface defined in Phase 5 |
| Native mobile apps (iOS, Android) | API design from Phase 1 onward |
| Messaging integration (SMS, push) | Notification service interface from Phase 4 |
| Caching layer (Redis) | Load test results from Phase 5 |
| Background job service (standalone) | Hangfire in-process approach from Phase 4 |
| Reporting and analytics dashboard | Data model stability from Phase 3 |

---

## 12. Requirements Classification

This section classifies every functional and non-functional requirement by priority. The three tiers follow the MoSCoW convention adapted for this project:

- **Mandatory** — the system cannot go live without this. Absence breaks a core user journey, a legal obligation, or a security control.
- **Optional** — strongly recommended and planned for current delivery, but deferred to a follow-on release without breaking core operation.
- **Nice to have** — desirable for user experience or operational efficiency but not required for a functioning, compliant system.

---

### 12.1 Mandatory Requirements

These requirements are non-negotiable for any production release.

#### Core domain

| Requirement | Rationale |
|---|---|
| FR-1: Family creation with at least one adult | Without this, the data model's foundational unit cannot be created. |
| FR-2: Unique, immutable identifiers for families and members | Required for referential integrity across all other features. |
| FR-3: JWT authentication with session expiry and account lockout | Legal and security baseline. No system should go live without it. |
| FR-4: Role-based access control enforced at the API layer | Without this, every piece of member data is potentially exposed. |
| FR-5: Multi-tenant data isolation | Cross-tenant data leakage is a critical compliance failure. |
| FR-7: Member record with name, date of birth, and adult contact | Minimum viable profile to support any other operation. |
| FR-8: Child classification enforcement (under-18 rule) | Required for privacy compliance and minor protection controls. |

#### QR and identification

| Requirement | Rationale |
|---|---|
| FR-10: QR token generation per family and per individual | Core identification mechanism the system is built around. |
| FR-11: QR codes restricted to lookup only, not authentication | Security constraint. Violating this exposes login to QR theft. |
| FR-12: Token expiry and rejection of expired tokens | Expired credentials being accepted is a security failure. |
| FR-13: Web-based QR camera scanning interface | The QR identification system has no practical use without a scanning interface. Requiring a separate device defeats the purpose. |

#### Membership and enrolment

| Requirement | Rationale |
|---|---|
| FR-16: Three membership subscription types | The billing model underpins all program and event access. |
| FR-17: No overlapping memberships; correct early renewal behavior | Without this, billing integrity cannot be guaranteed. |
| FR-19: Active membership required for program enrollment | Core access guard. Without it, unpaid access is trivially possible. |
| FR-22: Active membership required for event registration | Same rationale as FR-19. |
| FR-20 / FR-23: Capacity enforcement and wait list | Overbooking programs or events causes operational failure. |

#### Privacy and audit

| Requirement | Rationale |
|---|---|
| FR-14: Immutable audit log entry for every read of minor data | Legal requirement under GDPR, COPPA, and equivalent frameworks. |
| FR-15: Consent captured before any minor data is stored | Legal requirement. Storing minor data without consent is unlawful. |
| NFR — HTTPS enforced | No personally identifiable data may transit unencrypted. |
| NFR — Encryption at rest | Regulatory baseline for any system holding personal data. |
| NFR — Immutable audit log | Audit records that can be altered are not audit records. |
| NFR — Data retention configurable, default 7 years | Required for compliance with data protection obligations. |

---

### 12.2 Optional Requirements

These requirements are planned for current delivery and should be included unless schedule or resource pressure forces a deferral decision. Each has a stated fallback.

| Requirement | Rationale | Fallback if deferred |
|---|---|---|
| FR-6: API versioning from day one | Strongly recommended; retrofitting versioning later is expensive. | Accept that the first breaking change will require all consumers to update simultaneously. |
| FR-9: Adult profile fields (occupation, skills, business name) | Required for bulk email targeting (FR-31) and skill-based filtering. | Bulk email is limited to membership status and age filters only. |

| FR-21: Schedule and attendance tracking per program session | Needed for meaningful programme management. | Attendance is tracked externally (spreadsheet) and imported via CSV. |
| FR-24: Calendar view with role-based filtering | Important for coordinator and member experience. | Users navigate programs and events via list views only. |
| FR-25: Coordinator assignment and scoped access | Required for safe delegation to non-admin staff. | Administrators handle all program and event management directly. |
| FR-26: CSV bulk import for memberships, programs, and events | Necessary for initial data load and ongoing batch operations. | Data is entered manually record by record. |
| FR-27: Manual attendance correction by administrators | Required for operational accuracy. | Incorrect attendance records require a support request to fix at the database level. |
| FR-28: Automated notifications (expiry, program, event reminders) | High value for member retention and event attendance. | Administrators send reminders manually via bulk email. |
| FR-29: De-registration request and admin approval workflow | Required for GDPR right-to-erasure compliance. | Deregistration is handled by direct administrator action with no audit trail. |
| FR-30: Age-transition alert when a member turns 18 | Required for timely consent and data classification review. | Administrators run a periodic report to identify members approaching 18. |
| FR-34: Payment life cycle schema with status transition validation | Required to make the future gateway integration structurally sound. | Payment status is stored as a free-text field with no transition enforcement. |
| FR-35: CSV-based payment status updates | Current-phase substitute for gateway integration. | Payment status is updated by direct database edit. |
| FR-36: Future-ready payment service interface | Without this, gateway integration in the next phase requires breaking changes. | Gateway integration is treated as a greenfield addition in a future phase. |
| NFR — OWASP ZAP integrated into CI from Phase 1 | Security scanning from the start is far cheaper than retrofitting. | Manual security review before each release. |

---

### 12.3 Nice to Have

These requirements add meaningful value but are not required for a compliant, operational system. They are candidates for a post-launch backlog.

| Requirement | Value | Why it can wait |
|---|---|---|
| FR-31: Bulk email with multi-criteria targeting (age, skills, program, membership status) | Enables precise member communications and reduces noise. | Basic broadcast email covers the minimum communication need. Skills-based filtering also depends on FR-9 being complete. |
| FR-32: Natural language (AI/RAG) query interface | Reduces administrative query time and lowers the skill threshold for data access. | Standard search and filter interfaces cover the same queries, just more slowly. The RAG pipeline also requires stable, well-populated data from all earlier phases to return useful results. |
| FR-33: AI safety controls (prompt injection, rate limiting, role-filtered results) | Essential companion to FR-32 — the AI interface must not be deployed without it. | Conditional on FR-32. If FR-32 is deferred, FR-33 is not needed. |
| NFR-P1: 5,000 concurrent user target validated by load test | Required at scale. At smaller community sizes (under a few hundred concurrent users), the untested system will perform adequately. | The load test and any resulting caching layer addition are Phase 5 work. They should not block earlier phases from launching to a smaller initial user base. |
| Multi-tenant configurable settings per tenant | Useful for communities with different operating rules. | A single global configuration serves most cases at launch. Per-tenant overrides can be added incrementally. |
| API deprecation policy formally documented | Important for long-term API stability. | No second version exists yet. The policy can be written before the first `/api/v2/...` route is introduced. |

---

### 12.4 Classification Summary

| Tier | Count | Notes |
|---|---|---|
| Mandatory | 19 | All must be complete before any production release. |
| Optional | 16 | All planned for current delivery; each has a documented fallback. |
| Nice to have | 7 | Post-launch backlog candidates; none block go-live. |

---

*End of SRS v1.2*
