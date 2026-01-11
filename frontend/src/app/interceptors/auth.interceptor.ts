import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { switchMap } from 'rxjs';

import { API_BASE_URL, API_TOKEN_PATH } from '../config/api.config';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenUrl = `${API_BASE_URL}${API_TOKEN_PATH}`;
  if (req.url === tokenUrl) {
    return next(req);
  }

  if (!req.url.startsWith(API_BASE_URL)) {
    return next(req);
  }

  const authService = inject(AuthService);

  return authService.getAccessToken().pipe(
    switchMap((token) =>
      next(
        req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        }),
      ),
    ),
  );
};
