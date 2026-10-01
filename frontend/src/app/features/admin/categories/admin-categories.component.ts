import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../../core/services/product.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({ selector: 'app-admin-categories', templateUrl: './admin-categories.component.html', styleUrls: ['./admin-categories.component.css'] })
export class AdminCategoriesComponent implements OnInit {
  categories: any[] = [];
  loading = true;
  showForm = false;
  editCat: any = null;
  form: any = { name: '', description: '', isActive: true, displayOrder: 0 };
  saving = false;
  constructor(private service: ProductService, private toast: ToastService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.loading = true; this.service.getCategories(false).subscribe({ next: c => { this.categories = c; this.loading = false; }, error: () => { this.loading = false; } }); }
  open(c?: any): void { this.editCat = c ?? null; this.form = c ? { name:c.name, description:c.description, isActive:c.isActive, displayOrder:c.displayOrder } : { name:'', description:'', isActive:true, displayOrder:0 }; this.showForm = true; }
  save(): void {
    this.saving = true;
    const obs = this.editCat ? this.service.updateCategory(this.editCat.id, this.form) : this.service.createCategory(this.form);
    obs.subscribe({ next: () => { this.toast.success('Salvo!'); this.saving = false; this.showForm = false; this.load(); }, error: () => { this.saving = false; } });
  }
  remove(c: any): void { if (!confirm('Excluir?')) return; this.service.deleteCategory(c.id).subscribe({ next: () => { this.toast.success('Removida.'); this.load(); } }); }
}
