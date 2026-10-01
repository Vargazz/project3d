import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/services/auth.service';
import { AuthModalService } from '../../core/services/auth-modal.service';
import { Cart, CartProduct } from '../../shared/models/cart.models';
import { ShippingOption, CouponValidation } from '../../shared/models/order.models';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  cart!: Cart;
  couponCode = '';
  couponError = '';
  couponLoading = false;
  zipCode = '';
  shippingOptions: ShippingOption[] = [];
  selectedShippingId = '';
  shippingLoading = false;
  shippingError = '';

  objectKeys = Object.keys;

  constructor(
    public cartService: CartService,
    private orderService: OrderService,
    private toast: ToastService,
    public authService: AuthService,
    private authModal: AuthModalService,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.cartService.cart$.subscribe(c => { this.cart = c; });
  }

  get selectedShipping(): ShippingOption | undefined {
    return this.shippingOptions.find(s => s.id === this.selectedShippingId);
  }

  updateQty(item: CartProduct, delta: number): void {
    const newQty = item.quantity + delta;
    this.cartService.updateQuantity(item.productId, item.variantId, newQty);
  }

  removeItem(item: CartProduct): void {
    this.cartService.removeItem(item.productId, item.variantId);
    this.toast.info(`${item.name} removido do carrinho.`);
  }

  applyCoupon(): void {
    if (!this.couponCode.trim()) return;
    this.couponLoading = true;
    this.couponError = '';
    this.http.post<CouponValidation>(
      `${environment.apiUrl}/orders/validate-coupon`,
      { code: this.couponCode, subtotal: this.cart.subtotal }
    ).subscribe({
      next: res => {
        this.couponLoading = false;
        if (res.isValid) {
          this.cartService.applyCoupon(this.couponCode, res.discountAmount);
          this.toast.success(`Cupom aplicado! Desconto de ${res.discountAmount.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}`);
        } else {
          this.couponError = res.errorMessage ?? 'Cupom inválido';
        }
      },
      error: () => { this.couponLoading = false; this.couponError = 'Cupom inválido ou expirado.'; }
    });
  }

  removeCoupon(): void {
    this.cartService.removeCoupon();
    this.couponCode = '';
    this.couponError = '';
    this.toast.info('Cupom removido.');
  }

  calcShipping(): void {
    if (this.zipCode.replace(/\D/g, '').length !== 8) {
      this.shippingError = 'CEP inválido. Informe 8 dígitos.';
      return;
    }
    this.shippingLoading = true;
    this.shippingError = '';
    this.http.get<ShippingOption[]>(
      `${environment.apiUrl}/orders/shipping-options`,
      { params: { zipCode: this.zipCode.replace(/\D/g, '') } }
    ).subscribe({
      next: opts => {
        this.shippingOptions = opts;
        this.shippingLoading = false;
        if (opts.length === 0) this.shippingError = 'Nenhuma opção de frete disponível para este CEP.';
      },
      error: () => { this.shippingLoading = false; this.shippingError = 'Erro ao calcular frete.'; }
    });
  }

  selectShipping(opt: ShippingOption): void {
    this.selectedShippingId = opt.id;
    this.cartService.setShipping(opt.price);
  }

  clearCart(): void {
    if (!confirm('Deseja limpar todo o carrinho?')) return;
    this.cartService.clear();
    this.toast.info('Carrinho limpo.');
  }

  goToCheckout(): void {
    if (!this.authService.isLoggedIn) {
      // Abre o modal de login/cadastro em vez de redirecionar para página separada
      this.authModal.open('login', '/checkout');
      return;
    }
    this.router.navigate(['/checkout']);
  }
}
