import { UserRole } from '../types/enums';

export const STAFF_ROLES: UserRole[] = [
  UserRole.Administrator,
  UserRole.PrimaryAccountHolder,
  UserRole.ProgramCoordinator,
  UserRole.EventCoordinator,
];

export function isAdmin(role: UserRole | null): boolean {
  return role === UserRole.Administrator;
}

export function isStaff(role: UserRole | null): boolean {
  return role !== null && STAFF_ROLES.includes(role);
}

export function canManageFamilies(role: UserRole | null): boolean {
  return isAdmin(role);
}
