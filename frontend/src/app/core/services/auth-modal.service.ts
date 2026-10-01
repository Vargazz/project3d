import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type AuthModalTab = 'login' | 'register' | 'forgot';

export interface AuthModalState {
  tab: AuthModalTab;
  returnUrl?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthModalService {
  private _state$ = new BehaviorSubject<AuthModalState | null>(null);
  readonly state$ = this._state$.asObservable();

  open(tab: AuthModalTab = 'login', returnUrl?: string): void {
    this._state$.next({ tab, returnUrl });
    document.body.style.overflow = 'hidden';
  }

  close(): void {
    this._state$.next(null);
    document.body.style.overflow = '';
  }

  get isOpen(): boolean { return this._state$.value !== null; }
  get currentTab(): AuthModalTab | null { return this._state$.value?.tab ?? null; }
  get returnUrl(): string | undefined { return this._state$.value?.returnUrl; }
}
