import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface TenantState {
  tenantSlug: string | null;
  setTenantSlug: (slug: string) => void;
}

export const useTenantStore = create<TenantState>()(
  persist(
    (set) => ({
      tenantSlug: null,
      setTenantSlug: (slug) => set({ tenantSlug: slug }),
    }),
    {
      name: 'tenant-storage',
    }
  )
);
