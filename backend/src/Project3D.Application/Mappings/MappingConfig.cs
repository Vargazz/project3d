using Mapster;
using Project3D.Application.DTOs.Products;
using Project3D.Application.DTOs.Orders;
using Project3D.Application.DTOs.CustomRequests;
using Project3D.Application.DTOs.Users;
using Project3D.Domain.Entities;

namespace Project3D.Application.Mappings;

/// <summary>
/// Configurações de mapeamento Mapster.
/// Registradas automaticamente via Scan() no Program.cs.
/// </summary>
public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Produto → ProductDto
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty)
            .Map(dest => dest.SubcategoryName, src => src.Subcategory != null ? src.Subcategory.Name : null)
            .Map(dest => dest.PrimaryImageUrl, src => src.Images
                .Where(i => i.IsPrimary)
                .Select(i => i.Url)
                .FirstOrDefault()
                ?? src.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault());

        // Order → OrderDto (campos de usuário populados via query separada nos services)
        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.CustomerName, src => string.Empty)
            .Map(dest => dest.CustomerEmail, src => (string?)null);

        // CustomRequest → CustomRequestDto
        config.NewConfig<CustomRequest, CustomRequestDto>()
            .Map(dest => dest.CustomerName, src => src.ContactName);
    }
}
