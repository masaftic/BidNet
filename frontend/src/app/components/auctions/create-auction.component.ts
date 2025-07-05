import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuctionService } from '../../services/auction.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';
import { CreateAuctionRequest } from '../../models/auction.model';
import { signal } from '@angular/core';

@Component({
  selector: 'app-create-auction',
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
        <div class="bg-white shadow rounded-lg p-6">
          <h1 class="text-2xl font-semibold mb-6">Create New Auction</h1>

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
                placeholder="Enter auction title"
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
                placeholder="Describe your auction item"
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
                placeholder="0.00"
              />
              @if (auctionForm.get('startingPrice')?.invalid && (auctionForm.get('startingPrice')?.dirty || auctionForm.get('startingPrice')?.touched)) {
                @if (auctionForm.get('startingPrice')?.errors?.['required']) {
                  <p class="mt-1 text-sm text-red-600">Starting price is required</p>
                } @else if (auctionForm.get('startingPrice')?.errors?.['min']) {
                  <p class="mt-1 text-sm text-red-600">Starting price must be at least $0.01</p>
                }
              }
            </div>

            <!-- Images (Stretch Goal) -->
            <div>
              <label class="block text-sm font-medium text-gray-700">Images</label>
              <p class="text-xs text-gray-500 mb-2">Image upload functionality coming soon!</p>
              <div class="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-gray-300 border-dashed rounded-md">
                <div class="space-y-1 text-center">
                  <svg class="mx-auto h-12 w-12 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48">
                    <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                  </svg>
                  <p class="text-sm text-gray-600">Upload images placeholder</p>
                </div>
              </div>
            </div>

            <div class="flex justify-end space-x-4 pt-4">
              <app-button
                type="button"
                routerLink="/auctions"
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
                Create Auction
              </app-button>
            </div>
          </form>
        </div>
      </main>
    </div>
  `
})
export class CreateAuctionComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auctionService = inject(AuctionService);
  private router = inject(Router);

  auctionForm: FormGroup;
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

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
    // Set default start date to now
    const now = new Date();
    const startDate = this.formatDateForInput(now);

    // Set default end date to 7 days from now
    const endDate = new Date();
    endDate.setDate(now.getDate() + 7);

    this.auctionForm.patchValue({
      startDate: startDate,
      endDate: this.formatDateForInput(endDate)
    });
  }

  onSubmit(): void {
    if (this.auctionForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const auction: CreateAuctionRequest = {
      title: this.auctionForm.value.title,
      description: this.auctionForm.value.description,
      startDate: new Date(this.auctionForm.value.startDate).toISOString(),
      endDate: new Date(this.auctionForm.value.endDate).toISOString(),
      startingPrice: this.auctionForm.value.startingPrice
    };

    this.auctionService.createAuction(auction).subscribe({
      next: (createdAuction) => {
        this.isSubmitting.set(false);
        this.router.navigate(['/auctions', createdAuction.id]);
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error.error?.message || 'There was an error creating your auction. Please try again.');
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
