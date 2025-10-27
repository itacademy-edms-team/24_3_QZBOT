import { Component, OnInit } from '@angular/core';
import { TestService, Test } from '../../services/test.service';

@Component({
  selector: 'app-management-create',
  templateUrl: './management-create.component.html',
  styleUrls: ['./management-create.component.css']
})
export class ManagementCreateComponent {
  test!: Test;
  test_title: string = '';
  test_title_adding: string = '';
  is_exist: boolean = false;
  confirm_add: boolean = false;
  success_add: boolean = false;

  constructor(
    private testService: TestService
  ) { }

  btnClick() {
    this.testService.checkTestExists(this.test_title).subscribe({
      next: (data) => {
        this.is_exist = data;
        if (!this.is_exist) {
          this.confirm_add = true;
          this.test_title_adding = this.test_title;
        } else {
          this.confirm_add = false;
          this.success_add = false;
        }
      }
    })
  }

  btnConfirm() {
    this.test.title = this.test_title;
    this.test.questions = [];
    this.testService.addTest(this.test).subscribe({
      next: (data) => {
        if (data) {
          this.confirm_add = false;
          this.success_add = true;
        }
      }
    });
  }
}
