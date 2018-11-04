import { Component } from '@angular/core';

@Component({
  selector: 'app-documentation-dashboard',
  templateUrl: './documentation-dashboard.component.html',
  styleUrls: ['./documentation-dashboard.component.css']
})
export class DocumentationDashboardComponent {
  text = '## some mark down RIGHT HERE';

  cards = [
    { title: 'System information', cols: 2, rows: 1 },
    { title: 'Setup', cols: 2, rows: 1 },
    { title: 'How to', cols: 2, rows: 1 },
  ];
}
