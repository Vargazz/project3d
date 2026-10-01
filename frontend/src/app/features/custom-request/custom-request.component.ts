import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CustomRequestService } from '../../core/services/custom-request.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

interface ImagePreview { file: File; url: string; name: string; }

@Component({ selector: 'app-custom-request', templateUrl: './custom-request.component.html', styleUrls: ['./custom-request.component.css'] })
export class CustomRequestComponent implements OnInit {
  form!: FormGroup;
  images: ImagePreview[] = [];
  loading = false;
  submitted = false;
  requestId: string | null = null;
  uploadingImages = false;
  dragOver = false;

  productTypes = ['Funko Pop', 'Miniatura', 'Action Figure', 'Chaveiro', 'Porta-lata', 'Comedouro Pet', 'Organização', 'Decoração', 'Presente', 'Outro'];

  constructor(
    private fb: FormBuilder,
    private service: CustomRequestService,
    private auth: AuthService,
    private toast: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const u = this.auth.currentUser;
    this.form = this.fb.group({
      productType:    ['', Validators.required],
      description:    ['', [Validators.required, Validators.minLength(20)]],
      characteristics:[''],
      desiredColors:  [''],
      approximateSize:[''],
      quantity:       [1, [Validators.required, Validators.min(1)]],
      observations:   [''],
      contactName:    [u?.fullName ?? '', Validators.required],
      contactEmail:   [u?.email ?? '', [Validators.required, Validators.email]],
      contactWhatsApp:[''],
    });
  }

  onFileDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
    const files = event.dataTransfer?.files;
    if (files) this.addFiles(Array.from(files));
  }

  onFileInput(event: Event): void {
    const files = (event.target as HTMLInputElement).files;
    if (files) this.addFiles(Array.from(files));
  }

  private addFiles(files: File[]): void {
    const allowed = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    const maxMb = 10;
    files.forEach(f => {
      if (!allowed.includes(f.type)) { this.toast.error(`${f.name}: tipo não permitido.`); return; }
      if (f.size > maxMb * 1024 * 1024) { this.toast.error(`${f.name}: máximo ${maxMb}MB.`); return; }
      if (this.images.length >= 5) { this.toast.warning('Máximo de 5 imagens.'); return; }
      const reader = new FileReader();
      reader.onload = (e) => {
        this.images.push({ file: f, url: e.target!.result as string, name: f.name });
      };
      reader.readAsDataURL(f);
    });
  }

  removeImage(i: number): void { this.images.splice(i, 1); }

  async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    try {
      const req = await this.service.create(this.form.value).toPromise();
      if (req && this.images.length) {
        this.uploadingImages = true;
        for (const img of this.images) {
          await this.service.addImage(req.id, img.file).toPromise();
        }
        this.uploadingImages = false;
      }
      this.requestId = req?.id ?? null;
      this.submitted = true;
      this.loading = false;
    } catch {
      this.loading = false;
      this.uploadingImages = false;
    }
  }
}
