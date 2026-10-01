import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-account-layout',
  template: `
<app-header></app-header>
<div class="min-h-screen" style="background: var(--bg);">
  <div class="container-app py-8">
    <div class="flex gap-6 flex-col md:flex-row">
      <!-- Sidebar -->
      <aside class="w-full md:w-60 flex-shrink-0">
        <div class="card p-4">
          <!-- Avatar + nome -->
          <div class="flex items-center gap-3 mb-4 pb-4" style="border-bottom: 1px solid var(--border);">
            <div class="w-11 h-11 rounded-full flex items-center justify-center text-white font-bold text-base flex-shrink-0"
              style="background: linear-gradient(135deg, #9B59B6, #00BCD4);">
              {{user?.fullName?.[0]?.toUpperCase()}}
            </div>
            <div class="min-w-0">
              <p class="font-semibold truncate text-sm" style="color: var(--text);">{{user?.fullName}}</p>
              <p class="text-xs truncate" style="color: var(--text-muted);">{{user?.email}}</p>
            </div>
          </div>
          <!-- Nav -->
          <nav class="space-y-0.5">
            <a routerLink="pedidos" routerLinkActive="active-nav"
              class="account-nav-link">
              <span>📦</span> Meus Pedidos
            </a>
            <a routerLink="personalizados" routerLinkActive="active-nav"
              class="account-nav-link">
              <span>✨</span> Personalizados
            </a>
            <a routerLink="orcamentos" routerLinkActive="active-nav"
              class="account-nav-link">
              <span>💰</span> Orçamentos
            </a>
            <a routerLink="enderecos" routerLinkActive="active-nav"
              class="account-nav-link">
              <span>📍</span> Endereços
            </a>
            <a routerLink="perfil" routerLinkActive="active-nav"
              class="account-nav-link">
              <span>👤</span> Meu Perfil
            </a>
          </nav>
        </div>
      </aside>
      <!-- Content -->
      <div class="flex-1 min-w-0"><router-outlet></router-outlet></div>
    </div>
  </div>
</div>
<app-footer></app-footer>
  `,
  styles: [`
    .account-nav-link {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 9px 12px;
      border-radius: 10px;
      font-size: 0.875rem;
      color: #475569;
      transition: all 0.15s;
      text-decoration: none;
      font-weight: 500;
    }
    .account-nav-link:hover {
      background: rgba(155, 89, 182, 0.07);
      color: #9B59B6;
    }
    .active-nav {
      background: rgba(155, 89, 182, 0.1) !important;
      color: #9B59B6 !important;
      font-weight: 600 !important;
    }
  `]
})
export class AccountLayoutComponent {
  user: any = null;
  constructor(private auth: AuthService) {
    this.user = this.auth.currentUser;
  }
}
