import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { TestService, TestType, Test, Question, Option, UserTest, UserTestDto, SubmitAnswerDto, SubmitAnswerResult } from '../../services/test.service';
import { AuthService } from '../../services/auth.service';
import { Observable, map } from 'rxjs';

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
    editDate: new Date(0),
    minimumSuccessPercent: 70
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
  isUnauth: boolean = false;

  tryedTest: Test = {
    id: 0,
    title: '',
    types: [],
    questions: [],
    creatorId: '',
    published: false,
    publishDate: new Date(0),
    createdDate: new Date(0),
    editDate: new Date(0),
    minimumSuccessPercent: 70
  };

  try: UserTest | null = {
    id: 0,
    userId: '',
    test: this.tryedTest,
    startedAt: new Date(0),
    finishedAt: new Date(0),
    score: 0,
    isFinished: false
  }

  userTest!: UserTestDto;
  isCreator: boolean = false;

  mode = {
    strict: false,
    timeLimited: false,
    shuffle: false,
    authOnly: false,
    allowBack: false,
    showAfterEach: false,
    manyTimes: false
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

            data.types.forEach(type => {
              if (type === "AuthOnly") {
                this.mode.authOnly = true;
              } else if (type === "Strict") {
                this.mode.strict = true;
              } else if (type === "AllowBack") {
                this.mode.allowBack = true;
              } else if (type === "ShowAfterEach") {
                this.mode.showAfterEach = true;
              } else if (type === "Shuffle") {
                this.mode.shuffle = true;
              } else if (type === "ManyTimes") {
                this.mode.manyTimes = true;
              }
            })


            if (this.mode.authOnly) {
              if (!this.authService.isAuthenticated) {
                this.isUnauth = true;
              }
            }

            if (this.mode.shuffle) {
              this.shuffle(this.test.questions);
            }


            if (this.test.questions.length > 0) {
              this.currentQuestion = this.test.questions[0];
            }

            this.testService.isPassed(this.test.id).subscribe({
              next: (record) => {
                this.try = record

                if (this.try !== null && !this.mode.manyTimes) {
                  this.isPassedModalOpen = true;
                  this.textModal = `Вы уже проходили этот тест ${this.try.finishedAt}`
                }

                this.updateIsLast();
                this.updateIsFirst();
              }
            });

            this.authService.currentUserId.subscribe({
              next: (data) => {
                if (data === this.test.creatorId) {
                  this.isCreator = true;
                }
              }
            })
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


  lastQuestion() {
    const currentQuestionIndex = this.test.questions.indexOf(this.currentQuestion);
    if (currentQuestionIndex > 0) {
      this.currentQuestion = this.test.questions[currentQuestionIndex - 1];
    }

    this.updateIsLast();
    this.updateIsFirst();
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
    this.testService.passTest(this.test.id, this.rightAnswers).subscribe({
      next: (isPassed) => {
        if (isPassed) {
          this.isFinishModalOpen = true;
          this.textModal = "Результат " + this.rightAnswers + "/" + this.test.questions.length + ". Тест успешно пройден."
        } else {
          this.isFinishModalOpen = true;
          this.textModal = "Результат " + this.rightAnswers + "/" + this.test.questions.length + ". Тест не пройден."
        }
      }
    });
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

  shuffle(array: any[]) {
    for (let i = array.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [array[i], array[j]] = [array[j], array[i]]
    }
  }
}
