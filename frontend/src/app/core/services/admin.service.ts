import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly api = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<any> {
    return this.http.get(`${this.api}/dashboard`);
  }

  getLowStockAlerts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/inventory/alerts`);
  }

  adjustStock(body: any): Observable<void> {
    return this.http.post<void>(`${this.api}/inventory/adjust`, body);
  }

  getUsers(page = 1, pageSize = 20): Observable<any> {
    return this.http.get(`${this.api}/users`, { params: { page, pageSize } });
  }

  toggleUser(id: string): Observable<void> {
    return this.http.patch<void>(`${this.api}/users/${id}/toggle`, {});
  }

  getCoupons(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/coupons`);
  }

  createCoupon(body: any): Observable<any> {
    return this.http.post(`${this.api}/coupons`, body);
  }

  updateCoupon(id: string, body: any): Observable<any> {
    return this.http.put(`${this.api}/coupons/${id}`, body);
  }

  deleteCoupon(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/coupons/${id}`);
  }

  getSettings(): Observable<Record<string, string>> {
    return this.http.get<Record<string, string>>(`${this.api}/settings`);
  }

  updateSettings(settings: Record<string, string>): Observable<void> {
    return this.http.put<void>(`${this.api}/settings`, settings);
  }

  // ---- Administradores ----

  getAdmins(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/admins`);
  }

  createAdmin(body: { fullName: string; email: string; password: string }): Observable<any> {
    return this.http.post(`${this.api}/admins`, body);
  }

  deleteAdmin(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admins/${id}`);
  }

  resetAdminPassword(id: string, newPassword: string): Observable<void> {
    return this.http.patch<void>(`${this.api}/admins/${id}/reset-password`, { newPassword });
  }
}
