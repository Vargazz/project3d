import { Product, ProductVariant } from './product.models';

export interface CartProduct {
  productId: string;
  variantId?: string;
  name: string;
  variantLabel?: string;
  price: number;
  imageUrl?: string;
  slug: string;
  quantity: number;
  stock: number;
  customizationData?: Record<string, string>;
}

export interface Cart {
  items: CartProduct[];
  subtotal: number;
  discount: number;
  shippingCost: number;
  total: number;
  couponCode?: string;
  couponDiscount?: number;
}
