import { apiClient } from './client';
import type { CreateProgramRequest, EnrolMemberRequest } from '../types/api.types';
import type { Program, Enrolment } from '../types/domain.types';

export const programsApi = {
  create: (data: CreateProgramRequest) =>
    apiClient.post<Program>('/programs', data).then((r) => r.data),

  enrol: (programId: string, data: EnrolMemberRequest) =>
    apiClient.post<Enrolment>(`/programs/${programId}/enrolments`, data).then((r) => r.data),

  cancelEnrolment: (programId: string, enrolmentId: string) =>
    apiClient.delete(`/programs/${programId}/enrolments/${enrolmentId}`),
};
