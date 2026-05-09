import { apiClient } from './client';
import type { Member } from '../types/domain.types';

export const usersApi = {
  getAll: () => apiClient.get<Member[]>('/members').then((r) => r.data),
};
