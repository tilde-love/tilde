import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GraphComponent } from './graph/graph.component';
import { GraphLineComponent } from './graph-line/graph-line.component';
import { ControlComponent } from './control/control.component';
import { PanelComponent } from './panel/panel.component';
import {AppMaterialModule} from '../app-material.module';
// import {ShowdownModule} from 'ngx-showdown';
import {MarkdownModule} from 'ngx-markdown';
import {FormsModule} from '@angular/forms';

@NgModule({
  imports: [
    CommonModule,
    AppMaterialModule,
    MarkdownModule,
    FormsModule
  ],
  exports: [PanelComponent],
  declarations: [GraphComponent, GraphLineComponent, ControlComponent, PanelComponent]
})
export class ControlsModule { }
