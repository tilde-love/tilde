import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TransportButtonsComponent } from './transport-buttons.component';

describe('TransportButtonsComponent', () => {
  let component: TransportButtonsComponent;
  let fixture: ComponentFixture<TransportButtonsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TransportButtonsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TransportButtonsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
