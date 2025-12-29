import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
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

export interface MeResponse {
  id: string;
  username: string;
}


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:44356/api/auth';
  private currentUserSubject = new BehaviorSubject<string | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private currentUserIdCache: string | null = null;

  redirectUrl: string | null = null;

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  register(model: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, model);
  }

  login(model: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}/login`,
      model,
      { withCredentials: true }
    ).pipe(
      map(response => {
        this.currentUserSubject.next(response.username);
        localStorage.setItem('username', response.username);
        this.router.navigate(['/profile', response.username]);

        return response;
      })
    );
  }

  checkAuth(): Observable<boolean> {
    return this.http.get<LoginResponse>(
      `${this.apiUrl}/me`,
      { withCredentials: true }
    ).pipe(
      tap(res => {
        this.currentUserSubject.next(res.username);
        localStorage.setItem('username', res.username);
      }),
      map(() => true),
      catchError(() => {
        this.currentUserSubject.next(null);
        localStorage.removeItem('username');
        return of(false);
      })
    )
  }

  logout() {
    return this.http.post(
      `${this.apiUrl}/logout`,
      {},
      { withCredentials: true }
    ).subscribe(() => {
      this.currentUserIdCache = null;
      localStorage.removeItem('username');
      this.currentUserSubject.next(null);
      this.router.navigate(['/login']);
    });
  }


  get currentUser() {
    return this.currentUserSubject.value;
  }

  get isAuthenticated(): boolean {
    return !!this.currentUserSubject.value;
  }

  get currentUserId(): Observable<string | null> {
    if (this.currentUserIdCache) {
      return of(this.currentUserIdCache)
    }

    return this.http.get<MeResponse>(
      `${this.apiUrl}/me`,
      { withCredentials: true }
    ).pipe(
      tap(res => {
        this.currentUserIdCache = res.id;
      }),
      map(res => res.id),
      catchError(() => of(null))
    );
  }

  get currentUserUsername(): string | null { // разобраться с этим методом
    return localStorage.getItem('username');
  }
}
