using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Servicios_bufete.Models;
using Vistas_bufete.Data;
using Vistas_bufete.Enum;

namespace Vistas_bufete.Controllers
{
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Citas
        public async Task<IActionResult> Index()
        {
            var IdUsuario = User.Identity.Name;
            var citas = new List<Cita>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7289/api/");
                var url = String.Format("Citas/ObtenerCitasPorUsuario/{0}", IdUsuario);
                //HTTP GET
                var responseTask = client.GetAsync(url);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadFromJsonAsync<List<Cita>>();
                    readTask.Wait();

                    citas = readTask.Result;
                }
                else //web api sent error response 
                {
                    //log response status here..

                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            List<SelectListItem> especialidades = new()
            {
                new SelectListItem { Value = "1", Text = "Laboral" },
                new SelectListItem { Value = "2", Text = "Penal" },
                new SelectListItem { Value = "3", Text = "Familia" },
                new SelectListItem { Value = "4", Text = "Civil" },
                new SelectListItem { Value = "5", Text = "Comercio" },
                new SelectListItem { Value = "6", Text = "Notariado" },
            };
            ViewBag.Especialidades = especialidades;
            return View(citas);
        }

        // GET: Citas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            List<SelectListItem> especialidades = new()
            {
                new SelectListItem { Value = "1", Text = "Laboral" },
                new SelectListItem { Value = "2", Text = "Penal" },
                new SelectListItem { Value = "3", Text = "Familia" },
                new SelectListItem { Value = "4", Text = "Civil" },
                new SelectListItem { Value = "5", Text = "Comercio" },
                new SelectListItem { Value = "6", Text = "Notariado" },
            };
            ViewBag.Especialidades = especialidades;
            Cita cita = null;
            if (id == null || _context.Cita == null)
            {
                return NotFound();
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7289/api/");
                var url = String.Format("Citas/{0}", id);
                //HTTP GET
                var responseTask = client.GetAsync(url);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadFromJsonAsync<Cita>();
                    readTask.Wait();

                    cita = readTask.Result;
                }
                else //web api sent error response 
                {
                    //log response status here..

                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        // GET: Citas/Create
        public IActionResult Create()
        {
            List<SelectListItem> horas = new()
            {
                new SelectListItem { Value = "8", Text = "8:00AM" },
                new SelectListItem { Value = "9", Text = "9:00AM" },
                new SelectListItem { Value = "10", Text = "10:00AM" },
                new SelectListItem { Value = "11", Text = "11:00AM" },
                new SelectListItem { Value = "13", Text = "1:00AM" },
                new SelectListItem { Value = "14", Text = "2:00AM" },
                new SelectListItem { Value = "15", Text = "3:00AM" },
                new SelectListItem { Value = "16", Text = "4:00AM" },
            };

            List<SelectListItem> especialidades = new()
            {
                new SelectListItem { Value = "1", Text = "Laboral" },
                new SelectListItem { Value = "2", Text = "Penal" },
                new SelectListItem { Value = "3", Text = "Familia" },
                new SelectListItem { Value = "4", Text = "Civil" },
                new SelectListItem { Value = "5", Text = "Comercio" },
                new SelectListItem { Value = "6", Text = "Notariado" },
            };

            ViewBag.Horas = horas;
            ViewBag.Especialidades = especialidades;
            return View();
        }

        // POST: Citas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Especialidad,Hora,Dia,Fecha")] Cita cita)
        {
            cita.Estado = (int)Estado.Activa;
            cita.IdUsuario = Int32.Parse(User.Identity.Name);
            cita.Solicitud = DateTime.Now;
            if (ModelState.IsValid)
            {
                if (cita.Fecha.DayOfWeek == DayOfWeek.Monday || cita.Fecha.DayOfWeek == DayOfWeek.Tuesday ||
                    cita.Fecha.DayOfWeek == DayOfWeek.Wednesday || cita.Fecha.DayOfWeek == DayOfWeek.Thursday ||
                    cita.Fecha.DayOfWeek == DayOfWeek.Friday
                    )
                {

                    DateTime fecha22 = DateTime.Now;
                    DateTime fecha1 = DateTime.Now;
                    fecha22 = fecha22.AddDays(22);
                    if (cita.Fecha > fecha1 && cita.Fecha < fecha22)
                    {

                        if (ObtenerCitasActivasPorEspecialidad(cita.IdUsuario, cita.Especialidad).Count() < 1)
                        {


                            if (ObtenerCitasActivasPorEspecialidadPorHora(cita.Especialidad, cita.Hora, cita.Fecha).Count() < 2)
                            {

                                using (var client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri("https://localhost:7289/api/");
                                    //HTTP GET
                                    var postTask = client.PostAsJsonAsync<Cita>("Citas", cita);
                                    postTask.Wait();

                                    var result = postTask.Result;
                                    if (result.IsSuccessStatusCode)
                                    {
                                        var readTask = result.Content.ReadFromJsonAsync<Cita>();
                                        readTask.Wait();

                                        cita = readTask.Result;
                                        return RedirectToAction(nameof(Details), new { id = cita.Id });
                                    }
                                    else
                                    {


                                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                                    }
                                }


                            }
                            else
                            {


                                ModelState.AddModelError(string.Empty, "No se pueden tener mas de dos citas por especialidad por Hora");
                            }








                        }
                        else
                        {


                            ModelState.AddModelError(string.Empty, "Ya tiene una cita activa en esa especialidad");
                        }




                    }
                    else
                    {


                        ModelState.AddModelError(string.Empty, "La fecha debe de ser de mañana o maximo 22 días");
                    }
                }
                else
                {


                    ModelState.AddModelError(string.Empty, "Los fines de semana no se trabaja");
                }

            }
            else
            {


                ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
            }

            List<SelectListItem> horas = new()
            {
                new SelectListItem { Value = "8", Text = "8:00AM" },
                new SelectListItem { Value = "9", Text = "9:00AM" },
                new SelectListItem { Value = "10", Text = "10:00AM" },
                new SelectListItem { Value = "11", Text = "11:00AM" },
                new SelectListItem { Value = "13", Text = "1:00AM" },
                new SelectListItem { Value = "14", Text = "2:00AM" },
                new SelectListItem { Value = "15", Text = "3:00AM" },
                new SelectListItem { Value = "16", Text = "4:00AM" },
            };

            List<SelectListItem> especialidades = new()
            {
                new SelectListItem { Value = "1", Text = "Laboral" },
                new SelectListItem { Value = "2", Text = "Penal" },
                new SelectListItem { Value = "3", Text = "Familia" },
                new SelectListItem { Value = "4", Text = "Civil" },
                new SelectListItem { Value = "5", Text = "Comercio" },
                new SelectListItem { Value = "6", Text = "Notariado" },
            };

            ViewBag.Horas = horas;
            ViewBag.Especialidades = especialidades;

            return View(cita);
        }

        // GET: Citas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Cita == null)
            {
                return NotFound();
            }

            var cita = await _context.Cita.FindAsync(id);
            if (cita == null)
            {
                return NotFound();
            }
            return View(cita);
        }

