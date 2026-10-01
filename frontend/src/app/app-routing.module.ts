import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';

const routes: Routes = [
  // ── Loja ──
  { path: '',             loadChildren: () => import('./features/home/home.module').then(m => m.HomeModule) },
  { path: 'produtos',     loadChildren: () => import('./features/catalog/catalog.module').then(m => m.CatalogModule) },
  { path: 'produtos/:slug', loadChildren: () => import('./features/product-detail/product-detail.module').then(m => m.ProductDetailModule) },
  { path: 'categorias/:slug', loadChildren: () => import('./features/catalog/catalog.module').then(m => m.CatalogModule) },
  { path: 'carrinho',     loadChildren: () => import('./features/cart/cart.module').then(m => m.CartModule) },
  { path: 'personalizado',loadChildren: () => import('./features/custom-request/custom-request.module').then(m => m.CustomRequestModule) },
  { path: 'pedido',       loadChildren: () => import('./features/order-confirmation/order-confirmation.module').then(m => m.OrderConfirmationModule) },

  // Rotas protegidas — guard abre o modal se não logado
  { path: 'checkout',     loadChildren: () => import('./features/checkout/checkout.module').then(m => m.CheckoutModule),   canActivate: [AuthGuard] },
  { path: 'minha-conta',  loadChildren: () => import('./features/account/account.module').then(m => m.AccountModule),      canActivate: [AuthGuard] },
  { path: 'admin',        loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule),            canActivate: [AdminGuard] },

  // ── Auth pages (fallback para acesso direto por URL) ──
  // Sem GuestGuard — permite acesso mesmo logado (redireciona para home no componente)
  { path: 'auth',         loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule) },

  // ── Fallback ──
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { scrollPositionRestoration: 'top' })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
