import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from '../ui/navbar/navbar.component';
import { ButtonComponent } from '../ui/button/button.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, ButtonComponent],
  template: `
    <div class="min-h-screen bg-gray-100">
      <app-navbar></app-navbar>

      <main class="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-xl font-semibold mb-4">Welcome to BidNet</h2>
          <p class="text-gray-600">
            Your premier auction platform. Start browsing auctions or create your own!
          </p>

          <div class="mt-6">
            <h3 class="text-lg font-medium mb-3">Featured Auctions</h3>
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              <div class="border rounded-lg p-4 shadow-sm">
                <h4 class="font-medium">Vintage Watch Collection</h4>
                <p class="text-sm text-gray-500 mt-1">Current bid: $320</p>
                <p class="text-sm text-gray-500">Ends in: 2 days</p>
                <app-button size="sm" variant="primary" class="mt-3">
                  Place Bid
                </app-button>
              </div>
              <div class="border rounded-lg p-4 shadow-sm">
                <h4 class="font-medium">Modern Art Painting</h4>
                <p class="text-sm text-gray-500 mt-1">Current bid: $450</p>
                <p class="text-sm text-gray-500">Ends in: 1 day</p>
                <app-button size="sm" variant="primary" class="mt-3">
                  Place Bid
                </app-button>
              </div>
              <div class="border rounded-lg p-4 shadow-sm">
                <h4 class="font-medium">Gaming Console</h4>
                <p class="text-sm text-gray-500 mt-1">Current bid: $210</p>
                <p class="text-sm text-gray-500">Ends in: 6 hours</p>
                <app-button size="sm" variant="primary" class="mt-3">
                  Place Bid
                </app-button>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  `
})
export class HomeComponent {
  constructor(public authService: AuthService) {}

  logout(): void {
    this.authService.logout();
  }
}
