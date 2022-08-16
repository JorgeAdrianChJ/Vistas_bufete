using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Servicios_bufete.Models;

namespace Vistas_bufete.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Servicios_bufete.Models.Cita>? Cita { get; set; }
        public DbSet<Servicios_bufete.Models.Usuario>? Usuario { get; set; }
    }
}