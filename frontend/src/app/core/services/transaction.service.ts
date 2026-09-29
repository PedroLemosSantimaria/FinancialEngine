import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TransactionEventRequest, TransactionResponse } from '../models/transaction.model';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private readonly baseUrl = '/api/transactions';

  constructor(private http: HttpClient) {}

  process(request: TransactionEventRequest): Observable<TransactionResponse> {
    return this.http.post<TransactionResponse>(this.baseUrl, request);
  }
}