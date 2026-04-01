import axios, { AxiosError } from 'axios';
import { useAuthStore } from '../store/authStore';
import { useTenantStore } from '../store/tenantStore';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5142';

export const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api/v1`,
  headers: { 'Content-Type': 'application/json' },
});

// Attach auth + tenant headers on every request
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  const tenantSlug = useTenantStore.getState().tenantSlug;

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  if (tenantSlug) {
    config.headers['X-Tenant-Slug'] = tenantSlug;
  }
  return config;
});

// Handle 401 → clear auth and redirect to login
// Handle 429 → surface to caller via error
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().clearAuth();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
