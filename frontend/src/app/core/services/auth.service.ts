import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import {
  LoginRequest, RegisterRequest, AuthResponse, UserInfo, ChangePasswordRequest, ForgotPasswordRequest
} from '../../shared/models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'p3d_token';
  private readonly REFRESH_KEY = 'p3d_refresh';
  private readonly USER_KEY = 'p3d_user';

  private _user$ = new BehaviorSubject<UserInfo | null>(this.storedUser());
  readonly user$ = this._user$.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  get currentUser(): UserInfo | null { return this._user$.value; }
  get isLoggedIn(): boolean          { return !!this.token; }
  get isAdmin(): boolean             { return this._user$.value?.role === 'Admin'; }
  get token(): string | null         { return localStorage.getItem(this.TOKEN_KEY); }

  register(req: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/register`, req)
      .pipe(tap(r => this.persist(r)));
  }

  login(req: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/login`, req)
      .pipe(tap(r => this.persist(r)));
  }

  logout(): void {
    this.http.post(`${this.api}/logout`, {}).subscribe({ error: () => {} });
    this.clear();
    this.router.navigate(['/']);
  }

  changePassword(req: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.api}/change-password`, req);
  }

  forgotPassword(req: ForgotPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.api}/forgot-password`, req);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem(this.REFRESH_KEY) ?? '';
    return this.http.post<AuthResponse>(`${this.api}/refresh`, { refreshToken })
      .pipe(tap(r => this.persist(r)));
  }

  private persist(r: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, r.token);
    localStorage.setItem(this.REFRESH_KEY, r.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(r.user));
    this._user$.next(r.user);
  }

  private clear(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.USER_KEY);
    this._user$.next(null);
  }

  private storedUser(): UserInfo | null {
    try {
      const raw = localStorage.getItem(this.USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch { return null; }
  }
}
