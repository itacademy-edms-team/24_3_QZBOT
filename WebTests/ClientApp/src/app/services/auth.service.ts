import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Router } from '@angular/router';

export interface RegisterRequest {
  username: string;
  email: string;
  phoneNumber: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  username: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:44356/api/auth';
  private currentUserSubject = new BehaviorSubject<string | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  redirectUrl: string | null = null;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    const username = localStorage.getItem('username');
    if (username) {
      this.currentUserSubject.next(username);
    }
  }

  register(model: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, model);
  }

  login(model: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}/login`,
      model,
      { withCredentials: true } // ⭐ ОБЯЗАТЕЛЬНО
    ).pipe(
      map(response => {
        // cookie уже сохранена браузером автоматически

        localStorage.setItem('username', response.username);
        this.currentUserSubject.next(response.username);

        // навигация ВСЕГДА при успехе
        if (this.redirectUrl) {
          this.router.navigate([this.redirectUrl]);
          this.redirectUrl = null;
        } else {
          this.router.navigate(['/profile', response.username]);
        }

        return response;
      })
    );
  }


  logout(): void {
    this.http.post(
      `${this.apiUrl}/logout`,
      {},
      { withCredentials: true }
    ).subscribe(() => {
      localStorage.removeItem('username');
      this.currentUserSubject.next(null);
      this.router.navigate(['/login']);
    });
  }

  checkAuth(): Observable<LoginResponse> {
    return this.http.get<LoginResponse>(
      `${this.apiUrl}/me`,
      { withCredentials: true }
    );
  }

  get isAuthenticated(): boolean {
    return !!this.currentUserSubject.value;
  }

  get currentUserUsername(): string | null {
    return localStorage.getItem('username');
  }
}
