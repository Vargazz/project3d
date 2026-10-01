import { Component, OnInit } from '@angular/core';
import { ToastService, Toast } from '../../../core/services/toast.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-notification-toast',
  templateUrl: './notification-toast.component.html',
  styleUrls: ['./notification-toast.component.css']
})
export class NotificationToastComponent implements OnInit {
  toasts$!: Observable<Toast[]>;

  constructor(private toastService: ToastService) {}

  ngOnInit(): void { this.toasts$ = this.toastService.toasts$; }

  remove(id: string): void { this.toastService.remove(id); }

  icon(type: Toast['type']): string {
    return {
      success: '✓',
      error:   '✕',
      warning: '⚠',
      info:    'ℹ'
    }[type];
  }

  bgClass(type: Toast['type']): string {
    return {
      success: 'bg-green-500',
      error:   'bg-red-500',
      warning: 'bg-yellow-500',
      info:    'bg-blue-500'
    }[type];
  }
}
