import {Component, Input, OnInit} from '@angular/core';

@Component({
  selector: '[app-graph-line]',
  templateUrl: './graph-line.component.html',
  styleUrls: ['./graph-line.component.scss']
})
export class GraphLineComponent implements OnInit {

  @Input('color')
  public color: string = 'white';

  @Input('path')
  public path: string = '';

  constructor() { }

  ngOnInit() {
  }

}
