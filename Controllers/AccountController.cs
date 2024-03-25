using DataBase;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Sagonne.DataBase.Table;
using Sagonne.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Sagonne.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public IActionResult Login()
        {
            ClaimsPrincipal claimuser = HttpContext.User;

            if (claimuser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            ViewBag.Title = "Login";
            ViewBag._isLogged = "";


            if (!string.IsNullOrEmpty(model.Login) && !string.IsNullOrEmpty(model.Mdp))
            {
                Users user = await Database.ExecuteReaderSingleResult<Users>(
                     "SELECT * FROM USERS WHERE LOGIN=@LOGIN AND MDP=@MDP",
                     new List<MySqlParameter> {
                     new MySqlParameter("@LOGIN", model.Login),
                     new MySqlParameter("@MDP", model.Mdp),
                     });

                if (user != null)
                {

                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, model.Login),
                        new Claim(ClaimTypes.Role, user.ADMIN ? "ADMIN":""),
                    };

                    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                        IsPersistent = model.KeepLoggedIn
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), properties);

                    return RedirectToAction("Index", "Home");

                }
            }

            
            ViewData["ValidateMessage"] = "User not found";
            return View(model);
        }

    }
}