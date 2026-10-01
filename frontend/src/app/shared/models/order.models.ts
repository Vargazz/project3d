export type OrderStatus =
  | 'Received' | 'PaymentPending' | 'PaymentApproved'
  | 'InProduction' | 'Ready' | 'Shipped' | 'Delivered' | 'Cancelled';

export type PaymentStatus =
  | 'Pending' | 'WaitingPayment' | 'Paid' | 'Rejected' | 'Cancelled' | 'Refunded';

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  Received:        'Pedido recebido',
  PaymentPending:  'Aguardando pagamento',
  PaymentApproved: 'Pagamento aprovado',
  InProduction:    'Em produção',
  Ready:           'Pronto',
  Shipped:         'Enviado',
  Delivered:       'Entregue',
  Cancelled:       'Cancelado',
};

export const PAYMENT_STATUS_LABELS: Record<PaymentStatus, string> = {
  Pending:        'Pendente',
  WaitingPayment: 'Aguardando pagamento',
  Paid:           'Pago',
  Rejected:       'Recusado',
  Cancelled:      'Cancelado',
  Refunded:       'Estornado',
};

export interface Address {
  id?: string;
  zipCode: string;
  state: string;
  city: string;
  neighborhood: string;
  street: string;
  number: string;
  complement?: string;
  label?: string;
  isDefault?: boolean;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  variantName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  customizationData?: string;
  productImageUrl?: string;
}

export interface Order {
  id: string;
  orderNumber: string;
  userId: string;
  customerName: string;
  customerEmail?: string;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  subtotal: number;
  shippingCost: number;
  discount: number;
  total: number;
  paymentMethod?: string;
  trackingCode?: string;
  items: OrderItem[];
  shippingAddress?: Address;
  createdAt: string;
  updatedAt: string;
}

export interface CartItem {
  productId: string;
  variantId?: string;
  quantity: number;
  customizationData?: string;
}

export interface CreateOrderRequest {
  items: CartItem[];
  addressId?: string;
  newAddress?: Address;
  shippingOptionId?: string;
  couponCode?: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  customerCpf?: string;
}

export interface ShippingOption {
  id: string;
  name: string;
  description?: string;
  price: number;
  estimatedDays: number;
}

export interface CouponValidation {
  isValid: boolean;
  errorMessage?: string;
  discountAmount: number;
  couponId?: string;
}
