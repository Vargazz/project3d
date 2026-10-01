export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  isActive: boolean;
  displayOrder: number;
  subcategories: Subcategory[];
}

export interface Subcategory {
  id: string;
  categoryId: string;
  name: string;
  slug: string;
  isActive: boolean;
}

export interface ProductImage {
  id: string;
  url: string;
  altText?: string;
  displayOrder: number;
  isPrimary: boolean;
}

export interface ProductVariant {
  id: string;
  name: string;
  value: string;
  type: 'Color' | 'Size' | 'Material' | 'Custom';
  priceModifier: number;
  isActive: boolean;
}

export interface ProductCustomizationField {
  id: string;
  label: string;
  fieldType: 'Text' | 'Select' | 'Color' | 'Number' | 'Textarea';
  isRequired: boolean;
  options?: string;   // JSON string de array de opções
  displayOrder: number;
}

export interface Product {
  id: string;
  name: string;
  slug: string;
  description: string;
  sku: string;
  categoryId: string;
  categoryName: string;
  subcategoryId?: string;
  subcategoryName?: string;
  price: number;
  promotionalPrice?: number;
  isActive: boolean;
  isFeatured: boolean;
  isNew: boolean;
  soldCount: number;
  primaryImageUrl?: string;
  images: ProductImage[];
  variants: ProductVariant[];
  customizationFields: ProductCustomizationField[];
  stockQuantity?: number;
  tags?: string;
  metaTitle?: string;
  metaDescription?: string;
  weightGrams?: number;
  createdAt: string;
}

export interface ProductFilterRequest {
  search?: string;
  categoryId?: string;
  subcategoryId?: string;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  isFeatured?: boolean;
  isNew?: boolean;
  sortBy?: string;
  sortDirection?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  sku: string;
  categoryId: string;
  subcategoryId?: string;
  price: number;
  promotionalPrice?: number;
  isActive: boolean;
  isFeatured: boolean;
  isNew: boolean;
  weightGrams?: number;
  heightCm?: number;
  widthCm?: number;
  depthCm?: number;
  metaTitle?: string;
  metaDescription?: string;
  tags?: string;
  initialStock?: number;
  minStock?: number;
}
