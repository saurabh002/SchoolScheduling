import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Absence } from './absence';

describe('Absence', () => {
  let component: Absence;
  let fixture: ComponentFixture<Absence>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Absence]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Absence);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
