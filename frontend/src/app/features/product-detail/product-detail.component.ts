import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../core/services/toast.service';
import { Product, ProductVariant, ProductCustomizationField } from '../../shared/models/product.models';

@Component({
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css']
})
export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  relatedProducts: Product[] = [];
  loading = true;
  loadingRelated = false;
  notFound = false;

  selectedImageIndex = 0;
  selectedVariant: ProductVariant | null = null;
  quantity = 1;
  customForm!: FormGroup;
  addingToCart = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private productService: ProductService,
    private cartService: CartService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const slug = params['slug'];
      if (slug) this.loadProduct(slug);
    });
  }

  private loadProduct(slug: string): void {
    this.loading = true;
    this.productService.getBySlug(slug).subscribe({
      next: p => {
        this.product = p;
        this.loading = false;
        this.selectedImageIndex = 0;
        this.selectedVariant = null;
        this.buildCustomForm(p.customizationFields);
        this.loadRelated(p.id);
      },
      error: () => { this.loading = false; this.notFound = true; }
    });
  }

  private buildCustomForm(fields: ProductCustomizationField[]): void {
    const group: Record<string, any> = {};
    fields.forEach(f => {
      group[f.id] = [null, f.isRequired ? Validators.required : []];
    });
    this.customForm = this.fb.group(group);
  }

  private loadRelated(id: string): void {
    this.loadingRelated = true;
    this.productService.getRelated(id, 4).subscribe({
      next: r => { this.relatedProducts = r; this.loadingRelated = false; },
      error: () => { this.loadingRelated = false; }
    });
  }

  get currentPrice(): number {
    if (!this.product) return 0;
    const base = this.product.promotionalPrice ?? this.product.price;
    const mod = this.selectedVariant?.priceModifier ?? 0;
    return base + mod;
  }

  get currentImages(): { url: string; altText?: string }[] {
    return this.product?.images?.length ? this.product.images : [];
  }

  get currentImageUrl(): string {
    return this.currentImages[this.selectedImageIndex]?.url ?? this.product?.primaryImageUrl ?? '';
  }

  get hasDiscount(): boolean {
    return !!(this.product?.promotionalPrice && this.product.promotionalPrice < this.product.price);
  }

  get discountPercent(): number {
    if (!this.product || !this.hasDiscount) return 0;
    return Math.round((1 - this.product.promotionalPrice! / this.product.price) * 100);
  }

  get inStock(): boolean {
    return (this.product?.stockQuantity ?? 1) > 0;
  }

  getFieldOptions(field: ProductCustomizationField): string[] {
    if (!field.options) return [];
    try { return JSON.parse(field.options); } catch { return []; }
  }

  selectVariant(variant: ProductVariant): void {
    this.selectedVariant = this.selectedVariant?.id === variant.id ? null : variant;
  }

  changeQty(delta: number): void {
    const max = this.product?.stockQuantity ?? 99;
    this.quantity = Math.min(Math.max(1, this.quantity + delta), max);
  }

  addToCart(): void {
    if (!this.product) return;
    if (this.product.customizationFields.some(f => f.isRequired) && this.customForm.invalid) {
      this.customForm.markAllAsTouched();
      this.toast.warning('Preencha todos os campos obrigatórios de personalização.');
      return;
    }
    this.addingToCart = true;
    const customData: Record<string, string> = {};
    this.product.customizationFields.forEach(f => {
      const val = this.customForm.get(f.id)?.value;
      if (val) customData[f.label] = val;
    });

    this.cartService.addItem({
      productId: this.product.id,
      variantId: this.selectedVariant?.id,
      name: this.product.name,
      variantLabel: this.selectedVariant ? `${this.selectedVariant.name}: ${this.selectedVariant.value}` : undefined,
      price: this.currentPrice,
      imageUrl: this.currentImageUrl,
      slug: this.product.slug,
      quantity: this.quantity,
      stock: this.product.stockQuantity ?? 99,
      customizationData: Object.keys(customData).length ? customData : undefined,
    });

    setTimeout(() => {
      this.addingToCart = false;
      this.toast.success(`${this.product!.name} adicionado ao carrinho!`);
    }, 400);
  }

  buyNow(): void {
    this.addToCart();
    setTimeout(() => this.router.navigate(['/carrinho']), 600);
  }
}
