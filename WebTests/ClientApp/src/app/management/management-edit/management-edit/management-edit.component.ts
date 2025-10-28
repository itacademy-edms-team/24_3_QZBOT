import { Component, OnInit } from '@angular/core';
import { TestService, Test, Question, Option } from '../../../services/test.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-management-edit',
  templateUrl: './management-edit.component.html',
  styleUrls: ['./management-edit.component.css']
})
export class ManagementEditComponent implements OnInit {
  isEmpty: boolean = false;
  questions: Question[] = [];
  title: string = '';

  constructor(
    private route: ActivatedRoute,
    private testService: TestService,
  ) { }
  

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const testName = params.get('name');
      if (testName) {
        this.title = testName;
      }

      this.testService.getTestsByName(this.title).subscribe({
        next: (data) => {
          if (data.length == 0) {
            this.isEmpty = true;
          }
          this.questions = data;
        }
      })
    });
  }
}
