export interface AuthTokenRequest {
  userName: string;
}

export interface AuthTokenResponse {
  accessToken: string;
  expiresInSeconds: number;
}
