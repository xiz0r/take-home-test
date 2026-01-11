import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { finalize, map, shareReplay, tap } from 'rxjs/operators';

import { API_BASE_URL, API_TOKEN_PATH } from '../config/api.config';
import { AuthTokenRequest, AuthTokenResponse } from '../models/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userName = 'test-user';
  private readonly expirySkewMs = 30_000;

  private token: string | null = null;
  private tokenExpiresAt = 0;
  private inFlight$: Observable<string> | null = null;

  constructor(private readonly http: HttpClient) {}

  getAccessToken(): Observable<string> {
    if (this.token && Date.now() < this.tokenExpiresAt) {
      return of(this.token);
    }

    if (this.inFlight$) {
      return this.inFlight$;
    }

    const payload: AuthTokenRequest = { userName: this.userName };

    this.inFlight$ = this.http
      .post<AuthTokenResponse>(`${API_BASE_URL}${API_TOKEN_PATH}`, payload)
      .pipe(
        tap((response) => {
          const expiresInMs = Math.max(
            0,
            response.expiresInSeconds * 1000 - this.expirySkewMs,
          );
          this.token = response.accessToken;
          this.tokenExpiresAt = Date.now() + expiresInMs;
        }),
        map((response) => response.accessToken),
        finalize(() => {
          this.inFlight$ = null;
        }),
        shareReplay(1),
      );

    return this.inFlight$;
  }
}
