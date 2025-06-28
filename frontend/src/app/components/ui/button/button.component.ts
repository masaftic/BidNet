import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';

export type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'danger' | 'success';
export type ButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      [type]="type()"
      [class]="
        'flex items-center justify-center rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 ' +
        sizeClass() +
        ' ' +
        variantClass() +
        ' ' +
        (disabled() || loading() ? 'opacity-50 cursor-not-allowed' : '')
      "
      [disabled]="disabled() || loading()"
      (click)="onClick($event)"
    >
      @if (loading()) {
        <svg class="animate-spin -ml-1 mr-2 h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg>
      }
      <ng-content />
    </button>
  `,
  styles: `
    :host {
      display: inline-block;
    }
  `
})
export class ButtonComponent {
  variant = input<ButtonVariant>('primary');
  size = input<ButtonSize>('md');
  disabled = input<boolean>(false);
  loading = input<boolean>(false);
  type = input<'button' | 'submit' | 'reset'>('button');
  clicked = output<MouseEvent>();

  onClick(event: MouseEvent): void {
    if (!this.disabled() && !this.loading()) {
      this.clicked.emit(event);
    }
  }

  variantClass() {
    const classes: Record<ButtonVariant, string> = {
      primary: 'bg-blue-600 hover:bg-blue-700 text-white focus:ring-blue-500',
      secondary: 'bg-gray-600 hover:bg-gray-700 text-white focus:ring-gray-500',
      outline: 'border border-gray-300 bg-transparent hover:bg-gray-50 text-gray-700 focus:ring-blue-500',
      danger: 'bg-red-600 hover:bg-red-700 text-white focus:ring-red-500',
      success: 'bg-green-600 hover:bg-green-700 text-white focus:ring-green-500'
    };

    return classes[this.variant()];
  }

  sizeClass() {
    const classes: Record<ButtonSize, string> = {
      sm: 'text-xs px-2.5 py-1.5',
      md: 'text-sm px-4 py-2',
      lg: 'text-base px-6 py-3'
    };

    return classes[this.size()];
  }
}
