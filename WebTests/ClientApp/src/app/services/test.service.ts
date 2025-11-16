import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TestService {
  private baseUrl = "https://localhost:44356/api/tests"

  constructor(private http: HttpClient) { }

  getTestTitleById(id: number): Observable<string> {
    return this.http.get<string>(`${this.baseUrl}/${id}/title`);
  }

  getTestByName(name: string): Observable<Question[]> {
    return this.http.get<Question[]>(`${this.baseUrl}/${name}`);
  }

  getTestById(id: number): Observable<Question[]> {
    return this.http.get<Question[]>(`${this.baseUrl}/id/${id}`)
  }

  getAllTests(): Observable<Test[]> {
    return this.http.get<Test[]>(`${this.baseUrl}/all`);
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

  addTest(title: string, questions: Question[]) {
    return this.http.post<boolean>(`${this.baseUrl}/add`, {
      title,
      questions
    });
  }

  editTest(title: string, test: Test) {
    return this.http.post<boolean>(`${this.baseUrl}/edit/${title}`, test);
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

      newQuestion.options.forEach((newOpt, j) => {
        const oldOpt = oldQuestion.options[j];

        if (!oldOpt) {
          changes.push(`Добавлен новый вариант в вопросе ${i + 1}: "${newOpt.text}"`);
          return;
        }

        if (oldOpt.text !== newOpt.text) {
          changes.push(`Изменён вариант ${j + 1} в вопросе ${i + 1}: "${oldOpt.text}" → "${newOpt.text}"`);
        }

        if (oldOpt.isCorrect !== newOpt.isCorrect) {
          changes.push(`Изменён правильный ответ в вопросе ${i + 1}`);
        }
      });
    });

    if (original.questions.length > updated.questions.length) {
      changes.push(`Удалено ${original.questions.length - updated.questions.length} вопрос(ов)`);
    }

    return changes;
  }
}

export interface Test {
  id: number;
  title: string;
  questions: Question[];
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
