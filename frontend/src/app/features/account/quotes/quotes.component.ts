import { Component, OnInit } from '@angular/core';
import { CustomRequestService } from '../../../core/services/custom-request.service';
import { Router } from '@angular/router';

@Component({ selector: 'app-quotes', templateUrl: './quotes.component.html', styleUrls: ['./quotes.component.css'] })
export class QuotesComponent implements OnInit {
  requests: any[] = [];
  loading = true;
  paying: string | null = null;

  constructor(private service: CustomRequestService, private router: Router) {}
  ngOnInit(): void {
    this.loading = true;
    this.service.getMyRequests(1, 50).subscribe({
      next: r => { this.requests = r.items.filter((x: any) => x.quotes?.some((q: any) => q.status === 'Sent' || q.status === 'Accepted')); this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  pay(quoteId: string): void {
    this.paying = quoteId;
    this.service.createQuotePaymentPreference(quoteId).subscribe({
      next: r => { window.location.href = r.checkoutUrl; },
      error: () => { this.paying = null; }
    });
  }
}
