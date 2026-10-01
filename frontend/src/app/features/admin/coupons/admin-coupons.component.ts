import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({ selector: 'app-admin-coupons', templateUrl: './admin-coupons.component.html', styleUrls: ['./admin-coupons.component.css'] })
export class AdminCouponsComponent implements OnInit {
  coupons: any[] = [];
  loading = true;
  showForm = false;
  editCoupon: any = null;
  form: any = { code:'', type:'Percent', value:10, minOrderValue:null, maxUses:null, validFrom:null, validUntil:null, isActive:true };
  saving = false;
  constructor(private admin: AdminService, private toast: ToastService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.loading = true; this.admin.getCoupons().subscribe({ next: c => { this.coupons = c; this.loading = false; }, error: () => { this.loading = false; } }); }
  open(c?: any): void {
    this.editCoupon = c ?? null;
    this.form = c ? { ...c } : { code:'', type:'Percent', value:10, minOrderValue:null, maxUses:null, validFrom:null, validUntil:null, isActive:true };
    this.showForm = true;
  }
  save(): void {
    this.saving = true;
    const obs = this.editCoupon ? this.admin.updateCoupon(this.editCoupon.id, this.form) : this.admin.createCoupon(this.form);
    obs.subscribe({ next: () => { this.toast.success('Cupom salvo!'); this.saving = false; this.showForm = false; this.load(); }, error: () => { this.saving = false; } });
  }
  remove(c: any): void { if (!confirm('Excluir cupom?')) return; this.admin.deleteCoupon(c.id).subscribe({ next: () => { this.toast.success('Cupom excluído.'); this.load(); } }); }
}
