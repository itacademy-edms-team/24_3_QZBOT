import { Component, OnInit } from '@angular/core';
import { TestService, Test, Question, Option } from '../../../services/test.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-management-edit',
  templateUrl: './management-edit.component.html',
  styleUrls: ['./management-edit.component.css']
})
export class ManagementEditComponent implements OnInit {
  test: Test = { id: 0, title: '', questions: [] }
  //test_title: string = "";
  text_error: string = "";

  constructor(
    private route: ActivatedRoute,
    private testService: TestService,
  ) { }
  

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const testName = params.get('name');
      if (testName) {
        this.test.title = testName;
      }

      this.testService.getTestsByName(this.test.title).subscribe({
        next: (data) => {
          this.test.questions = data;
        }
      });
    });
  }

  addQuestion() {
    this.test.questions.push({
      id: 0,
      text: '',
      options: []
    })
  }

  removeQuestion(index: number) {
    this.test.questions.splice(index, 1);
  }

  addOption(questions: Question) {
    questions.options.push({
      id: 0,
      text: '',
      isCorrect: false
    });
  }

  removeOption(question: Question, index: number) {
    question.options.splice(index, 1);
  }

  selectCorrectOption(question: any, selectedIndex: number) {
    question.options.forEach((option: any, index: number) => {
      option.isCorrect = index === selectedIndex;
    });
  }

  btnSaveChanges() {
    this.testService.editTest(this.test.title, this.test.questions).subscribe({
      next: (data) => {
        if (data) {
          this.text_error = "Тест успешно изменен"
        }
        else {
          this.text_error = "Тест не изменен"
        }
      }
    })
  }
}
