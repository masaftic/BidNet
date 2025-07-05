import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuctionService } from '../../../services/auction.service';
import { NavbarComponent } from '../../ui/navbar/navbar.component';
import { ButtonComponent } from '../../ui/button/button.component';
import { Auction } from '../../../models/auction.model';
import { AuthService } from '../../../services/auth.service';
import { switchMap } from 'rxjs';
import { AuctionBidsComponent } from '../../bids/auction-bids/auction-bids.component';
import { ImageService } from '../../../services/image.service';

@Component({
  selector: 'app-auction-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NavbarComponent,
    ButtonComponent,
    AuctionBidsComponent
  ],
  templateUrl: './auction-details.component.html',
})
export class AuctionDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private auctionService = inject(AuctionService);
  private authService = inject(AuthService);
  private imageService = inject(ImageService);

  auction = signal<Auction | null>(null);
  isLoading = signal(true);
  isDeleting = signal(false);
  errorMessage = signal<string | null>(null);
  selectedImage = signal<{ id: string, imageUrl: string, auctionId: string, isPrimary: boolean } | null>(null);

  apiBaseUrl = 'http://localhost:5000';

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (!id) {
          return [];
        }
        return this.auctionService.getAuctionById(id);
      })
    ).subscribe({
      next: (auction) => {
        this.auction.set(auction);
        this.isLoading.set(false);

        // Set the primary image if available
        const primaryImage = auction.images.find(img => img.isPrimary);
        if (primaryImage) {
          this.selectedImage.set(primaryImage);
        } else if (auction.images.length > 0) {
          this.selectedImage.set(auction.images[0]);
        }
      },
      error: (error) => {
        console.error('Error loading auction', error);
        this.isLoading.set(false);
        this.auction.set(null);
      }
    });
  }

  isAuctionActive(): boolean {
    if (!this.auction()) return false;
    const endDate = new Date(this.auction()!.endDate);
    return new Date() < endDate;
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  isCreator(): boolean {
    if (!this.auction() || !this.isLoggedIn()) return false;

    // Compare the current user ID with the auction creator ID
    const currentUser = this.authService.getUserFromStorage();
    return currentUser?.userId.toString() === this.auction()!.createdBy;
  }

  deleteAuction(): void {
    if (!this.auction() || !this.isCreator()) return;

    if (!confirm('Are you sure you want to delete this auction? This action cannot be undone.')) {
      return;
    }

    this.isDeleting.set(true);

    this.auctionService.deleteAuction(this.auction()!.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.router.navigate(['/auctions']);
      },
      error: (error) => {
        this.isDeleting.set(false);
        this.errorMessage.set(error.error?.message || 'There was an error deleting the auction. Please try again.');
      }
    });
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
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
      return `${diffDays} day${diffDays !== 1 ? 's' : ''} remaining`;
    } else {
      return `${diffHours} hour${diffHours !== 1 ? 's' : ''} remaining`;
    }
  }

  scrollToBidSection(): void {
    const element = document.getElementById('place-bid-section');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  getImageUrl(imageUrl: string): string {
    return this.imageService.getImageUrl(imageUrl);
  }
}
