import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { WalletService } from '../../services/wallet.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';
import { DepositRequest } from '../../models/wallet.model';

@Component({
  selector: 'app-deposit',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    NavbarComponent,
    ButtonComponent
  ],
  templateUrl: './deposit.component.html'
})
export class DepositComponent {
  private walletService = inject(WalletService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  
  depositForm: FormGroup;
  
  paymentMethods = [
    { value: 'CreditCard', label: 'Credit Card' },
    { value: 'DebitCard', label: 'Debit Card' },
    { value: 'BankTransfer', label: 'Bank Transfer' },
    { value: 'PayPal', label: 'PayPal' }
  ];
  
  constructor() {
    this.depositForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(1), Validators.max(10000)]],
      paymentMethod: ['CreditCard', [Validators.required]],
      cardNumber: ['', [Validators.required, Validators.pattern(/^\d{16}$/)]],
      expiryDate: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]],
      cvv: ['', [Validators.required, Validators.pattern(/^\d{3,4}$/)]],
      cardholderName: ['', [Validators.required]]
    });
  }
  
  onSubmit(): void {
    if (this.depositForm.invalid) {
      this.markFormGroupTouched();
      return;
    }
    
    const formValue = this.depositForm.value;
    const depositRequest: DepositRequest = {
      amount: formValue.amount,
      paymentMethod: formValue.paymentMethod,
      paymentDetails: JSON.stringify({
        cardNumber: formValue.cardNumber,
        expiryDate: formValue.expiryDate,
        cvv: formValue.cvv,
        cardholderName: formValue.cardholderName
      })
    };
    
    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    
    this.walletService.deposit(depositRequest).subscribe({
      next: (wallet) => {
        this.isSubmitting.set(false);
        this.successMessage.set(`Successfully deposited $${formValue.amount}!`);
        
        // Redirect to wallet after 2 seconds
        setTimeout(() => {
          this.router.navigate(['/wallet']);
        }, 2000);
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error.error?.message || 'Failed to process deposit. Please try again.');
      }
    });
  }
  
  private markFormGroupTouched(): void {
    Object.keys(this.depositForm.controls).forEach(key => {
      const control = this.depositForm.get(key);
      control?.markAsTouched();
    });
  }
  
  isFieldInvalid(fieldName: string): boolean {
    const field = this.depositForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
  
  getFieldError(fieldName: string): string {
    const field = this.depositForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['min']) return `Minimum amount is $1`;
      if (field.errors['max']) return `Maximum amount is $10,000`;
      if (field.errors['pattern']) {
        switch (fieldName) {
          case 'cardNumber': return 'Card number must be 16 digits';
          case 'expiryDate': return 'Expiry date must be in MM/YY format';
          case 'cvv': return 'CVV must be 3-4 digits';
          default: return 'Invalid format';
        }
      }
    }
    return '';
  }
}
