import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private refreshing = false;
  private refreshSubject$ = new BehaviorSubject<string | null>(null);

  constructor(private auth: AuthService, private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.auth.token;
    const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

    return next.handle(authReq).pipe(
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401 && token) {
          return this.handle401(req, next);
        }
        return throwError(() => err);
      })
    );
  }

  private handle401(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.refreshing) {
      this.refreshing = true;
      this.refreshSubject$.next(null);
      return this.auth.refreshToken().pipe(
        switchMap(r => {
          this.refreshing = false;
          this.refreshSubject$.next(r.token);
          return next.handle(req.clone({ setHeaders: { Authorization: `Bearer ${r.token}` } }));
        }),
        catchError(e => {
          this.refreshing = false;
          this.auth.logout();
          return throwError(() => e);
        })
      );
    }
    return this.refreshSubject$.pipe(
      filter(t => t !== null), take(1),
      switchMap(t => next.handle(req.clone({ setHeaders: { Authorization: `Bearer ${t}` } })))
    );
  }
}
