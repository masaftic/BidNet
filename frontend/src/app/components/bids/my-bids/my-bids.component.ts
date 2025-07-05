import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BidService } from '../../../services/bid.service';
import { NavbarComponent } from '../../ui/navbar/navbar.component';
import { ButtonComponent } from '../../ui/button/button.component';
import { UserBidsResponse, Bid } from '../../../models/bid.model';
import { AuctionService } from '../../../services/auction.service';

@Component({
  selector: 'app-my-bids',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NavbarComponent,
    ButtonComponent
  ],
  templateUrl: './my-bids.component.html'
})
export class MyBidsComponent implements OnInit {
  private bidService = inject(BidService);
  private auctionService = inject(AuctionService);

  userBids = signal<UserBidsResponse | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadMyBids();
  }

  loadMyBids(): void {
    this.isLoading.set(true);

    this.bidService.getUserBids().subscribe({
      next: (response) => {
        this.userBids.set(response);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading bids', error);
        this.errorMessage.set('Failed to load your bids. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  isAuctionActive(bidDate: string, winningStatus?: boolean): boolean {
    // If we know it's a winning bid, it's in an active auction
    if (winningStatus) return true;

    // Otherwise, check if the bid was placed recently (within the last 7 days)
    // This is a simplification - in a real app we'd have the auction end date to check
    const bidTime = new Date(bidDate).getTime();
    const oneWeekAgo = new Date();
    oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);

    return bidTime > oneWeekAgo.getTime();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
  }

  getBidStatusLabel(bid: Bid): string {
    if (bid.isWinning) {
      return 'Winning';
    }

    const bidDate = new Date(bid.createdAt);
    const now = new Date();
    const oneWeekAgo = new Date();
    oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);

    // Again simplified - in real app we'd know auction state
    if (bidDate.getTime() > oneWeekAgo.getTime()) {
      return 'Active';
    }

    return 'Outbid';
  }

  getBidStatusClass(bid: Bid): string {
    if (bid.isWinning) {
      return 'bg-green-100 text-green-800';
    }

    const bidDate = new Date(bid.createdAt);
    const now = new Date();
    const oneWeekAgo = new Date();
    oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);

    if (bidDate.getTime() > oneWeekAgo.getTime()) {
      return 'bg-blue-100 text-blue-800';
    }

    return 'bg-red-100 text-red-800';
  }
}
