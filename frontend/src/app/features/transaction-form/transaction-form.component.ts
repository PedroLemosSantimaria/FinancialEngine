import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TransactionService } from '../../core/services/transaction.service';
import { ProblemDetails } from '../../core/models/problem-details.model';

@Component({
  selector: 'app-transaction-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './transaction-form.component.html',
  styleUrl: './transaction-form.component.scss'
})
export class TransactionFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private transactionService = inject(TransactionService);
  private snackBar = inject(MatSnackBar);

  accountId = '';
  isSubmitting = false;

  form = this.fb.nonNullable.group({
    type: ['Credit' as 'Credit' | 'Debit', Validators.required],
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]]
  });

  ngOnInit(): void {
    this.accountId = this.route.snapshot.paramMap.get('id') ?? '';
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    const { type, amount } = this.form.getRawValue();

    this.isSubmitting = true;

    this.transactionService.process({
      eventId: crypto.randomUUID(),
      accountId: this.accountId,
      type: type,
      amount: amount!,
      occurredAt: new Date().toISOString()
    }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.snackBar.open('Transação registrada com sucesso!', 'Fechar', { duration: 4000 });
        this.router.navigate(['/accounts', this.accountId, 'statement']);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.snackBar.open(this.resolveErrorMessage(err), 'Fechar', { duration: 6000 });
      }
    });
  }

  private resolveErrorMessage(err: HttpErrorResponse): string {
    const problem = err.error as ProblemDetails | undefined;

    if (err.status === 409) {
      return 'Este evento já foi processado anteriormente (duplicado).';
    }
    if (err.status === 422) {
      return problem?.detail ?? 'Saldo insuficiente para esta operação.';
    }
    if (err.status === 404) {
      return 'Conta não encontrada.';
    }
    if (err.status === 400) {
      return problem?.detail ?? 'Dados inválidos. Verifique o formulário.';
    }
    return 'Erro de comunicação com o servidor. Tente novamente.';
  }
}