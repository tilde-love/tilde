import {Component, HostBinding, Inject, OnDestroy, OnInit} from '@angular/core';
import {Project, ProjectDataService} from '../project-data.service';
import {Observable, Subscription} from 'rxjs';
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from '@angular/material';
import {CreateFileDialogComponent, DeleteFileDialogComponent} from '../project-view/project-view.component';

@Component({
  selector: 'app-project-list',
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit, OnDestroy {
  @HostBinding('style.display') display = 'flex';
  @HostBinding('style.height') height = '100%';

  private fileList: Array<File>;
  private _droppedFilesSubscription: Subscription;

  public get projectUris() {
    return this.projectDataService.projectUris;
  }

  constructor(private projectDataService: ProjectDataService,
              public dialog: MatDialog) {

    this.projectDataService.loadProjects();
    this.projectDataService.getRuntime();

    this._droppedFilesSubscription = this.projectDataService.droppedFiles.subscribe((file) => {

      const ext = file.name.split('.')[file.name.split('.').length - 1];

      if (ext !== 'zip') {
        return;
      }

      this.projectDataService.uploadProject(file);
    });
  }

  ngOnInit() {
  }

  ngOnDestroy(): void {
    this._droppedFilesSubscription.unsubscribe();
  }

  public loadProject(projectUri: string) {
    this.projectDataService.loadProject(projectUri);
  }

  public get currentProject(): string {
    return this.projectDataService.currentProject;
  }

  getFileUrl(projectUri: string): string {
    return `/api/1.0/files/${projectUri}`;
  }

  getRoute(projectUri: string): string {
    return `/projects/${projectUri}/readme`;
  }

  openDeleteProjectDialog(projectUri: string) {
    const dialogRef = this.dialog.open(DeleteProjectDialogComponent, {
      width: '400px',
      data: { name: projectUri }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === undefined) {
        return;
      }

      this.projectDataService
        .deleteProject(projectUri)
        .then((t) => {
        });
    });
  }

  openCreateProjectDialog(): void {

    const dialogRef = this.dialog.open(CreateProjectDialogComponent, {
      width: '400px',
      data: { title: 'Create project', name: ''}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === undefined) {
        return;
      }

      this.projectDataService.createProject(result);
    });
  }
}

@Component({
  selector: 'app-create-project-dialog',
  templateUrl: './create-project-dialog.component.html',
})
export class CreateProjectDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<CreateProjectDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  onNoClick(): void {
    this.dialogRef.close();
  }
}

@Component({
  selector: 'app-delete-project-dialog',
  templateUrl: './delete-project-dialog.component.html',
})
export class DeleteProjectDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<DeleteProjectDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  onNoClick(): void {
    this.dialogRef.close();
  }
}
