import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { finalize } from 'rxjs';
import { AccountService } from '../../core/services/account.service';
import { TransactionResponse } from '../../core/models/transaction.model';

@Component({
  selector: 'app-statement',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule
  ],
  templateUrl: './statement.component.html',
  styleUrl: './statement.component.scss'
})
export class StatementComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private accountService = inject(AccountService);
  private router = inject(Router);

  accountId = '';
  transactions: TransactionResponse[] = [];
  totalCount = 0;
  pageIndex = 0;      // MatPaginator é zero-based
  pageSize = 10;

  isLoading = false;
  errorMessage: string | null = null;

  displayedColumns = ['occurredAt', 'type', 'amount', 'balanceAfter'];

  ngOnInit(): void {
    this.accountId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadStatement();
  }

  loadStatement(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.accountService.getStatement(this.accountId, this.pageIndex + 1, this.pageSize)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (result) => {
          this.transactions = result.items;
          this.totalCount = result.totalCount;
        },
        error: () => (this.errorMessage = 'Não foi possível carregar o extrato. Tente novamente.')
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadStatement();
  }

  goToNewTransaction(): void {
    this.router.navigate(['/accounts', this.accountId, 'new-transaction']);
  }
}