import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, AuthResponseWithUser, LoginRequest, RegisterRequest, User } from '../models/user.model';
import { WalletService } from './wallet.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  setRefreshToken(refreshToken: string) {
    localStorage.setItem(this.refreshTokenKey, refreshToken);
  }
  setAccessToken(newToken: string) {
    localStorage.setItem(this.tokenKey, newToken);
  }
  private apiUrl = 'http://localhost:5000/api';
  private tokenKey = 'auth_token';
  private refreshTokenKey = 'refresh_token';
  private userKey = 'user';
  private http = inject(HttpClient);
  private walletService = inject(WalletService);

  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    // Set auth service reference in wallet service to avoid circular dependency
    this.walletService.setAuthService(this);
  }


  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => this.setSession(response)),
        // After setting session, fetch user profile
        tap(() => this.getCurrentUserProfile().subscribe())
      );
  }

  register(user: RegisterRequest): Observable<AuthResponseWithUser> {
    return this.http.post<AuthResponseWithUser>(`${this.apiUrl}/auth/register`, user)
      .pipe(
        tap(response => console.log('User registered:', response)),
        tap(response => this.setSession(response)),
        // After setting session, fetch user profile
        tap(() => this.getCurrentUserProfile().subscribe())
      );
  }

  refresh(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/refresh-token`, { refreshToken })
      .pipe(
        tap(response => this.setSession(response))
      );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next(null);

    // Clear wallet data on logout
    this.walletService.clearWallet();
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getRefreshToken() {
    return localStorage.getItem(this.refreshTokenKey);
  }

  private setSession(authResult: AuthResponse): void {
    const { accessToken, refreshToken } = authResult;

    localStorage.setItem(this.tokenKey, accessToken);
    localStorage.setItem(this.refreshTokenKey, refreshToken);
  }


  getUserFromStorage(): User | null {
    const userJson = localStorage.getItem(this.userKey);
    return userJson ? JSON.parse(userJson) : null;
  }

  /**
   * Fetch current user profile from the server
   */
  getCurrentUserProfile(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/users/me/profile`)
      .pipe(
        tap(user => {
          // Store the user data and update the current user subject
          localStorage.setItem(this.userKey, JSON.stringify(user));
          this.currentUserSubject.next(user);
        })
      );
  }

  /**
   * Refresh user profile data (useful after wallet transactions, etc.)
   */
  refreshUserProfile(): Observable<User> {
    return this.getCurrentUserProfile();
  }

  /**
   * Initialize user session if token exists but no user data is cached
   */
  initializeSession(): Observable<User | null> {
    if (this.isLoggedIn() && !this.getUserFromStorage()) {
      return this.getCurrentUserProfile();
    }
    return new Observable(observer => {
      observer.next(this.getUserFromStorage());
      observer.complete();
    });
  }

  /**
   * Get current user (signal-based)
   */
  currentUser = () => this.currentUserSubject.value;
}
