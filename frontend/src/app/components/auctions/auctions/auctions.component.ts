import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuctionService } from '../../../services/auction.service';
import { NavbarComponent } from '../../ui/navbar/navbar.component';
import { ButtonComponent } from '../../ui/button/button.component';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Auction, AuctionFilters } from '../../../models/auction.model';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { ImageService } from '../../../services/image.service';

@Component({
  selector: 'app-auctions',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NavbarComponent,
    ButtonComponent,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: 'auctions.component.html',
})
export class AuctionsComponent implements OnInit {
  private auctionService = inject(AuctionService);
  private imageService = inject(ImageService);

  auctions = signal<Auction[]>([]);
  isLoading = signal(true);
  searchControl = new FormControl('');

  // Pagination
  currentPage = signal(0);
  pageSize = signal(9);
  totalItems = signal(0);
  totalPages = signal(0);

  // Filters
  filters: AuctionFilters = {
    search: '',
    onlyActive: true
  };

  ngOnInit(): void {
    // Setup search debounce
    this.searchControl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(value => {
      this.filters.search = value || '';
      this.currentPage.set(0);
      this.loadAuctions();
    });

    this.loadAuctions();
  }

  loadAuctions(): void {
    this.isLoading.set(true);
    this.auctionService.getAuctions(
      this.currentPage(),
      this.pageSize(),
      this.filters
    ).subscribe({
      next: (response) => {
        console.log('Auctions loaded:', response.items);
        this.auctions.set(response.items);
        this.totalItems.set(response.totalCount);
        this.totalPages.set(Math.ceil(response.totalCount / this.pageSize()));
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading auctions', error);
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.currentPage.set(0);
    this.loadAuctions();
  }

  nextPage(): void {
    if (this.currentPage() + 1 < this.totalPages()) {
      this.currentPage.update(page => page + 1);
      this.loadAuctions();
    }
  }

  previousPage(): void {
    if (this.currentPage() > 0) {
      this.currentPage.update(page => page - 1);
      this.loadAuctions();
    }
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

  getPrimaryImage(auction: Auction) {
    const primaryImage = auction.images.find(img => img.isPrimary);
    return primaryImage;
  }

  getImageUrl(imageUrl: string): string {
    return this.imageService.getImageUrl(imageUrl);
  }
}
