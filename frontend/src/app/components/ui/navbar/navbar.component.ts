import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ButtonComponent } from '../button/button.component';
import { AuthService } from '../../../services/auth.service';
import { AsyncPipe } from '@angular/common';
import { User } from '../../../models/user.model';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, ButtonComponent],
  template: `
    <header class="bg-white shadow">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16">
          <div class="flex">
            <div class="flex-shrink-0 flex items-center">
              <a routerLink="/" class="text-2xl font-bold text-blue-600">
                BidNet
              </a>
            </div>

            @if (authService.currentUser$ | async) {
              <nav class="hidden sm:ml-6 sm:flex sm:space-x-8">
                <a
                  routerLink="/home"
                  routerLinkActive="border-blue-500 text-gray-900"
                  [routerLinkActiveOptions]="{exact: true}"
                  class="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
                >
                  Home
                </a>
                <a
                  routerLink="/auctions"
                  routerLinkActive="border-blue-500 text-gray-900"
                  class="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
                >
                  Auctions
                </a>
                <a
                  routerLink="/my-bids"
                  routerLinkActive="border-blue-500 text-gray-900"
                  class="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
                >
                  My Bids
                </a>
              </nav>
            }
          </div>

          <div class="flex items-center">
            @if (currentUser()) {
              <div class="flex items-center space-x-4">
                <span class="text-gray-700">
                  Welcome, {{currentUser()!.userName}}!
                </span>
                <div class="hidden md:block">
                  <app-button
                    variant="outline"
                    size="sm"
                    (clicked)="logout()"
                  >
                    Sign Out
                  </app-button>
                </div>
                <div class="block md:hidden">
                  <button
                    type="button"
                    class="p-2 rounded-md text-gray-400 hover:text-gray-500 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-500"
                    (click)="toggleMobileMenu()"
                  >
                    <span class="sr-only">Open menu</span>
                    <svg class="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                    </svg>
                  </button>
                </div>
              </div>
            } @else {
              <div class="flex space-x-2">
                <app-button
                  variant="outline"
                  size="sm"
                  routerLink="/login"
                >
                  Sign In
                </app-button>
                <app-button
                  variant="primary"
                  size="sm"
                  routerLink="/register"
                >
                  Sign Up
                </app-button>
              </div>
            }
          </div>
        </div>
      </div>

      <!-- Mobile menu, show/hide based on menu state -->
      @if (isMobileMenuOpen()) {
        <div class="sm:hidden">
          <div class="pt-2 pb-3 space-y-1">
            <a
              routerLink="/home"
              routerLinkActive="bg-blue-50 border-blue-500 text-blue-700"
              [routerLinkActiveOptions]="{exact: true}"
              class="border-transparent text-gray-500 hover:bg-gray-50 hover:border-gray-300 hover:text-gray-700 block pl-3 pr-4 py-2 border-l-4 text-base font-medium"
            >
              Home
            </a>
            <a
              routerLink="/auctions"
              routerLinkActive="bg-blue-50 border-blue-500 text-blue-700"
              class="border-transparent text-gray-500 hover:bg-gray-50 hover:border-gray-300 hover:text-gray-700 block pl-3 pr-4 py-2 border-l-4 text-base font-medium"
            >
              Auctions
            </a>
            <a
              routerLink="/my-bids"
              routerLinkActive="bg-blue-50 border-blue-500 text-blue-700"
              class="border-transparent text-gray-500 hover:bg-gray-50 hover:border-gray-300 hover:text-gray-700 block pl-3 pr-4 py-2 border-l-4 text-base font-medium"
            >
              My Bids
            </a>
            @if (currentUser()) {
              <div class="border-t border-gray-200 pt-4 pb-3">
                <div class="pl-3 pr-4 py-2">
                  <app-button
                    variant="outline"
                    size="sm"
                    (clicked)="logout()"
                    class="w-full"
                  >
                    Sign Out
                  </app-button>
                </div>
              </div>
            }
          </div>
        </div>
      }
    </header>
  `
})
export class NavbarComponent implements OnInit {
  authService = inject(AuthService);
  isMobileMenuOpen = signal(false);

  currentUser = signal<User | null>(null);

  ngOnInit(): void {
    this.authService.currentUser$.subscribe({
      next: (user) => {
        this.currentUser.set(user);
      }
    })
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update(state => !state);
  }

  logout(): void {
    this.authService.logout();
    this.isMobileMenuOpen.set(false);
  }
}
