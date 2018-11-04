import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-host-settings',
  templateUrl: './host-settings.component.html',
  styleUrls: ['./host-settings.component.scss']
})
export class HostSettingsComponent implements OnInit {
  cards = [
    { title: 'Card 1', cols: 2, rows: 1 },
    { title: 'Card 2', cols: 1, rows: 1 },
    { title: 'Card 3', cols: 1, rows: 2 },
    { title: 'Card 4', cols: 1, rows: 1 }
  ];

  constructor() { }

  ngOnInit() {
  }

}
