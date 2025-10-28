import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManagementEditListComponent } from './management-edit-list.component';

describe('ManagementEditListComponent', () => {
  let component: ManagementEditListComponent;
  let fixture: ComponentFixture<ManagementEditListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ManagementEditListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManagementEditListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
