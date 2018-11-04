import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GraphComponent } from './graph/graph.component';
import { GraphLineComponent } from './graph-line/graph-line.component';
import { ControlComponent } from './control/control.component';

@NgModule({
  imports: [
    CommonModule
  ],
  exports: [GraphComponent],
  declarations: [GraphComponent, GraphLineComponent, ControlComponent]
})
export class ControlsModule { }
