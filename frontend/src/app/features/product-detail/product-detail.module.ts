import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ProductDetailComponent } from './product-detail.component';

@NgModule({
  declarations: [ProductDetailComponent],
  imports: [
    SharedModule,
    RouterModule.forChild([{ path: '', component: ProductDetailComponent }])
  ]
})
export class ProductDetailModule {}
