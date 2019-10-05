import {ApiErrorType} from './api-error-type';
import {Observable, throwError} from 'rxjs';

export class ApiError {
  errorType: ApiErrorType;
  message: string;
  field: string;

  public static ServerError(message: string): Observable<never> {
    return throwError({errorType: ApiErrorType.ServerError, field: null, message: message});
  }

  public static BadRequest(message: string): Observable<never> {
    return throwError({errorType: ApiErrorType.BadRequest, field: null, message: message});
  }

  public static SignInRequired(): Observable<never> {
    return throwError({errorType: ApiErrorType.SignInRequired, field: null, message: null});
  }

  public static InvalidField(field: string, message: string): Observable<never> {
    return throwError({errorType: ApiErrorType.InvalidField, field: field, message: message});
  }

  public static Forbidden(message: string): Observable<never> {
    return throwError({errorType: ApiErrorType.Forbidden, field: null, message: message});
  }

  public static Conflict(message: string): Observable<never> {
    return throwError({errorType: ApiErrorType.Conflict, field: null, message: message});
  }

  public static Timeout(message: string): Observable<never> {
    return throwError({errorType: ApiErrorType.Timeout, field: null, message: message});
  }

  public static NotFound(message: string): Observable<never> {
    return throwError({errorType: ApiErrorType.NotFound, field: null, message: message});
  }
}
