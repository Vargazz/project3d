import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CustomRequest, CreateCustomRequestRequest
} from '../../shared/models/custom-request.models';
import { PagedResult } from '../../shared/models/product.models';

@Injectable({ providedIn: 'root' })
export class CustomRequestService {
  private readonly api = `${environment.apiUrl}/custom-requests`;

  constructor(private http: HttpClient) {}

  create(req: CreateCustomRequestRequest): Observable<CustomRequest> {
    return this.http.post<CustomRequest>(this.api, req);
  }

  getById(id: string): Observable<CustomRequest> {
    return this.http.get<CustomRequest>(`${this.api}/${id}`);
  }

  getMyRequests(page = 1, pageSize = 10): Observable<PagedResult<CustomRequest>> {
    return this.http.get<PagedResult<CustomRequest>>(`${this.api}/my`, {
      params: new HttpParams().set('page', page).set('pageSize', pageSize)
    });
  }

  addImage(requestId: string, file: File): Observable<{ url: string }> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<{ url: string }>(`${this.api}/${requestId}/images`, fd);
  }

  respondToQuote(requestId: string, quoteId: string, accepted: boolean): Observable<any> {
    return this.http.post(`${this.api}/${requestId}/quotes/${quoteId}/respond`, { accepted });
  }

  createQuotePaymentPreference(quoteId: string): Observable<{ preferenceId: string; checkoutUrl: string; sandboxCheckoutUrl: string }> {
    return this.http.post<any>(`${environment.apiUrl}/payments/quotes/${quoteId}/preference`, {});
  }

  // ---- Admin ----
  getAll(page = 1, pageSize = 20, status?: string): Observable<PagedResult<CustomRequest>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<CustomRequest>>(this.api, { params });
  }

  updateStatus(id: string, body: { status: string; adminNotes?: string }): Observable<CustomRequest> {
    return this.http.put<CustomRequest>(`${this.api}/${id}/status`, body);
  }

  createQuote(requestId: string, body: { price: number; deadlineDays: number; description?: string }): Observable<any> {
    return this.http.post(`${this.api}/${requestId}/quotes`, body);
  }
}
