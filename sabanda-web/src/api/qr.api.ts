import { apiClient } from './client';
import type { QrLookupResult } from '../types/domain.types';

export const qrApi = {
  lookup: (token: string) =>
    apiClient.get<QrLookupResult>(`/qr/lookup?token=${encodeURIComponent(token)}`).then((r) => r.data),
};
