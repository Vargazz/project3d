export type CustomRequestStatus =
  | 'Received' | 'UnderAnalysis' | 'WaitingInfo' | 'QuoteSent'
  | 'WaitingApproval' | 'Approved' | 'WaitingPayment' | 'Paid'
  | 'InProduction' | 'Finished' | 'Shipped' | 'Cancelled';

export type QuoteStatus = 'Draft' | 'Sent' | 'Accepted' | 'Rejected' | 'Cancelled';

export const CUSTOM_REQUEST_STATUS_LABELS: Record<CustomRequestStatus, string> = {
  Received:        'Recebido',
  UnderAnalysis:   'Em análise',
  WaitingInfo:     'Aguardando informações',
  QuoteSent:       'Orçamento enviado',
  WaitingApproval: 'Aguardando aprovação',
  Approved:        'Aprovado',
  WaitingPayment:  'Aguardando pagamento',
  Paid:            'Pago',
  InProduction:    'Em produção',
  Finished:        'Finalizado',
  Shipped:         'Enviado',
  Cancelled:       'Cancelado',
};

export interface CustomRequestImage {
  id: string;
  url: string;
  originalName: string;
}

export interface CustomQuote {
  id: string;
  price: number;
  deadlineDays: number;
  description?: string;
  status: QuoteStatus;
  sentAt?: string;
  respondedAt?: string;
  createdAt: string;
}

export interface CustomRequest {
  id: string;
  requestNumber: string;
  userId: string;
  customerName: string;
  contactName: string;
  contactEmail: string;
  contactWhatsApp?: string;
  productType: string;
  description: string;
  characteristics?: string;
  desiredColors?: string;
  approximateSize?: string;
  quantity: number;
  observations?: string;
  status: CustomRequestStatus;
  images: CustomRequestImage[];
  quotes: CustomQuote[];
  createdAt: string;
}

export interface CreateCustomRequestRequest {
  productType: string;
  description: string;
  characteristics?: string;
  desiredColors?: string;
  approximateSize?: string;
  quantity: number;
  observations?: string;
  contactName: string;
  contactEmail: string;
  contactWhatsApp?: string;
}
