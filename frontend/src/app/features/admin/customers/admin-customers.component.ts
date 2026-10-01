import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({ selector: 'app-admin-customers', templateUrl: './admin-customers.component.html', styleUrls: ['./admin-customers.component.css'] })
export class AdminCustomersComponent implements OnInit {
  customers: any[] = [];
  loading = true;
  page = 1; total = 0;
  constructor(private admin: AdminService, private toast: ToastService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.loading = true; this.admin.getUsers(this.page).subscribe({ next: r => { this.customers = r.items; this.total = r.totalItems; this.loading = false; }, error: () => { this.loading = false; } }); }
  toggle(c: any): void { this.admin.toggleUser(c.id).subscribe({ next: () => { this.toast.success('Usuário atualizado.'); this.load(); } }); }
}
