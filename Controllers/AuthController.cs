using LINELoginOIDCImplementationDemo.Models;
using LINELoginOIDCImplementationDemo.Models.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LINELoginOIDCImplementationDemo.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly Authentication _auth;

    public AuthController(Authentication auth)
    {
        _auth = auth;
    }

    public IActionResult Login()
    {
        _auth.SetupRedirectUri(Request);

        return Redirect(_auth.AuthorizeUrl);
    }

    public async Task<IActionResult> SigninOIDCAsync(SigninOIDCResponse signin)
    {
        _auth.SetupRedirectUri(Request);

        if (signin.state != _auth.State)
        {
            return BadRequest("State not match!");
        }

        if (!string.IsNullOrEmpty(signin.error))
        {
            ISigninOIDCResponse_Error err = signin;
            return BadRequest($"{err.error}: {err.error_description}");
        }

        ISigninOIDCResponse_Success success = signin;

        string code = success.code;

        ITokenResponse token = await _auth.GetTokenAsync(code);
        IVerifyResponse verify = await _auth.VerifyTokenAsync(token);
        ClaimsIdentity claimsIdentity = Authentication.MakeClaimsIdentity(verify);

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

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProp
        );

        return RedirectToAction("Index", "Home");
    }
}
