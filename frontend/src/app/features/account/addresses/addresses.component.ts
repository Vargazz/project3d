import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../../core/services/toast.service';
import { environment } from '../../../../environments/environment';

@Component({ selector: 'app-addresses', templateUrl: './addresses.component.html', styleUrls: ['./addresses.component.css'] })
export class AddressesComponent implements OnInit {
  addresses: any[] = [];
  loading = true;
  showForm = false;
  saving = false;
  form!: FormGroup;

  constructor(private http: HttpClient, private fb: FormBuilder, private toast: ToastService) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      label: ['Casa'], zipCode: ['', Validators.required], state: ['', [Validators.required, Validators.maxLength(2)]],
      city: ['', Validators.required], neighborhood: ['', Validators.required],
      street: ['', Validators.required], number: ['', Validators.required],
      complement: [''], isDefault: [false]
    });
    this.load();
  }

  load(): void {
    this.loading = true;
    this.http.get<any[]>(`${environment.apiUrl}/users/me/addresses`).subscribe({
      next: a => { this.addresses = a; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.http.post(`${environment.apiUrl}/users/me/addresses`, this.form.value).subscribe({
      next: () => { this.saving = false; this.showForm = false; this.form.reset({ label: 'Casa' }); this.load(); this.toast.success('Endereço adicionado!'); },
      error: () => { this.saving = false; }
    });
  }

  remove(id: string): void {
    this.http.delete(`${environment.apiUrl}/users/me/addresses/${id}`).subscribe({
      next: () => { this.load(); this.toast.success('Endereço removido.'); }
    });
  }
}
