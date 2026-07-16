import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubstituteDropdown } from './substitute-dropdown';

describe('SubstituteDropdown', () => {
  let component: SubstituteDropdown;
  let fixture: ComponentFixture<SubstituteDropdown>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SubstituteDropdown]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SubstituteDropdown);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
