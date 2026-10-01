import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthModalService, AuthModalTab, AuthModalState } from '../../../core/services/auth-modal.service';
import { UserInfo } from '../../models/auth.models';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, OnDestroy {
  user: UserInfo | null = null;
  cartCount = 0;
  unreadNotifications = 0;
  searchQuery = '';
  mobileMenuOpen = false;
  userMenuOpen = false;
  deptMenuOpen = false;
  scrolled = false;
  isDark = false;

  // Modal login
  loginModalOpen = false;
  loginTab: 'login' | 'register' | 'forgot' = 'login';
  private _loginReturnUrl?: string;
  loginForm!: FormGroup;
  registerForm!: FormGroup;
  forgotForm!: FormGroup;
  loginLoading = false;
  registerLoading = false;
  forgotLoading = false;
  forgotSent = false;
  showLoginPwd = false;

  departments = [
    { icon: '🗿', label: 'Figures & Miniaturas', slug: 'figures' },
    { icon: '🎮', label: 'Geek & Colecionáveis',  slug: 'geek' },
    { icon: '🥤', label: 'Porta-latas',           slug: 'porta-latas' },
    { icon: '🔑', label: 'Chaveiros',             slug: 'chaveiros' },
    { icon: '🏠', label: 'Casa & Decoração',      slug: 'casa' },
    { icon: '🐾', label: 'Produtos Pet',          slug: 'pet' },
    { icon: '🔧', label: 'Utilidades',            slug: 'utilidades' },
    { icon: '🪴', label: 'Decoração',             slug: 'decoracao' },
    { icon: '✨', label: 'Personalizados',         slug: 'personalizados' },
  ];

  private destroy$ = new Subject<void>();

  constructor(
    public auth: AuthService,
    private cart: CartService,
    private notifications: NotificationService,
    public theme: ThemeService,
    private toast: ToastService,
    private authModal: AuthModalService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.buildForms();

    this.auth.user$.pipe(takeUntil(this.destroy$)).subscribe(u => {
      this.user = u;
      if (u) { this.loadUnread(); this.closeLoginModal(); }
    });
    this.cart.cart$.pipe(takeUntil(this.destroy$)).subscribe(c => {
      this.cartCount = c.items.reduce((s, i) => s + i.quantity, 0);
    });
    this.notifications.unread$.pipe(takeUntil(this.destroy$)).subscribe(n => {
      this.unreadNotifications = n;
    });
    this.theme.dark$.pipe(takeUntil(this.destroy$)).subscribe(d => this.isDark = d);

    // Escutar o serviço global de modal
    this.authModal.state$.pipe(takeUntil(this.destroy$)).subscribe(state => {
      if (state) {
        this.loginModalOpen = true;
        this.loginTab = state.tab;
        this._loginReturnUrl = state.returnUrl;
        this.forgotSent = false;
      } else {
        this.loginModalOpen = false;
        this._loginReturnUrl = undefined;
      }
    });
  }

  @HostListener('window:scroll')
  onScroll(): void { this.scrolled = window.scrollY > 10; }

  @HostListener('document:keydown.escape')
  onEsc(): void { this.closeLoginModal(); this.userMenuOpen = false; this.deptMenuOpen = false; }

  @HostListener('document:click', ['$event'])
  onDocClick(e: MouseEvent): void {
    const t = e.target as HTMLElement;
    if (!t.closest('.user-menu-wrap')) this.userMenuOpen = false;
    if (!t.closest('.dept-menu-wrap')) this.deptMenuOpen = false;
  }

  // ── Busca ────────────────────────────────────────────────────────────────
  search(): void {
    const q = this.searchQuery.trim();
    if (!q) return;
    this.router.navigate(['/produtos'], { queryParams: { search: q } });
    this.searchQuery = '';
    this.mobileMenuOpen = false;
  }
  onKey(e: KeyboardEvent): void { if (e.key === 'Enter') this.search(); }

  // ── Auth ─────────────────────────────────────────────────────────────────
  logout(): void { this.userMenuOpen = false; this.auth.logout(); }

  // ── Modal login ──────────────────────────────────────────────────────────
  openLoginModal(tab: AuthModalTab = 'login'): void {
    this.authModal.open(tab);
    this.loginForm.reset();
    this.registerForm.reset();
    this.forgotForm.reset();
    this.forgotSent = false;
  }

  closeLoginModal(): void {
    this.authModal.close();
  }

  submitLogin(): void {
    if (this.loginForm.invalid) { this.loginForm.markAllAsTouched(); return; }
    this.loginLoading = true;
    this.auth.login(this.loginForm.value).subscribe({
      next: () => {
        this.loginLoading = false;
        this.toast.success('Bem-vindo de volta!');
        const returnUrl = this._loginReturnUrl;
        this.closeLoginModal();
        if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
        }
      },
      error: (err: any) => {
        this.loginLoading = false;
        this.toast.error(err?.error?.message ?? 'E-mail ou senha incorretos.');
      }
    });
  }

  submitForgot(): void {
    if (this.forgotForm.invalid) { this.forgotForm.markAllAsTouched(); return; }
    this.forgotLoading = true;
    this.auth.forgotPassword(this.forgotForm.value).subscribe({
      next: () => { this.forgotLoading = false; this.forgotSent = true; },
      error: () => { this.forgotLoading = false; this.forgotSent = true; } // não revelar
    });
  }

  submitRegister(): void {
    if (this.registerForm.invalid) { this.registerForm.markAllAsTouched(); return; }
    this.registerLoading = true;
    const { confirmPassword, ...req } = this.registerForm.value;
    this.auth.register(req).subscribe({
      next: () => {
        this.registerLoading = false;
        this.toast.success('Conta criada! Bem-vindo à VM3D!');
        const returnUrl = this._loginReturnUrl;
        this.closeLoginModal();
        if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
        }
      },
      error: (err: any) => {
        this.registerLoading = false;
        this.toast.error(err?.error?.message ?? 'Erro ao criar conta.');
      }
    });
  }

  // ── Helpers ──────────────────────────────────────────────────────────────
  private buildForms(): void {
    this.loginForm = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });

    this.registerForm = this.fb.group({
      fullName:        ['', [Validators.required, Validators.minLength(3)]],
      email:           ['', [Validators.required, Validators.email]],
      password:        ['', [Validators.required, Validators.minLength(8),
                             Validators.pattern(/^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])/)]],
      confirmPassword: ['', Validators.required],
    }, { validators: this.passwordMatch });

    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  private passwordMatch(g: AbstractControl) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  private loadUnread(): void {
    this.notifications.getNotifications(true, 1).subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    document.body.style.overflow = '';
  }
}
