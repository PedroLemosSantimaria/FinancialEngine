import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { fakeAsync, tick } from '@angular/core/testing';
import { TransactionFormComponent } from './transaction-form.component';
import { TransactionService } from '../../core/services/transaction.service';

describe('TransactionFormComponent', () => {
  let component: TransactionFormComponent;
  let fixture: ComponentFixture<TransactionFormComponent>;

  let transactionServiceSpy: {
    process: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    transactionServiceSpy = {
      process: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [TransactionFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),

        {
          provide: TransactionService,
          useValue: transactionServiceSpy
        },

        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: () => 'acc-1'
              }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TransactionFormComponent);
    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should be invalid when amount is empty', () => {
    component.form.controls.amount.setValue(null);

    expect(component.form.valid).toBe(false);
  });

  it('should be invalid when amount is zero or negative', () => {
    component.form.controls.amount.setValue(0);

    expect(component.form.valid).toBe(false);
  });

  it('should be valid with a positive amount and a type', () => {
    component.form.controls.amount.setValue(50);
    component.form.controls.type.setValue('Credit');

    expect(component.form.valid).toBe(true);
  });

  it('should not call the service when the form is invalid', () => {
    component.form.controls.amount.setValue(null);

    component.submit();

    expect(transactionServiceSpy.process).not.toHaveBeenCalled();
  });

  it('should call the service with the account id when the form is valid', () => {
    transactionServiceSpy.process.mockReturnValue(
      of({
        eventId: 'evt-1',
        accountId: 'acc-1',
        type: 'Credit',
        amount: 50,
        balanceAfter: 150,
        occurredAt: new Date().toISOString()
      })
    );

    component.form.controls.amount.setValue(50);
    component.form.controls.type.setValue('Credit');

    component.submit();

    expect(transactionServiceSpy.process).toHaveBeenCalledWith(
      expect.objectContaining({
        accountId: 'acc-1',
        amount: 50,
        type: 'Credit'
      })
    );
  });

  it('should show a duplicate-event message on 409', fakeAsync(() => {
  const snackBar = TestBed.inject(MatSnackBar);
  const snackBarSpy = vi.spyOn(snackBar, 'open');

  transactionServiceSpy.process.mockReturnValue(
    throwError(() => new HttpErrorResponse({ status: 409 }))
  );

  component.form.controls.amount.setValue(50);
  component.form.controls.type.setValue('Credit');

  component.submit();

  tick();

  expect(snackBarSpy).toHaveBeenCalledWith(
    'Este evento já foi processado anteriormente (duplicado).',
    'Fechar',
    { duration: 6000 }
  );
}));

  it('should show an insufficient-balance message on 422', fakeAsync(() => {
  const snackBar = TestBed.inject(MatSnackBar);
  const snackBarSpy = vi.spyOn(snackBar, 'open');

  transactionServiceSpy.process.mockReturnValue(
    throwError(
      () =>
        new HttpErrorResponse({
          status: 422,
          error: {
            detail: 'Saldo insuficiente na conta X'
          }
        })
    )
  );

  component.form.controls.amount.setValue(50);
  component.form.controls.type.setValue('Credit');

  component.submit();

  tick();

  expect(snackBarSpy).toHaveBeenCalledWith(
    'Saldo insuficiente na conta X',
    'Fechar',
    { duration: 6000 }
  );
}));
});

