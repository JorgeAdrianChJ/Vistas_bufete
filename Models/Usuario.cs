using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Servicios_bufete.Models
{
    public partial class Usuario
    {
        public Usuario()
        {
            Cita = new HashSet<Cita>();
        }

        public int Id { get; set; }
        public string Identificacion { get; set; } = null!;
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public int TipoPago { get; set; }

        public virtual ICollection<Cita> Cita { get; set; }
    }
}
