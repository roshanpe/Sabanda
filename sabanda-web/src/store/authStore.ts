import { create } from 'zustand';
import { UserRole } from '../types/enums';

interface AuthState {
  token: string | null;
  userId: string | null;
  role: UserRole | null;
  familyId: string | null;
  expiresAt: string | null;
  setAuth: (token: string, userId: string, role: UserRole, expiresAt: string, familyId?: string) => void;
  clearAuth: () => void;
  isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  token: null,
  userId: null,
  role: null,
  familyId: null,
  expiresAt: null,

  setAuth: (token, userId, role, expiresAt, familyId) =>
    set({ token, userId, role, expiresAt, familyId: familyId ?? null }),

  clearAuth: () =>
    set({ token: null, userId: null, role: null, familyId: null, expiresAt: null }),

  isAuthenticated: () => {
    const { token, expiresAt } = get();
    if (!token || !expiresAt) return false;
    return new Date(expiresAt) > new Date();
  },
}));
