import { createContext, useContext } from 'react';
import { UserRole } from '../types/enums';

export interface AuthContextValue {
  token: string | null;
  userId: string | null;
  role: UserRole | null;
  familyId: string | null;
  isAuthenticated: boolean;
  login: (token: string, userId: string, role: UserRole, expiresAt: string, familyId?: string) => void;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}
