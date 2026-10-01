import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../../core/services/admin.service';

@Component({ selector: 'app-dashboard', templateUrl: './dashboard.component.html', styleUrls: ['./dashboard.component.css'] })
export class DashboardComponent implements OnInit {
  data: any = null;
  loading = true;
  alerts: any[] = [];

  constructor(private admin: AdminService) {}

  ngOnInit(): void {
    this.admin.getDashboard().subscribe({ next: d => { this.data = d; this.loading = false; }, error: () => { this.loading = false; } });
    this.admin.getLowStockAlerts().subscribe({ next: a => { this.alerts = a; }, error: () => {} });
  }

  get maxSales(): number {
    if (!this.data?.salesByDay?.length) return 1;
    return Math.max(...this.data.salesByDay.map((s: any) => s.amount));
  }

  barHeight(amount: number): number {
    return Math.round((amount / this.maxSales) * 100);
  }
}
