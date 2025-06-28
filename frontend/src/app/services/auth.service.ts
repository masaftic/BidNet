import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, AuthResponseWithUser, LoginRequest, RegisterRequest, User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api';
  private tokenKey = 'auth_token';
  private refreshTokenKey = 'refresh_token';
  private userKey = 'user';
  private http = inject(HttpClient);

  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();


  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => this.setSession(response))
      );
  }

  register(user: RegisterRequest): Observable<AuthResponseWithUser> {
    return this.http.post<AuthResponseWithUser>(`${this.apiUrl}/auth/register`, user)
      .pipe(
        tap(response => console.log('User registered:', response)),
        tap(response => this.setSession(response)),
      );
  }

  refresh(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(
        tap(response => this.setSession(response))
      );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next(null);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getRefreshToken() {
    return localStorage.getItem(this.refreshTokenKey);
  }

  private setSession(authResult: AuthResponse | AuthResponseWithUser): void {
    const { accessToken, refreshToken, TokenExpiration, ...user } = authResult;

    localStorage.setItem(this.tokenKey, accessToken);
    localStorage.setItem(this.refreshTokenKey, refreshToken);

    // Check if user object exists and has expected properties
    if (user && Object.keys(user).length > 0) {
      localStorage.setItem(this.userKey, JSON.stringify(user));
      this.currentUserSubject.next(user as User);
    }
  }


  private getUserFromStorage(): User | null {
    const userJson = localStorage.getItem(this.userKey);
    return userJson ? JSON.parse(userJson) : null;
  }
}
