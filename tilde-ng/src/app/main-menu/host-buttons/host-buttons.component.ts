import { Component, OnInit } from '@angular/core';
import {ProjectDataService} from '../../scripting/project-data.service';

@Component({
  selector: 'app-host-buttons',
  templateUrl: './host-buttons.component.html',
  styleUrls: ['./host-buttons.component.scss']
})
export class HostButtonsComponent implements OnInit {

  public get currentProject(): string {
    return this.projectDataService.runningProject;
  }

  constructor(private projectDataService: ProjectDataService) {

  }

  ngOnInit() {
  }

}
