import { AxiosError } from 'axios';
import type { ProblemDetails } from '../types/api.types';

export function getErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const problem = error.response?.data as ProblemDetails | undefined;
    if (problem?.detail) return problem.detail;
    if (problem?.title) return problem.title;
    if (error.response?.status === 429) return 'Too many requests. Please wait and try again.';
  }
  if (error instanceof Error) return error.message;
  return 'An unexpected error occurred.';
}

export function getFieldErrors(error: unknown): Record<string, string[]> {
  if (error instanceof AxiosError) {
    const problem = error.response?.data as ProblemDetails | undefined;
    return problem?.errors ?? {};
  }
  return {};
}
