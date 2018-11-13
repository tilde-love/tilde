import {Component, HostBinding, OnInit} from '@angular/core';
import {GraphLine} from '../project-types';

@Component({
  selector: 'app-project-settings',
  templateUrl: './project-settings.component.html',
  styleUrls: ['./project-settings.component.scss']
})
export class ProjectSettingsComponent implements OnInit {

  @HostBinding('style.display') display = 'flex';
  @HostBinding('style.height') height = '100%';
  @HostBinding('style.width') width = '100%';

  public lines: GraphLine[] = [];

  constructor() {
    this.lines.push({ path: 'M0,0L1024,250z',
      color: 'red'
    });
    this.lines.push({ path: 'M0,100L1024,200z',
      color: 'cyan'
    });
    this.lines.push({ path: 'M0,200L1024,100z',
      color: 'green'
    });
    this.lines.push({ path: 'M0,250L1024,0z',
      color: 'pink'
    });
  }

  ngOnInit() {
  }

}
