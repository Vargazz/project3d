import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: string;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _toasts$ = new BehaviorSubject<Toast[]>([]);
  readonly toasts$ = this._toasts$.asObservable();

  success(message: string, duration = 4000): void { this.add(message, 'success', duration); }
  error(message: string, duration = 5000): void   { this.add(message, 'error', duration); }
  warning(message: string, duration = 4000): void { this.add(message, 'warning', duration); }
  info(message: string, duration = 4000): void    { this.add(message, 'info', duration); }

  remove(id: string): void {
    this._toasts$.next(this._toasts$.value.filter(t => t.id !== id));
  }

  private add(message: string, type: Toast['type'], duration: number): void {
    const id = Math.random().toString(36).slice(2);
    this._toasts$.next([...this._toasts$.value, { id, message, type, duration }]);
    setTimeout(() => this.remove(id), duration);
  }
}
