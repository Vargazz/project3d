using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Guid? SubcategoryId { get; set; }

    public decimal Price { get; set; }
    public decimal? PromotionalPrice { get; set; }

    // Dimensões e peso
    public decimal? WeightGrams { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? DepthCm { get; set; }

    // Flags
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsNew { get; set; } = false;
    public int SoldCount { get; set; } = 0;

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; } // JSON array serializado como string

    // Navegação
    public virtual Category Category { get; set; } = null!;
    public virtual Subcategory? Subcategory { get; set; }
    public virtual ICollection<ProductImage> Images { get; set; } = [];
    public virtual ICollection<ProductVariant> Variants { get; set; } = [];
    public virtual ICollection<ProductCustomizationField> CustomizationFields { get; set; } = [];
    public virtual Inventory? Inventory { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
}
