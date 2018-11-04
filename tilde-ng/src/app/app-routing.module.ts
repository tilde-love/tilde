import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ProjectListComponent } from './scripting/project-list/project-list.component';
import { ProjectViewComponent } from './scripting/project-view/project-view.component';
import { ProfileDashboardComponent } from './profile-dashboard/profile-dashboard.component';
import { HostSettingsComponent } from './host-settings/host-settings.component';
import {DocumentationComponent} from './documentation/documentation.component';

const routes: Routes =
  [
    { path: '', component: DocumentationComponent },
    { path: 'profile', component: ProfileDashboardComponent },
    { path: 'settings', component: HostSettingsComponent },
    { path: 'projects', component: ProjectListComponent },
    // { path: 'projects/:project', component: ProjectViewComponent },
    { path: 'projects/:project', // component: ProjectViewComponent,
      children: [
        { path: '', component: ProjectViewComponent },
        { path: '**', component: ProjectViewComponent }
      ]
    },
    // { path: 'projects/:project/:type/:file', component: ProjectViewComponent },
    // { path: 'projects/:project/:type/:folder1/:file', component: ProjectViewComponent },
    // { path: 'projects/:project/:type/:folder1/:folder2/:file', component: ProjectViewComponent },
    { path: '**', component: DocumentationComponent },
  ];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
