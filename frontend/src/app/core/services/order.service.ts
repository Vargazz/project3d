import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order, CreateOrderRequest } from '../../shared/models/order.models';
import { PagedResult } from '../../shared/models/product.models';

type PagedOrders = import('../../shared/models/product.models').PagedResult<Order>;

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly api = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  create(req: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.api, req);
  }

  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.api}/${id}`);
  }

  getMyOrders(page = 1, pageSize = 10): Observable<import('../../shared/models/product.models').PagedResult<Order>> {
    return this.http.get<import('../../shared/models/product.models').PagedResult<Order>>(`${this.api}/my`, {
      params: new HttpParams().set('page', page).set('pageSize', pageSize)
    });
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.api}/${id}/cancel`, {});
  }

  createPaymentPreference(orderId: string): Observable<{ preferenceId: string; checkoutUrl: string; sandboxCheckoutUrl: string }> {
    return this.http.post<any>(`${environment.apiUrl}/payments/orders/${orderId}/preference`, {});
  }

  // ---- Admin ----
  getAllOrders(page = 1, pageSize = 20, status?: string): Observable<import('../../shared/models/product.models').PagedResult<Order>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status) params = params.set('status', status);
    return this.http.get<import('../../shared/models/product.models').PagedResult<Order>>(this.api, { params });
  }

  updateStatus(id: string, body: { status: string; notes?: string; trackingCode?: string }): Observable<Order> {
    return this.http.put<Order>(`${this.api}/${id}/status`, body);
  }
}
