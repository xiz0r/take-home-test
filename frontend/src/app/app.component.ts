import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { LoanService } from './services/loan.service';
import { Loan } from './models/loan';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  displayedColumns: string[] = [
    'amount',
    'currentBalance',
    'applicantName',
    'status',
  ];
  loans: Loan[] = [];

  constructor(private readonly loanService: LoanService) {}

  ngOnInit(): void {
    this.loanService.getLoans()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (loans) => {
          this.loans = loans;
        },
        error: (error) => {
          console.error('Failed to load loans', error);
        },
      });
  }
}
