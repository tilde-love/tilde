import {Component, OnDestroy, OnInit} from '@angular/core';
import {ProjectDataService} from '../../scripting/project-data.service';
import {combineLatest, Subscription} from 'rxjs';

@Component({
  selector: 'app-project-buttons',
  templateUrl: './project-buttons.component.html',
  styleUrls: ['./project-buttons.component.scss']
})
export class ProjectButtonsComponent implements OnInit, OnDestroy {
  private _selectedProjectSubscription: Subscription;

  public runningProject = null;
  public projectName = null;

  constructor(private projectDataService: ProjectDataService) {
    this._selectedProjectSubscription = combineLatest(
      this.projectDataService.runtime,
      this.projectDataService.selectedProject)
      .subscribe(([runtime, projectName]) => {
        this.runningProject = runtime.project;
        this.projectName = projectName;
      });
  }

  ngOnInit() {
  }

  ngOnDestroy(): void {
    this._selectedProjectSubscription.unsubscribe();
  }

}
