import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Cart, CartProduct } from '../../shared/models/cart.models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly STORAGE_KEY = 'p3d_cart';

  private _cart$ = new BehaviorSubject<Cart>(this.loadCart());
  readonly cart$ = this._cart$.asObservable();

  get cart(): Cart           { return this._cart$.value; }
  get itemCount(): number    { return this._cart$.value.items.reduce((s, i) => s + i.quantity, 0); }
  get totalItems(): number   { return this.itemCount; }

  addItem(item: CartProduct): void {
    const cart = { ...this._cart$.value };
    const idx = cart.items.findIndex(i => i.productId === item.productId && i.variantId === item.variantId);
    if (idx >= 0) {
      const newQty = cart.items[idx].quantity + item.quantity;
      cart.items[idx] = { ...cart.items[idx], quantity: Math.min(newQty, item.stock) };
    } else {
      cart.items = [...cart.items, item];
    }
    this.update(cart);
  }

  removeItem(productId: string, variantId?: string): void {
    const cart = { ...this._cart$.value };
    cart.items = cart.items.filter(i => !(i.productId === productId && i.variantId === variantId));
    this.update(cart);
  }

  updateQuantity(productId: string, variantId: string | undefined, quantity: number): void {
    const cart = { ...this._cart$.value };
    const idx = cart.items.findIndex(i => i.productId === productId && i.variantId === variantId);
    if (idx >= 0) {
      if (quantity <= 0) {
        cart.items = cart.items.filter((_, i) => i !== idx);
      } else {
        cart.items[idx] = { ...cart.items[idx], quantity: Math.min(quantity, cart.items[idx].stock) };
      }
    }
    this.update(cart);
  }

  applyCoupon(code: string, discount: number): void {
    const cart = { ...this._cart$.value, couponCode: code, couponDiscount: discount };
    this.update(cart);
  }

  removeCoupon(): void {
    const cart = { ...this._cart$.value };
    delete cart.couponCode;
    delete cart.couponDiscount;
    this.update(cart);
  }

  setShipping(cost: number): void {
    const cart = { ...this._cart$.value, shippingCost: cost };
    this.update(cart);
  }

  clear(): void {
    const empty: Cart = { items: [], subtotal: 0, discount: 0, shippingCost: 0, total: 0 };
    this._cart$.next(empty);
    localStorage.removeItem(this.STORAGE_KEY);
  }

  private update(cart: Cart): void {
    cart.subtotal = cart.items.reduce((s, i) => s + i.price * i.quantity, 0);
    cart.discount = cart.couponDiscount ?? 0;
    cart.total = cart.subtotal + cart.shippingCost - cart.discount;
    this._cart$.next(cart);
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(cart));
  }

  private loadCart(): Cart {
    try {
      const raw = localStorage.getItem(this.STORAGE_KEY);
      return raw ? JSON.parse(raw) : { items: [], subtotal: 0, discount: 0, shippingCost: 0, total: 0 };
    } catch {
      return { items: [], subtotal: 0, discount: 0, shippingCost: 0, total: 0 };
    }
  }
}
