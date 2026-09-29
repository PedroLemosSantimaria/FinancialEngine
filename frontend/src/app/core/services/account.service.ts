import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Account } from '../models/account.model';
import { PagedResult, TransactionResponse } from '../models/transaction.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private readonly baseUrl = '/api/accounts';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Account[]> {
    return this.http.get<Account[]>(this.baseUrl);
  }

  getStatement(accountId: string, page: number, pageSize: number): Observable<PagedResult<TransactionResponse>> {
    return this.http.get<PagedResult<TransactionResponse>>(
      `${this.baseUrl}/${accountId}/transactions`,
      { params: { page, pageSize } }
    );
  }
}