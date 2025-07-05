import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WalletService } from '../../services/wallet.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';
import { TransactionHistoryResponse, Transaction } from '../../models/wallet.model';

@Component({
  selector: 'app-transaction-history',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NavbarComponent,
    ButtonComponent
  ],
  templateUrl: './transaction-history.component.html'
})
export class TransactionHistoryComponent implements OnInit {
  private walletService = inject(WalletService);
  
  transactionHistory = signal<TransactionHistoryResponse | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);
  
  // Pagination
  currentPage = signal(0);
  pageSize = signal(20);
  
  ngOnInit(): void {
    this.loadTransactionHistory();
  }
  
  loadTransactionHistory(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    
    this.walletService.getTransactionHistory(this.currentPage(), this.pageSize()).subscribe({
      next: (response) => {
        this.transactionHistory.set(response);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading transaction history', error);
        this.errorMessage.set('Failed to load transaction history');
        this.isLoading.set(false);
      }
    });
  }
  
  nextPage(): void {
    const totalPages = Math.ceil((this.transactionHistory()?.totalCount || 0) / this.pageSize());
    if (this.currentPage() + 1 < totalPages) {
      this.currentPage.update(page => page + 1);
      this.loadTransactionHistory();
    }
  }
  
  previousPage(): void {
    if (this.currentPage() > 0) {
      this.currentPage.update(page => page - 1);
      this.loadTransactionHistory();
    }
  }
  
  getTotalPages(): number {
    return Math.ceil((this.transactionHistory()?.totalCount || 0) / this.pageSize());
  }
  
  getTransactionTypeClass(type: string): string {
    switch (type.toLowerCase()) {
      case 'deposit':
        return 'bg-green-100 text-green-800';
      case 'withdrawal':
        return 'bg-red-100 text-red-800';
      case 'hold':
        return 'bg-yellow-100 text-yellow-800';
      case 'release':
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }
  
  getTransactionIcon(type: string): string {
    switch (type.toLowerCase()) {
      case 'deposit':
        return '↗️';
      case 'withdrawal':
        return '↘️';
      case 'hold':
        return '⏸️';
      case 'release':
        return '▶️';
      default:
        return '💰';
    }
  }
  
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
  }
}
