import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TestService, Test, Question, Option } from '../../services/test.service';

@Component({
  selector: 'app-test',
  templateUrl: './tests.component.html',
  styleUrls: ['./tests.component.css']
})
export class TestComponent implements OnInit {

  test: Test = { id: 0, title: '', questions: [] };
  errorMessage = '';
  currentQuestionIndex = 0;
  answers: { [id: number]: string } = {};
  currentQuestion: any;
  isFirst: boolean = true;
  isLast: boolean = false;

  isModalOpen: boolean = false;
  textModal: string = '';

  constructor(
    private testService: TestService,
    private route: ActivatedRoute,
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const testName = params.get('name');
      if (testName) {
        this.loadTests(testName);
        this.test.title = testName;
      }
    });
  }


  selectedAnswers(questionId: number, option: string, title: string) {
    const selectedIndex = this.currentQuestion.options.indexOf(option);

    console.log('Selected option: ', option, 'Index: ', selectedIndex, 'Title: ', title);

    this.testService.checkAnswer(title, questionId, selectedIndex).subscribe({
      next: (response) => {
        console.log('Server response: ', response);
        if (response) {
          this.isModalOpen = true;
          this.textModal = "Правильно";
        } else {
          this.isModalOpen = true;
          this.textModal = "Неправильно";
        }
      },
      error: (err) => console.error('Error: ', err)
    });
  }

  nextQuestion() {
    const currentQuestionIndex = this.test.questions.indexOf(this.currentQuestion);
    if (currentQuestionIndex < this.test.questions.length - 1) {
      this.currentQuestion = this.test.questions[currentQuestionIndex + 1];
    }

    this.updateIsLast();
    this.updateIsFirst();
  }

  lastQuestion() {
    const currentQuestionIndex = this.test.questions.indexOf(this.currentQuestion);
    if (currentQuestionIndex > 0) {
      this.currentQuestion = this.test.questions[currentQuestionIndex - 1];
    }

    this.updateIsLast();
    this.updateIsFirst();
  }  


  loadTests(name: string) {
    this.testService.getTestByName(name).subscribe({
      next: (data) => {
        this.test = data;

        if (data.questions.length > 0) {
          this.currentQuestion = data.questions[0];
        }

        this.updateIsLast();
        this.updateIsFirst();
      },
      error: (err) => console.error(err),
    });
  }

  updateIsLast() {
    const currentQuestionIndex = this.test.questions.indexOf(this.currentQuestion);

    if (currentQuestionIndex == this.test.questions.length - 1) {
      this.isLast = true;
    } else {
      this.isLast = false;
    }
  }

  updateIsFirst() {
    const currentQuestionIndex = this.test.questions.indexOf(this.currentQuestion);

    if (currentQuestionIndex == 0) {
      this.isFirst = true;
    } else {
      this.isFirst = false;
    }
  }

  finishTest() {
    this.isModalOpen = true;
    this.textModal = "Тест завершен!";
  }

  closeModal() {
    this.isModalOpen = false;
  }
}
