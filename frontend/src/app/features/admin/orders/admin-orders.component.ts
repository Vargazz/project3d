import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../../core/services/order.service';
import { Order, ORDER_STATUS_LABELS } from '../../../shared/models/order.models';
import { ToastService } from '../../../core/services/toast.service';

@Component({ selector: 'app-admin-orders', templateUrl: './admin-orders.component.html', styleUrls: ['./admin-orders.component.css'] })
export class AdminOrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = true;
  page = 1; pageSize = 20; total = 0;
  statusFilter = '';
  selected: Order | null = null;
  statusLabels = ORDER_STATUS_LABELS;
  updateStatus = '';
  updateTracking = '';
  updating = false;
  statusOptions = Object.entries(ORDER_STATUS_LABELS).map(([k, v]) => ({ value: k, label: v }));

  constructor(private service: OrderService, private toast: ToastService) {}
  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.service.getAllOrders(this.page, this.pageSize, this.statusFilter || undefined).subscribe({
      next: r => { this.orders = r.items; this.total = r.totalItems; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  select(o: Order): void { this.selected = o; this.updateStatus = o.status; this.updateTracking = o.trackingCode ?? ''; }

  update(): void {
    if (!this.selected) return;
    this.updating = true;
    this.service.updateStatus(this.selected.id, { status: this.updateStatus, trackingCode: this.updateTracking }).subscribe({
      next: () => { this.toast.success('Pedido atualizado!'); this.updating = false; this.selected = null; this.load(); },
      error: () => { this.updating = false; }
    });
  }

  statusClass(s: string): string {
    return { Received:'badge-gray', PaymentPending:'badge-yellow', PaymentApproved:'badge-blue',
      InProduction:'badge-primary', Ready:'badge-blue', Shipped:'badge-blue',
      Delivered:'badge-green', Cancelled:'badge-red' }[s] ?? 'badge-gray';
  }
}
