import { create } from 'zustand';

interface TenantState {
  tenantSlug: string | null;
  setTenantSlug: (slug: string) => void;
}

export const useTenantStore = create<TenantState>((set) => ({
  tenantSlug: null,
  setTenantSlug: (slug) => set({ tenantSlug: slug }),
}));
