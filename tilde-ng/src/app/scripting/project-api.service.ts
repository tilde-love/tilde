import { Injectable } from '@angular/core';
import {Observable, throwError, } from 'rxjs';
import {HttpClient, HttpEvent, HttpEventType, HttpRequest} from '@angular/common/http';
import { environment } from '../../environments/environment';
import { map, filter, scan, catchError } from 'rxjs/operators';
import {Project, Runtime} from './project-types';

export class UploadInfo {
  fileName: string;
  progress: number;
  bytes: number;
  complete: boolean;
}

export enum ApiErrorType {
  ServerError,
  BadRequest,
  SignInRequired,
  InvalidField,
  Forbidden,
  Conflict,
  Timeout,
  NotFound
}

export class FileContents {
  public uri: string;
  public mimeType: string;
  public text: string;
}

export class ApiError {
  errorType: ApiErrorType;
  message: string;
  field: string;

  public static ServerError(message: string): Observable<never> {
    return throwError({ errorType: ApiErrorType.ServerError, field: null, message: message });
  }

  public static BadRequest(message: string): Observable<never> {
    return throwError({ errorType: ApiErrorType.BadRequest, field: null, message: message });
  }

  public static SignInRequired(): Observable<never> {
    return throwError({ errorType: ApiErrorType.SignInRequired, field: null, message: null });
  }

  public static InvalidField(field: string, message: string): Observable<never> {
    return throwError({ errorType: ApiErrorType.InvalidField, field: field, message: message });
  }

  public static Forbidden(message: string): Observable<never> {
    return throwError({ errorType: ApiErrorType.Forbidden, field: null, message: message });
  }

  public static Conflict(message: string): Observable<never> {
    return throwError({ errorType: ApiErrorType.Conflict, field: null, message: message });
  }

  public static Timeout(message: string): Observable<never> {
    return throwError({ errorType: ApiErrorType.Timeout, field: null, message: message });
  }

  public static NotFound(message: string): Observable<never> {
    return throwError({ errorType: ApiErrorType.NotFound, field: null, message: message });
  }
}

@Injectable({
  providedIn: 'root'
})
export class ProjectApiService {

  constructor(private http: HttpClient) {
  }

