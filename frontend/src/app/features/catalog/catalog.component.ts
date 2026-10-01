import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, combineLatest } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { FormControl } from '@angular/forms';
import { ProductService } from '../../core/services/product.service';
import { Product, Category, ProductFilterRequest } from '../../shared/models/product.models';

@Component({
  selector: 'app-catalog',
  templateUrl: './catalog.component.html',
  styleUrls: ['./catalog.component.css']
})
export class CatalogComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  categories: Category[] = [];
  loading = false;
  totalItems = 0;
  totalPages = 0;
  currentPage = 1;
  pageSize = 12;

  searchControl = new FormControl('');
  selectedCategoryId = '';
  selectedSubcategoryId = '';
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sortBy = 'createdAt';
  sortDirection = 'desc';
  inStockOnly = false;
  showFeatured = false;
  showNew = false;
  sidebarOpen = false;

  sortOptions = [
    { value: 'createdAt|desc', label: 'Mais recentes'  },
    { value: 'price|asc',      label: 'Menor preço'    },
    { value: 'price|desc',     label: 'Maior preço'    },
    { value: 'name|asc',       label: 'A–Z'            },
    { value: 'name|desc',      label: 'Z–A'            },
    { value: 'soldCount|desc', label: 'Mais vendidos'  },
  ];

  // Categorias de fallback — exibidas quando a API não responde
  private readonly fallbackCategories: Category[] = [
    { id: '', name: 'Figures & Miniaturas', slug: 'figures',       isActive: true, displayOrder: 1, subcategories: [], description: null, imageUrl: null } as any,
    { id: '', name: 'Geek & Colecionáveis', slug: 'geek',          isActive: true, displayOrder: 2, subcategories: [], description: null, imageUrl: null } as any,
    { id: '', name: 'Porta-latas',          slug: 'porta-latas',   isActive: true, displayOrder: 3, subcategories: [], description: null, imageUrl: null } as any,
    { id: '', name: 'Chaveiros',            slug: 'chaveiros',     isActive: true, displayOrder: 4, subcategories: [], description: null, imageUrl: null } as any,
    { id: '', name: 'Casa & Decoração',     slug: 'casa',          isActive: true, displayOrder: 5, subcategories: [], description: null, imageUrl: null } as any,
    { id: '', name: 'Produtos Pet',         slug: 'pet',           isActive: true, displayOrder: 6, subcategories: [], description: null, imageUrl: null } as any,
    { id: '', name: 'Utilidades',           slug: 'utilidades',    isActive: true, displayOrder: 7, subcategories: [], description: null, imageUrl: null } as any,
    { id: '', name: 'Decoração',            slug: 'decoracao',     isActive: true, displayOrder: 8, subcategories: [], description: null, imageUrl: null } as any,
    { id: '', name: 'Personalizados',       slug: 'personalizados',isActive: true, displayOrder: 9, subcategories: [], description: null, imageUrl: null } as any,
  ];

  // Retorna categorias da API ou fallback se a API não respondeu
  get displayCategories(): Category[] {
    return this.categories.length > 0 ? this.categories : this.fallbackCategories;
  }

  get sortValue(): string { return `${this.sortBy}|${this.sortDirection}`; }

  get activeCategory(): Category | undefined {
    return this.displayCategories.find(c => c.id === this.selectedCategoryId);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  private destroy$ = new Subject<void>();

  constructor(
    private productService: ProductService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.productService.getCategories(true).subscribe({
      next: cats => { this.categories = cats; },
      error: () => { /* usa fallback */ }
    });

    combineLatest([this.route.params, this.route.queryParams])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([params, query]) => {
        if (params['slug']) {
          // Aguarda categorias carregarem antes de filtrar por slug
          const trySelect = () => {
            const cat = this.displayCategories.find(c => c.slug === params['slug']);
            if (cat?.id) this.selectedCategoryId = cat.id;
          };
          setTimeout(trySelect, 200);
        }
        if (query['search'])     this.searchControl.setValue(query['search'], { emitEvent: false });
        if (query['categoryId']) this.selectedCategoryId = query['categoryId'];
        if (query['isFeatured']) this.showFeatured = query['isFeatured'] === 'true';
        if (query['isNew'])      this.showNew = query['isNew'] === 'true';
        if (query['sortBy'])     this.sortBy = query['sortBy'];
        if (query['page'])       this.currentPage = +query['page'];
        this.loadProducts();
      });

    this.searchControl.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => { this.currentPage = 1; this.loadProducts(); });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  loadProducts(): void {
    this.loading = true;
    const filter: ProductFilterRequest = {
      page:          this.currentPage,
      pageSize:      this.pageSize,
      sortBy:        this.sortBy,
      sortDirection: this.sortDirection,
    };
    if (this.searchControl.value)   filter.search       = this.searchControl.value!;
    if (this.selectedCategoryId)    filter.categoryId   = this.selectedCategoryId;
    if (this.selectedSubcategoryId) filter.subcategoryId = this.selectedSubcategoryId;
    if (this.minPrice != null)      filter.minPrice     = this.minPrice;
    if (this.maxPrice != null)      filter.maxPrice     = this.maxPrice;
    if (this.inStockOnly)           filter.inStock      = true;
    if (this.showFeatured)          filter.isFeatured   = true;
    if (this.showNew)               filter.isNew        = true;

    this.productService.getProducts(filter).subscribe({
      next: r => { this.products = r.items; this.totalItems = r.totalItems; this.totalPages = r.totalPages; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  onSortChange(val: string): void {
    const [by, dir] = val.split('|');
    this.sortBy = by; this.sortDirection = dir;
    this.currentPage = 1; this.loadProducts();
  }

  clearCategoryFilter(): void {
    this.selectedCategoryId = '';
    this.selectedSubcategoryId = '';
    this.currentPage = 1;
    this.loadProducts();
  }

  onCategoryChange(cat: Category): void {
    if (cat.id) {
      // Categoria real da API — filtra por ID
      this.selectedCategoryId = cat.id;
      this.selectedSubcategoryId = '';
      this.currentPage = 1;
      this.loadProducts();
    } else {
      // Categoria do fallback — navega por slug para a rota de categoria
      this.router.navigate(['/categorias', cat.slug]);
    }
  }

  onSubcategoryChange(id: string): void {
    this.selectedSubcategoryId = id; this.currentPage = 1; this.loadProducts();
  }

  applyPriceFilter(): void { this.currentPage = 1; this.loadProducts(); }

  clearFilters(): void {
    this.selectedCategoryId = ''; this.selectedSubcategoryId = '';
    this.minPrice = null; this.maxPrice = null;
    this.inStockOnly = false; this.showFeatured = false; this.showNew = false;
    this.searchControl.setValue('');
    this.sortBy = 'createdAt'; this.sortDirection = 'desc';
    this.currentPage = 1; this.loadProducts();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page; this.loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
