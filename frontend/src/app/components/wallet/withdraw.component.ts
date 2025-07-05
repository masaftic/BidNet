import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { WalletService } from '../../services/wallet.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';
import { WithdrawalRequest } from '../../models/wallet.model';

@Component({
  selector: 'app-withdraw',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    NavbarComponent,
    ButtonComponent
  ],
  templateUrl: './withdraw.component.html'
})
export class WithdrawComponent {
  private walletService = inject(WalletService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  
  withdrawForm: FormGroup;
  
  withdrawalMethods = [
    { value: 'BankTransfer', label: 'Bank Transfer' },
    { value: 'PayPal', label: 'PayPal' },
    { value: 'Check', label: 'Check' }
  ];
  
  constructor() {
    this.withdrawForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(1)]],
      withdrawalMethod: ['BankTransfer', [Validators.required]],
      accountNumber: ['', [Validators.required]],
      routingNumber: ['', [Validators.required]],
      accountHolderName: ['', [Validators.required]]
    });
  }
  
  onSubmit(): void {
    if (this.withdrawForm.invalid) {
      this.markFormGroupTouched();
      return;
    }
    
    const formValue = this.withdrawForm.value;
    const withdrawRequest: WithdrawalRequest = {
      amount: formValue.amount,
      withdrawalMethod: formValue.withdrawalMethod,
      withdrawalDetails: JSON.stringify({
        accountNumber: formValue.accountNumber,
        routingNumber: formValue.routingNumber,
        accountHolderName: formValue.accountHolderName
      })
    };
    
    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    
    this.walletService.withdraw(withdrawRequest).subscribe({
      next: (wallet) => {
        this.isSubmitting.set(false);
        this.successMessage.set(`Withdrawal of $${formValue.amount} has been initiated!`);
        
        // Redirect to wallet after 2 seconds
        setTimeout(() => {
          this.router.navigate(['/wallet']);
        }, 2000);
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error.error?.message || 'Failed to process withdrawal. Please try again.');
      }
    });
  }
  
  private markFormGroupTouched(): void {
    Object.keys(this.withdrawForm.controls).forEach(key => {
      const control = this.withdrawForm.get(key);
      control?.markAsTouched();
    });
  }
  
  isFieldInvalid(fieldName: string): boolean {
    const field = this.withdrawForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
  
  getFieldError(fieldName: string): string {
    const field = this.withdrawForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['min']) return `Minimum amount is $1`;
    }
    return '';
  }
}
