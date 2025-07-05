import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuctionBidsResponse, Bid, PlaceBidRequest, UserBidsResponse } from '../models/bid.model';

@Injectable({
  providedIn: 'root'
})
export class BidService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/bids';

  // Use signals for maintaining bid state
  userBids = signal<UserBidsResponse | null>(null);

  /**
   * Place a bid on an auction
   */
  placeBid(bid: PlaceBidRequest): Observable<Bid> {
    return this.http.post<Bid>(this.apiUrl, bid);
  }

  /**
   * Get bid history for a specific auction
   */
  getAuctionBids(auctionId: string): Observable<AuctionBidsResponse> {
    return this.http.get<AuctionBidsResponse>(`${this.apiUrl}/auctions/${auctionId}`);
  }

  /**
   * Get all bids placed by the current user
   */
  getUserBids(): Observable<UserBidsResponse> {
    return this.http.get<UserBidsResponse>(`${this.apiUrl}/mine`)
      .pipe(
        tap(response => {
          this.userBids.set(response);
        })
      );
  }
}
