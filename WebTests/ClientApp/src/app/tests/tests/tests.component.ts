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
  selectedOptionIds: number[] = [];
  rightAnswers: number = 0;

  isFinishModalOpen: boolean = false;
  isModalAnswerOpen: boolean = false;
  isModalTryOpen: boolean = false;
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
    isFinished: false,
    isPassed: false
  }

  userTest!: UserTestDto;
  userTestId: number = 0;
  savedAnswers: { [questionId: number]: number[] } = {};
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

            this.testService.startTest(testId).subscribe({
              next: (res) => {
                this.userTestId = res.userTestId;

                if (res.status == "Finished") {
                  this.router.navigate(['/tests']) // здесь будет страница итогов
                  return;
                }

                else if (res.status == "Active") {
                  this.isModalTryOpen = true;
                  this.textModal = `Продолжение попытки от ${res.startedAt}`
                }

                this.savedAnswers = {};

                res.answers.forEach(a => {
                  this.savedAnswers[a.questionId] = a.selectedOptionIds;
                });

                const firstUnansweredIndex = this.test.questions.findIndex(q => !this.savedAnswers[q.id])

                if (firstUnansweredIndex !== -1) {
                  this.loadQuestion(this.test.questions[firstUnansweredIndex]);
                } else {
                  this.loadQuestion(this.test.questions[this.test.questions.length - 1]);
                }

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


                this.authService.currentUserId.subscribe({
                  next: (data) => {
                    if (data === this.test.creatorId) {
                      this.isCreator = true;
                    }
                  }
                })
              },
              error: (err) => {
                this.userTest.userTestId = 0;
              }
            })
          }
        })
      }
    });
  }


  onOptionToggle(option: Option) {
    const isMultiple = this.currentQuestion.isMultiple;

    if (isMultiple) {
      const i = this.selectedOptionIds.indexOf(option.id);

      if (i === -1) {
        this.selectedOptionIds.push(option.id);
      } else {
        this.selectedOptionIds.splice(i, 1);
      }
    } else {
      this.selectedOptionIds = [option.id];
    }

    this.isSelected = true;
  }


  submitAnswer(questionId: number, title: string) {
    this.testService.submitAnswer({
      userTestId: this.userTestId,
      questionId,
      selectedOptionIds: this.selectedOptionIds
    }).subscribe({
      next: (response) => {

        this.savedAnswers[questionId] = [...this.selectedOptionIds];

        const score = response.score;

        if (score == 1) {
          this.isModalAnswerOpen = true;
          this.textModal = "Правильно";
        } else if (score == 0) {
          this.isModalAnswerOpen = true;
          this.textModal = "Неправильно";
        } else if (score > 0 && score < 1) {
          this.isModalAnswerOpen = true;
          this.textModal = "Почти правильно";
        }
      }
    })

    this.isSubmited = true;
  }



  nextQuestion() {
    const i = this.test.questions.indexOf(this.currentQuestion);

    if (i < this.test.questions.length - 1) {
      this.goToQuestion(i + 1);
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


  loadQuestion(question: Question) {
    this.currentQuestion = question;

    this.selectedOptionIds =
      this.savedAnswers[question.id]
        ? [...this.savedAnswers[question.id]]
        : [];

    this.isSubmited = !!this.savedAnswers[question.id];
  }


  goToQuestion(index: number) {
    const question = this.test.questions[index];
    this.loadQuestion(question);
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
    this.testService.finishTest(this.test.id).subscribe({
      next: (res) => {
        if (res.isPassed) {
          this.textModal = `Результат ${res.score}/${res.maxScore}. Тест успешно пройден`;
        } else {
          this.textModal = `Результат ${res.score}/${res.maxScore}. Тест не пройден`;
        }

        this.isFinishModalOpen = true;
      }
    })
  }

  restoreSelection() {
    const saved = this.savedAnswers[this.currentQuestion.id];
    if (!saved) return;

    this.selectedOptionIds = [...saved];
    this.isSubmited = true;
  }

  auth() {
    this.router.navigate(['/login'])
  }

  closeAnswerModal() {
    this.isModalAnswerOpen = false;
  }

  closeModelTryOpen() {
    this.isModalTryOpen = false;
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
