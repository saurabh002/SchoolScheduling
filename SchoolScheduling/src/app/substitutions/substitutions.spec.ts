import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Substitutions } from './substitutions';

describe('Substitutions', () => {
  let component: Substitutions;
  let fixture: ComponentFixture<Substitutions>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Substitutions]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Substitutions);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
