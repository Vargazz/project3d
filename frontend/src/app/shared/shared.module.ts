import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { CurrencyBrlPipe } from './pipes/currency-brl.pipe';
import { HeaderComponent } from './components/header/header.component';
import { FooterComponent } from './components/footer/footer.component';
import { ProductCardComponent } from './components/product-card/product-card.component';
import { LoadingComponent } from './components/loading/loading.component';
import { NotificationToastComponent } from './components/notification-toast/notification-toast.component';

@NgModule({
  declarations: [
    CurrencyBrlPipe,
    HeaderComponent,
    FooterComponent,
    ProductCardComponent,
    LoadingComponent,
    NotificationToastComponent,
  ],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  exports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    CurrencyBrlPipe,
    HeaderComponent,
    FooterComponent,
    ProductCardComponent,
    LoadingComponent,
    NotificationToastComponent,
  ]
})
export class SharedModule {}
