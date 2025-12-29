import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { TestService, Test, Question, Option, UserTest } from '../../services/test.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-test',
  templateUrl: './tests.component.html',
  styleUrls: ['./tests.component.css']
})
export class TestComponent implements OnInit {

  test: Test = {
    id: 0,
    title: '',
    questions: [],
    creatorId: '',
    published: false,
    publishDate: new Date(0),
    createdDate: new Date(0),
    editDate: new Date(0)
  };

  errorMessage = '';
  currentQuestionIndex = 0;
  answers: { [id: number]: string } = {};
  currentQuestion: any;
  isFirst: boolean = true;
  isLast: boolean = false;
  isLocked: boolean = false;
  rightAnswers: number = 0;

  isFinishModalOpen: boolean = false;
  isModalOpen: boolean = false;
  textModal: string = '';
  isPassedModalOpen = false;

  tryedTest: Test = {
    id: 0,
    title: '',
    questions: [],
    creatorId: '',
    published: false,
    publishDate: new Date(0),
    createdDate: new Date(0),
    editDate: new Date(0)
  };

  try: UserTest | null = {
    id: 0,
    userId: '',
    test: this.tryedTest,
    passedAt: new Date(0),
    score: 0,
    isPassed: false
  }

  constructor(
    private testService: TestService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
  ) { }


  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const testId = Number(params.get('id'));
      if (testId) {
        this.testService.getTestById(testId).subscribe({
          next: (data) => {
            this.test = data;

            if (data.questions.length > 0) {
              this.currentQuestion = data.questions[0];
            }

            this.testService.isPassed(this.test.id).subscribe({
              next: (record) => {
                this.try = record

                if (this.try !== null) {
                  this.isPassedModalOpen = true;
                  this.textModal = `Вы уже проходили этот тест ${this.try.passedAt}`
                }

                this.updateIsLast();
                this.updateIsFirst();
              }
            });
          }
        })
      }
    });
  }


  selectedAnswers(questionId: number, option: string, title: string) {
    const selectedIndex = this.currentQuestion.options.indexOf(option);

    this.testService.checkAnswer(title, questionId, selectedIndex).subscribe({
      next: (response) => {
        if (response) {
          this.isModalOpen = true;
          this.textModal = "Правильно";
          this.isLocked = true;
          this.rightAnswers += 1;
        } else {
          this.isModalOpen = true;
          this.textModal = "Неправильно";
          this.isLocked = true;
        }
      },
      error: (err) => console.error('Error: ', err)
    });
  }

  nextQuestion() {
    if (!this.isLocked) {
      return;
    }

    const i = this.test.questions.indexOf(this.currentQuestion);

    if (i < this.test.questions.length - 1) {
      this.currentQuestion = this.test.questions[i + 1];
    }

    this.isLocked = false;
    this.updateIsLast();
    this.updateIsFirst();
  }

  //lastQuestion() {
  //  const currentQuestionIndex = this.test.questions.indexOf(this.currentQuestion);
  //  if (currentQuestionIndex > 0) {
  //    this.currentQuestion = this.test.questions[currentQuestionIndex - 1];
  //  }

  //  this.updateIsLast();
  //  this.updateIsFirst();
  //}  


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
    this.isFinishModalOpen = true;
    this.textModal = "Тест завершен! Результат " + this.rightAnswers + "/" + this.test.questions.length;

    //const token = localStorage.getItem("token");

    //if (!token) {
    //  return;
    //}

    if (!this.authService.isAuthenticated) {
      return;
    }

    this.testService.passTest(this.test.id, this.rightAnswers).subscribe();
  }

  closeModal() {
    this.isModalOpen = false;
  }

  closeFinishModal() {
    this.isFinishModalOpen = false;
    this.router.navigate(['/tests'])
  }

  closePassedModal() {
    this.router.navigate(['/tests'])
  }
}
