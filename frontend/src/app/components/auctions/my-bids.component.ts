import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuctionService } from '../../services/auction.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';
import { Auction } from '../../models/auction.model';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-my-bids',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NavbarComponent,
    ButtonComponent
  ],
  template: `
    <div class="min-h-screen bg-gray-100">
      <app-navbar></app-navbar>

      <main class="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        <h1 class="text-2xl font-semibold text-gray-900 mb-6">My Bids</h1>

        <!-- Loading state -->
        @if (isLoading()) {
          <div class="flex justify-center my-12">
            <div class="w-12 h-12 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
          </div>
        } @else if (auctions().length === 0) {
          <div class="bg-white shadow rounded-lg p-6 text-center">
            <h2 class="text-lg font-medium text-gray-900">No bids yet</h2>
            <p class="mt-1 text-gray-500">You haven't placed any bids on auctions</p>
            <div class="mt-4">
              <app-button routerLink="/auctions" variant="primary">Browse Auctions</app-button>
            </div>
          </div>
        } @else {
          <!-- Auctions Grid -->
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @for (auction of auctions(); track auction.id) {
              <div class="bg-white overflow-hidden shadow rounded-lg">
                @if (auction.images.length > 0) {
                  @if (getPrimaryImage(auction)) {
                    <img
                      [src]="getImageUrl(getPrimaryImage(auction)!.imageUrl)"
                      alt="{{ auction.title }}"
                      class="h-48 w-full object-cover"
                    />
                  } @else {
                    <img
                      [src]="getImageUrl(auction.images[0].imageUrl)"
                      alt="{{ auction.title }}"
                      class="h-48 w-full object-cover"
                    />
                  }
                } @else {
                  <div class="h-48 w-full bg-gray-200 flex items-center justify-center">
                    <span class="text-gray-500">No image</span>
                  </div>
                }

                <div class="p-4">
                  <div class="flex justify-between items-start">
                    <h3 class="text-lg font-medium text-gray-900 truncate">{{ auction.title }}</h3>
                    <span class="px-2 py-1 text-xs font-semibold rounded-full"
                          [class]="isAuctionActive(auction) ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'">
                      {{ isAuctionActive(auction) ? 'Active' : 'Ended' }}
                    </span>
                  </div>

                  <div class="mt-4 flex justify-between">
                    <div>
                      <p class="text-sm text-gray-500">Current bid</p>
                      <p class="text-lg font-semibold">{{ auction.currentPrice || auction.startingPrice | currency }}</p>
                    </div>
                    <div class="text-right">
                      <p class="text-sm text-gray-500">Ends in</p>
                      <p class="text-sm font-medium">{{ getTimeRemaining(auction.endDate) }}</p>
                    </div>
                  </div>

                  <div class="mt-4">
                    @if (isHighestBidder(auction)) {
                      <div class="bg-green-50 border border-green-200 rounded-md p-2 mb-2 flex items-center justify-center">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-green-500 mr-1" viewBox="0 0 20 20" fill="currentColor">
                          <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                        </svg>
                        <span class="text-green-700 text-sm font-medium">You are the highest bidder!</span>
                      </div>
                    }
                    <app-button
                      [routerLink]="['/auctions', auction.id]"
                      variant="primary"
                      class="w-full"
                    >
                      View Auction
                    </app-button>
                  </div>
                </div>
              </div>
            }
          </div>
        }
      </main>
    </div>
  `
})
export class MyBidsComponent implements OnInit {
  private auctionService = inject(AuctionService);
  private imageService = inject(ImageService);

  auctions = signal<Auction[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadMyBids();
  }

  loadMyBids(): void {
    this.isLoading.set(true);

    this.auctionService.getMyBids().subscribe({
      next: (auctions) => {
        this.auctions.set(auctions);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading bids', error);
        this.isLoading.set(false);
      }
    });
  }

  getPrimaryImage(auction: Auction) {
    return auction.images.find(img => img.isPrimary);
  }

  isAuctionActive(auction: Auction): boolean {
    const endDate = new Date(auction.endDate);
    return new Date() < endDate;
  }

  isHighestBidder(auction: Auction): boolean {
    // This is a placeholder. In a real app, you'd have a way to know if the current user is the highest bidder
    // For now, we'll just assume that if the auction has a current price, the user is the highest bidder
    return !!auction.currentPrice;
  }

  getTimeRemaining(endDate: string): string {
    const end = new Date(endDate);
    const now = new Date();

    if (now > end) {
      return 'Ended';
    }

    const diffTime = Math.abs(end.getTime() - now.getTime());
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
    const diffHours = Math.floor((diffTime % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

    if (diffDays > 0) {
      return `${diffDays} day${diffDays !== 1 ? 's' : ''}`;
    } else {
      return `${diffHours} hour${diffHours !== 1 ? 's' : ''}`;
    }
  }

  getImageUrl(imageUrl: string): string {
    return this.imageService.getImageUrl(imageUrl);
  }
}
