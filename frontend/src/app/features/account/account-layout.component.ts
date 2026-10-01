import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-account-layout',
  template: `
<app-header></app-header>
<div class="min-h-screen bg-gray-50">
  <div class="container-app py-8">
    <div class="flex gap-6 flex-col md:flex-row">
      <!-- Sidebar -->
      <aside class="w-full md:w-56 flex-shrink-0">
        <div class="card p-4">
          <div class="flex items-center gap-3 mb-4 pb-4 border-b border-gray-100">
            <div class="w-10 h-10 bg-primary-100 rounded-full flex items-center justify-center">
              <span class="text-primary-700 font-bold">{{user?.fullName?.[0]?.toUpperCase()}}</span>
            </div>
            <div class="min-w-0">
              <p class="font-semibold text-dark-900 truncate text-sm">{{user?.fullName}}</p>
              <p class="text-xs text-gray-500 truncate">{{user?.email}}</p>
            </div>
          </div>
          <nav class="space-y-1">
            <a routerLink="pedidos" routerLinkActive="bg-primary-50 text-primary-600 font-semibold"
              class="flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm text-dark-700 hover:bg-gray-50 transition-colors">
              📦 Meus Pedidos
            </a>
            <a routerLink="personalizados" routerLinkActive="bg-primary-50 text-primary-600 font-semibold"
              class="flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm text-dark-700 hover:bg-gray-50 transition-colors">
              ✨ Personalizados
            </a>
            <a routerLink="orcamentos" routerLinkActive="bg-primary-50 text-primary-600 font-semibold"
              class="flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm text-dark-700 hover:bg-gray-50 transition-colors">
              💰 Orçamentos
            </a>
            <a routerLink="enderecos" routerLinkActive="bg-primary-50 text-primary-600 font-semibold"
              class="flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm text-dark-700 hover:bg-gray-50 transition-colors">
              📍 Endereços
            </a>
            <a routerLink="perfil" routerLinkActive="bg-primary-50 text-primary-600 font-semibold"
              class="flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-sm text-dark-700 hover:bg-gray-50 transition-colors">
              👤 Meu Perfil
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
  `
})
export class AccountLayoutComponent {
  user: any = null;
  constructor(private auth: AuthService) {
    this.user = this.auth.currentUser;
  }
}
