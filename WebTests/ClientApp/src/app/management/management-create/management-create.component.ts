import { Component, OnInit } from '@angular/core';
import { TestService, Test } from '../../services/test.service';

@Component({
  selector: 'app-management-create',
  templateUrl: './management-create.component.html',
  styleUrls: ['./management-create.component.css']
})
export class ManagementCreateComponent implements OnInit {
  test: any;
  test_title: string = '';
  is_exist: any;

  constructor(
    private testService: TestService
  ) { }

  ngOnInit() {
    
  }

  btnClick() {
    this.testService.checkTestExists(this.test_title).subscribe({
      next: (data) => {
        this.is_exist = data;
      }
    })
  }
}
