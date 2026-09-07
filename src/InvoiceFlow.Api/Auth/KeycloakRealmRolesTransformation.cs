using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace InvoiceFlow.Api.Auth;

public sealed class KeycloakRealmRolesTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        string? realmAccess = principal.FindFirst("realm_access")?.Value;

        if (identity is null || string.IsNullOrWhiteSpace(realmAccess))
        {
            return Task.FromResult(principal);
        }

        using JsonDocument document = JsonDocument.Parse(realmAccess);
        if (!document.RootElement.TryGetProperty("roles", out JsonElement roles))
        {
            return Task.FromResult(principal);
        }

        foreach (JsonElement role in roles.EnumerateArray())
        {
            string? value = role.GetString();
            if (!string.IsNullOrWhiteSpace(value) && !principal.IsInRole(value))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, value));
            }
        }

        return Task.FromResult(principal);
    }
}

