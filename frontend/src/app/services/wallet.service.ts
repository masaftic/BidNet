import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap, switchMap } from 'rxjs';
import {
  Wallet,
  DepositRequest,
  WithdrawalRequest,
  TransferFundsRequest,
  TransactionHistoryResponse
} from '../models/wallet.model';

@Injectable({
  providedIn: 'root'
})
export class WalletService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/wallet';

  // Use signals for maintaining wallet state
  wallet = signal<Wallet | null>(null);

  // Store auth service reference (will be injected later to avoid circular dependency)
  private authService: any = null;

  setAuthService(authService: any) {
    this.authService = authService;
  }

  /**
   * Get the current user's wallet information
   */
  getWallet(): Observable<Wallet> {
    return this.http.get<Wallet>(this.apiUrl)
      .pipe(
        tap(wallet => {
          this.wallet.set(wallet);
        })
      );
  }

  /**
   * Deposit funds into the wallet
   */
  deposit(request: DepositRequest): Observable<Wallet> {
    return this.http.post<Wallet>(`${this.apiUrl}/deposit`, request)
      .pipe(
        tap(wallet => {
          this.wallet.set(wallet);
          // Refresh user profile to sync wallet balance
          if (this.authService) {
            this.authService.refreshUserProfile().subscribe();
          }
        })
      );
  }

  /**
   * Withdraw funds from the wallet
   */
  withdraw(request: WithdrawalRequest): Observable<Wallet> {
    return this.http.post<Wallet>(`${this.apiUrl}/withdraw`, request)
      .pipe(
        tap(wallet => {
          this.wallet.set(wallet);
          // Refresh user profile to sync wallet balance
          if (this.authService) {
            this.authService.refreshUserProfile().subscribe();
          }
        })
      );
  }

  /**
   * Transfer funds to another user
   */
  transferFunds(request: TransferFundsRequest): Observable<Wallet> {
    return this.http.post<Wallet>(`${this.apiUrl}/transfer`, request)
      .pipe(
        tap(wallet => {
          this.wallet.set(wallet);
          // Refresh user profile to sync wallet balance
          if (this.authService) {
            this.authService.refreshUserProfile().subscribe();
          }
        })
      );
  }

  /**
   * Get transaction history with pagination
   */
  getTransactionHistory(page: number = 0, pageSize: number = 20): Observable<TransactionHistoryResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<TransactionHistoryResponse>(`${this.apiUrl}/transactions`, { params });
  }

  /**
   * Get cached wallet data
   */
  getCachedWallet(): Wallet | null {
    return this.wallet();
  }

  /**
   * Clear wallet state (useful for logout)
   */
  clearWallet(): void {
    this.wallet.set(null);
  }
}
