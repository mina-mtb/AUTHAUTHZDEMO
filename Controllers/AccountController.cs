using AuthAuthzDemo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace AuthAuthzDemo.Controllers;
public class AccountController : Controller
{
    private const string MockUserName = "admin";
    private const string MockPassword = "pass";

    public IActionResult Login()
    {
        return View();
    }
public IActionResult GoogleLogin()
{
    var authProperties = new AuthenticationProperties 
    {
        RedirectUri = Url.Action("GoogleLoginCallback", "Account")
    };
    return Challenge(authProperties, GoogleDefaults.AuthenticationScheme);
}

public IActionResult CognitoLogin()
{
    return Challenge(
        new AuthenticationProperties{
            RedirectUri = Url.Action("Index", "Home")
        },
        OpenIdConnectDefaults.AuthenticationScheme
    );
}

public async Task<IActionResult> GoogleLoginCallbackAsync()
{
    var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    if (!result.Succeeded)
    {
        // Handle failure: return to the login page, show an error, etc.
        return RedirectToAction("Login");
    }

    // Here, you could fetch information from result.Principal to store in your database, 
    // or to find an existing user.

    return RedirectToAction("Index", "Home");
}


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(LoginView loginView)
    {
        if (!ModelState.IsValid)
        {
            return View(loginView);
        }
        if(loginView.UserName == MockUserName && loginView.Password == MockPassword)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, loginView.UserName)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError(string.Empty, "Invalid username or password");
        return View(loginView);
    }

[Authorize]
    public IActionResult SecretInfo()
    {
        return View();
    }

    
[Authorize]
public IActionResult Logout()
{
    return SignOut(
        new AuthenticationProperties
        {
            RedirectUri = Url.Action("Index", "Home")
        },
        CookieAuthenticationDefaults.AuthenticationScheme,
        OpenIdConnectDefaults.AuthenticationScheme);
}
}