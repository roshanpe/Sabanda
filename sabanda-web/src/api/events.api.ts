import { apiClient } from './client';
import type { CreateEventRequest, RegisterEventRequest } from '../types/api.types';
import type { Event, Registration } from '../types/domain.types';

export const eventsApi = {
  create: (data: CreateEventRequest) =>
    apiClient.post<Event>('/events', data).then((r) => r.data),

  register: (eventId: string, data: RegisterEventRequest) =>
    apiClient.post<Registration>(`/events/${eventId}/registrations`, data).then((r) => r.data),

  cancelRegistration: (eventId: string, registrationId: string) =>
    apiClient.delete(`/events/${eventId}/registrations/${registrationId}`),
};
