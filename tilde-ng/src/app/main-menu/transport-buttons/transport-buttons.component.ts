import {ChangeDetectorRef, Component, OnDestroy, OnInit} from '@angular/core';
import {ProjectDataService} from '../../scripting/project-data.service';
import {ActivatedRoute, Router} from '@angular/router';
import {BehaviorSubject, combineLatest, Subscription} from 'rxjs';
import {isNullOrUndefined} from 'util';

@Component({
  selector: 'app-transport-buttons',
  templateUrl: './transport-buttons.component.html',
  styleUrls: ['./transport-buttons.component.scss']
})
export class TransportButtonsComponent implements OnInit, OnDestroy {
  private _selectedProjectSubscription: Subscription;

  public projectName = '';
  public currentProject = '';
  public transportDisabled = false;
  public transportColor = '';

  constructor(private projectDataService: ProjectDataService,
              private changeDetectorRefs: ChangeDetectorRef) {
    this._selectedProjectSubscription = combineLatest(
      this.projectDataService.runtime,
      this.projectDataService.selectedProject)
      .subscribe(([runtime, projectName]) => {
        this.currentProject = runtime.project;
        this.projectName = projectName;

        // console.log('projectName: ' + this.projectName);
        // console.log('currentProject: ' + this.currentProject);

        if (this.projectName === 'docs') {
          this.transportDisabled = true;
          return;
        }

        if (isNullOrUndefined(projectName) === true) {
          if (isNullOrUndefined(this.currentProject) === true) {
            this.transportDisabled = true;
          } else {
            this.transportDisabled = false;
            this.transportColor = '';
          }
        } else {
          if (isNullOrUndefined(this.currentProject) === false
            && this.currentProject !== projectName) {
            this.transportDisabled = false;
            this.transportColor = '';
          } else {
            this.transportDisabled = false;
            this.transportColor = 'accent';
          }
        }
      });
  }

  ngOnInit() {
  }

  public run() {

    if (isNullOrUndefined(this.projectName) === false) {
      this.projectDataService.loadProject(this.projectName);
    }

    this.projectDataService.run(this.projectName);
  }

  public stop() {
    this.projectDataService.stop(this.projectName);
  }

  public pause() {
    this.projectDataService.pause(this.projectName);
  }

  ngOnDestroy(): void {
    this._selectedProjectSubscription.unsubscribe();
  }
}
