import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TestService, Test } from '../services/test.service';

@Component({
  selector: 'app-test',
  templateUrl: './tests.component.html',
  styleUrls: ['./tests.component.css']
})
export class TestComponent implements OnInit {

  tests: Test[] = [];
  errorMessage = '';
  title: string = '';
  currentQuestionIndex = 0;
  answers: { [id: number]: string } = {};
  currentQuestion: any;
  isFirst: boolean = true;
  isLast: boolean = false;


  selectedAnswers(questionId: number, option: string, title: string) {
    const selectedIndex = this.currentQuestion.options.indexOf(option);

    console.log('Selected option: ', option, 'Index: ', selectedIndex, 'Title: ', title);

    this.testService.checkAnswer(title, questionId, selectedIndex).subscribe({
      next: (response) => {
        console.log('Server response: ', response);
        if (response) {
          alert("Правильно");
        } else {
          alert("Неправильно");
        }
      },
      error: (err) => console.error('Error: ', err)
    });
  }

  nextQuestion() {
    const currentQuestionIndex = this.tests.indexOf(this.currentQuestion);
    if (currentQuestionIndex < this.tests.length - 1) {
      this.currentQuestion = this.tests[currentQuestionIndex + 1];
    }

    this.updateIsLast();
    this.updateIsFirst();
  }

  lastQuestion() {
    const currentQuestionIndex = this.tests.indexOf(this.currentQuestion);
    if (currentQuestionIndex > 0) {
      this.currentQuestion = this.tests[currentQuestionIndex - 1];
    }

    this.updateIsLast();
    this.updateIsFirst();
  }


  constructor(
    private testService: TestService,
    private route: ActivatedRoute,
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const testName = params.get('name');
      if (testName) {
        this.loadTests(testName);
        this.title = testName;
      }
    });
  }

  loadTests(name: string) {
    this.testService.getTestsByName(name).subscribe({
      next: (data) => {
        this.tests = data;

        if (data.length > 0) {
          this.currentQuestion = data[0];
        }

        this.updateIsLast();
        this.updateIsFirst();
      },
      error: (err) => console.error(err),
    });
  }

  updateIsLast() {
    const currentQuestionIndex = this.tests.indexOf(this.currentQuestion);

    if (currentQuestionIndex == this.tests.length - 1) {
      this.isLast = true;
    } else {
      this.isLast = false;
    }
  }

  updateIsFirst() {
    const currentQuestionIndex = this.tests.indexOf(this.currentQuestion);

    if (currentQuestionIndex == 0) {
      this.isFirst = true;
    } else {
      this.isFirst = false;
    }
  }
}
