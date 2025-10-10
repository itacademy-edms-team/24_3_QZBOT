import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CsharpTestComponent } from './csharp-test.component';

describe('CsharpTestComponent', () => {
  let component: CsharpTestComponent;
  let fixture: ComponentFixture<CsharpTestComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CsharpTestComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CsharpTestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
