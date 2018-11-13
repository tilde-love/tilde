import {Component, HostBinding, Input, OnInit} from '@angular/core';
import {GraphLine} from '../../scripting/project-types';

@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.scss']
})
export class GraphComponent implements OnInit {

  @HostBinding('style.height') height = '100%';
  @HostBinding('style.width') width = '100%';

  @Input('lines') lines: GraphLine[] = [];

  constructor() { }

  ngOnInit() {
  }

}
