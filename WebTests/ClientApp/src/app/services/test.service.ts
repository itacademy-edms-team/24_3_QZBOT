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
    return this.http.get<Test>(`${this.baseUrl}/${name}`);
  }



  getTestById(id: number): Observable<Test> {
    return this.http.get<Test>(`${this.baseUrl}/id/${id}`)
  }



  getMyTests() {
    return this.http.get<Test[]>(`${this.baseUrl}/my`);
  }


  getPassedTests() {
    return this.http.get<UserTest[]>(`${this.baseUrl}/passed`);
  }



  getPublishedTests() {
    return this.http.get<Test[]>(`${this.baseUrl}/published`);
  }



  checkTestExists(name: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/exist/${name}`)
  }



  checkAnswer(title: string, questionId: number, selectedOptionIndex: number) {
    return this.http.post<boolean>(`${this.baseUrl}/check`, {
      title,
      questionId,
      selectedOptionIndex
    });
  }



  passTest(testId: number, score: number) {
    return this.http.post(`${this.baseUrl}/pass/${testId}`, score);
  }



  addTest(title: string, questions: Question[]) {
    return this.http.post<boolean>(`${this.baseUrl}/add`, {
      title,
      published: false,
      questions
    });
  }



  editTest(id: number, test: Test) {
    return this.http.post(`${this.baseUrl}/edit/${id}`, test);
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

      const oldCorrect = oldQuestion.options.findIndex(o => o.isCorrect);
      const newCorrect = newQuestion.options.findIndex(o => o.isCorrect);

      if (oldCorrect !== newCorrect) {
        changes.push(`Изменён правильный ответ в вопросе ${i + 1}`);
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

      if (count_of_true > 1) {
        return "Для вопроса " + (i + 1) + " указано несколько правильных вариантов ответа";
      }
      else if (count_of_true == 0) {
        return "Для вопроса " + (i + 1) + " не указан правильный вариант ответа";
      }
    }

    return "true";
  }



  isPassed(testId: number): Observable<UserTest | null> {
    if (this.authService.isAuthenticated) {
      return this.http.get<UserTest | null>(`${this.baseUrl}/isPassed/${testId}`);
    }
    return of(null);
  }



  get currentUserId(): string {
    const token = localStorage.getItem("token");
    if (!token) return "";

    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload["nameid"];
  }
}




export interface Test {
  id: number;
  title: string;
  questions: Question[];
  creatorId: string;
  published: boolean;
}

export interface UserTest {
  id: number;
  userId: string;
  test: Test;
  passedAt: Date;
  score: number;
  isPassed: boolean;
}

export interface TestType {
  id: number;
  name: string;
  desription: string;
}

export interface Question {
  id: number;
  text: string;
  options: Option[];
}

export interface Option {
  id: number;
  text: string;
  isCorrect: boolean;
}
