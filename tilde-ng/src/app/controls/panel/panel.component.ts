import {Component, HostBinding, Input, OnInit} from '@angular/core';
import {ControlPanel, LayoutSize, Project} from '../../scripting/_model/project-types';
import {isNullOrUndefined} from 'util';

@Component({
  selector: 'app-panel',
  templateUrl: './panel.component.html',
  styleUrls: ['./panel.component.scss']
})
export class PanelComponent implements OnInit {
  @HostBinding('style.display') display = 'flex';
  @HostBinding('style.height') height = '100%';

  @Input('project') public project: Project;
  @Input('panel') public panel: ControlPanel;

  layoutSize = LayoutSize;

  public isNullOrUndefined: (object: any) => object is null | undefined;

  constructor() {
    this.isNullOrUndefined = isNullOrUndefined;
  }

  ngOnInit() {
  }

  GetSizeClass(size: LayoutSize): string {

    switch (size) {
      case LayoutSize.Full:
        return 'full-item';
      case LayoutSize.Half:
        return 'half-item';
      case LayoutSize.Quarter:
        return 'quarter-item';
      default:
        return 'quarter-item';
    }
  }
}
