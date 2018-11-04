import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HostSettingsComponent } from './host-settings.component';

describe('HostSettingsComponent', () => {
  let component: HostSettingsComponent;
  let fixture: ComponentFixture<HostSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ HostSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HostSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
