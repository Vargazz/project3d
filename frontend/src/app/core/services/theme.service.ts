import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly KEY = 'vm3d_theme';
  private _dark$ = new BehaviorSubject<boolean>(this.loadPreference());
  readonly dark$ = this._dark$.asObservable();

  get isDark(): boolean { return this._dark$.value; }

  constructor() { this.apply(this._dark$.value); }

  toggle(): void {
    const next = !this._dark$.value;
    this._dark$.next(next);
    localStorage.setItem(this.KEY, next ? 'dark' : 'light');
    this.apply(next);
  }

  private apply(dark: boolean): void {
    const html = document.documentElement;
    if (dark) html.classList.add('dark');
    else      html.classList.remove('dark');
  }

  private loadPreference(): boolean {
    const stored = localStorage.getItem(this.KEY);
    if (stored) return stored === 'dark';
    // respeitar preferência do sistema
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }
}
