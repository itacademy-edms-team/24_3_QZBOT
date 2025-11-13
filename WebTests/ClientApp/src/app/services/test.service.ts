import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TestService {
  private baseUrl = "https://localhost:44356/api/tests"

  constructor(private http: HttpClient) { }

  getTestsByName(name: string): Observable<Question[]> {
    return this.http.get<Question[]>(`${this.baseUrl}/${name}`);
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

  editTest(title: string, questions: Question[]) {
    return this.http.post<boolean>(`${this.baseUrl}/edit/${title}`, questions);
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
