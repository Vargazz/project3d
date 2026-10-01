import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { Cart } from '../../shared/models/cart.models';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ShippingOption } from '../../shared/models/order.models';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent implements OnInit {
  form!: FormGroup;
  cart!: Cart;
  step: 'info' | 'shipping' | 'review' = 'info';
  submitting = false;
  shippingOptions: ShippingOption[] = [];
  selectedShipping: ShippingOption | null = null;
  shippingLoading = false;
  shippingError = '';
  couponCode = '';
  couponError = '';
  couponLoading = false;

  constructor(
    private fb: FormBuilder,
    private cartService: CartService,
    private orderService: OrderService,
    private authService: AuthService,
    private toast: ToastService,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.cartService.cart$.subscribe(c => { this.cart = c; });

    const user = this.authService.currentUser;
    this.form = this.fb.group({
      customerName:  [user?.fullName ?? '', [Validators.required, Validators.minLength(3)]],
      customerEmail: [user?.email ?? '', [Validators.required, Validators.email]],
      customerPhone: ['', [Validators.required, Validators.minLength(10)]],
      customerCpf:   [''],
      // Address
      zipCode:       ['', [Validators.required, Validators.minLength(8)]],
      state:         ['', Validators.required],
      city:          ['', Validators.required],
      neighborhood:  ['', Validators.required],
      street:        ['', Validators.required],
      number:        ['', Validators.required],
      complement:    [''],
    });

    // Redirect if cart empty
    if (!this.cartService.cart.items.length) {
      this.router.navigate(['/carrinho']);
    }
  }

  get f() { return this.form.controls; }

  lookupZip(): void {
    const zip = this.f['zipCode'].value?.replace(/\D/g, '');
    if (zip?.length !== 8) return;
    this.http.get<any>(`https://viacep.com.br/ws/${zip}/json/`).subscribe({
      next: data => {
        if (data.erro) return;
        this.form.patchValue({
          street:       data.logradouro,
          neighborhood: data.bairro,
          city:         data.localidade,
          state:        data.uf,
        });
      }
    });
  }

  calcShipping(): void {
    const zip = this.f['zipCode'].value?.replace(/\D/g, '');
    if (zip?.length !== 8) { this.shippingError = 'CEP inválido.'; return; }
    this.shippingLoading = true;
    this.shippingError = '';
    this.http.get<ShippingOption[]>(
      `${environment.apiUrl}/orders/shipping-options`,
      { params: { zipCode: zip } }
    ).subscribe({
      next: opts => {
        this.shippingOptions = opts;
        this.shippingLoading = false;
      },
      error: () => { this.shippingLoading = false; this.shippingError = 'Erro ao calcular frete.'; }
    });
  }

  selectShipping(opt: ShippingOption): void {
    this.selectedShipping = opt;
    this.cartService.setShipping(opt.price);
  }

  applyCoupon(): void {
    if (!this.couponCode.trim()) return;
    this.couponLoading = true;
    this.couponError = '';
    this.http.post<any>(
      `${environment.apiUrl}/orders/validate-coupon`,
      { code: this.couponCode, subtotal: this.cart.subtotal }
    ).subscribe({
      next: res => {
        this.couponLoading = false;
        if (res.isValid) {
          this.cartService.applyCoupon(this.couponCode, res.discountAmount);
          this.toast.success('Cupom aplicado!');
        } else {
          this.couponError = res.errorMessage ?? 'Cupom inválido';
        }
      },
      error: () => { this.couponLoading = false; this.couponError = 'Cupom inválido ou expirado.'; }
    });
  }

  goToStep(s: 'info' | 'shipping' | 'review'): void {
    if (s === 'shipping' && this.form.get('customerName')?.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.step = s;
  }

  placeOrder(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting = true;

    const v = this.form.value;
    const req = {
      customerName:  v.customerName,
      customerEmail: v.customerEmail,
      customerPhone: v.customerPhone,
      customerCpf:   v.customerCpf || undefined,
      newAddress: {
        zipCode:      v.zipCode,
        state:        v.state,
        city:         v.city,
        neighborhood: v.neighborhood,
        street:       v.street,
        number:       v.number,
        complement:   v.complement || undefined,
      },
      shippingOptionId: this.selectedShipping?.id,
      couponCode:       this.cart.couponCode,
      items: this.cart.items.map(i => ({
        productId: i.productId,
        variantId: i.variantId,
        quantity:  i.quantity,
        customizationData: i.customizationData ? JSON.stringify(i.customizationData) : undefined
      }))
    };

    this.orderService.create(req as any).subscribe({
      next: order => {
        // Create Mercado Pago preference
        this.orderService.createPaymentPreference(order.id).subscribe({
          next: pref => {
            this.cartService.clear();
            // Redirect to Mercado Pago
            window.location.href = pref.checkoutUrl;
          },
          error: () => {
            this.submitting = false;
            this.toast.error('Pedido criado mas falha ao gerar link de pagamento. Acesse Minha Conta > Pedidos.');
            this.router.navigate(['/minha-conta/pedidos']);
          }
        });
      },
      error: err => {
        this.submitting = false;
        this.toast.error(err?.error?.message ?? 'Erro ao criar pedido. Tente novamente.');
      }
    });
  }
}
