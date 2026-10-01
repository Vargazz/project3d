import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { OrderConfirmationComponent } from './order-confirmation.component';

@NgModule({
  declarations: [OrderConfirmationComponent],
  imports: [
    SharedModule,
    RouterModule.forChild([
      { path: 'sucesso',  component: OrderConfirmationComponent },
      { path: 'falha',    component: OrderConfirmationComponent },
      { path: 'pendente', component: OrderConfirmationComponent },
      { path: '',         redirectTo: 'sucesso', pathMatch: 'full' },
    ])
  ]
})
export class OrderConfirmationModule {}
