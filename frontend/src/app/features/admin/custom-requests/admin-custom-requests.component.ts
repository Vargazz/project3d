import { Component, OnInit } from '@angular/core';
import { CustomRequestService } from '../../../core/services/custom-request.service';
import { CustomRequest, CUSTOM_REQUEST_STATUS_LABELS } from '../../../shared/models/custom-request.models';
import { ToastService } from '../../../core/services/toast.service';

@Component({ selector: 'app-admin-custom-requests', templateUrl: './admin-custom-requests.component.html', styleUrls: ['./admin-custom-requests.component.css'] })
export class AdminCustomRequestsComponent implements OnInit {
  requests: CustomRequest[] = [];
  loading = true;
  selected: CustomRequest | null = null;
  statusLabels = CUSTOM_REQUEST_STATUS_LABELS;
  statusFilter = '';
  updateStatusVal = '';
  quotePrice = 0;
  quoteDays = 7;
  quoteDesc = '';
  submitting = false;
  statusOptions = Object.entries(CUSTOM_REQUEST_STATUS_LABELS).map(([k, v]) => ({ value: k, label: v }));

  constructor(private service: CustomRequestService, private toast: ToastService) {}
  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.service.getAll(1, 50, this.statusFilter || undefined).subscribe({
      next: r => { this.requests = r.items; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  select(r: CustomRequest): void { this.selected = r; this.updateStatusVal = r.status; }

  updateStatus(): void {
    if (!this.selected) return;
    this.submitting = true;
    this.service.updateStatus(this.selected.id, { status: this.updateStatusVal }).subscribe({
      next: () => { this.toast.success('Status atualizado!'); this.submitting = false; this.selected = null; this.load(); },
      error: () => { this.submitting = false; }
    });
  }

  sendQuote(): void {
    if (!this.selected || !this.quotePrice) return;
    this.submitting = true;
    this.service.createQuote(this.selected.id, { price: this.quotePrice, deadlineDays: this.quoteDays, description: this.quoteDesc }).subscribe({
      next: () => { this.toast.success('Orçamento enviado!'); this.submitting = false; this.quotePrice = 0; this.load(); },
      error: () => { this.submitting = false; }
    });
  }

  statusClass(s: string): string {
    return { Received:'badge-gray', UnderAnalysis:'badge-yellow', QuoteSent:'badge-blue',
      Approved:'badge-green', InProduction:'badge-primary', Cancelled:'badge-red' }[s] ?? 'badge-gray';
  }
}
