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
        //_auth.SetupRedirectUri(Request);

        return Redirect(_auth.AuthorizeUrl);
    }

    public async Task<IActionResult> SigninOIDCAsync(SigninOIDCResponse signin)
    {
        //_auth.SetupRedirectUri(Request);

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
        AuthenticationProperties authenticationProperties = Authentication.MakeAuthenticationProperties(token,verify);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authenticationProperties
        );

        return RedirectToAction("Index", "Home");
    }
}
