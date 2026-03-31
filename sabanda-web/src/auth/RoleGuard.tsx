import type { ReactNode } from 'react';
import { useAuth } from './AuthContext';
import { UserRole } from '../types/enums';

interface RoleGuardProps {
  roles: UserRole[];
  fallback?: ReactNode;
  children: ReactNode;
}

export function RoleGuard({ roles, fallback = null, children }: RoleGuardProps) {
  const { role } = useAuth();
  if (!role || !roles.includes(role)) return <>{fallback}</>;
  return <>{children}</>;
}
