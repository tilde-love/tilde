import { Component, OnInit } from '@angular/core';
import {ProjectDataService} from '../../scripting/project-data.service';

@Component({
  selector: 'app-project-buttons',
  templateUrl: './project-buttons.component.html',
  styleUrls: ['./project-buttons.component.scss']
})
export class ProjectButtonsComponent implements OnInit {

  public get currentProject(): string {
    return this.projectDataService.currentProject;
  }

  constructor(private projectDataService: ProjectDataService) {

  }

  ngOnInit() {
  }

}
