
import { fakeAsync, ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentationDashboardComponent } from './documentation-dashboard.component';

describe('DocumentationDashboardComponent', () => {
  let component: DocumentationDashboardComponent;
  let fixture: ComponentFixture<DocumentationDashboardComponent>;

  beforeEach(fakeAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DocumentationDashboardComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DocumentationDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should compile', () => {
    expect(component).toBeTruthy();
  });
});
