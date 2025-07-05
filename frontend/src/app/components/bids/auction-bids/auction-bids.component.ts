import { Component, Input, OnChanges, OnInit, SimpleChanges, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuctionBidsResponse, PlaceBidRequest } from '../../../models/bid.model';
import { BidService } from '../../../services/bid.service';
import { AuthService } from '../../../services/auth.service';
import { ButtonComponent } from '../../ui/button/button.component';

@Component({
  selector: 'app-auction-bids',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent],
  templateUrl: './auction-bids.component.html'
})
export class AuctionBidsComponent implements OnInit, OnChanges {
  private bidService = inject(BidService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);

  auctionId = input.required<string>();
  isAuctionActive = input(true);

  bids = signal<AuctionBidsResponse>({ auctionId: '', startingPrice: 0, bidHistory: [] });
  isLoading = signal(true);
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  bidForm: FormGroup;

  constructor() {
    this.bidForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(0.01)]]
    });
  }

  ngOnInit(): void {
    this.loadBids();
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Reload bids when the auction ID changes
    if (changes['auctionId'] && !changes['auctionId'].firstChange) {
      this.loadBids();
    }

    // Update minimum bid validator when auction active state changes
    if (changes['isAuctionActive']) {
      this.updateMinBidValidator();
    }
  }

  loadBids(): void {
    if (!this.auctionId()) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.bidService.getAuctionBids(this.auctionId()).subscribe({
      next: (response) => {
        this.bids.set(response);
        this.isLoading.set(false);
        this.updateMinBidValidator();
      },
      error: (error) => {
        console.error('Error loading bids', error);
        this.errorMessage.set(error.error?.message || 'Failed to load bid history');
        this.isLoading.set(false);
      }
    });
  }

  placeBid(): void {
    if (this.bidForm.invalid || !this.auctionId()) return;

    const bidRequest: PlaceBidRequest = {
      auctionId: this.auctionId(),
      amount: this.bidForm.value.amount
    };

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.bidService.placeBid(bidRequest).subscribe({
      next: (response) => {
        this.loadBids();

        this.isSubmitting.set(false);
        this.successMessage.set('Your bid was placed successfully!');
        this.bidForm.reset();

        // Update minimum bid validator
        this.updateMinBidValidator();
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error.error?.message || 'Failed to place bid');
      }
    });
  }

  getMinimumBid(): number {
    // If there's a current price, the new bid should be at least 1 more than current
    if (this.bids().currentPrice !== null && this.bids().currentPrice !== undefined) {
      return Number(this.bids().currentPrice) + 1;
    }

    // Otherwise, use the starting price
    return Number(this.bids().startingPrice);
  }

  updateMinBidValidator(): void {
    const minBid = this.getMinimumBid();
    this.bidForm.get('amount')?.setValidators([
      Validators.required,
      Validators.min(minBid)
    ]);
    this.bidForm.get('amount')?.updateValueAndValidity();
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
  }
}
