import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { TestService, TestType, Test, Question, Option, UserTest, UserTestDto, SubmitAnswerDto, SubmitAnswerResult } from '../../services/test.service';
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
    types: [],
    creatorId: '',
    published: false,
    publishDate: new Date(0),
    createdDate: new Date(0),
    editDate: new Date(0)
  };

  errorMessage = '';
  currentQuestionIndex = 0;
  answers: { [id: number]: string } = {};
  currentQuestion!: Question;
  isFirst: boolean = true;
  isLast: boolean = false;
  isSubmited: boolean = false;
  isSelected: boolean = false;
  selectedOptionIndexes: number[] = [];
  rightAnswers: number = 0;

  isFinishModalOpen: boolean = false;
  isModalOpen: boolean = false;
  textModal: string = '';
  isPassedModalOpen: boolean = false;
  isUnauthModalOpen: boolean = false;

  tryedTest: Test = {
    id: 0,
    title: '',
    types: [],
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

  userTest!: UserTestDto;

  mode = {
    strict: false,
    timeLimited: false,
    shuffle: false,
    authOnly: false,
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

            data.types.forEach(type => {
              if (type.name === "AuthOnly") {
                this.mode.authOnly = true;
              } else if (type.name === "Strict") {
                this.mode.strict = true;
              }
            })

            if (this.mode.authOnly) {
              if (!this.authService.isAuthenticated) {
                this.isUnauthModalOpen = true;
                this.textModal = "Этот тест доступен только авторизованным пользователям!"
              }
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


  onOptionToggle(option: Option) {
    const index = this.currentQuestion.options.indexOf(option);

    const correctCount = this.currentQuestion.options.filter((o: Option) => o.isCorrect).length;

    const isMultiple = correctCount > 1;

    if (isMultiple) {
      const i = this.selectedOptionIndexes.indexOf(index);

      if (i === -1) {
        this.selectedOptionIndexes.push(index);
      } else {
        this.selectedOptionIndexes.splice(i, 1);
      }
    } else {
      this.selectedOptionIndexes = [index];
    }

    this.isSelected = true;
  }


  submitAnswer(questionId: number, title: string) {
    this.testService.checkAnswer(title, questionId, this.selectedOptionIndexes).subscribe({
      next: (response) => {
        const totalCorrect = this.currentQuestion.options.filter(o => o.isCorrect).length;
        const isMultiple = totalCorrect > 1;

        if (isMultiple) {
          const selectedCount = response.length;
          const correctSelected = response.filter(x => x).length;
          const wrongSelected = selectedCount - correctSelected;

          const totalCorrect = this.currentQuestion.options.filter(o => o.isCorrect).length;

          const score = Math.max(0, (correctSelected - wrongSelected) / totalCorrect);
          this.rightAnswers += score;

          this.isModalOpen = true;
          this.textModal = "Правильно " + correctSelected;

        } else {
          if (response[0] == true) {
            this.isModalOpen = true;
            this.textModal = "Правильно";
            this.rightAnswers += 1;
          } else {
            this.isModalOpen = true;
            this.textModal = "Неправильно";
          }
        }
      }
    })

    this.isSubmited = true;
  }



  nextQuestion() {
    const i = this.test.questions.indexOf(this.currentQuestion);

    if (i < this.test.questions.length - 1) {
      this.currentQuestion = this.test.questions[i + 1];
    }

    this.updateIsLast();
    this.updateIsFirst();

    this.selectedOptionIndexes = [];
    this.isSubmited = false;
    this.isSelected = false;
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

    if (!this.authService.isAuthenticated) {
      return;
    }

    this.testService.passTest(this.test.id, this.rightAnswers).subscribe();
  }

  auth() {
    this.router.navigate(['/login'])
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
