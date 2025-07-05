export interface User {
  id: string;
  userName: string;
  email: string;
  roles: string;
  emailConfirmed: boolean;
  phoneNumber: string;
  phoneNumberConfirmed: boolean;
  createdDate: string;
  lastLoginDate: string;

  // Wallet information
  balance: number;
  heldBalance: number;
  currency: string;

  // Activity stats
  totalAuctions: number;
  activeAuctions: number;
  totalBids: number;
  wonAuctions: number;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  TokenExpiration: string;
}

export interface AuthResponseWithUser extends AuthResponse, User {}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}
