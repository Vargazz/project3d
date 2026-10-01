using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Project3D.Application.DTOs.Auth;
using Project3D.Application.Services.Interfaces;
using Project3D.Infrastructure.Identity;

namespace Project3D.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;

    // Mapa simples de refresh tokens (em produção: persistir no banco)
    private static readonly Dictionary<string, (Guid UserId, DateTime Expires)> RefreshTokens = new();

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var user = new ApplicationUser
        {
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.Phone,
            Cpf = request.Cpf,
            Role = Domain.Enums.UserRole.Customer,
            IsActive = true,
            EmailConfirmed = true // Em produção: enviar e-mail de confirmação
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Falha ao criar usuário: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Conta desativada. Entre em contato com o suporte.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            throw new UnauthorizedAccessException("Conta bloqueada temporariamente. Tente novamente em 15 minutos.");

        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (!RefreshTokens.TryGetValue(refreshToken, out var tokenData))
            throw new UnauthorizedAccessException("Refresh token inválido.");

        if (tokenData.Expires < DateTime.UtcNow)
        {
            RefreshTokens.Remove(refreshToken);
            throw new UnauthorizedAccessException("Refresh token expirado. Faça login novamente.");
        }

        var user = await _userManager.FindByIdAsync(tokenData.UserId.ToString())
            ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

        RefreshTokens.Remove(refreshToken);
        return await GenerateTokensAsync(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Erro ao alterar senha: {errors}");
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return; // Não revelar se e-mail existe

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // TODO: Enviar e-mail com link de recuperação contendo o token
        // await _emailService.SendPasswordResetAsync(user.Email, token);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Erro ao redefinir senha: {errors}");
        }
    }

    public Task RevokeTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var keysToRemove = RefreshTokens.Where(kvp => kvp.Value.UserId == userId).Select(kvp => kvp.Key).ToList();
        foreach (var key in keysToRemove)
            RefreshTokens.Remove(key);
        return Task.CompletedTask;
    }

    // ---- Privado ----

    private async Task<AuthResponse> GenerateTokensAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? user.Role.ToString();

        var jwtSection = _config.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]!;
        var issuer = jwtSection["Issuer"]!;
        var audience = jwtSection["Audience"]!;
        var expiresInMinutes = int.Parse(jwtSection["ExpiresInMinutes"] ?? "60");
        var refreshExpiresDays = int.Parse(jwtSection["RefreshExpiresInDays"] ?? "7");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Refresh token
        var refreshToken = GenerateRefreshToken();
        RefreshTokens[refreshToken] = (user.Id, DateTime.UtcNow.AddDays(refreshExpiresDays));

        return new AuthResponse(
            tokenString,
            refreshToken,
            expiresAt,
            new UserInfo(user.Id, user.FullName, user.Email!, role)
        );
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
