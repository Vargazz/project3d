import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

@Component({ selector: 'app-profile', templateUrl: './profile.component.html', styleUrls: ['./profile.component.css'] })
export class ProfileComponent implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  loading = false;
  pwLoading = false;

  constructor(private fb: FormBuilder, private http: HttpClient, private toast: ToastService, public auth: AuthService) {}

  ngOnInit(): void {
    const u = this.auth.currentUser;
    this.profileForm = this.fb.group({
      fullName: [u?.fullName ?? '', [Validators.required]],
      phone:    [''],
      cpf:      [''],
    });
    this.passwordForm = this.fb.group({
      currentPassword:    ['', Validators.required],
      newPassword:        ['', [Validators.required, Validators.minLength(8)]],
      confirmNewPassword: ['', Validators.required],
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    this.loading = true;
    this.http.put(`${environment.apiUrl}/users/me`, this.profileForm.value).subscribe({
      next: () => { this.loading = false; this.toast.success('Perfil atualizado!'); },
      error: () => { this.loading = false; }
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;
    if (this.passwordForm.value.newPassword !== this.passwordForm.value.confirmNewPassword) {
      this.toast.error('Senhas não conferem.'); return;
    }
    this.pwLoading = true;
    this.auth.changePassword(this.passwordForm.value).subscribe({
      next: () => { this.pwLoading = false; this.toast.success('Senha alterada!'); this.passwordForm.reset(); },
      error: () => { this.pwLoading = false; }
    });
  }
}
