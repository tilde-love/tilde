import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {HttpClient, HttpEvent, HttpEventType, HttpRequest} from '@angular/common/http';
import {environment} from '../../../environments/environment';
import {catchError, map} from 'rxjs/operators';
import {Project, Runtime} from '../_model/project-types';
import {UploadInfo} from '../_model/upload-info';
import {FileContents} from '../_model/file-contents';
import {ApiError} from '../_model/api-error';

@Injectable({
  providedIn: 'root'
})
export class WorkApiService {

  constructor(private http: HttpClient) {
  }

  public loadProject(projectUri: string): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/work/${projectUri}/from-project/${projectUri}`, { })
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

  public run(name: string): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/work/${name}/start`, { })
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

  public pause(name: string): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/work/${name}/pause`, { })
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

  public stop(name: string): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/work/${name}/stop`, { })
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

  public reload(name: string): Observable<string> {
    return this.http.get<string>(`${environment.apiUri}/work/${name}/start`, { })
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
