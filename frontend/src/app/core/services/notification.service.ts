import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification, NotificationsResponse } from '../../shared/models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly api = `${environment.apiUrl}/notifications`;
  private _unread$ = new BehaviorSubject<number>(0);
  readonly unread$ = this._unread$.asObservable();

  constructor(private http: HttpClient) {}

  getNotifications(unreadOnly = false, limit = 20): Observable<NotificationsResponse> {
    return this.http.get<NotificationsResponse>(this.api, {
      params: { unreadOnly, limit }
    }).pipe(tap(r => this._unread$.next(r.unreadCount)));
  }

  markAsRead(id: string): Observable<void> {
    return this.http.patch<void>(`${this.api}/${id}/read`, {})
      .pipe(tap(() => {
        const current = this._unread$.value;
        if (current > 0) this._unread$.next(current - 1);
      }));
  }

  markAllAsRead(): Observable<void> {
    return this.http.patch<void>(`${this.api}/read-all`, {})
      .pipe(tap(() => this._unread$.next(0)));
  }
}
