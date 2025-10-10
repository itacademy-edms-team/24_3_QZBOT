import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TestService {
  private baseUrl = "https://localhost:44356/api/tests"

  constructor(private http: HttpClient) { }

  getTestsByName(name: string): Observable<Test[]> {
    return this.http.get<Test[]>(`${this.baseUrl}/${name}`);
  }

  getTestById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }
}

export interface Test {
  id: number;
  question: string;
  options: string[];
  correctOption: number;
}