  public createProject(projectUri: string): Observable<string> {
    return this.http.post<string>(`${environment.apiUri}/projects/${projectUri}`, { })
      .pipe(
        map((result) => result),
        catchError((error) => {
          switch (error.status) {
            case 201:
              // we good
              break;
            case 404:
              return ApiError.NotFound(`A server error occurred while creating the project`);
            case 500:
              return ApiError.ServerError(`A server error occurred while creating the project`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            // case 401:
            //   return ApiError.SignInRequired();
            // case 403:
            //   return ApiError.Forbidden(`You do not have permission to delete this file`);
            // case 408:
            //   return ApiError.Timeout(`File could not be deleted as the operation timed out`);
            case 409:
              return ApiError.Conflict(`Project could not be created because of a conflict`);
            default:
              return ApiError.ServerError(`An unknown error occurred while creating the project (${error.status})`);
          }
        })
      );
  }

  public deleteProject(projectUri: string): Observable<string> {
    return this.http.delete<string>(`${environment.apiUri}/projects/${projectUri}`, { })
      .pipe(
        map((result) => result),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 404:
              return ApiError.NotFound(`Project not found`);
            case 500:
              return ApiError.ServerError(`A server error occurred while deleting the project`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            // case 401:
            //   return ApiError.SignInRequired();
            // case 403:
            //   return ApiError.Forbidden(`You do not have permission to delete this file`);
            // case 408:
            //   return ApiError.Timeout(`File could not be deleted as the operation timed out`);
            // case 409:
            //   return ApiError.Conflict(`File could not be deleted because of a conflict`);
            default:
              return ApiError.ServerError(`An unknown error occurred while deleting the project (${error.status})`);
          }
        })
      );
  }

  public uploadProject(file: File): Observable<UploadInfo> {
    const formData = new FormData();

    formData.append('file', file, file.name);

    const req = new HttpRequest('POST', `${environment.apiUri}/files/project`, formData, {
      reportProgress: true
    });

    return this.http.request(req)
      .pipe(map<HttpEvent<any>, UploadInfo>((event: HttpEvent<any>) => {
        switch (event.type) {
          case HttpEventType.Sent:
            return {fileName: file.name, progress: 0, bytes: 0, complete: false};

          case HttpEventType.ResponseHeader:
            return {fileName: file.name, progress: 0, bytes: 0, complete: false};

          case HttpEventType.UploadProgress:
            const progress = (1.0 / event.total) * event.loaded;
            return {fileName: file.name, progress: progress, bytes: event.loaded, complete: false};

          case HttpEventType.Response:
            return {fileName: file.name, progress: 1, bytes: 0, complete: true};
        }

        return {fileName: file.name, progress: 0, bytes: 0, complete: false};
      },
      catchError((error) => {
        switch (error.status) {
          case 200:
          case 201:
            // we good
            break;
          case 500:
            return ApiError.ServerError(`An server error occurred while uploading the file`);
          case 504:
            return ApiError.ServerError(`Cannot contact server`);
          case 401:
            return ApiError.SignInRequired();
          case 403:
            return ApiError.Forbidden(`You do not have permission to upload this file`);
          case 408:
            return ApiError.Timeout(`File could not be uploaded as the operation timed out`);
          case 409:
            return ApiError.Conflict(`File could not be uploaded because of a conflict`);
          default:
            return ApiError.ServerError(`An unknown error occurred while uploading the file (${error.status})`);
        }
      })));
  }

  public getProjects(): Observable<{ [ uri: string]: Project}> {
    return this.http.get<{ [ uri: string]: Project}>(`${environment.apiUri}/projects`, { })
      .pipe(
        map((result) => {
          return result;
        }),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 500:
              return ApiError.ServerError(`A server error occurred while getting projects`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting projects (${error.status})`);
          }
        })
      );
  }

  public getProject(projectUri: string): Observable<Project> {
    return this.http.get<Project>(`${environment.apiUri}/projects/${projectUri}`, { })
      .pipe(
        map((result) => result),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 404:
              return ApiError.NotFound(`Project not found`);
            case 500:
              return ApiError.ServerError(`A server error occurred while getting project`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting project (${error.status})`);
          }
        })
      );
  }

  public getFileText(projectUri: string, filename: string): Observable<FileContents> {
    return this.http.get<FileContents>(`${environment.apiUri}/projects/${projectUri}/${filename}`, {})
      .pipe(
        map((result) => {
          return result;
        }),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 404:
              return ApiError.NotFound(`File not found`);
            case 500:
              return ApiError.ServerError(`A server error occurred while getting file contents`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting file contents (${error.status})`);
          }
        })
      );
  }

  public setFileText(projectUri: string, filename: string, contents: string): Observable<string> {
    const formData = new FormData();

    formData.append('content', contents);

    return this.http.post<string>(`${environment.apiUri}/projects/${projectUri}/${filename}`, formData)
      .pipe(
        map((result) => result),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 404:
              return ApiError.NotFound(`File not found`);
            case 500:
              return ApiError.ServerError(`A server error occurred while setting file contents`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while setting file contents (${error.status})`);
          }
        })
      );
  }

  public deleteFile(projectUri: string, filename: string): Observable<string> {
    return this.http.delete<string>(`${environment.apiUri}/projects/${projectUri}/${filename}`, { })
      .pipe(
        map((result) => result),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 404:
              return ApiError.NotFound(`File not found`);
            case 500:
              return ApiError.ServerError(`A server error occurred while deleting the file`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while deleting the file (${error.status})`);
          }
        })
      );
  }

  public uploadFile(projectUri: string, file: File): Observable<UploadInfo> {
    const formData = new FormData();

    formData.append('file', file, file.name);

    const req = new HttpRequest('POST', `${environment.apiUri}/files/${projectUri}/${file.name}`, formData, {
      reportProgress: true
    });

    return this.http.request(req)
      .pipe(map<HttpEvent<any>, UploadInfo>((event: HttpEvent<any>) => {
          switch (event.type) {
            case HttpEventType.Sent:
              return {fileName: file.name, progress: 0, bytes: 0, complete: false};

            case HttpEventType.ResponseHeader:
              return {fileName: file.name, progress: 0, bytes: 0, complete: false};

            case HttpEventType.UploadProgress:
              const progress = (1.0 / event.total) * event.loaded;
              return {fileName: file.name, progress: progress, bytes: event.loaded, complete: false};

            case HttpEventType.Response:
              return {fileName: file.name, progress: 1, bytes: 0, complete: true};
          }

          return {fileName: file.name, progress: 0, bytes: 0, complete: false};
        },
        catchError((error) => {
          switch (error.status) {
            case 200:
            case 201:
              // we good
              break;
            case 401:
              return ApiError.SignInRequired();
            case 403:
              return ApiError.Forbidden(`You do not have permission to upload this file`);
            case 408:
              return ApiError.Timeout(`File could not be uploaded as the operation timed out`);
            case 409:
              return ApiError.Conflict(`File could not be uploaded because of a conflict`);
            case 500:
              return ApiError.ServerError(`An server error occurred while uploading the file`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while uploading the file (${error.status})`);
          }
        })));
  }

  public getRuntime(): Observable<Runtime> {
    return this.http.get<Runtime>(`${environment.apiUri}/script/runtime`, { })
      .pipe(
        map((result) => {
          return result;
        }),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 500:
              return ApiError.ServerError(`A server error occurred while getting runtime`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting runtime (${error.status})`);
          }
        })
      );
  }

  public loadProject(projectUri: string): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/script/runtime/run/${projectUri}`, { })
      .pipe(
        map((result) => {
          return result;
        }),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 500:
              return ApiError.ServerError(`A server error occurred while getting runtime`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting runtime (${error.status})`);
          }
        })
      );
  }

  public run(): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/script/runtime/run`, { })
      .pipe(
        map((result) => {
          return result;
        }),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 500:
              return ApiError.ServerError(`A server error occurred while getting runtime`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting runtime (${error.status})`);
          }
        })
      );
  }

  public pause(): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/script/runtime/pause`, { })
      .pipe(
        map((result) => {
          return result;
        }),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 500:
              return ApiError.ServerError(`A server error occurred while getting runtime`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting runtime (${error.status})`);
          }
        })
      );
  }

  public stop(): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/script/runtime/stop`, { })
      .pipe(
        map((result) => {
          return result;
        }),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 500:
              return ApiError.ServerError(`A server error occurred while getting runtime`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting runtime (${error.status})`);
          }
        })
      );
  }

  public reload(): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/script/runtime/reload`, { })
      .pipe(
        map((result) => {
          return result;
        }),
        catchError((error) => {
          switch (error.status) {
            case 200:
              // we good
              break;
            case 500:
              return ApiError.ServerError(`A server error occurred while getting runtime`);
            case 504:
              return ApiError.ServerError(`Cannot contact server`);
            default:
              return ApiError.ServerError(`An unknown error occurred while getting runtime (${error.status})`);
          }
        })
      );
  }
}
