import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { Product } from '../../models/product.models';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-product-card',
  templateUrl: './product-card.component.html',
  styleUrls: ['./product-card.component.css']
})
export class ProductCardComponent {
  @Input() product!: Product;
  @Input() compact = false;
  @Output() addToCart = new EventEmitter<Product>();

  constructor(
    private router: Router,
    private cart: CartService,
    private toast: ToastService
  ) {}

  get displayPrice(): number {
    return this.product.promotionalPrice ?? this.product.price;
  }

  get hasDiscount(): boolean {
    return !!this.product.promotionalPrice && this.product.promotionalPrice < this.product.price;
  }

  get discountPercent(): number {
    if (!this.hasDiscount) return 0;
    return Math.round((1 - this.product.promotionalPrice! / this.product.price) * 100);
  }

  get inStock(): boolean {
    return (this.product.stockQuantity ?? 1) > 0;
  }

  navigate(): void {
    this.router.navigate(['/produtos', this.product.slug]);
  }

  onAddToCart(e: Event): void {
    e.stopPropagation();
    if (!this.inStock) return;

    // Se produto tem variações ou personalização, navegar para a página do produto
    if (this.product.variants.length > 0 || this.product.customizationFields.length > 0) {
      this.router.navigate(['/produtos', this.product.slug]);
      return;
    }

    this.cart.addItem({
      productId: this.product.id,
      name: this.product.name,
      price: this.displayPrice,
      imageUrl: this.product.primaryImageUrl,
      slug: this.product.slug,
      quantity: 1,
      stock: this.product.stockQuantity ?? 99
    });
    this.toast.success(`${this.product.name} adicionado ao carrinho!`);
    this.addToCart.emit(this.product);
  }
}
