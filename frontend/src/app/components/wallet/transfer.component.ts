import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { WalletService } from '../../services/wallet.service';
import { TransferFundsRequest } from '../../models/wallet.model';

@Component({
  selector: 'app-transfer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container mx-auto px-4 py-8">
      <div class="max-w-md mx-auto bg-white rounded-lg shadow-md p-6">
        <h2 class="text-2xl font-bold text-gray-800 mb-6">Transfer Funds</h2>

        <form [formGroup]="transferForm" (ngSubmit)="onSubmit()">
          <div class="mb-4">
            <label for="recipientId" class="block text-sm font-medium text-gray-700 mb-2">
              Recipient ID
            </label>
            <input
              type="text"
              id="recipientId"
              formControlName="recipientId"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Enter recipient's user ID"
            />
            <div *ngIf="transferForm.get('recipientId')?.invalid && transferForm.get('recipientId')?.touched"
                 class="text-red-500 text-sm mt-1">
              <span *ngIf="transferForm.get('recipientId')?.errors?.['required']">
                Recipient ID is required
              </span>
            </div>
          </div>

          <div class="mb-4">
            <label for="amount" class="block text-sm font-medium text-gray-700 mb-2">
              Amount ($)
            </label>
            <input
              type="number"
              id="amount"
              formControlName="amount"
              step="0.01"
              min="0.01"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="0.00"
            />
            <div *ngIf="transferForm.get('amount')?.invalid && transferForm.get('amount')?.touched"
                 class="text-red-500 text-sm mt-1">
              <span *ngIf="transferForm.get('amount')?.errors?.['required']">
                Amount is required
              </span>
              <span *ngIf="transferForm.get('amount')?.errors?.['min']">
                Amount must be at least $0.01
              </span>
            </div>
          </div>

          <div class="mb-6">
            <label for="description" class="block text-sm font-medium text-gray-700 mb-2">
              Description (Optional)
            </label>
            <textarea
              id="description"
              formControlName="description"
              rows="3"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Add a description for this transfer..."
            ></textarea>
          </div>

          <div *ngIf="errorMessage" class="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
            {{ errorMessage }}
          </div>

          <div *ngIf="successMessage" class="mb-4 p-3 bg-green-100 border border-green-400 text-green-700 rounded">
            {{ successMessage }}
          </div>

          <div class="flex gap-4">
            <button
              type="submit"
              [disabled]="transferForm.invalid || isLoading"
              class="flex-1 bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span *ngIf="!isLoading">Transfer Funds</span>
              <span *ngIf="isLoading">Processing...</span>
            </button>

            <button
              type="button"
              (click)="onCancel()"
              class="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-md hover:bg-gray-400"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class TransferComponent {
  private fb = inject(FormBuilder);
  private walletService = inject(WalletService);
  private router = inject(Router);

  transferForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor() {
    this.transferForm = this.fb.group({
      recipientId: ['', [Validators.required]],
      amount: ['', [Validators.required, Validators.min(0.01)]],
      description: ['']
    });
  }

  onSubmit(): void {
    if (this.transferForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      const transferRequest: TransferFundsRequest = {
        recipientId: this.transferForm.value.recipientId,
        amount: this.transferForm.value.amount,
        description: this.transferForm.value.description || ''
      };

      this.walletService.transferFunds(transferRequest).subscribe({
        next: (wallet) => {
          this.isLoading = false;
          this.successMessage = `Successfully transferred $${transferRequest.amount} to user ${transferRequest.recipientId}`;

          // Reset form after successful transfer
          setTimeout(() => {
            this.transferForm.reset();
            this.successMessage = '';
          }, 3000);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Failed to transfer funds. Please try again.';
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/wallet']);
  }
}
