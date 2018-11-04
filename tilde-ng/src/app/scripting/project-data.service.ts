import {Injectable, OnDestroy} from '@angular/core';
import {ApiError, FileContents, ProjectApiService, UploadInfo} from './project-api.service';
import {Subscription, BehaviorSubject, Observable, Subject} from 'rxjs';
import { take } from 'rxjs/operators';
import {FileNode} from './project-tree/project-tree.component';
import {isNullOrUndefined, log} from 'util';
import { environment } from '../../environments/environment';
import { HubConnection } from '@aspnet/signalr';
import * as signalR from '@aspnet/signalr';
import {forEach} from '@angular/router/src/utils/collection';

export class ErrorSpan {
  public sl: number;
  public sc: number;
  public el: number;
  public ec: number;
}

export class Error {
  public type: string;
  public text: string;
  public span: ErrorSpan;
}

export class Project {
  public uri: string;
  public deleted: boolean;
  public files: string[]; // { [name: string]: string };
  public errors: { [ uri: string]: Error[] } = {};
}

export enum ScriptHostState {
  Running = 0,
  Stopped,
  Paused,
}

export class Runtime {
  public state: ScriptHostState;
  public isInError: boolean;
  public project: string;
}

export enum EditorState {
  NotCached = 0,
  NotFound,
  Cached,
  Edited,
  Superseded,
}

export class EditorItem {
  public state: EditorState;
  public uri: string;
  public mimeType: string;
  public original: string;
  public edited: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectDataService implements OnDestroy {

  private _projectUris: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);
  private _projects: BehaviorSubject<{ [ uri: string]: Project}> = new BehaviorSubject<{ [ uri: string]: Project}>({});
  private _editorCache: { [ uri: string]: BehaviorSubject<EditorItem> } = {};
  private _droppedFiles: Subject<File> = new Subject<File>();
  private _runtime: BehaviorSubject<Runtime>
    = new BehaviorSubject<Runtime>(
      { project: null, isInError: false, state: ScriptHostState.Stopped }
    );
  private _hubConnection: HubConnection;
  private _darkTheme: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(localStorage.getItem('dark-theme') !== 'false');

  static getProjectTreeState(project: Project) {
    const value = localStorage.getItem(`TreeState:${project.uri}`);

    if (value != null) {
      return JSON.parse(value);
    } else {
      return {};
    }
  }
  static setProjectTreeState(project: Project, treeState: { [p: string]: boolean }) {
    localStorage.setItem(`TreeState:${project.uri}`, JSON.stringify(treeState));
  }

  public get projectUris() { return this._projectUris; }
  public get projects() { return this._projects; }
  public get runtime() { return this._runtime; }
  public get droppedFiles() { return this._droppedFiles; }
  public get darkTheme() { return this._darkTheme; }

  public get currentProject(): string {
    return this._runtime.getValue().project;
  }

  public get runtimeState(): ScriptHostState  {
    return this._runtime.getValue().state;
  }

  // public get isInError(): boolean  {
  //   return this._runtime.getValue().isInError;
  // }

  constructor(private api: ProjectApiService) {
    // TimerObservable.timer(1000)
    //   .takeWhile(() => this.alive) // only fires when component is alive
    //   .subscribe(() => {
    //     this.myExampleService.get()
    //       .subscribe(data => {
    //         this.data = data;
    //       });
    //   });

    // this.interval = setInterval(() => {
    //   // console.log('auto-load projects');
    //   this.loadProjects();
    // }, 2000);

    this._hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.notifyUri)
      .configureLogging(signalR.LogLevel.Critical)
      .build();

    this._hubConnection.on('OnStateChange', (data: any) => {
      const received = `OnStateChange: ${data}`;

      console.log(received);
    });

    // this._hubConnection.on('OnErrorMessage', (data: any) => {
    //   const received = `OnErrorMessage: ${data}`;
    //
    //   console.log(received);
    //   // this.messages.push(received);
    // });

    this._hubConnection.on('OnScriptProject', (project: Project) => {
      const projects = this._projects.getValue();

      projects[project.uri] = project;

      this._projects.next(projects);
    });

    // this._hubConnection.on('OnIsInError', (data: any) => {
    //   const received = `OnIsInError: ${data}`;
    //   console.log(received);
    // });

    this._hubConnection.onclose((e) => setTimeout(this.startHubConnection(), 5000) );

    this.startHubConnection();

