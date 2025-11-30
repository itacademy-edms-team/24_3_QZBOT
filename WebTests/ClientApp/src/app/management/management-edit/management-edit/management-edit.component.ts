import { Component, OnInit } from '@angular/core';
import { TestService, Test, Question, Option } from '../../../services/test.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-management-edit',
  templateUrl: './management-edit.component.html',
  styleUrls: ['./management-edit.component.css']
})
export class ManagementEditComponent implements OnInit {
  test: Test = { id: 0, title: '', questions: [], creatorId: '', published: false }
  test_title: string = "";
  edited_test: Test = { id: 0, title: '', questions: [], creatorId: '', published: false }
  text_error: string = "";
  confirm_edit: boolean = false;
  changes: string[] = [];
  is_editing_locked: boolean = false;
  success_edit: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private testService: TestService,
  ) { }
  

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const test_id = params.get('id') as unknown as number;
      if (test_id) {
        this.test.id = test_id;
      }

      this.testService.getTestById(this.test.id).subscribe({
        next: (data) => {
          this.test = data;
          this.edited_test = JSON.parse(JSON.stringify(this.test));
          this.test_title = data.title;
        }
      });
    });
  }

  addQuestion() {
    this.edited_test.questions.push({
      id: 0,
      text: '',
      options: []
    })
  }

  removeQuestion(index: number) {
    this.edited_test.questions.splice(index, 1);
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
    if (this.testService.checkTest(this.edited_test) == "true") {
      this.changes = this.testService.getTestChanges(this.test, this.edited_test);

      if (this.changes.length == 0) {
        this.text_error = "Нет изменений";
        return;
      }

      this.is_editing_locked = true;
      this.confirm_edit = true;
      this.text_error = "";
    } else {
      this.text_error = this.testService.checkTest(this.edited_test);
    }
  }

  btnCancel() {
    this.confirm_edit = false;
    this.is_editing_locked = false;
  }

  btnConfirm() {
    this.testService.editTest(this.test.id, this.edited_test).subscribe({
      next: (data) => {
        if (data) {
          this.success_edit = true;
          this.is_editing_locked = false;
          this.text_error = "";
        }
        else {
          this.text_error = "Ошибка редактирования теста"
        }
      }
    })
  }
}
