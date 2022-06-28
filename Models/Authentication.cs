using LINELoginOIDCImplementationDemo.Models.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using System.Security.Claims;

namespace LINELoginOIDCImplementationDemo.Models;

public class Authentication
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private string _redirectUri = "";

    // 這應該要在每一次進行登入時修改。
    // Authentication是DI為Scoped，在兩次連入(Login、signin-oidc)時是不同實例，不能在此宣告時做隨機生成。
    // 需要實做其它方式來記憶狀態。
    public string State { get; } = "12345GG";

    public string AuthorizeUrl
    {
        get
        {
            var authUrl = "https://access.line.me/oauth2/v2.1/authorize";

            QueryBuilder qb = new()
            {
                { "response_type", "code" },
                { "client_id", _config["OpenIDConnect:ClientId"] },
                { "redirect_uri", _redirectUri },
                { "scope", "openid profile" },
                { "state", State }
            };

            return $"{authUrl}{qb.ToQueryString().Value}";
        }
    }

    public Authentication(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    internal void SetupRedirectUri(HttpRequest request)
    {
        Uri uri = new(UriHelper.GetDisplayUrl(request));
        string port = (uri.Scheme == "https" && uri.Port == 443)
                      || (uri.Scheme == "http" && uri.Port == 80)
                      ? ""
                      : $":{uri.Port}";
        _redirectUri = $"{uri.Scheme}://{uri.Host}{port}/signin-oidc";
    }

    public async Task<ITokenResponse> GetTokenAsync(string code)
    {
        var client = _httpClientFactory.CreateClient();
        using HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.line.me/oauth2/v2.1/token"),
            Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _redirectUri },
                { "client_id", _config["OpenIDConnect:ClientId"] },
                { "client_secret", _config["OpenIDConnect:ClientSecret"] }
            })
        };

        HttpResponseMessage message = await client.SendAsync(request);

        message.EnsureSuccessStatusCode();

        return await message.Content.ReadAsAsync<TokenResponse>();
    }

    public async Task<VerifyResponse> VerifyTokenAsync(ITokenResponse token)
    {
        if (string.IsNullOrEmpty(token.id_token))
        {
            throw new ArgumentException("id_token is empty");
        }

        var client = _httpClientFactory.CreateClient();
        using HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.line.me/oauth2/v2.1/verify"),
            Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "id_token", token.id_token },
                { "client_id", _config["OpenIDConnect:ClientId"]}
            })
        };

        HttpResponseMessage message = await client.SendAsync(request);

        message.EnsureSuccessStatusCode();

        return await message.Content.ReadAsAsync<VerifyResponse>();
    }

    public static ClaimsIdentity MakeClaimsIdentity(IVerifyResponse verify)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, verify.sub),
        };

        if (!string.IsNullOrEmpty(verify.name))
            claims.Add(new Claim(ClaimTypes.Name, verify.name));

        if (!string.IsNullOrEmpty(verify.picture))
            claims.Add(new Claim("picture", verify.picture));

        if (null != verify.amr && verify.amr.Count > 0)
        {
            foreach (var amr in verify.amr)
            {
                claims.Add(new Claim("http://schemas.microsoft.com/claims/authnmethodsreferences", amr));
            }
        }

        return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
