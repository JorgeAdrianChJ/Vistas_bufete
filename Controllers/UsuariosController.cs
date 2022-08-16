using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Servicios_bufete.Models;
using Vistas_bufete.Data;

namespace Vistas_bufete.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        public UsuariosController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {

            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // GET: Usuarios/Create
        public IActionResult Register()
        {
            List<SelectListItem> tipoPago = new()
            {
                new SelectListItem { Value = "1", Text = "Tarjeta" },
                new SelectListItem { Value = "2", Text = "depósito" },
                new SelectListItem { Value = "3", Text = "SINPE" },
            };
            ViewBag.TipoPagos = tipoPago;
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Identificacion,FechaNacimiento,NombreCompleto,TipoPago")] Usuario usuario)
        {
            List<SelectListItem> tipoPago = new()
            {
                new SelectListItem { Value = "1", Text = "Tarjeta" },
                new SelectListItem { Value = "2", Text = "depósito" },
                new SelectListItem { Value = "3", Text = "SINPE" },
            };
            ViewBag.TipoPagos = tipoPago;
            if (ModelState.IsValid)
            {
                Usuario usuarioBD = UsuarioExists(usuario.Identificacion, usuario.FechaNacimiento);
                if (usuarioBD == null)
                {
                    int edad = DateTime.Now.Year - usuario.FechaNacimiento.Year;

                    if (edad > 15)
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("https://localhost:7289/api/");
                            //HTTP GET
                            var postTask = client.PostAsJsonAsync<Usuario>("Usuarios", usuario);
                            postTask.Wait();

                            var result = postTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                var readTask = result.Content.ReadFromJsonAsync<Usuario>();
                                readTask.Wait();

                                string fecha = usuario.FechaNacimiento.ToString("MM-dd-yyyy");
                                usuario = readTask.Result;
                                var user = new IdentityUser
                                {
                                    Id = usuario.Id.ToString(),
                                    UserName = usuario.Id.ToString(),
                                    NormalizedUserName = usuario.NombreCompleto
                                };
                                var singIn = await userManager.CreateAsync(user);
                                if (singIn.Succeeded)
                                {
                                    return RedirectToAction("Login");
                                }
                                else //web api sent error response 
                                {
                                    //log response status here..

                                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                                }
                            }
                        }
                    }
                    else
                    {
                        //log response status here..

                        ModelState.AddModelError(string.Empty, "El usuario debe de ser mayor de 15");
                    }
                }
                else
                {
                    //log response status here..

                    ModelState.AddModelError(string.Empty, "El usuario ya esta registrado");
                }

            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Login()
        {
            return View();
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Identificacion,FechaNacimiento")] string Identificacion, DateTime FechaNacimiento)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Usuario usuario = UsuarioExists(Identificacion, FechaNacimiento);
                    if (usuario != null)
                    {
                        var user = await userManager.FindByIdAsync(usuario.Id.ToString());

                        List<Claim> claims = new List<Claim>();
                        claims.Add(new Claim("UserRole", "Admin"));
                        claims.Add(new Claim("Id", usuario.Id.ToString()));
                        claims.Add(new Claim("Identificacion", Identificacion));
                        claims.Add(new Claim("NombreCompleto", usuario.NombreCompleto));
                        claims.Add(new Claim("FechaNacimiento", FechaNacimiento.ToString("MM-dd-yyyy")));
                        await signInManager.SignInWithClaimsAsync(user, false, (IEnumerable<Claim>)claims);
                        return RedirectToAction("Index", "Home", new { area = "" });

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "La identificación y/o la fecha de nacimiento es incorrecto");
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
            return View();
        }
        private Usuario UsuarioExists(string Identificacion, DateTime FechaNacimiento)
        {
            string fecha = FechaNacimiento.ToString("MM-dd-yyyy");
            Usuario usuario = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7289/api/");
                var url = String.Format("Usuarios/GetByIdentificacionAndFechaNacimiento/{0}/{1}", Identificacion, fecha);
                //HTTP GET
                var responseTask = client.GetAsync(url);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadFromJsonAsync<Usuario>();
                    readTask.Wait();

                    usuario = readTask.Result;
                }
            }
            return usuario;
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
