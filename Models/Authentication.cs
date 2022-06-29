using LINELoginOIDCImplementationDemo.Models.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using System.Security.Claims;

namespace LINELoginOIDCImplementationDemo.Models;

public class Authentication
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private string _redirectUri = "https://localhost:9001/signin-oidc";

    /// <summary>
    /// A value included in the request that is also returned in the token response.
    /// It can be a string of any content that you wish. 
    /// A randomly generated unique value is typically used for <see href="https://tools.ietf.org/html/rfc6749#section-10.12">preventing cross-site request forgery attacks</see>.
    /// </summary>
    // 這應該要在每一次進行登入時修改。
    // Authentication是DI為Scoped，在兩次連入(Login、signin-oidc)時是不同實例，不能在此宣告時做隨機生成。
    // 需要實做其它方式來記憶狀態。
    public string State { get; } = "12345GG";

    /// <summary>
    /// Authorize Url to request an authorization code.
    /// <see href="https://developers.line.biz/en/docs/line-login/integrate-line-login/#making-an-authorization-request">Document</see>
    /// </summary>
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
                { "scope", "openid profile email" },
                { "state", State }
            };

            return $"{authUrl}{qb.ToQueryString().Value}";
        }
    }

    public Authentication(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;

        // Read redirectUri from configuration if it exists.
        _redirectUri = config["OpenIDConnect:RedirectUri"] ?? _redirectUri;
    }

    /// <summary>
    /// Request access token with authorization code.
    /// </summary>
    /// <param name="authorization_code"></param>
    /// <returns></returns>
    public async Task<ITokenResponse> GetTokenAsync(string authorization_code)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        using HttpRequestMessage request = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.line.me/oauth2/v2.1/token"),
            Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", authorization_code },
                { "redirect_uri", _redirectUri },
                { "client_id", _config["OpenIDConnect:ClientId"] },
                { "client_secret", _config["OpenIDConnect:ClientSecret"] }
            })
        };

        HttpResponseMessage message = await client.SendAsync(request);

        message.EnsureSuccessStatusCode();

        return await message.Content.ReadAsAsync<TokenResponse>();
    }

    /// <summary>
    /// Verify the ID token and get the corresponding user's profile information and email address.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<VerifyResponse> VerifyTokenAsync(ITokenResponse token)
    {
        if (string.IsNullOrEmpty(token.id_token))
        {
            throw new ArgumentException("id_token is empty");
        }

        using HttpClient client = _httpClientFactory.CreateClient();
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

    /// <summary>
    /// Make ClaimsIdentity from the user's profile information.
    /// </summary>
    /// <param name="verify"></param>
    /// <returns></returns>
    public static ClaimsIdentity MakeClaimsIdentity(IVerifyResponse verify)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, verify.sub),
        };

        if (!string.IsNullOrEmpty(verify.name))
            claims.Add(new Claim(ClaimTypes.Name, verify.name));

        if (!string.IsNullOrEmpty(verify.picture))
            claims.Add(new Claim("picture", verify.picture));

        if (!string.IsNullOrEmpty(verify.email))
            claims.Add(new Claim(ClaimTypes.Email, verify.email));

        if (null != verify.amr && verify.amr.Count > 0)
        {
            foreach (var amr in verify.amr)
            {
                claims.Add(new Claim("http://schemas.microsoft.com/claims/authnmethodsreferences", amr));
            }
        }

        return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Make AuthenticationProperties from the user's profile information.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="verify"></param>
    /// <returns></returns>
    internal static AuthenticationProperties MakeAuthenticationProperties(ITokenResponse token,IVerifyResponse verify)
    {
        AuthenticationProperties authProp = new()
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.FromUnixTimeSeconds(verify.exp),
            IssuedUtc = DateTimeOffset.FromUnixTimeSeconds(verify.iat),
        };
        authProp.Items.Add(".Token.access_token", token.access_token);
        authProp.Items.Add(".Token.id_token", token.id_token);
        authProp.Items.Add(".Token.refresh_token", token.refresh_token);
        authProp.Items.Add(".Token.token_type", token.token_type);
        authProp.Items.Add(".Token.expires_at", DateTime.UtcNow.AddSeconds(token.expires_in).ToString("u"));
        authProp.Items.Add(".TokenNames", "access_token;id_token;refresh_token;token_type;expires_at");

        return authProp;
    }

    #region Additional Feature
    /// <summary>
    /// 由Request動態取得request URL，以產生並覆寫RedirectUri。在一進入Action時呼叫。
    /// </summary>
    /// <param name="request"></param>
    internal void SetupRedirectUri(HttpRequest request)
    {
        Uri uri = new(UriHelper.GetDisplayUrl(request));
        string port = (uri.Scheme == "https" && uri.Port == 443)
                      || (uri.Scheme == "http" && uri.Port == 80)
                      ? ""
                      : $":{uri.Port}";
        _redirectUri = $"{uri.Scheme}://{uri.Host}{port}/signin-oidc";
    }
    #endregion
}
