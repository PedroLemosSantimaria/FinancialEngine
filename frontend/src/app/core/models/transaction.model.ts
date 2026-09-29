export type TransactionType = 'Credit' | 'Debit';

export interface TransactionEventRequest {
  eventId: string;
  accountId: string;
  type: TransactionType;
  amount: number;
  occurredAt: string; // ISO 8601
}

export interface TransactionResponse {
  eventId: string;
  accountId: string;
  type: TransactionType;
  amount: number;
  balanceAfter: number;
  occurredAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}