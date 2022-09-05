using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class proveedorII
    {
        public int id { get; set; }
        public string nit { get; set; }
        public string razon_social { get; set; }
        public string representante_legal { get; set; }
        public string  tel { get; set; }
        public string  asesor { get; set; }
        public string correo { get; set; }
        public string tel_asesor { get; set; }
        public int estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
