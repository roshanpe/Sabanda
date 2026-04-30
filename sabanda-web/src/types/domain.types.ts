import {
  MembershipType,
  PaymentStatus,
  EnrolmentStatus,
  RegistrationStatus,
  EventBillingType,
  ProgramFrequency,
  ProgramDay,
} from './enums';

export interface Family {
  id: string;
  displayName: string;
  code: string;
  primaryHolderUserId: string;
  createdAt: string;
}

export interface FamilySummary extends Family {
  memberCount: number;
}

export interface Member {
  id: string;
  familyId: string;
  fullName: string;
  code: string;
  dateOfBirth: string;
  isAdult: boolean;
  gender?: string;
  email?: string;
  phone?: string;
  isPrimaryHolder: boolean;
  consentGiven: boolean;
  occupation?: string;
  businessName?: string;
  createdAt: string;
}

export interface Membership {
  id: string;
  familyId: string;
  memberId?: string;
  type: MembershipType;
  startDate: string;
  endDate: string;
  paymentStatus: PaymentStatus;
  createdAt: string;
}

export interface Program {
  id: string;
  name: string;
  description?: string;
  capacity: number;
  coordinatorUserId?: string;
  ageGroup?: string;
  frequency?: ProgramFrequency;
  venue?: string;
  day?: ProgramDay;
  time?: string;
  createdAt: string;
}

export interface Enrolment {
  id: string;
  programId: string;
  memberId: string;
  status: EnrolmentStatus;
  waitlistPosition?: number;
  enrolledAt: string;
  cancelledAt?: string;
}

export interface Event {
  id: string;
  name: string;
  description?: string;
  eventDate: string;
  capacity: number;
  billingType: EventBillingType;
  coordinatorUserId?: string;
  createdAt: string;
}

export interface Registration {
  id: string;
  eventId: string;
  familyId: string;
  memberId?: string;
  status: RegistrationStatus;
  waitlistPosition?: number;
  registeredAt: string;
  cancelledAt?: string;
}

export interface QrToken {
  token: string;
}

export interface QrLookupResult {
  subjectType: string;
  subjectId: string;
  tenantId: string;
}
