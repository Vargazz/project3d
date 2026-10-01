import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { AccountLayoutComponent } from './account-layout.component';
import { OrdersComponent } from './orders/orders.component';
import { CustomOrdersComponent } from './custom-orders/custom-orders.component';
import { QuotesComponent } from './quotes/quotes.component';
import { ProfileComponent } from './profile/profile.component';
import { AddressesComponent } from './addresses/addresses.component';

@NgModule({
  declarations: [
    AccountLayoutComponent,
    OrdersComponent,
    CustomOrdersComponent,
    QuotesComponent,
    ProfileComponent,
    AddressesComponent,
  ],
  imports: [
    SharedModule,
    RouterModule.forChild([
      {
        path: '',
        component: AccountLayoutComponent,
        children: [
          { path: '',               redirectTo: 'pedidos', pathMatch: 'full' },
          { path: 'pedidos',        component: OrdersComponent },
          { path: 'personalizados', component: CustomOrdersComponent },
          { path: 'orcamentos',     component: QuotesComponent },
          { path: 'perfil',         component: ProfileComponent },
          { path: 'enderecos',      component: AddressesComponent },
        ]
      }
    ])
  ]
})
export class AccountModule {}
