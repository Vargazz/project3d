import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../../core/services/order.service';
import { Order, ORDER_STATUS_LABELS, PAYMENT_STATUS_LABELS } from '../../../shared/models/order.models';

@Component({ selector: 'app-orders', templateUrl: './orders.component.html', styleUrls: ['./orders.component.css'] })
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = true;
  page = 1; pageSize = 10; totalItems = 0;
  selectedOrder: Order | null = null;
  statusLabels = ORDER_STATUS_LABELS;
  paymentLabels = PAYMENT_STATUS_LABELS;

  constructor(private orderService: OrderService) {}

  ngOnInit(): void { this.loadOrders(); }

  loadOrders(): void {
    this.loading = true;
    this.orderService.getMyOrders(this.page, this.pageSize).subscribe({
      next: r => { this.orders = r.items; this.totalItems = r.totalItems; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  statusClass(s: string): string {
    return { Received:'badge-gray', PaymentPending:'badge-yellow', PaymentApproved:'badge-blue',
      InProduction:'badge-primary', Ready:'badge-blue', Shipped:'badge-blue',
      Delivered:'badge-green', Cancelled:'badge-red' }[s] ?? 'badge-gray';
  }
}
