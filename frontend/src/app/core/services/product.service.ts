import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product, ProductFilterRequest, PagedResult, Category } from '../../shared/models/product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly api = `${environment.apiUrl}/products`;
  private readonly catApi = `${environment.apiUrl}/categories`;

  constructor(private http: HttpClient) {}

  getProducts(filter: ProductFilterRequest = {}): Observable<PagedResult<Product>> {
    let params = new HttpParams();
    Object.entries(filter).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== '') params = params.set(k, String(v));
    });
    return this.http.get<PagedResult<Product>>(this.api, { params });
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.api}/${id}`);
  }

  getBySlug(slug: string): Observable<Product> {
    return this.http.get<Product>(`${this.api}/slug/${slug}`);
  }

  getFeatured(limit = 8): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.api}/featured`, { params: { limit } });
  }

  getNewest(limit = 8): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.api}/newest`, { params: { limit } });
  }

  getBestSellers(limit = 8): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.api}/best-sellers`, { params: { limit } });
  }

  getRelated(id: string, limit = 4): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.api}/${id}/related`, { params: { limit } });
  }

  // ---- Admin ----
  create(data: any): Observable<Product> {
    return this.http.post<Product>(this.api, data);
  }

  update(id: string, data: any): Observable<Product> {
    return this.http.put<Product>(`${this.api}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }

  toggle(id: string): Observable<Product> {
    return this.http.patch<Product>(`${this.api}/${id}/toggle`, {});
  }

  addImage(id: string, file: File): Observable<{ url: string }> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<{ url: string }>(`${this.api}/${id}/images`, fd);
  }

  removeImage(productId: string, imageId: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${productId}/images/${imageId}`);
  }

  // ---- Categories ----
  getCategories(activeOnly = true): Observable<Category[]> {
    return this.http.get<Category[]>(this.catApi, { params: { activeOnly } });
  }

  createCategory(data: any): Observable<Category> {
    return this.http.post<Category>(this.catApi, data);
  }

  updateCategory(id: string, data: any): Observable<Category> {
    return this.http.put<Category>(`${this.catApi}/${id}`, data);
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.catApi}/${id}`);
  }
}