        // POST: Citas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Especialidad,Estado,IdUsuario,Hora,Dia")] Cita cita)
        {
            if (id != cita.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cita);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cita);
        }

        // GET: Citas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            List<SelectListItem> especialidades = new()
            {
                new SelectListItem { Value = "1", Text = "Laboral" },
                new SelectListItem { Value = "2", Text = "Penal" },
                new SelectListItem { Value = "3", Text = "Familia" },
                new SelectListItem { Value = "4", Text = "Civil" },
                new SelectListItem { Value = "5", Text = "Comercio" },
                new SelectListItem { Value = "6", Text = "Notariado" },
            };
            ViewBag.Especialidades = especialidades;
            Cita cita = null;
            if (id == null || _context.Cita == null)
            {
                return NotFound();
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7289/api/");
                var url = String.Format("Citas/{0}", id);
                //HTTP GET
                var responseTask = client.GetAsync(url);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadFromJsonAsync<Cita>();
                    readTask.Wait();

                    cita = readTask.Result;
                }
                else //web api sent error response 
                {
                    //log response status here..

                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            if (cita == null)
            {
                return NotFound();
            }

            return View(cita);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Cita cita = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7289/api/");
                var url = String.Format("Citas/{0}", id);
                //HTTP GET
                var responseTask = client.DeleteAsync(url);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {

                }
                else //web api sent error response 
                {
                    //log response status here..

                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public List<Cita> ObtenerCitasActivasPorEspecialidad(int IdUsuario, int Especialidad)
        {
            List<Cita> citas = new List<Cita>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7289/api/");
                var url = String.Format("Citas/ObtenerCitasActivasPorEspecialidad/{0}/{1}", IdUsuario, Especialidad);
                //HTTP GET
                var responseTask = client.GetAsync(url);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadFromJsonAsync<IEnumerable<Cita>>();
                    readTask.Wait();

                    citas = (List<Cita>)readTask.Result;
                }
                else //web api sent error response 
                {
                    //log response status here..

                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            return citas;
        }
        public List<Cita> ObtenerCitasActivasPorEspecialidadPorHora(int Especialidad, int Hora, DateTime Fecha)
        {
            string NuevaFecha = Fecha.ToString("MM-dd-yyyy");
            List<Cita> citas = new List<Cita>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7289/api/");
                var url = String.Format("Citas/ObtenerCitasActivasPorEspecialidadPorHora/{0}/{1}/{2}", Especialidad, Hora, NuevaFecha);
                //HTTP GET
                var responseTask = client.GetAsync(url);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadFromJsonAsync<IEnumerable<Cita>>();
                    readTask.Wait();

                    citas = (List<Cita>)readTask.Result;
                }
                else //web api sent error response 
                {
                    //log response status here..

                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            return citas;
        }
    }
}
