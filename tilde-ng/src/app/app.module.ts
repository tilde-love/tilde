import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserModule, HAMMER_GESTURE_CONFIG } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppMaterialModule } from './app-material.module';
import { ScriptingModule } from './scripting/scripting.module';
import { ProjectDataService } from './scripting/project-data.service';
import { ProfileDashboardComponent } from './profile-dashboard/profile-dashboard.component';
import {
  MatGridListModule,
  MatCardModule,
  MatMenuModule,
  MatIconModule,
  MatButtonModule, MatToolbarModule, MatSidenavModule, MatListModule, GestureConfig
} from '@angular/material';
import { TransportButtonsComponent } from './main-menu/transport-buttons/transport-buttons.component';
import { ProjectButtonsComponent } from './main-menu/project-buttons/project-buttons.component';
import { HostButtonsComponent } from './main-menu/host-buttons/host-buttons.component';
import { HostSettingsComponent } from './host-settings/host-settings.component';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { LayoutModule } from '@angular/cdk/layout';
import {MarkdownModule} from 'ngx-markdown';
import {CreateFileDialogComponent, DeleteFileDialogComponent, ProjectViewComponent} from './scripting/project-view/project-view.component';
import {CreateProjectDialogComponent, DeleteProjectDialogComponent} from './scripting/project-list/project-list.component';

@NgModule({
  declarations: [
    AppComponent,
    ProfileDashboardComponent,
    TransportButtonsComponent,
    ProjectButtonsComponent,
    HostButtonsComponent,
    HostSettingsComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    AppMaterialModule,
    ScriptingModule,
    MatGridListModule,
    MatCardModule,
    MatMenuModule,
    MatIconModule,
    MatButtonModule,
    ServiceWorkerModule.register('/ngsw-worker.js', { enabled: environment.production }),
    LayoutModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MarkdownModule,
  ],
  providers: [ ProjectDataService, { provide: HAMMER_GESTURE_CONFIG, useClass: GestureConfig }, ],
  bootstrap: [ AppComponent ],
  entryComponents: [
    CreateFileDialogComponent,
    DeleteFileDialogComponent,
    CreateProjectDialogComponent,
    DeleteProjectDialogComponent
  ],
})
export class AppModule { }

// ProjectViewComponent, CreateFileDialogComponent
