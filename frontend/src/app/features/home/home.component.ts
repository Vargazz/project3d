import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ProductService } from '../../core/services/product.service';
import { Product, Category } from '../../shared/models/product.models';

interface SlideItem {
  image: string;
  title: string;
  sub: string;
  cta: string;
  link: string;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  // ── Slider ────────────────────────────────────────────────────────────────
  slides: SlideItem[] = [
    { image: 'assets/slides/sauron-holder.webp', title: 'Suporte Sauron PS5',   sub: 'Impressão 3D de alta definição — peças únicas para seu setup gamer',          cta: 'Ver produtos', link: '/categorias/geek'    },
    { image: 'assets/slides/kratos-holder.webp', title: 'Suporte Kratos',        sub: 'God of War em tamanho real para segurar seus controles com estilo',            cta: 'Ver produtos', link: '/categorias/geek'    },
    { image: 'assets/slides/daredevil.webp',      title: 'Daredevil Miniatura',   sub: 'Figures e miniaturas com nível profissional de detalhamento',                  cta: 'Ver Figures',  link: '/categorias/figures' },
    { image: 'assets/slides/joker.webp',          title: 'Joker — Heath Ledger',  sub: 'Colecionáveis com alto nível de detalhamento',                                 cta: 'Ver coleção',  link: '/categorias/figures' },
    { image: 'assets/slides/batman.webp',         title: 'Batman Armored',        sub: 'Modelos premium com acabamento artesanal e impressão 3D de qualidade',         cta: 'Ver produtos', link: '/produtos'           },
  ];

  currentSlide = 0;
  private slideTimer: any;

  // ── Dados ─────────────────────────────────────────────────────────────────
  categories: Category[] = [];
  featuredProducts: Product[] = [];
  bestSellers: Product[] = [];
  newestProducts: Product[] = [];
  loading = true;

  // ── Abas de categorias ────────────────────────────────────────────────────
  activeTab = 'destaque';
  tabs = [
    { id: 'destaque',    label: '⭐ Destaques' },
    { id: 'vendidos',    label: '🔥 Mais vendidos' },
    { id: 'novidades',   label: '🆕 Novidades' },
    { id: 'figures',     label: '🗿 Figures' },
    { id: 'geek',        label: '🎮 Geek' },
    { id: 'pets',        label: '🐾 Pets' },
    { id: 'casa',        label: '🏠 Casa' },
    { id: 'chaveiros',   label: '🔑 Chaveiros' },
    { id: 'utilidades',  label: '🔧 Utilidades' },
  ];

  tabProducts: Product[] = [];
  loadingTab = false;

  benefits = [
    { icon: '🖨️', title: 'Impressão 3D Premium',     desc: 'Filamentos de alta qualidade, acabamento profissional' },
    { icon: '🎨', title: 'Diversas cores',             desc: 'Amplo catálogo de filamentos e acabamentos' },
    { icon: '⚡', title: 'Sob demanda',                desc: 'Produzimos só o que você pede, sem estoque parado' },
    { icon: '✏️', title: '100% personalizável',        desc: 'Do modelo à cor — do seu jeito' },
    { icon: '💬', title: 'Atendimento exclusivo',      desc: 'Suporte personalizado do início ao fim' },
  ];

  private categoryProducts: Map<string, Product[]> = new Map();

  constructor(
    private productService: ProductService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.startSlider();
    forkJoin({
      featured:    this.productService.getFeatured(8),
      bestSellers: this.productService.getBestSellers(8),
      newest:      this.productService.getNewest(8),
      categories:  this.productService.getCategories(true),
    }).subscribe({
      next: r => {
        this.featuredProducts = r.featured;
        this.bestSellers      = r.bestSellers;
        this.newestProducts   = r.newest;
        this.categories       = r.categories;
        this.tabProducts      = r.featured;
        this.loading          = false;
      },
      error: () => { this.loading = false; }
    });
  }

  // ── Slider controls ───────────────────────────────────────────────────────
  startSlider(): void {
    this.slideTimer = setInterval(() => {
      this.nextSlide();
      this.cdr.markForCheck();
    }, 4000);
  }

  nextSlide(): void  { this.currentSlide = (this.currentSlide + 1) % this.slides.length; }
  prevSlide(): void  { this.currentSlide = (this.currentSlide - 1 + this.slides.length) % this.slides.length; }
  goToSlide(i: number): void { this.currentSlide = i; this.resetTimer(); }

  resetTimer(): void {
    clearInterval(this.slideTimer);
    this.startSlider();
  }

  // ── Abas de produto ───────────────────────────────────────────────────────
  setTab(id: string): void {
    this.activeTab = id;
    // Se já temos dados em cache, usar
    const cached = this.categoryProducts.get(id);
    if (cached) { this.tabProducts = cached; return; }

    switch (id) {
      case 'destaque':  this.tabProducts = this.featuredProducts; break;
      case 'vendidos':  this.tabProducts = this.bestSellers; break;
      case 'novidades': this.tabProducts = this.newestProducts; break;
      default:
        this.loadingTab = true;
        const cat = this.categories.find(c => c.slug === id);
        if (cat) {
          this.productService.getProducts({ categoryId: cat.id, pageSize: 8 }).subscribe({
            next: r => {
              this.categoryProducts.set(id, r.items);
              this.tabProducts = r.items;
              this.loadingTab = false;
            },
            error: () => { this.loadingTab = false; }
          });
        } else {
          this.tabProducts = [];
          this.loadingTab = false;
        }
    }
  }

  getCategoryEmoji(slug: string): string {
    const map: Record<string, string> = {
      casa:'🏠', decoracao:'🪴', pet:'🐾', geek:'🎮',
      chaveiros:'🔑', 'porta-latas':'🥤', utilidades:'🔧',
      figures:'🗿', personalizados:'✨'
    };
    return map[slug] ?? '📦';
  }

  ngOnDestroy(): void { clearInterval(this.slideTimer); }
}
