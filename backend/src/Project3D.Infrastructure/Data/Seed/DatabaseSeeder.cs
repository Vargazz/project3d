using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Project3D.Domain.Entities;
using Project3D.Domain.Enums;
using Project3D.Infrastructure.Identity;

namespace Project3D.Infrastructure.Data.Seed;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Iniciando seed do banco de dados...");

        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedGuilhermeAdminAsync();
        await SeedCategoriesAsync();
        await SeedProductsAsync();
        await SeedShippingOptionsAsync();
        await SeedStoreSettingsAsync();

        _logger.LogInformation("Seed concluído.");
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Customer" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                _logger.LogInformation("Role criada: {Role}", role);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var admins = new[]
        {
            (
                Email: _configuration["Seed:AdminEmail"] ?? "admin@project3d.com",
                Password: _configuration["Seed:AdminPassword"] ?? "Admin@123456",
                Name: _configuration["Seed:AdminName"] ?? "Administrador"
            ),
            (
                Email: "guilherme.vargas@vm3d.com",
                Password: "Admin@123456",
                Name: "Guilherme Vargas"
            ),
        };

        foreach (var a in admins)
        {
            var existing = await _userManager.FindByEmailAsync(a.Email);
            if (existing is not null) continue;

            var admin = new ApplicationUser
            {
                FullName = a.Name,
                Email = a.Email,
                UserName = a.Email,
                EmailConfirmed = true,
                Role = UserRole.Admin,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(admin, a.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                _logger.LogInformation("Admin criado: {Email}", a.Email);
            }
            else
            {
                _logger.LogError("Falha ao criar admin {Email}: {Errors}", a.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedGuilhermeAdminAsync()
    {
        const string email = "guilherme.vargas@vm3d.com";
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null) return;

        var admin = new ApplicationUser
        {
            FullName = "Guilherme Vargas",
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            Role = UserRole.Admin,
            IsActive = true
        };

        var password = _configuration["Seed:GuilhermePassword"] ?? "Admin@123456";
        var result = await _userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
            _logger.LogInformation("Admin Guilherme criado: {Email}", email);
        }
        else
        {
            _logger.LogError("Falha ao criar admin Guilherme: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Casa", Slug = "casa", Description = "Produtos para sua casa impressos em 3D", IsActive = true, DisplayOrder = 1 },
            new() { Name = "Decoração", Slug = "decoracao", Description = "Peças decorativas únicas", IsActive = true, DisplayOrder = 2 },
            new() { Name = "Pet", Slug = "pet", Description = "Produtos para animais de estimação", IsActive = true, DisplayOrder = 3 },
            new() { Name = "Geek", Slug = "geek", Description = "Itens geek e colecionáveis", IsActive = true, DisplayOrder = 4 },
            new() { Name = "Chaveiros", Slug = "chaveiros", Description = "Chaveiros personalizados", IsActive = true, DisplayOrder = 5 },
            new() { Name = "Porta-latas", Slug = "porta-latas", Description = "Porta-latas e acessórios para bebidas", IsActive = true, DisplayOrder = 6 },
            new() { Name = "Utilidades", Slug = "utilidades", Description = "Utensílios e organizadores", IsActive = true, DisplayOrder = 7 },
            new() { Name = "Figures", Slug = "figures", Description = "Action figures, miniaturas e colecionáveis", IsActive = true, DisplayOrder = 8 },
            new() { Name = "Personalizados", Slug = "personalizados", Description = "Produtos 100% personalizados sob demanda", IsActive = true, DisplayOrder = 9 },
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
        _logger.LogInformation("{Count} categorias criadas.", categories.Count);
    }

    private async Task SeedProductsAsync()
    {
        if (await _context.Products.AnyAsync()) return;

        var catPortaLatas = await _context.Categories.FirstAsync(c => c.Slug == "porta-latas");
        var catGeek = await _context.Categories.FirstAsync(c => c.Slug == "geek");
        var catChaveiros = await _context.Categories.FirstAsync(c => c.Slug == "chaveiros");
        var catDecoracao = await _context.Categories.FirstAsync(c => c.Slug == "decoracao");
        var catPet = await _context.Categories.FirstAsync(c => c.Slug == "pet");

        var products = new List<Product>
        {
            new()
            {
                Name = "Porta-lata Monster Personalizado",
                Slug = "porta-lata-monster-personalizado",
                Description = "Porta-lata para lata 473ml (Monster, Red Bull, etc) com acabamento premium. Disponível em diversas cores. Pode ser personalizado com nome ou texto.",
                Sku = "PL-MONSTER-001",
                CategoryId = catPortaLatas.Id,
                Price = 39.90m,
                PromotionalPrice = 34.90m,
                WeightGrams = 120m,
                HeightCm = 15m,
                WidthCm = 7m,
                DepthCm = 7m,
                IsActive = true,
                IsFeatured = true,
                IsNew = true,
                MetaTitle = "Porta-lata Monster Personalizado | Impressão 3D",
                MetaDescription = "Porta-lata para Monster personalizado com seu nome. Impressão 3D de alta qualidade.",
                Tags = "[\"porta-lata\",\"monster\",\"personalizado\",\"impressão 3D\"]"
            },
            new()
            {
                Name = "Chaveiro Personalizado com Nome",
                Slug = "chaveiro-personalizado-nome",
                Description = "Chaveiro resistente impresso em 3D com seu nome ou texto personalizado. Acabamento liso ou texturizado.",
                Sku = "CH-NOME-001",
                CategoryId = catChaveiros.Id,
                Price = 19.90m,
                WeightGrams = 15m,
                HeightCm = 4m,
                WidthCm = 2m,
                DepthCm = 0.5m,
                IsActive = true,
                IsFeatured = true,
                IsNew = false,
                MetaTitle = "Chaveiro Personalizado com Nome | Impressão 3D",
                MetaDescription = "Chaveiro com seu nome impresso em 3D. Diversas cores disponíveis.",
                Tags = "[\"chaveiro\",\"personalizado\",\"nome\"]"
            },
            new()
            {
                Name = "Funko Pop Personalizado",
                Slug = "funko-pop-personalizado",
                Description = "Figure no estilo Funko Pop baseado em foto. Envie uma foto e criamos um modelo 3D único para você. Perfeito para presentes!",
                Sku = "FG-FUNKO-001",
                CategoryId = catGeek.Id,
                Price = 149.90m,
                WeightGrams = 80m,
                HeightCm = 10m,
                WidthCm = 6m,
                DepthCm = 6m,
                IsActive = true,
                IsFeatured = true,
                IsNew = true,
                MetaTitle = "Funko Pop Personalizado | Impressão 3D",
                MetaDescription = "Crie seu próprio Funko Pop personalizado em impressão 3D baseado em foto.",
                Tags = "[\"funko pop\",\"personalizado\",\"presente\",\"geek\",\"colecionável\"]"
            },
            new()
            {
                Name = "Organizador de Mesa Modular",
                Slug = "organizador-mesa-modular",
                Description = "Organizador de mesa composto por módulos encaixáveis. Ideal para canetas, clipes, post-its e outros itens. Personalize as cores.",
                Sku = "ORG-MESA-001",
                CategoryId = catDecoracao.Id,
                Price = 59.90m,
                WeightGrams = 200m,
                HeightCm = 10m,
                WidthCm = 20m,
                DepthCm = 12m,
                IsActive = true,
                IsFeatured = false,
                IsNew = true,
                Tags = "[\"organizador\",\"mesa\",\"escritório\",\"modular\"]"
            },
            new()
            {
                Name = "Comedouro Pet Personalizado",
                Slug = "comedouro-pet-personalizado",
                Description = "Comedouro com o nome do seu pet impresso em 3D. Material resistente e fácil de higienizar. Disponível para cão e gato.",
                Sku = "PET-COMEDU-001",
                CategoryId = catPet.Id,
                Price = 44.90m,
                WeightGrams = 180m,
                HeightCm = 6m,
                WidthCm = 18m,
                DepthCm = 18m,
                IsActive = true,
                IsFeatured = true,
                IsNew = false,
                Tags = "[\"pet\",\"comedouro\",\"personalizado\",\"cachorro\",\"gato\"]"
            },
            new()
            {
                Name = "Miniatura de Personagem Geek",
                Slug = "miniatura-personagem-geek",
                Description = "Miniaturas de personagens de jogos, animes e filmes. Alta fidelidade e detalhamento. Consulte disponibilidade do personagem desejado.",
                Sku = "MIN-GEEK-001",
                CategoryId = catGeek.Id,
                Price = 79.90m,
                PromotionalPrice = 69.90m,
                WeightGrams = 60m,
                HeightCm = 8m,
                WidthCm = 5m,
                DepthCm = 5m,
                IsActive = true,
                IsFeatured = false,
                IsNew = false,
                Tags = "[\"miniatura\",\"geek\",\"anime\",\"games\",\"colecionável\"]"
            }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Criar estoque para cada produto
        var inventories = products.Select(p => new Inventory
        {
            ProductId = p.Id,
            Quantity = 20,
            MinStock = 5
        }).ToList();

        await _context.Inventories.AddRangeAsync(inventories);
        await _context.SaveChangesAsync();

        // ── Produtos extras baseados nas fotos dos slides ─────────────────
        var catFigures = await _context.Categories.FirstAsync(c => c.Slug == "figures");

        var slideProducts = new List<Product>
        {
            new()
            {
                Name = "Suporte Sauron PS5",
                Slug = "suporte-sauron-ps5",
                Description = "Suporte para controle PS5 no estilo Sauron — o Senhor dos Anéis. Impressão 3D com alto nível de detalhamento. Perfeito para gamers colecionadores.",
                Sku = "SUP-SAURON-001",
                CategoryId = catGeek.Id,
                Price = 189.90m,
                PromotionalPrice = 159.90m,
                WeightGrams = 280m, HeightCm = 25m, WidthCm = 15m, DepthCm = 15m,
                IsActive = true, IsFeatured = true, IsNew = true,
                SoldCount = 47,
                Tags = "[\"suporte\",\"ps5\",\"sauron\",\"senhor dos aneis\",\"gamer\"]",
                MetaTitle = "Suporte Sauron PS5 | VM3D",
                MetaDescription = "Suporte para controle PS5 Sauron impresso em 3D."
            },
            new()
            {
                Name = "Suporte Kratos God of War",
                Slug = "suporte-kratos-god-of-war",
                Description = "Suporte para controles no estilo Kratos de God of War. Impressão 3D com pintura artesanal opcional. Aguenta até 2 controles simultaneamente.",
                Sku = "SUP-KRATOS-001",
                CategoryId = catGeek.Id,
                Price = 219.90m,
                WeightGrams = 320m, HeightCm = 22m, WidthCm = 22m, DepthCm = 15m,
                IsActive = true, IsFeatured = true, IsNew = true,
                SoldCount = 38,
                Tags = "[\"suporte\",\"kratos\",\"god of war\",\"ps5\",\"gamer\"]",
                MetaTitle = "Suporte Kratos God of War | VM3D",
                MetaDescription = "Suporte para controles Kratos God of War impresso em 3D."
            },
            new()
            {
                Name = "Miniatura Daredevil",
                Slug = "miniatura-daredevil",
                Description = "Miniatura colecionável do Daredevil em escala 1:8. Alta fidelidade ao personagem, com base decorativa. Impressão 3D com filamento premium.",
                Sku = "MIN-DAREDEVIL-001",
                CategoryId = catFigures.Id,
                Price = 129.90m,
                PromotionalPrice = 109.90m,
                WeightGrams = 150m, HeightCm = 18m, WidthCm = 10m, DepthCm = 10m,
                IsActive = true, IsFeatured = true, IsNew = false,
                SoldCount = 62,
                Tags = "[\"daredevil\",\"marvel\",\"miniatura\",\"colecionável\",\"figures\"]",
                MetaTitle = "Miniatura Daredevil | VM3D",
                MetaDescription = "Miniatura colecionável Daredevil Marvel impressa em 3D."
            },
            new()
            {
                Name = "Estátua Joker Heath Ledger",
                Slug = "estatua-joker-heath-ledger",
                Description = "Estátua colecionável do Joker inspirada na performance de Heath Ledger. Detalhamento artesanal, base em concreto impresso. Peça exclusiva.",
                Sku = "EST-JOKER-001",
                CategoryId = catFigures.Id,
                Price = 249.90m,
                WeightGrams = 400m, HeightCm = 25m, WidthCm = 15m, DepthCm = 12m,
                IsActive = true, IsFeatured = true, IsNew = false,
                SoldCount = 29,
                Tags = "[\"joker\",\"batman\",\"dc\",\"estatua\",\"colecionável\",\"heath ledger\"]",
                MetaTitle = "Estátua Joker Heath Ledger | VM3D",
                MetaDescription = "Estátua colecionável Joker impressa em 3D."
            },
            new()
            {
                Name = "Batman Armored Miniatura",
                Slug = "batman-armored-miniatura",
                Description = "Miniatura do Batman Armored em impressão 3D com alto detalhamento. Base decorativa inclusa. Disponível em cinza grafite ou preto fosco.",
                Sku = "MIN-BATMAN-001",
                CategoryId = catFigures.Id,
                Price = 159.90m,
                WeightGrams = 220m, HeightCm = 20m, WidthCm = 10m, DepthCm = 10m,
                IsActive = true, IsFeatured = false, IsNew = false,
                SoldCount = 51,
                Tags = "[\"batman\",\"dc\",\"miniatura\",\"armored\",\"colecionável\"]",
                MetaTitle = "Batman Armored Miniatura | VM3D",
                MetaDescription = "Miniatura Batman Armored impressa em 3D."
            },
        };

        await _context.Products.AddRangeAsync(slideProducts);
        await _context.SaveChangesAsync();

        // Estoque dos slide products
        var slideInventories = slideProducts.Select(p => new Inventory
        {
            ProductId = p.Id,
            Quantity = 15,
            MinStock = 3
        }).ToList();
        await _context.Inventories.AddRangeAsync(slideInventories);

        // Imagens dos slide products (usando as fotos que já existem no frontend)
        // Em produção o Angular serve de /assets/slides/ — em dev é o mesmo caminho
        var slideImages = new List<ProductImage>
        {
            new() { ProductId = slideProducts[0].Id, Url = "/assets/slides/sauron-holder.webp",  AltText = "Suporte Sauron PS5",        IsPrimary = true, DisplayOrder = 0 },
            new() { ProductId = slideProducts[1].Id, Url = "/assets/slides/kratos-holder.webp",  AltText = "Suporte Kratos God of War",  IsPrimary = true, DisplayOrder = 0 },
            new() { ProductId = slideProducts[2].Id, Url = "/assets/slides/daredevil.webp",       AltText = "Miniatura Daredevil",        IsPrimary = true, DisplayOrder = 0 },
            new() { ProductId = slideProducts[3].Id, Url = "/assets/slides/joker.webp",           AltText = "Estátua Joker",              IsPrimary = true, DisplayOrder = 0 },
            new() { ProductId = slideProducts[4].Id, Url = "/assets/slides/batman.webp",          AltText = "Batman Armored Miniatura",   IsPrimary = true, DisplayOrder = 0 },
        };
        await _context.ProductImages.AddRangeAsync(slideImages);
        await _context.SaveChangesAsync();

        _logger.LogInformation("5 produtos dos slides criados com imagens.");

        // Adicionar campos de personalização ao porta-lata
        var portaLata = products.First(p => p.Slug == "porta-lata-monster-personalizado");
        var customFields = new List<ProductCustomizationField>
        {
            new() { ProductId = portaLata.Id, Label = "Nome ou texto personalizado", FieldType = CustomFieldType.Text, IsRequired = false, DisplayOrder = 1 },
            new() { ProductId = portaLata.Id, Label = "Cor desejada", FieldType = CustomFieldType.Select, IsRequired = true, Options = "[\"Preto\",\"Branco\",\"Vermelho\",\"Azul\",\"Verde\",\"Amarelo\",\"Rosa\",\"Roxo\"]", DisplayOrder = 2 }
        };

        // Campos para comedouro pet
        var comedouro = products.First(p => p.Slug == "comedouro-pet-personalizado");
        customFields.AddRange(new[]
        {
            new ProductCustomizationField { ProductId = comedouro.Id, Label = "Nome do pet", FieldType = CustomFieldType.Text, IsRequired = true, DisplayOrder = 1 },
            new ProductCustomizationField { ProductId = comedouro.Id, Label = "Cor do comedouro", FieldType = CustomFieldType.Select, IsRequired = true, Options = "[\"Azul\",\"Verde\",\"Vermelho\",\"Amarelo\",\"Rosa\",\"Roxo\",\"Laranja\",\"Branco\"]", DisplayOrder = 2 }
        });

        await _context.ProductCustomizationFields.AddRangeAsync(customFields);
        await _context.SaveChangesAsync();

        _logger.LogInformation("{Count} produtos criados com estoque.", products.Count);
    }

    private async Task SeedShippingOptionsAsync()
    {
        if (await _context.ShippingOptions.AnyAsync()) return;

        var options = new List<ShippingOption>
        {
            new() { Name = "PAC", Description = "Entrega econômica pelos Correios", Price = 12.00m, EstimatedDays = 10, IsActive = true, DisplayOrder = 1 },
            new() { Name = "SEDEX", Description = "Entrega expressa pelos Correios", Price = 22.00m, EstimatedDays = 3, IsActive = true, DisplayOrder = 2 },
            new() { Name = "Retirada em mãos", Description = "Retire diretamente conosco (combinar local)", Price = 0m, EstimatedDays = 0, IsActive = true, DisplayOrder = 3 }
        };

        await _context.ShippingOptions.AddRangeAsync(options);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Opções de frete criadas.");
    }

    private async Task SeedStoreSettingsAsync()
    {
        if (await _context.StoreSettings.AnyAsync()) return;

        var settings = new List<StoreSetting>
        {
            new() { Key = "store.name", Value = "Print3D Studio", Description = "Nome da loja" },
            new() { Key = "store.email", Value = "contato@print3dstudio.com.br", Description = "E-mail de contato" },
            new() { Key = "store.whatsapp", Value = "(11) 99999-9999", Description = "WhatsApp de contato" },
            new() { Key = "store.instagram", Value = "@print3dstudio", Description = "Instagram" },
            new() { Key = "store.description", Value = "Especialistas em impressão 3D personalizada. Damos vida às suas ideias.", Description = "Descrição da loja" },
            new() { Key = "store.privacy_policy", Value = "Sua privacidade é importante para nós...", Description = "Política de privacidade" },
            new() { Key = "store.terms_of_use", Value = "Ao utilizar nossa plataforma...", Description = "Termos de uso" },
        };

        await _context.StoreSettings.AddRangeAsync(settings);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Configurações da loja criadas.");
    }
}
