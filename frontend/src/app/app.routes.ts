import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./components/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'home'
  },
  {
    path: 'home',
    loadComponent: () => import('./components/home/home.component').then(m => m.HomeComponent),
    canActivate: [authGuard]
  },
  // Auction routes
  {
    path: 'auctions',
    loadComponent: () => import('./components/auctions/auctions/auctions.component').then(m => m.AuctionsComponent)
  },
  {
    path: 'auctions/create',
    loadComponent: () => import('./components/auctions/create-auction.component').then(m => m.CreateAuctionComponent),
    canActivate: [authGuard]
  },
  {
    path: 'auctions/:id',
    loadComponent: () => import('./components/auctions/auction-details/auction-details.component').then(m => m.AuctionDetailsComponent)
  },
  {
    path: 'auctions/:id/edit',
    loadComponent: () => import('./components/auctions/edit-auction.component').then(m => m.EditAuctionComponent),
    canActivate: [authGuard]
  },
  {
    path: 'my-bids',
    loadComponent: () => import('./components/bids/my-bids/my-bids.component').then(m => m.MyBidsComponent),
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: 'home'
  }
];
