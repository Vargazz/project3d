import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../../core/services/product.service';
import { ToastService } from '../../../core/services/toast.service';
import { Product } from '../../../shared/models/product.models';

@Component({
  selector: 'app-admin-products',
  templateUrl: './admin-products.component.html',
  styleUrls: ['./admin-products.component.css']
})
export class AdminProductsComponent implements OnInit {
  products: Product[] = [];
  categories: any[] = [];
  loading = true;
  search = '';
  page = 1; pageSize = 20; total = 0; totalPages = 0;

  showForm = false;
  editProduct: Product | null = null;
  saving = false;
  form: any = { name:'', description:'', sku:'', categoryId:'', price:0, isActive:true, isFeatured:false, isNew:false, initialStock:0, minStock:5 };

  // Imagem
  selectedImageFile: File | null = null;
  imagePreviewUrl: string | null = null;
  uploadingImage = false;

  constructor(private service: ProductService, private toast: ToastService) {}

  ngOnInit(): void {
    this.load();
    this.service.getCategories(false).subscribe(c => this.categories = c);
  }

  load(): void {
    this.loading = true;
    this.service.getProducts({ search: this.search || undefined, page: this.page, pageSize: this.pageSize }).subscribe({
      next: r => { this.products = r.items; this.total = r.totalItems; this.totalPages = r.totalPages; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  openCreate(): void {
    this.editProduct = null;
    this.form = { name:'', description:'', sku:'', categoryId:'', price:0, isActive:true, isFeatured:false, isNew:false, initialStock:0, minStock:5 };
    this.clearImage();
    this.showForm = true;
  }

  openEdit(p: Product): void {
    this.editProduct = p;
    this.form = { name:p.name, description:p.description, sku:p.sku, categoryId:p.categoryId, price:p.price, promotionalPrice:p.promotionalPrice, isActive:p.isActive, isFeatured:p.isFeatured, isNew:p.isNew };
    this.clearImage();
    // Mostra imagem existente como preview
    if (p.primaryImageUrl) {
      this.imagePreviewUrl = p.primaryImageUrl;
    }
    this.showForm = true;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      this.toast.error('Selecione um arquivo de imagem (JPG, PNG, WebP).');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.toast.error('A imagem deve ter no máximo 5 MB.');
      return;
    }
    this.selectedImageFile = file;
    const reader = new FileReader();
    reader.onload = e => { this.imagePreviewUrl = e.target?.result as string; };
    reader.readAsDataURL(file);
  }

  clearImage(): void {
    this.selectedImageFile = null;
    this.imagePreviewUrl = null;
  }

  save(): void {
    this.saving = true;
    const obs = this.editProduct
      ? this.service.update(this.editProduct.id, this.form)
      : this.service.create(this.form);

    obs.subscribe({
      next: (savedProduct: Product) => {
        // Faz upload da imagem selecionada, se houver
        if (this.selectedImageFile) {
          this.uploadingImage = true;
          this.service.addImage(savedProduct.id, this.selectedImageFile).subscribe({
            next: () => {
              this.uploadingImage = false;
              this.toast.success(this.editProduct ? 'Produto atualizado com foto!' : 'Produto criado com foto!');
              this.saving = false;
              this.showForm = false;
              this.load();
            },
            error: () => {
              this.uploadingImage = false;
              this.toast.error('Produto salvo, mas falha ao enviar a imagem. Tente novamente.');
              this.saving = false;
              this.showForm = false;
              this.load();
            }
          });
        } else {
          this.toast.success(this.editProduct ? 'Produto atualizado!' : 'Produto criado!');
          this.saving = false;
          this.showForm = false;
          this.load();
        }
      },
      error: () => { this.saving = false; }
    });
  }

  toggle(p: Product): void {
    this.service.toggle(p.id).subscribe({ next: () => this.load(), error: () => {} });
  }

  delete(p: Product): void {
    if (!confirm(`Excluir "${p.name}"?`)) return;
    this.service.delete(p.id).subscribe({
      next: () => { this.toast.success('Produto excluído.'); this.load(); }
    });
  }
}
