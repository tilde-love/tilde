import { Component } from '@angular/core';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-documentation',
  templateUrl: './documentation.component.html',
  styleUrls: ['./documentation.component.css']
})
export class DocumentationComponent {
  isHandset: Observable<BreakpointState> = this.breakpointObserver.observe(Breakpoints.Handset);
  // topGap = 64 * 2;

  constructor(private breakpointObserver: BreakpointObserver) {
    // document.addEventListener('scroll', () => {
    //   this.topGap = Math.max(64, 64 * 2 - document.body.parentElement.scrollTop);
    // });
  }
}
