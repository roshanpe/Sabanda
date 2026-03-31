import { type ReactNode } from 'react';
import { AuthContext } from './AuthContext';
import { useAuthStore } from '../store/authStore';
import { authApi } from '../api/auth.api';
import { UserRole } from '../types/enums';

export function AuthProvider({ children }: { children: ReactNode }) {
  const store = useAuthStore();

  const login = (
    token: string,
    userId: string,
    role: UserRole,
    expiresAt: string,
    familyId?: string
  ) => {
    store.setAuth(token, userId, role, expiresAt, familyId);
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } finally {
      store.clearAuth();
    }
  };

  return (
    <AuthContext.Provider
      value={{
        token: store.token,
        userId: store.userId,
        role: store.role,
        familyId: store.familyId,
        isAuthenticated: store.isAuthenticated(),
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
