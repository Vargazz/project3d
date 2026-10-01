import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

// Login, registro e recuperação de senha são tratados pelo modal global no header.
// Qualquer acesso direto por URL é redirecionado para a home, onde o modal pode ser aberto.
@NgModule({
  imports: [
    RouterModule.forChild([
      { path: 'login',           redirectTo: '/', pathMatch: 'full' },
      { path: 'register',        redirectTo: '/', pathMatch: 'full' },
      { path: 'forgot-password', redirectTo: '/', pathMatch: 'full' },
      { path: '',                redirectTo: '/', pathMatch: 'full' },
    ])
  ]
})
export class AuthModule {}
