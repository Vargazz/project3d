import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { AuthModalService } from '../services/auth-modal.service';

@Injectable({ providedIn: 'root' })
export class AdminGuard implements CanActivate {
  constructor(
    private auth: AuthService,
    private authModal: AuthModalService,
    private router: Router
  ) {}

  canActivate(): boolean | UrlTree {
    // Logado e é admin — acesso liberado
    if (this.auth.isLoggedIn && this.auth.isAdmin) return true;

    // Logado mas não é admin — volta para home
    if (this.auth.isLoggedIn && !this.auth.isAdmin) {
      return this.router.createUrlTree(['/']);
    }

    // Não logado — abre o modal de login e bloqueia a navegação
    // O modal abre após a home renderizar (header precisa estar no DOM)
    setTimeout(() => this.authModal.open('login'), 100);
    return this.router.createUrlTree(['/']);
  }
}
