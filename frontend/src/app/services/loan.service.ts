import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/api.config';
import { Loan } from '../models/loan';

@Injectable({ providedIn: 'root' })
export class LoanService {
  constructor(private readonly http: HttpClient) {}

  getLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(`${API_BASE_URL}/loans`);
  }
}
