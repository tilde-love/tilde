import { Component, OnInit } from '@angular/core';
import {ProjectDataService} from '../../scripting/project-data.service';
import {ActivatedRoute, Router} from '@angular/router';
import {BehaviorSubject} from 'rxjs';

@Component({
  selector: 'app-transport-buttons',
  templateUrl: './transport-buttons.component.html',
  styleUrls: ['./transport-buttons.component.scss']
})
export class TransportButtonsComponent implements OnInit {

  public get currentProject(): string {
    return this.projectDataService.currentProject;
  }

  constructor(private projectDataService: ProjectDataService) {
  }

  ngOnInit() {
  }

  public run() {
    this.projectDataService.run();
  }

  public stop() {
    this.projectDataService.stop();
  }

  public pause() {
    this.projectDataService.pause();
  }
}
