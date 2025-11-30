import { Component, OnInit } from '@angular/core';
import { TestService, Test, Question } from '../../services/test.service';

@Component({
  selector: 'app-management-create',
  templateUrl: './management-create.component.html',
  styleUrls: ['./management-create.component.css']
})
export class ManagementCreateComponent {
  test: Test = { id: 0, title: '', questions: [], creatorId: '', published: false };
  is_exist: boolean = false;
  confirm_add: boolean = false;
  success_add: boolean = false;
  text_error: string = '';
  is_editing_locked: boolean = false;

  constructor(
    private testService: TestService
  ) { }

  btnAdd() {
    this.success_add = false;

    if (this.test.title.length == 0) {
      this.text_error = "Пустое поле названия";
      return;
    }

    this.testService.checkTestExists(this.test.title).subscribe({
      next: (data) => {
        this.is_exist = data;

        if (!this.is_exist) {

          if (this.testService.checkTest(this.test) == "true") {
            this.confirm_add = true;
            this.is_editing_locked = true;
            this.text_error = "";
          } else {
            this.text_error = this.testService.checkTest(this.test);
          }

        } else {
          this.text_error == "Такое название уже занято";
          this.confirm_add = false;
          this.success_add = false;
        }
      }
    })
  }

  btnConfirm() {
    this.testService.addTest(this.test.title, this.test.questions).subscribe({
      next: (data) => {
        if (data) {
          this.confirm_add = false;
          this.success_add = true;
          this.text_error = "";
          this.is_editing_locked = false;
        }
      }
    });
  }

  btnCancel() {
    this.confirm_add = false;
    this.is_editing_locked = false;
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
