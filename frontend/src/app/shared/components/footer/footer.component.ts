import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { AuthModalService } from '../../../core/services/auth-modal.service';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent {
  year = new Date().getFullYear();

  constructor(
    public auth: AuthService,
    private authModal: AuthModalService
  ) {}

  openLogin():    void { this.authModal.open('login'); }
  openRegister(): void { this.authModal.open('register'); }
}
