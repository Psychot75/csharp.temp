using Duende.IdentityModel.OidcClient;
using System.Threading.Tasks;
using Duende.IdentityModel.OidcClient.Browser;

namespace Arcraven.Avalonia.HMI.Services;

public class AuthService
{
    private readonly OidcClient _oidcClient;

    public AuthService(IBrowser browser, AppSettings settings)
    {
        var options = new OidcClientOptions
        {
            Authority = "https://keycloak.yourdomain.com/realms/HMI",
            ClientId = "hmi-client",
            RedirectUri = "http://localhost:5000/callback/",
            Scope = "openid profile email",
            Browser = browser // Injected helper
        };

        _oidcClient = new OidcClient(options);
    }

    public async Task<LoginResult> LoginAsync() => await _oidcClient.LoginAsync();
}