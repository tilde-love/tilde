import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CreateFileDialogComponent, DeleteFileDialogComponent, ProjectViewComponent} from './project-view/project-view.component';
import { ScriptEditorComponent } from './script-editor/script-editor.component';
import { AceEditorModule } from 'ng2-ace-editor';
import { ProjectTreeComponent } from './project-tree/project-tree.component';
import {CreateProjectDialogComponent, DeleteProjectDialogComponent, ProjectListComponent} from './project-list/project-list.component';
import { AppMaterialModule } from '../app-material.module';
import { RouterModule} from '@angular/router';
import { ProjectSettingsComponent } from './project-settings/project-settings.component';
import {ShowdownModule} from 'ngx-showdown';
import {FormsModule} from '@angular/forms';
import {HttpClientModule} from '@angular/common/http';
import { DropzoneDirective } from './dropzone.directive';
import {ControlsModule} from '../controls/controls.module';

@NgModule({
  imports: [
    CommonModule,
    AceEditorModule,
    AppMaterialModule,
    RouterModule,
    ShowdownModule,
    FormsModule,
    HttpClientModule,
    ControlsModule
  ],
  exports: [
    ProjectViewComponent,
    ProjectListComponent,
    ProjectTreeComponent,
    ScriptEditorComponent,
    ProjectSettingsComponent,
    DropzoneDirective,
    // CreateFileDialogComponent
  ],
  declarations: [
    ProjectViewComponent,
    ProjectListComponent,
    ProjectTreeComponent,
    ScriptEditorComponent,
    ProjectSettingsComponent,
    CreateFileDialogComponent,
    DeleteFileDialogComponent,
    CreateProjectDialogComponent,
    DeleteProjectDialogComponent,
    DropzoneDirective
  ],
  entryComponents: []
})
export class ScriptingModule { }
