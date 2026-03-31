import { apiClient } from './client';
import type { CreateFamilyRequest, CreateMemberRequest } from '../types/api.types';
import type { Family, FamilySummary, Member, QrToken } from '../types/domain.types';

export const familiesApi = {
  create: (data: CreateFamilyRequest) =>
    apiClient.post<Family>('/families', data).then((r) => r.data),

  getById: (id: string) =>
    apiClient.get<Family>(`/families/${id}`).then((r) => r.data),

  getSummary: (id: string) =>
    apiClient.get<FamilySummary>(`/families/${id}/summary`).then((r) => r.data),

  regenerateQr: (id: string) =>
    apiClient.post<QrToken>(`/families/${id}/qr/regenerate`).then((r) => r.data),

  createMember: (familyId: string, data: CreateMemberRequest) =>
    apiClient.post<Member>(`/families/${familyId}/members`, data).then((r) => r.data),
};
