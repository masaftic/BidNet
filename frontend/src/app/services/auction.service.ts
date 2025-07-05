import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { Auction, AuctionFilters, CreateAuctionRequest, CreateAuctionWithImagesRequest, PaginatedResponse, UpdateAuctionRequest } from '../models/auction.model';

@Injectable({
  providedIn: 'root'
})
export class AuctionService {
  private http = inject(HttpClient);
  private auctionsUrl = 'http://localhost:5000/api/auctions';

  // Use signals for maintaining local state
  activeAuctions = signal<Auction[]>([]);
  featuredAuctions = signal<Auction[]>([]);

  getAuctions(pageIndex: number = 0, pageSize: number = 10, filters?: AuctionFilters): Observable<PaginatedResponse<Auction>> {
    let params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    if (filters) {
      if (filters.search) params = params.set('search', filters.search);
      if (filters.minPrice) params = params.set('minPrice', filters.minPrice.toString());
      if (filters.maxPrice) params = params.set('maxPrice', filters.maxPrice.toString());
      if (filters.onlyActive) params = params.set('onlyActive', filters.onlyActive.toString());
    }

    return this.http.get<PaginatedResponse<Auction>>(this.auctionsUrl, { params });
  }

  getFeaturedAuctions(): Observable<Auction[]> {
    return this.http.get<Auction[]>(`${this.auctionsUrl}/featured`)
      .pipe(tap(auctions => this.featuredAuctions.set(auctions)));
  }

  getActiveAuctions(): Observable<Auction[]> {
    return this.http.get<Auction[]>(`${this.auctionsUrl}/active`)
      .pipe(tap(auctions => this.activeAuctions.set(auctions)));
  }

  getAuctionById(id: string): Observable<Auction> {
    return this.http.get<Auction>(`${this.auctionsUrl}/${id}`);
  }

  createAuction(auction: CreateAuctionRequest): Observable<Auction> {
    return this.http.post<Auction>(this.auctionsUrl, auction);
  }

  createAuctionWithImages(auction: CreateAuctionWithImagesRequest, images: File[]): Observable<Auction> {
    const formData = new FormData();

    // Add auction data
    formData.append('title', auction.title);
    formData.append('description', auction.description);
    formData.append('startDate', auction.startDate);
    formData.append('endDate', auction.endDate);
    formData.append('startingPrice', auction.startingPrice.toString());

    if (auction.primaryImageIndex !== undefined) {
      formData.append('primaryImageIndex', auction.primaryImageIndex.toString());
    }

    // Add images
    for (let i = 0; i < images.length; i++) {
      formData.append('images', images[i]);
    }

    return this.http.post<Auction>(`${this.auctionsUrl}/with-images`, formData);
  }

  updateAuction(id: string, auction: UpdateAuctionRequest): Observable<Auction> {
    return this.http.put<Auction>(`${this.auctionsUrl}/${id}`, auction);
  }

  deleteAuction(id: string): Observable<void> {
    return this.http.delete<void>(`${this.auctionsUrl}/${id}`);
  }

  placeBid(auctionId: string, amount: number): Observable<Auction> {
    return this.http.post<Auction>(`${this.auctionsUrl}/${auctionId}/bids`, { amount });
  }

  getMyBids(): Observable<Auction[]> {
    return this.http.get<Auction[]>(`${this.auctionsUrl}/my-bids`);
  }
}
