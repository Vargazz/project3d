export type NotificationType =
  | 'OrderUpdate' | 'PaymentUpdate' | 'CustomRequestUpdate'
  | 'QuoteAvailable' | 'StockAlert' | 'General';

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  referenceType?: string;
  referenceId?: string;
  createdAt: string;
}

export interface NotificationsResponse {
  notifications: Notification[];
  unreadCount: number;
}
