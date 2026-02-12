import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class TestService {
  private baseUrl = "https://localhost:44356/api/tests"

  constructor(private http: HttpClient, private authService: AuthService) { }



  getAllTests(): Observable<Test[]> {
    return this.http.get<Test[]>(`${this.baseUrl}/all`);
  }



  getTestByName(name: string): Observable<Test> {
    return this.http.get<Test>(`${this.baseUrl}/${name}`, { withCredentials: true });
  }



  getTestById(id: number): Observable<Test> {
    return this.http.get<Test>(`${this.baseUrl}/id/${id}`, { withCredentials: true })
  }



  getMyTests() {
    return this.http.get<Test[]>(`${this.baseUrl}/my`, { withCredentials: true });
  }



  getPassedTests() {
    return this.http.get<UserTest[]>(`${this.baseUrl}/passed`, { withCredentials: true });
  }



  getPublishedTests() {
    return this.http.get<Test[]>(`${this.baseUrl}/published`, { withCredentials: true });
  }



  checkTestExists(name: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/exist/${name}`, { withCredentials: true })
  }



  checkAnswer(title: string, questionId: number, selectedOptionIndexes: number[]) {
    return this.http.post<boolean[]>(`${this.baseUrl}/check`, {
      title,
      questionId,
      selectedOptionIndexes
    }, { withCredentials: true });
  }



  passTest(testId: number, score: number) {
    return this.http.post<boolean>(`${this.baseUrl}/pass/${testId}`, score, { withCredentials: true });
  }



  startTest(testId: number) {
    return this.http.post<UserTestDto>(
      `${this.baseUrl}/${testId}/start`,
      {},
      { withCredentials: true }
    );
  }



  submitAnswer(dto: SubmitAnswerDto) {
    return this.http.post<SubmitAnswerResult>(
      `${this.baseUrl}/answer`,
      dto,
      { withCredentials: true }
    );
  }



  addTest(test: Test) {
    return this.http.post<boolean>(`${this.baseUrl}/add`, test, { withCredentials: true });
  }



  editTest(id: number, test: Test) {
    return this.http.post(`${this.baseUrl}/edit/${id}`, test, { withCredentials: true });
  }



  getTestChanges(original: Test, updated: Test) {
    const changes: string[] = [];

    if (original.title !== updated.title) {
      changes.push(`Название теста изменено с "${original.title}" на "${updated.title}"`);
    }

    updated.questions.forEach((newQuestion, i) => {
      const oldQuestion = original.questions[i];

      if (!oldQuestion) {
        changes.push(`Добавлен новый вопрос: "${newQuestion.text}"`);
        return;
      }

      if (oldQuestion.text !== newQuestion.text) {
        changes.push(`Изменён текст вопроса ${i + 1}: "${oldQuestion.text}" → "${newQuestion.text}"`);
      }

      if (oldQuestion.isMultiple != newQuestion.isMultiple) {
        changes.push(`Изменен множественный выбор в вопросе ${i + 1}`)
      }

      const oldCorrectIndexes = oldQuestion.options
        .map((o, idx) => o.isCorrect ? idx : -1)
        .filter(i => i !== -1);

      const newCorrectIndexes = newQuestion.options
        .map((o, idx) => o.isCorrect ? idx : -1)
        .filter(i => i !== -1);

      const sameLength = oldCorrectIndexes.length === newCorrectIndexes.length;
      const sameSet = sameLength && oldCorrectIndexes.every(i => newCorrectIndexes.includes(i));

      if (!sameSet) {
        changes.push(`Изменены правильные ответы в вопросе ${i + 1}`);
      }

      newQuestion.options.forEach((newOpt, j) => {
        const oldOpt = oldQuestion.options[j];

        if (!oldOpt) {
          changes.push(`Добавлен новый вариант в вопросе ${i + 1}: "${newOpt.text}"`);
          return;
        }

        if (oldOpt.text !== newOpt.text) {
          changes.push(`Изменён вариант ${j + 1} в вопросе ${i + 1}: "${oldOpt.text}" → "${newOpt.text}"`);
        }
      });
    });

    if (original.minimumSuccessPercent > updated.minimumSuccessPercent) {
      changes.push(`Понижен минимальный процент правильных ответов`)
    } else if (original.minimumSuccessPercent < updated.minimumSuccessPercent) {
      changes.push(`Повышен минимальный процент правильных ответов`)
    }

    const addedTypes = updated.types.filter(type => !original.types.includes(type));
    const removedTypes = original.types.filter(type => !updated.types.includes(type));

    addedTypes.forEach(type => {
      changes.push(`Добавлена модификация ${type}`);
    });

    removedTypes.forEach(type => {
      changes.push(`Удалена модификация ${type}`);
    });


    if (original.questions.length > updated.questions.length) {
      changes.push(`Удалено ${original.questions.length - updated.questions.length} вопрос(ов)`);
    }

    if (original.published == true && updated.published == false) {
      changes.push(`Тест скрыт`);
    }
    if (original.published == false && updated.published == true) {
      changes.push(`Тест опубликован`);
    }

    return changes;
  }



  checkTest(test: Test) {
    for (var i = 0; i < test.questions.length; i++) {

      if (test.questions[i].text == "") {
        return "Поле вопроса на заполнено";
      }

      if (test.questions[i].options.length == 0) {
        return "У вопроса " + (i + 1) + " нет вариантов ответов";
      }

      if (test.questions[i].options.length < 2) {
        return "Для вопроса " + (i + 1) + " добавлено слишком мало вариантов ответа";
      }

      var count_of_true: number = 0;
      for (var j = 0; j < test.questions[i].options.length; j++) {
        if (test.questions[i].options[j].text == "") {
          return "Поле варианта " + (j + 1) + " вопроса " + (i + 1) + " не заполнено";
        }

        if (test.questions[i].options[j].isCorrect == true) {
          count_of_true += 1;
        }
      }

      if (count_of_true == 0) {
        return "Для вопроса " + (i + 1) + " не указан правильный вариант ответа";
      }
    }

    return "true";
  }



  isPassed(testId: number): Observable<UserTest | null> {
    if (this.authService.isAuthenticated) {
      return this.http.get<UserTest | null>(`${this.baseUrl}/isPassed/${testId}`, { withCredentials: true });
    }
    return of(null);
  }
}




export interface Test {
  id: number;
  title: string;
  types: string[],
  questions: Question[];
  creatorId: string;
  published: boolean;
  createdDate: Date;
  publishDate: Date;
  editDate: Date;
  minimumSuccessPercent: number;
}


export interface UserTest {
  id: number;
  userId: string;
  test: Test;
  startedAt: Date;
  finishedAt: Date;
  score: number;
  isFinished: boolean;
}

export interface UserTestDto {
  userTestId: number;
  startedAt: Date;
  isFinished: boolean;
  answers: UserAnswerDto[];
}

export interface UserAnswerDto {
  questionId: number;
  selectedOptionIds: number[]
}

export interface SubmitAnswerDto {
  userTestId: number;
  questionId: number;
  selectedOptionIds: number[]
}

export interface SubmitAnswerResult {
  isCorrect: boolean[];
  answeredQuestions: number;
}

export interface StartTestResponseDto {
  userTestId: number;
  answeredQuestionIds: number[];
  nextQuestion: Question | null;
}


export interface TestType {
  id: number;
  name: string;
  description: string;
}

export interface Question {
  id: number;
  text: string;
  options: Option[];
  isMultiple: boolean;
}

export interface Option {
  id: number;
  text: string;
  isCorrect: boolean;
}
