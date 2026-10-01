import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { AdminLayoutComponent } from './admin-layout.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AdminProductsComponent } from './products/admin-products.component';
import { AdminCategoriesComponent } from './categories/admin-categories.component';
import { AdminInventoryComponent } from './inventory/admin-inventory.component';
import { AdminOrdersComponent } from './orders/admin-orders.component';
import { AdminCustomRequestsComponent } from './custom-requests/admin-custom-requests.component';
import { AdminCustomersComponent } from './customers/admin-customers.component';
import { AdminCouponsComponent } from './coupons/admin-coupons.component';
import { AdminSettingsComponent } from './settings/admin-settings.component';
import { AdminAdminsComponent } from './admins/admin-admins.component';

@NgModule({
  declarations: [
    AdminLayoutComponent,
    DashboardComponent,
    AdminProductsComponent,
    AdminCategoriesComponent,
    AdminInventoryComponent,
    AdminOrdersComponent,
    AdminCustomRequestsComponent,
    AdminCustomersComponent,
    AdminCouponsComponent,
    AdminSettingsComponent,
    AdminAdminsComponent,
  ],
  imports: [
    SharedModule,
    RouterModule.forChild([
      {
        path: '',
        component: AdminLayoutComponent,
        children: [
          { path: '',             redirectTo: 'dashboard', pathMatch: 'full' },
          { path: 'dashboard',    component: DashboardComponent },
          { path: 'produtos',     component: AdminProductsComponent },
          { path: 'categorias',   component: AdminCategoriesComponent },
          { path: 'estoque',      component: AdminInventoryComponent },
          { path: 'pedidos',      component: AdminOrdersComponent },
          { path: 'personalizados', component: AdminCustomRequestsComponent },
          { path: 'clientes',     component: AdminCustomersComponent },
          { path: 'cupons',       component: AdminCouponsComponent },
          { path: 'configuracoes',component: AdminSettingsComponent },
          { path: 'administradores', component: AdminAdminsComponent },
        ]
      }
    ])
  ]
})
export class AdminModule {}
