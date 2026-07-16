import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TodaySubstitutions } from './today.substitutions';

describe('TodaySubstitutions', () => {
  let component: TodaySubstitutions;
  let fixture: ComponentFixture<TodaySubstitutions>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TodaySubstitutions]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TodaySubstitutions);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
