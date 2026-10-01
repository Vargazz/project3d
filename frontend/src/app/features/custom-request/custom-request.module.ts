import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { CustomRequestComponent } from './custom-request.component';

@NgModule({
  declarations: [CustomRequestComponent],
  imports: [
    SharedModule,
    RouterModule.forChild([{ path: '', component: CustomRequestComponent }])
  ]
})
export class CustomRequestModule {}
