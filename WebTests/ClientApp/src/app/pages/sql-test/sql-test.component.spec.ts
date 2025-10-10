import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SqlTestComponent } from './sql-test.component';

describe('SqlTestComponent', () => {
  let component: SqlTestComponent;
  let fixture: ComponentFixture<SqlTestComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SqlTestComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SqlTestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
