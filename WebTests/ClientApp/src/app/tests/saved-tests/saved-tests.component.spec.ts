import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SavedTestsComponent } from './saved-tests.component';

describe('SavedTestsComponent', () => {
  let component: SavedTestsComponent;
  let fixture: ComponentFixture<SavedTestsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SavedTestsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SavedTestsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
