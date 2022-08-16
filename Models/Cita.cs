using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vistas_bufete.Enum;

namespace Servicios_bufete.Models
{
    public partial class Cita
    {
        public int Id { get; set; }
        public int Especialidad { get; set; }
        public int Estado { get; set; }
        public int IdUsuario { get; set; }
        public int Hora { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Solicitud { get; set; }
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }
        public virtual Usuario? IdUsuarioNavigation { get; set; } = null!;
    }
}
