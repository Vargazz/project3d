import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../../core/services/toast.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-admin-admins',
  templateUrl: './admin-admins.component.html',
  styleUrls: ['./admin-admins.component.css']
})
export class AdminAdminsComponent implements OnInit {
  admins: any[] = [];
  loading = true;
  showForm = false;
  saving = false;
  resetId: string | null = null;
  resetPwd = '';
  resetting = false;
  form!: FormGroup;

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email:    ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8),
                      Validators.pattern(/^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])/)]],
    });
    this.load();
  }

  load(): void {
    this.loading = true;
    this.http.get<any[]>(`${environment.apiUrl}/admin/admins`).subscribe({
      next: a => { this.admins = a; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  create(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.http.post(`${environment.apiUrl}/admin/admins`, this.form.value).subscribe({
      next: () => {
        this.toast.success('Administrador criado!');
        this.saving = false;
        this.showForm = false;
        this.form.reset();
        this.load();
      },
      error: (err: any) => { this.saving = false; this.toast.error(err?.error?.message ?? 'Erro ao criar admin.'); }
    });
  }

  delete(admin: any): void {
    if (!confirm(`Excluir administrador ${admin.fullName}?`)) return;
    this.http.delete(`${environment.apiUrl}/admin/admins/${admin.id}`).subscribe({
      next: () => { this.toast.success('Admin removido.'); this.load(); },
      error: (err: any) => { this.toast.error(err?.error?.message ?? 'Erro ao remover.'); }
    });
  }

  openReset(id: string): void { this.resetId = id; this.resetPwd = ''; }

  confirmReset(): void {
    if (!this.resetId || !this.resetPwd) return;
    this.resetting = true;
    this.http.patch(`${environment.apiUrl}/admin/admins/${this.resetId}/reset-password`, { newPassword: this.resetPwd }).subscribe({
      next: () => { this.toast.success('Senha redefinida!'); this.resetting = false; this.resetId = null; },
      error: (err: any) => { this.resetting = false; this.toast.error(err?.error?.message ?? 'Erro.'); }
    });
  }
}
