import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.css']
})
export class AdminLayoutComponent {
  sidebarOpen = true;
  constructor(public auth: AuthService) {}

  navItems = [
    { label: 'Dashboard',           icon: '📊', route: 'dashboard' },
    { label: 'Produtos',            icon: '📦', route: 'produtos' },
    { label: 'Categorias',          icon: '🗂️', route: 'categorias' },
    { label: 'Estoque',             icon: '📋', route: 'estoque' },
    { label: 'Pedidos',             icon: '🛒', route: 'pedidos' },
    { label: 'Personalizados',      icon: '✨', route: 'personalizados' },
    { label: 'Clientes',            icon: '👥', route: 'clientes' },
    { label: 'Cupons',              icon: '🏷️', route: 'cupons' },
    { label: 'Administradores',     icon: '🔐', route: 'administradores' },
    { label: 'Configurações',       icon: '⚙️', route: 'configuracoes' },
  ];
}
