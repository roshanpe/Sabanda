export const UserRole = {
  Administrator: 'Administrator',
  PrimaryAccountHolder: 'PrimaryAccountHolder',
  ProgramCoordinator: 'ProgramCoordinator',
  EventCoordinator: 'EventCoordinator',
  FamilyMember: 'FamilyMember',
} as const;
export type UserRole = (typeof UserRole)[keyof typeof UserRole];

export const MembershipType = {
  Program: 'Program',
  Event: 'Event',
} as const;
export type MembershipType = (typeof MembershipType)[keyof typeof MembershipType];

export const PaymentStatus = {
  Initiated: 'Initiated',
  Pending: 'Pending',
  Completed: 'Completed',
  Failed: 'Failed',
  Refunded: 'Refunded',
} as const;
export type PaymentStatus = (typeof PaymentStatus)[keyof typeof PaymentStatus];

export const EnrolmentStatus = {
  Enrolled: 'Enrolled',
  Waitlisted: 'Waitlisted',
  Cancelled: 'Cancelled',
} as const;
export type EnrolmentStatus = (typeof EnrolmentStatus)[keyof typeof EnrolmentStatus];

export const RegistrationStatus = {
  Registered: 'Registered',
  Waitlisted: 'Waitlisted',
  Cancelled: 'Cancelled',
} as const;
export type RegistrationStatus = (typeof RegistrationStatus)[keyof typeof RegistrationStatus];

export const EventBillingType = {
  Family: 'Family',
  Individual: 'Individual',
} as const;
export type EventBillingType = (typeof EventBillingType)[keyof typeof EventBillingType];

export const ProgramFrequency = {
  Weekly: 'weekly',
  Fortnightly: 'fortnightly',
  Monthly: 'monthly',
} as const;
export type ProgramFrequency = (typeof ProgramFrequency)[keyof typeof ProgramFrequency];

export const ProgramDay = {
  Monday: 'Monday',
  Tuesday: 'Tuesday',
  Wednesday: 'Wednesday',
  Thursday: 'Thursday',
  Friday: 'Friday',
  Saturday: 'Saturday',
  Sunday: 'Sunday',
} as const;
export type ProgramDay = (typeof ProgramDay)[keyof typeof ProgramDay];
