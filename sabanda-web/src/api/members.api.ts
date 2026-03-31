import { apiClient } from './client';
import type { Member, QrToken } from '../types/domain.types';

export const membersApi = {
  getById: (id: string) =>
    apiClient.get<Member>(`/members/${id}`).then((r) => r.data),

  regenerateQr: (id: string) =>
    apiClient.post<QrToken>(`/members/${id}/qr/regenerate`).then((r) => r.data),
};
