using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project3D.Application.Services.Interfaces;
using Project3D.Domain.Interfaces;
using Project3D.Infrastructure.Data;
using Project3D.Infrastructure.Data.Seed;
using Project3D.Infrastructure.Identity;
using Project3D.Infrastructure.Repositories;
using Project3D.Infrastructure.Services;

namespace Project3D.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---- Banco de dados ----
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("Project3D.Infrastructure")
            )
        );

        // Registrar AppDbContext também como IAppDbContext
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // ---- ASP.NET Identity ----
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // ---- Unit of Work e repositório genérico ----
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        // ---- Serviços de infraestrutura (baixo nível) ----
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPaymentService, MercadoPagoService>();

        // Storage: local em dev, Cloudinary em prod
        var useLocal = configuration.GetValue<bool>("Upload:UseLocal");
        if (useLocal)
            services.AddScoped<IStorageService, LocalStorageService>();
        else
            services.AddScoped<IStorageService, CloudinaryStorageService>();

        // ---- Application Services (implementados na Infrastructure) ----
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomRequestService, CustomRequestService>();
        services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStoreSettingService, StoreSettingService>();
        services.AddScoped<IAdminUserService, AdminUserService>();

        // ---- Seed ----
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
