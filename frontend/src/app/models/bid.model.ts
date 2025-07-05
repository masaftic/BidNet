export interface Bid {
  id: string;
  auctionId: string;
  userId: string;
  userName: string;
  amount: number;
  isWinning: boolean;
  createdAt: string;
}

export interface PlaceBidRequest {
  auctionId: string;
  amount: number;
}

export interface AuctionBidsResponse {
  auctionId: string;
  startingPrice: number;
  currentPrice?: number;
  winningBid?: Bid;
  bidHistory: Bid[];
}

export interface UserBidsResponse {
  userId: string;
  bids: Bid[];
}
