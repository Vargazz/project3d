import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({ selector: 'app-admin-inventory', templateUrl: './admin-inventory.component.html', styleUrls: ['./admin-inventory.component.css'] })
export class AdminInventoryComponent implements OnInit {
  alerts: any[] = [];
  loading = true;
  showAdjust = false;
  adjustForm: any = { productId:'', quantity:0, movementType:'In', reason:'' };
  saving = false;
  constructor(private admin: AdminService, private toast: ToastService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.loading = true; this.admin.getLowStockAlerts().subscribe({ next: a => { this.alerts = a; this.loading = false; }, error: () => { this.loading = false; } }); }
  adjust(): void {
    this.saving = true;
    this.admin.adjustStock(this.adjustForm).subscribe({
      next: () => { this.toast.success('Estoque ajustado!'); this.saving = false; this.showAdjust = false; this.load(); },
      error: () => { this.saving = false; }
    });
  }
}
