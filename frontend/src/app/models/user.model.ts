export interface User {
  userId: number;
  userName: string;
  email: string;
  roles: string[];
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
