import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Router } from '@angular/router';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:44356/api/auth';
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  redirectUrl: string | null = null;

  constructor(
    private http: HttpClient,
    private router: Router  // <- Обязательно инжектим Router
  ) {
    const token = localStorage.getItem('token');
    if (token) {
      this.currentUserSubject.next({ token });
    }
  }

  login(model: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, model).pipe(
      map(response => {
        if (response.token) {
          localStorage.setItem('token', response.token);
          // Сохраняем username в localStorage или в BehaviorSubject
          localStorage.setItem('username', response.username);

          this.currentUserSubject.next(response);

          // Перенаправляем на /profile/username
          setTimeout(() => {
            if (this.redirectUrl) {
              this.router.navigate([this.redirectUrl!]);
              this.redirectUrl = null;
            } else {
              this.router.navigate(['/profile', response.username]); // <- Навигация с username
            }
          }, 0);
        }
        return response;
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  get token(): string | null {
    return localStorage.getItem('token');
  }

  get isAuthenticated(): boolean {
    return !!this.token;
