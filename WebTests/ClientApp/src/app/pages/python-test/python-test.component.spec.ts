import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PythonTestComponent } from './python-test.component';

describe('PythonTestComponent', () => {
  let component: PythonTestComponent;
  let fixture: ComponentFixture<PythonTestComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PythonTestComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PythonTestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
