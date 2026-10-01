import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({ selector: 'app-admin-settings', templateUrl: './admin-settings.component.html', styleUrls: ['./admin-settings.component.css'] })
export class AdminSettingsComponent implements OnInit {
  settings: Record<string, string> = {};
  loading = true;
  saving = false;
  constructor(private admin: AdminService, private toast: ToastService) {}
  ngOnInit(): void { this.admin.getSettings().subscribe({ next: s => { this.settings = s ?? {}; this.loading = false; }, error: () => { this.loading = false; } }); }
  save(): void {
    this.saving = true;
    this.admin.updateSettings(this.settings).subscribe({
      next: () => { this.toast.success('Configurações salvas!'); this.saving = false; },
      error: () => { this.saving = false; }
    });
  }
  objectKeys = Object.keys;
  labels: Record<string, string> = {
    'store.name': 'Nome da loja',
    'store.email': 'E-mail de contato',
    'store.whatsapp': 'WhatsApp',
    'store.instagram': 'Instagram',
    'store.description': 'Descrição',
    'store.privacy_policy': 'Política de privacidade',
    'store.terms_of_use': 'Termos de uso'
  };
}
