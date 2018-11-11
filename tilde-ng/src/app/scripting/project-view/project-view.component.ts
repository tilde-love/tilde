import {ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {HostBinding} from '@angular/core';
import { ProjectDataService} from '../project-data.service';
import {ActivatedRoute, Router} from '@angular/router';
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from '@angular/material';
import {BehaviorSubject, Observable, Subscription, combineLatest} from 'rxjs';
import {EditorItem, EditorState, Project, Error, ControlPanel} from '../project-types';

export enum ProjectItemType {
  ProjectNotFound = 0,
  Loading,
  FileNotFound,
  Cover,
  Readme,
  Script,
  Config,
  Asset,
  Settings,
  Log,
  Build,
  Panel,
}

const typesMap = {
  'text/markdown': 'markdown',
  'application/javascript': 'javascript',
  'application/json': 'json',
  'application/typescript': 'typescript',
  'application/csharp': 'csharp',
  'application/xml': 'xml'
};

@Component({
  selector: 'app-project-view',
  templateUrl: './project-view.component.html',
  styleUrls: ['./project-view.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectViewComponent implements OnInit, OnDestroy {

  @HostBinding('style.display') display = 'flex';
  @HostBinding('style.height') height = '100%';

  public mobileQuery: MediaQueryList;
  public tinyMobileQuery: MediaQueryList;
  public isSideNavOpen = true;
  private _loaded: boolean;
  public project: BehaviorSubject<Project> = new BehaviorSubject<Project>(null);

  public isDocument: boolean;
  public mode = 'text';
  public ProjectItemType = ProjectItemType;
  public projectItemType: BehaviorSubject<any> = new BehaviorSubject<any>(ProjectItemType.ProjectNotFound);

  public readonly: boolean;
  public text: string;
  public errors: Error[] = [];
  public activePath: string;
  public controlPanel: ControlPanel;

  private shouldCheckForScreenSize = true;
  private _activatedRouteSubscription: Subscription = null;
  private _getFileTextSubscription: Subscription = null;
  private fileItem: EditorItem;
  private _droppedFilesSubscription: Subscription = null;
  private allowExtensions: string[] = [ 'txt', 'md', 'js', 'json', 'ts'];

  constructor(private ref: ChangeDetectorRef,
              private projectDataService: ProjectDataService,
              private activatedRoute: ActivatedRoute,
              private router: Router,
              public dialog: MatDialog) {
    this.mobileQuery = window.matchMedia( '(min-width: 1025px)' );
    this.tinyMobileQuery = window.matchMedia( '(min-width: 414px)' );
  }

  ngOnInit() {

    this.projectDataService.loadProjects();

    this._activatedRouteSubscription = combineLatest(
      this.projectDataService.projects,
      this.activatedRoute.params,
      this.activatedRoute.url).subscribe(([projects, params, segments]) => {

        const projectName = params['project'];

        // const project = projects.find((proj) => proj.uri === projectName);
        const project = projects[projectName];

        this.project.next(project);

        if (project == null) {
          this.projectItemType.next(ProjectItemType.ProjectNotFound);
          return;
        }

        let type: string = null;
        const pathArray: string[] = [];

        for (let i = 0; i < segments.length; i++) {
          if (i === 0) {
            type = segments[i].path;
          } else {
            pathArray.push(segments[i].path);
          }
        }

        const fileName = pathArray.join('/');

        if (type != null && fileName !== '') {
          this.activePath = `${type}/${fileName}`;
        } else if (type != null) {
          this.activePath = `${type}`;
        } else {
          this.activePath = ``;
        }

        if (this._getFileTextSubscription !== null) { this._getFileTextSubscription.unsubscribe(); }

        switch (type) {
          case 'readme':
            this.projectItemType.next(ProjectItemType.Cover);
            this._getFileTextSubscription = this.projectDataService
              .getFileText(projectName, 'readme.md', false).subscribe((fileItem) => {
              this.text = fileItem.edited;
              this.ref.detectChanges();
            });
            this.isDocument = false;
            this.readonly = true;
            this.mode = 'markdown';
            break;
          case 'settings':
            this.projectItemType.next(ProjectItemType.Settings);
            this.isDocument = false;
            this.mode = 'json';
            this.text = '';
            break;
          case 'log':
          case 'build':
            this.isDocument = true;
            this.mode = 'text';
            this.readonly = true;
            this.text = '';
            this._getFileTextSubscription = this.projectDataService
              .getFileText(projectName, this.activePath, true)
              .subscribe((fileItem) => {
                this.fileItem = fileItem;
                switch (fileItem.state) {
                  case EditorState.NotCached:
                    this.projectItemType.next(ProjectItemType.Loading);
                    this.isDocument = false;
                    break;
                  case EditorState.NotFound:
                  case EditorState.Cached:
                  case EditorState.Edited:
                  case EditorState.Superseded:
                    this.projectItemType.next(ProjectItemType.Script);
                    this.isDocument = true;
                    this.readonly = true;
                    this.mode = 'text';
                    this.text = fileItem.edited;
                    break;
                }
              });
            break;
          default:
            const extension =
              this.activePath.lastIndexOf('.') >= 0
                ? this.activePath.substring(this.activePath.lastIndexOf('.'), this.activePath.length).toLowerCase()
                : null;

            this.errors = project.errors[this.activePath] ? project.errors[this.activePath] : [];

            if (extension === null) {
              this.projectItemType.next(ProjectItemType.FileNotFound);
              this.isDocument = false;
              this.mode = 'csharp'; // 'typescript';
              this.readonly = true;
            } else if (extension === '.panel') {
              this.projectItemType.next(ProjectItemType.Panel);
              this.controlPanel = project.controls.panels[this.activePath + '.json'];
              this.isDocument = false;
            } else {
              this.projectItemType.next(ProjectItemType.Loading);
              this.isDocument = false;
              this.mode = 'csharp';
              this.text = ''; // project.files[this.activePath];
              this._getFileTextSubscription = this.projectDataService
                .getFileText(projectName, this.activePath, false)
                .subscribe((fileItem) => {
                this.fileItem = fileItem;
                switch (fileItem.state) {
                  case EditorState.NotCached:
                    this.projectItemType.next(ProjectItemType.Loading);
                    this.isDocument = false;
                    break;
                  case EditorState.NotFound:
                  case EditorState.Cached:
                  case EditorState.Edited:
                  case EditorState.Superseded:
                    this.projectItemType.next(ProjectItemType.Script);
                    this.isDocument = true;
                    this.readonly = false;
                    this.mode = typesMap[fileItem.mimeType] ? typesMap[fileItem.mimeType] : 'text';
                    this.text = fileItem.edited;
                    break;
                }
              });
            }
            break;
        }

        this.ref.detectChanges();
      }
    );

    const me = this;

    this.mobileQuery.addListener(function(e) { me.widthChange(e); });
    this.widthChange(this.mobileQuery);

    this.ref.detectChanges();

    this._droppedFilesSubscription = this.projectDataService.droppedFiles.subscribe((file) => {

      if (this.project.getValue() === null) {
        return;
      }

      const ext = file.name.split('.')[file.name.split('.').length - 1];

      if (this.allowExtensions.lastIndexOf(ext) !== -1) {
        this.projectDataService.uploadFile(this.project.getValue().uri, file);
      }
    });
  }

  textChanged(text) {
    if (this.readonly === true) {
      return;
    }
    //
    // if (this.activePath === 'log' || this.activePath === 'log.txt') {
    //   return;
    // }

    const uri = this.project.getValue().uri;

    this.projectDataService.setFileText(uri, this.activePath, text)
      .then((thing) => {
        // if (this.projectDataService.currentProject === uri) {
        //  this.projectDataService.reload();
        // }
      });
  }

  reloadFile() {
    this.projectDataService.getFileText(this.project.getValue().uri, this.activePath, true)
      .toPromise()
      .then((fileItem) => {
        });
  }

  writeFile() {
    if (this.readonly === true) {
      return;
    }

    this.projectDataService
      .setFileText(this.project.getValue().uri, this.activePath, this.fileItem.edited)
      .then((t) => {
        this.projectDataService.loadProjects();
      });
  }

  getFileUrl(): string {
    return `/api/1.0/files/${this.project.getValue().uri}/${this.activePath}`;
  }

  widthChange(mq) {
    if (this.shouldCheckForScreenSize === false) {
      return;
    }

    this.isSideNavOpen = !!mq.matches;
    this.ref.detectChanges();
  }

  ngOnDestroy(): void {
    this._activatedRouteSubscription.unsubscribe();
    if (this._getFileTextSubscription !== null) { this._getFileTextSubscription.unsubscribe(); }
    if (this._droppedFilesSubscription !== null) { this._droppedFilesSubscription.unsubscribe(); }

    this.shouldCheckForScreenSize = false;
    this.ref.detach();
  }

  openDeleteFileDialog() {
    const uri = `${this.activePath}`;

    const dialogRef = this.dialog.open(DeleteFileDialogComponent, {
      width: '400px',
      data: { name: uri }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === undefined) {
        return;
      }

      this.projectDataService
        .deleteFile(this.project.getValue().uri, this.activePath)
        .then((t) => {
          this.router.navigate([`/projects/${this.project.getValue().uri}/readme`]);
        });
    });
  }

  openCreateFileDialog(): void {

    const uri = `${this.activePath}`;

    const dialogRef = this.dialog.open(CreateFileDialogComponent, {
      width: '400px',
      data: { title: 'Create file', name: uri}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === undefined) {
        return;
      }

      this.projectDataService
        .setFileText(this.project.getValue().uri, result, '')
        .then((thing) => {
          this.projectDataService.loadProjects()
            .then((thing2) => {
              this.router.navigate([`/projects/${this.project.getValue().uri}/${result}`]);
            });
        });
    });
  }

  openDuplicateFileDialog(): void {

    const uri = `${this.activePath}`;

    const dialogRef = this.dialog.open(CreateFileDialogComponent, {
      width: '400px',
      data: { title: 'Duplicate file', name: uri}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === undefined) {
        return;
      }

      this.projectDataService.setFileText(this.project.getValue().uri, result, this.fileItem.edited)
        .then((thing) => {
          this.projectDataService.loadProjects().then((thing2) => {
            this.router.navigate([`/projects/${this.project.getValue().uri}/${result}`]);
          });
        });
    });
  }
}

@Component({
  selector: 'app-create-file-dialog',
  templateUrl: './create-file-dialog.component.html',
})
export class CreateFileDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<CreateFileDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  onNoClick(): void {
    this.dialogRef.close();
  }
}

@Component({
  selector: 'app-delete-file-dialog',
  templateUrl: './delete-file-dialog.component.html',
})
export class DeleteFileDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<DeleteFileDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  onNoClick(): void {
    this.dialogRef.close();
  }
}
