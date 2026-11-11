import { HttpErrorResponse } from '@angular/common/http';

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

/**
 * Turns an API problem-details response into a single message the pages can show. Validation
 * errors win over the generic title so the user sees what to fix.
 */
export function describeHttpError(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Unexpected error. Please try again.';
  }

  const problem = error.error as ProblemDetails | null;

  if (problem && typeof problem === 'object') {
    const validationMessages = Object.values(problem.errors ?? {}).flat();

    if (validationMessages.length > 0) {
      return validationMessages.join(' ');
    }

    if (problem.detail) {
      return problem.detail;
    }

    if (problem.title) {
      return problem.title;
    }
  }

  return error.status === 0
    ? 'The API is not reachable. Is the backend running?'
    : error.message;
}