    // this.setTheme(localStorage.getItem('dark-theme') !== 'false');
  }

  public setTheme(dark: boolean) {
    localStorage.setItem('dark-theme', dark ? 'true' : 'false');
    this._darkTheme.next(dark);
  }

  public loadProjects() {
    return this.api.getProjects()
      .toPromise()
      .then(res => {
        this._projects.next(res);
        this._projectUris.next(Object.keys(res));
      })
      .catch(err => this.handleError(err as ApiError));
  }

  private handleError(error: ApiError) {

    console.log(error.message);

    // if (error.errorType === ApiErrorType.SignInRequired) {
    //   this._loginExpired.next(true);
    // } else {
    //   this.raiseMessage(error.message);
    // }
  }

  public getRuntime() {
    return this.api.getRuntime()
      .toPromise()
      .then(runtime => {
        this._runtime.next(runtime);
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public run() {
    return this.api.run()
      .toPromise()
      .then(thing => {
        // this.reload();
        // this.getRuntime();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public pause() {
    return this.api.pause()
      .toPromise()
      .then(thing => {
        this.getRuntime();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public stop() {
    return this.api.stop()
      .toPromise()
      .then(thing => {
        // this.getRuntime();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public reload() {
    return this.api.reload()
      .toPromise()
      .then(thing => {
        // this.getRuntime();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public loadProject(projectUri: string) {
    return this.api.loadProject(projectUri)
      .toPromise()
      .then(thing => {
        this.getRuntime();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public deleteProject(projectUri: string) {
    return this.api.deleteProject(projectUri)
      .toPromise()
      .then(thing => {
        // this.loadProjects();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public createProject(projectUri: string) {
    return this.api.createProject(projectUri)
      .toPromise()
      .then(thing => {
        // this.loadProjects();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public uploadProject(file: File) {
    // console.log(`upload file ${file.name}`);

    return this.api.uploadProject(file)
      .toPromise()
      .then(thing => {
        // this._editorCache[uri] = undefined;
        // this.loadProjects();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public deleteFile(projectUri: string, filename: string) {
    const uri = `/projects/${projectUri}/${filename}`;

    const currentItem$: BehaviorSubject<EditorItem> = this._editorCache[uri];

    if (currentItem$ === undefined) {
      return;
    }

    return this.api.deleteFile(projectUri, filename)
      .toPromise()
      .then(thing => {
        this._editorCache[uri] = undefined;

        this.loadProjects();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public uploadFile(projectUri: string, file: File) {
    return this.api.uploadFile(projectUri, file)
      .toPromise()
      .then(thing => {
        this.loadProjects();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public setFileText(projectUri: string, filename: string, contents: string) {

    const uri = `/projects/${projectUri}/${filename}`;

    let currentItem$: BehaviorSubject<EditorItem> = this._editorCache[uri];

    if (currentItem$ === undefined) {
      currentItem$ = new BehaviorSubject<EditorItem>({
        uri: uri,
        edited: contents,
        mimeType: 'text',
        original: contents,
        state: EditorState.NotFound
      });

      this._editorCache[uri] = currentItem$;
    }

    // console.log(`set file text: ${uri}`);

    currentItem$.getValue().edited = contents;

    return this.api.setFileText(projectUri, filename, contents)
      .toPromise()
      .then(thing => {
        currentItem$.next(currentItem$.getValue());
        this.loadProjects();
      })
      .catch(err => this.handleError(err as ApiError));
  }

  public getFileText(projectUri: string, filename: string, reload: boolean): BehaviorSubject<EditorItem> {
    const uri = `/projects/${projectUri}/${filename}`;

    let currentItem$: BehaviorSubject<EditorItem> = this._editorCache[uri];

    if (currentItem$ === undefined) {
      currentItem$ = new BehaviorSubject<EditorItem>({
        uri: uri,
        edited: null,
        mimeType: null,
        original: null,
        state: EditorState.NotCached
      });

      this._editorCache[uri] = currentItem$;
    }

    switch (currentItem$.getValue().state) {
      case EditorState.NotCached:
      case EditorState.NotFound:
        this.api.getFileText(projectUri, filename)
          .toPromise()
          .then(res => {
            const currentItem = currentItem$.getValue();

            currentItem.state = EditorState.Cached;
            currentItem.mimeType = res.mimeType;
            currentItem.original = res.text;
            currentItem.edited = res.text;

            currentItem$.next(currentItem);
          })
          .catch(err => {
            const currentItem = currentItem$.getValue();

            currentItem.state = EditorState.NotFound;
            currentItem$.next(currentItem);

            this.handleError(err as ApiError);
          });
        break;
      case EditorState.Cached:
      case EditorState.Edited:
      case EditorState.Superseded:
        if (reload === false) {
          break;
        }

        this.api.getFileText(projectUri, filename)
          .toPromise()
          .then(res => {
            const currentItem = currentItem$.getValue();

            currentItem.state = EditorState.Cached;
            currentItem.mimeType = res.mimeType;
            currentItem.original = res.text;
            currentItem.edited = res.text;

            currentItem$.next(currentItem);
          })
          .catch(err => {
            const currentItem = currentItem$.getValue();

            currentItem.state = EditorState.NotFound;
            currentItem$.next(currentItem);

            this.handleError(err as ApiError);
          });
        break;
      default:
        break;
    }

    return currentItem$;
  }

  public dropFile(file: File) {
    this._droppedFiles.next(file);
  }

  ngOnDestroy(): void {

  }

  private startHubConnection() {
    this._hubConnection
      .start()
      .then(e => { this.loadProjects(); })
      .catch(err => {
        // console.error(err.toString());
        setTimeout(this.startHubConnection(), 5000);
      });
  }
}
