import { TestBed } from '@angular/core/testing';

import { WorkApiService } from './work-api.service';

describe('WorkApiService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: WorkApiService = TestBed.get(WorkApiService);
    expect(service).toBeTruthy();
  });
});
