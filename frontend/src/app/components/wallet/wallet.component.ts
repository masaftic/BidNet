import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WalletService } from '../../services/wallet.service';
import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';
import { Wallet, Transaction } from '../../models/wallet.model';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NavbarComponent,
    ButtonComponent
  ],
  templateUrl: './wallet.component.html'
})
export class WalletComponent implements OnInit {
  private walletService = inject(WalletService);
  private authService = inject(AuthService);

  wallet = signal<Wallet | null>(null);
  currentUser = signal<User | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadWallet();
    this.loadUserProfile();
  }

  loadWallet(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.walletService.getWallet().subscribe({
      next: (wallet) => {
        this.wallet.set(wallet);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading wallet', error);
        this.errorMessage.set('Failed to load wallet information');
        this.isLoading.set(false);
      }
    });
  }

  loadUserProfile(): void {
    // Subscribe to current user from auth service
    this.authService.currentUser$.subscribe(user => {
      this.currentUser.set(user);
    });
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

  refreshWallet(): void {
    this.loadWallet();
    // Also refresh user profile to get updated balance
    this.authService.refreshUserProfile().subscribe();
  }
}
