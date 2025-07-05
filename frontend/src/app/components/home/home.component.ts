import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';
import { AuctionService } from '../../services/auction.service';
import { Auction } from '../../models/auction.model';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, ButtonComponent],
  template: `
    <div class="min-h-screen bg-gray-100">
      <app-navbar></app-navbar>

      <main class="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-semibold">Welcome to BidNet</h2>
            <div class="flex space-x-2">
              <app-button routerLink="/auctions" variant="outline" size="sm">
                Browse Auctions
              </app-button>
              @if (isLoggedIn()) {
                <app-button routerLink="/auctions/create" variant="primary" size="sm">
                  Create Auction
                </app-button>
              }
            </div>
          </div>

          <p class="text-gray-600">
            Your premier auction platform. Start browsing auctions or create your own!
          </p>

          <div class="mt-8">
            <div class="flex justify-between items-center mb-4">
              <h3 class="text-lg font-medium">Featured Auctions</h3>
              <app-button routerLink="/auctions" variant="outline" size="sm">
                See All
              </app-button>
            </div>

            @if (isLoading()) {
              <div class="flex justify-center my-12">
                <div class="w-12 h-12 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
              </div>
            } @else if (featuredAuctions().length === 0) {
              <div class="bg-gray-50 rounded-lg p-6 text-center">
                <p class="text-gray-500">No featured auctions available at the moment</p>
              </div>
            } @else {
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                @for (auction of featuredAuctions(); track auction.id) {
                  <div class="border rounded-lg overflow-hidden shadow-sm">
                    <!-- Auction Image -->
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
                      <h4 class="font-medium truncate">{{ auction.title }}</h4>
                      <p class="text-sm text-gray-500 mt-1">Current bid: {{ auction.currentPrice || auction.startingPrice | currency }}</p>
                      <p class="text-sm text-gray-500">Ends in: {{ getTimeRemaining(auction.endDate) }}</p>
                      <app-button
                        size="sm"
                        variant="primary"
                        class="mt-3"
                        [routerLink]="['/auctions', auction.id]"
                      >
                        View Auction
                      </app-button>
                    </div>
                  </div>
                }
              </div>
            }
          </div>
        </div>
      </main>
    </div>
  `
})
export class HomeComponent implements OnInit {
  private auctionService = inject(AuctionService);
  private authService = inject(AuthService);
  private imageService = inject(ImageService);

  featuredAuctions = signal<Auction[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadFeaturedAuctions();
  }

  loadFeaturedAuctions(): void {
    this.isLoading.set(true);

    this.auctionService.getFeaturedAuctions().subscribe({
      next: (auctions) => {
        this.featuredAuctions.set(auctions);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading featured auctions', error);
        this.isLoading.set(false);
      }
    });
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  getPrimaryImage(auction: Auction) {
    return auction.images.find(img => img.isPrimary);
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

  logout(): void {
    this.authService.logout();
  }

  getImageUrl(imageUrl: string): string {
    return this.imageService.getImageUrl(imageUrl);
  }
}
