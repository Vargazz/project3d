import { Component, OnInit } from '@angular/core';
import { CustomRequestService } from '../../../core/services/custom-request.service';
import { CustomRequest, CUSTOM_REQUEST_STATUS_LABELS } from '../../../shared/models/custom-request.models';

@Component({ selector: 'app-custom-orders', templateUrl: './custom-orders.component.html', styleUrls: ['./custom-orders.component.css'] })
export class CustomOrdersComponent implements OnInit {
  requests: CustomRequest[] = [];
  loading = true;
  selected: CustomRequest | null = null;
  statusLabels = CUSTOM_REQUEST_STATUS_LABELS;

  constructor(private service: CustomRequestService) {}
  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.service.getMyRequests().subscribe({
      next: r => { this.requests = r.items; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  statusClass(s: string): string {
    return { Received:'badge-gray', UnderAnalysis:'badge-yellow', WaitingInfo:'badge-yellow',
      QuoteSent:'badge-blue', WaitingApproval:'badge-blue', Approved:'badge-green',
      WaitingPayment:'badge-yellow', Paid:'badge-green', InProduction:'badge-primary',
      Finished:'badge-green', Shipped:'badge-blue', Cancelled:'badge-red' }[s] ?? 'badge-gray';
  }

  respond(requestId: string, quoteId: string, accepted: boolean): void {
    this.service.respondToQuote(requestId, quoteId, accepted).subscribe({
      next: () => this.load(),
      error: () => {}
    });
  }
}
