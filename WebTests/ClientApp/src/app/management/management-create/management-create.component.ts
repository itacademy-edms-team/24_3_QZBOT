import { Component, OnInit } from '@angular/core';
import { TestService, Test, Question } from '../../services/test.service';

@Component({
  selector: 'app-management-create',
  templateUrl: './management-create.component.html',
  styleUrls: ['./management-create.component.css']
})
export class ManagementCreateComponent {
  test: Test = { id: 0, title: '', questions: [] };
  test_title: string = '';
  test_title_adding: string = '';
  is_exist: boolean = false;
  confirm_add: boolean = false;
  success_add: boolean = false;
  test_empty: boolean = false;
  text_error: string = '';

  constructor(
    private testService: TestService
  ) { }

  btnAdd() {
    this.success_add = false;

    if (this.test_title.length == 0) {
      this.text_error = "Пустое поле названия";
      return;
    }

    this.testService.checkTestExists(this.test_title).subscribe({
      next: (data) => {
        this.is_exist = data;

        if (!this.is_exist) {

          for (var i = 0; i < this.test.questions.length; i++) {

            if (this.test.questions[i].text == "") {
              this.confirm_add = false;
              this.success_add = false;
              this.text_error = "Поле вопроса на заполнено";
              return;
            }

            if (this.test.questions[i].options.length == 0) {
              this.text_error = "У вопроса " + (i + 1) + " нет вариантов ответов";
              return;
            }

            for (var j = 0; j < this.test.questions[i].options.length; j++) {
              if (this.test.questions[i].options[j].text == "") {
                this.confirm_add = false;
                this.success_add = false;
                this.text_error = "Поле варианта " + (j+1) + " вопроса " + (i+1) + " не заполнено";
                return;
              }
            }
          }

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
    this.testService.addTest(this.test.title, this.test.questions).subscribe({
      next: (data) => {
        if (data) {
          this.confirm_add = false;
          this.success_add = true;
          this.text_error = "";
        }
      }
    });
  }

  addQuestion() {
    this.test.questions.push({
      id: 0,
      text: '',
      options: []
    });
  }

  removeQuestion(index: number) {
    this.test.questions.splice(index, 1);
  }

  addOption(questions: Question) {
    questions.options.push({
      id: 0,
      text: '',
      isCorrect: false
    })
  }

  removeOption(question: Question, index: number) {
    question.options.splice(index, 1);
  }

  selectCorrectOption(question: any, selectedIndex: number) {
    question.options.forEach((option: any, index: number) => {
      option.isCorrect = index === selectedIndex;
    });
  }
}
