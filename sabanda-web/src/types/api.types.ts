export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  userId: string;
  role: string;
  familyId?: string;
}

export interface ProblemDetails {
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export interface CreateFamilyRequest {
  displayName: string;
  primaryHolderEmail: string;
  primaryHolderPassword: string;
}

export interface CreateMemberRequest {
  fullName: string;
  dateOfBirth: string;
  gender?: string;
  email?: string;
  phone?: string;
  consentGiven?: boolean;
  consentGivenBy?: string;
  consentGivenAt?: string;
  occupation?: string;
  businessName?: string;
}

export interface CreateMembershipRequest {
  familyId: string;
  type: string;
  startDate: string;
  endDate: string;
  memberId?: string;
}

export interface UpdatePaymentStatusRequest {
  newStatus: string;
}

import type { ProgramFrequency, ProgramDay } from './enums';

export interface CreateProgramRequest {
  name: string;
  capacity: number;
  description?: string;
  coordinatorUserId?: string;
  ageGroup?: string;
  frequency?: ProgramFrequency;
  venue?: string;
  day?: ProgramDay;
  time?: string;
}

export interface EnrolMemberRequest {
  memberId: string;
}

export interface CreateEventRequest {
  name: string;
  eventDate: string;
  capacity: number;
  billingType: string;
  description?: string;
  coordinatorUserId?: string;
}

export interface RegisterEventRequest {
  familyId: string;
  memberId?: string;
}
