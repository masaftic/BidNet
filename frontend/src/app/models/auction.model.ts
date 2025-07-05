export interface AuctionImage {
  id: string;
  auctionId: string;
  imageUrl: string;
  isPrimary: boolean;
}

export interface Auction {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  startingPrice: number;
  currentPrice?: number;
  createdBy: string;
  images: AuctionImage[];
}

export interface CreateAuctionRequest {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  startingPrice: number;
}

export interface UpdateAuctionRequest {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  startingPrice: number;
}

export interface CreateAuctionWithImagesRequest {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  startingPrice: number;
  primaryImageIndex?: number;
}

export interface AuctionFilters {
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  onlyActive?: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
