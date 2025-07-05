import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuctionService } from '../../services/auction.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';
import { Auction, UpdateAuctionRequest } from '../../models/auction.model';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-edit-auction',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    NavbarComponent,
    ButtonComponent
  ],
  template: `
    <div class="min-h-screen bg-gray-100">
      <app-navbar></app-navbar>

      <main class="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        @if (isLoading()) {
          <div class="flex justify-center my-12">
            <div class="w-12 h-12 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
          </div>
        } @else if (auction()) {
          <div class="bg-white shadow rounded-lg p-6">
            <h1 class="text-2xl font-semibold mb-6">Edit Auction: {{ auction()!.title }}</h1>

            @if (errorMessage()) {
              <div class="bg-red-50 p-4 rounded-md mb-6">
                <p class="text-red-800">{{ errorMessage() }}</p>
              </div>
            }

            <form [formGroup]="auctionForm" (ngSubmit)="onSubmit()" class="space-y-6">
              <!-- Title -->
              <div>
                <label for="title" class="block text-sm font-medium text-gray-700">Title</label>
                <input
                  type="text"
                  id="title"
                  formControlName="title"
                  class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                />
                @if (auctionForm.get('title')?.invalid && (auctionForm.get('title')?.dirty || auctionForm.get('title')?.touched)) {
                  <p class="mt-1 text-sm text-red-600">Title is required</p>
                }
              </div>

              <!-- Description -->
              <div>
                <label for="description" class="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  id="description"
                  formControlName="description"
                  rows="4"
                  class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                ></textarea>
                @if (auctionForm.get('description')?.invalid && (auctionForm.get('description')?.dirty || auctionForm.get('description')?.touched)) {
                  <p class="mt-1 text-sm text-red-600">Description is required</p>
                }
              </div>

              <!-- Date Range -->
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label for="startDate" class="block text-sm font-medium text-gray-700">Start Date</label>
                  <input
                    type="datetime-local"
                    id="startDate"
                    formControlName="startDate"
                    class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  />
                  @if (auctionForm.get('startDate')?.invalid && (auctionForm.get('startDate')?.dirty || auctionForm.get('startDate')?.touched)) {
                    <p class="mt-1 text-sm text-red-600">Valid start date is required</p>
                  }
                </div>

                <div>
                  <label for="endDate" class="block text-sm font-medium text-gray-700">End Date</label>
                  <input
                    type="datetime-local"
                    id="endDate"
                    formControlName="endDate"
                    class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  />
                  @if (auctionForm.get('endDate')?.invalid && (auctionForm.get('endDate')?.dirty || auctionForm.get('endDate')?.touched)) {
                    <p class="mt-1 text-sm text-red-600">Valid end date is required</p>
                  }
                  @if (auctionForm.errors?.['endDateBeforeStart']) {
                    <p class="mt-1 text-sm text-red-600">End date must be after start date</p>
                  }
                </div>
              </div>

              <!-- Starting Price -->
              <div>
                <label for="startingPrice" class="block text-sm font-medium text-gray-700">Starting Price ($)</label>
                <input
                  type="number"
                  id="startingPrice"
                  formControlName="startingPrice"
                  min="0.01"
                  step="0.01"
                  class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                />
                @if (auctionForm.get('startingPrice')?.invalid && (auctionForm.get('startingPrice')?.dirty || auctionForm.get('startingPrice')?.touched)) {
                  @if (auctionForm.get('startingPrice')?.errors?.['required']) {
                    <p class="mt-1 text-sm text-red-600">Starting price is required</p>
                  } @else if (auctionForm.get('startingPrice')?.errors?.['min']) {
                    <p class="mt-1 text-sm text-red-600">Starting price must be at least $0.01</p>
                  }
                }
              </div>

              <div class="flex justify-end space-x-4 pt-4">
                <app-button
                  type="button"
                  [routerLink]="['/auctions', auction()!.id]"
                  variant="outline"
                >
                  Cancel
                </app-button>

                <app-button
                  type="submit"
                  [disabled]="auctionForm.invalid || isSubmitting()"
                  [loading]="isSubmitting()"
                  variant="primary"
                >
                  Save Changes
                </app-button>
              </div>
            </form>
          </div>
        } @else {
          <div class="bg-white shadow rounded-lg p-6 text-center">
            <h2 class="text-lg font-medium text-gray-900">Auction not found</h2>
            <p class="mt-2">The auction you're trying to edit doesn't exist or has been removed.</p>
            <div class="mt-4">
              <app-button routerLink="/auctions" variant="outline">Back to Auctions</app-button>
            </div>
          </div>
        }
      </main>
    </div>
  `
})
export class EditAuctionComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private auctionService = inject(AuctionService);

  auction = signal<Auction | null>(null);
  isLoading = signal(true);
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  auctionForm: FormGroup;

  constructor() {
    this.auctionForm = this.fb.group({
      title: ['', [Validators.required]],
      description: ['', [Validators.required]],
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]],
      startingPrice: ['', [Validators.required, Validators.min(0.01)]]
    }, { validators: this.dateRangeValidator });
  }

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

        // Format dates for datetime-local input
        const startDate = this.formatDateForInput(new Date(auction.startDate));
        const endDate = this.formatDateForInput(new Date(auction.endDate));

        // Populate form
        this.auctionForm.patchValue({
          title: auction.title,
          description: auction.description,
          startDate: startDate,
          endDate: endDate,
          startingPrice: auction.startingPrice
        });
      },
      error: (error) => {
        console.error('Error loading auction', error);
        this.isLoading.set(false);
        this.auction.set(null);
      }
    });
  }

  onSubmit(): void {
    if (this.auctionForm.invalid || !this.auction()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const updatedAuction: UpdateAuctionRequest = {
      title: this.auctionForm.value.title,
      description: this.auctionForm.value.description,
      startDate: new Date(this.auctionForm.value.startDate).toISOString(),
      endDate: new Date(this.auctionForm.value.endDate).toISOString(),
      startingPrice: this.auctionForm.value.startingPrice
    };

    this.auctionService.updateAuction(this.auction()!.id, updatedAuction).subscribe({
      next: (updatedAuction) => {
        this.isSubmitting.set(false);
        this.router.navigate(['/auctions', updatedAuction.id]);
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error.error?.message || 'There was an error updating your auction. Please try again.');
      }
    });
  }

  dateRangeValidator(group: FormGroup): { [key: string]: boolean } | null {
    const startDate = group.get('startDate')?.value;
    const endDate = group.get('endDate')?.value;

    if (!startDate || !endDate) {
      return null;
    }

    if (new Date(endDate) <= new Date(startDate)) {
      return { 'endDateBeforeStart': true };
    }

    return null;
  }

  formatDateForInput(date: Date): string {
    // Format date as YYYY-MM-DDThh:mm
    return date.toISOString().substring(0, 16);
  }
}
