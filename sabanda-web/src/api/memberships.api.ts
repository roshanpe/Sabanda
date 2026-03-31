import { apiClient } from './client';
import type { CreateMembershipRequest, UpdatePaymentStatusRequest } from '../types/api.types';
import type { Membership } from '../types/domain.types';

export const membershipsApi = {
  create: (data: CreateMembershipRequest) =>
    apiClient.post<Membership>('/memberships', data).then((r) => r.data),

  updatePaymentStatus: (id: string, data: UpdatePaymentStatusRequest) =>
    apiClient.patch<Membership>(`/memberships/${id}/payment-status`, data).then((r) => r.data),
};
