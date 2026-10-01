using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    public decimal Value { get; set; }              // Percentual (0-100) ou valor fixo
    public decimal? MinOrderValue { get; set; }
    public int? MaxUses { get; set; }               // Null = ilimitado
    public int UsedCount { get; set; } = 0;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegação
    public virtual ICollection<Order> Orders { get; set; } = [];
}
