
import { fakeAsync, ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileDashboardComponent } from './profile-dashboard.component';

describe('ProfileDashboardComponent', () => {
  let component: ProfileDashboardComponent;
  let fixture: ComponentFixture<ProfileDashboardComponent>;

  beforeEach(fakeAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProfileDashboardComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProfileDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should compile', () => {
    expect(component).toBeTruthy();
  });
});
