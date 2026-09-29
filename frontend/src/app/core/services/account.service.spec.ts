import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AccountService } from './account.service';
import { Account } from '../models/account.model';

describe('AccountService', () => {
  let service: AccountService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AccountService, provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(AccountService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should fetch all accounts', () => {
    const mockAccounts: Account[] = [{ id: '1', owner: 'Maria Silva', balance: 1000 }];

    service.getAll().subscribe((accounts) => {
      expect(accounts).toEqual(mockAccounts);
    });

    const req = httpMock.expectOne('/api/accounts');
    expect(req.request.method).toBe('GET');
    req.flush(mockAccounts);
  });

  it('should fetch the statement with pagination params', () => {
    service.getStatement('acc-1', 2, 10).subscribe();

    const req = httpMock.expectOne((r) => r.url === '/api/accounts/acc-1/transactions');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('10');
    req.flush({ items: [], page: 2, pageSize: 10, totalCount: 0, totalPages: 0 });
  });
});