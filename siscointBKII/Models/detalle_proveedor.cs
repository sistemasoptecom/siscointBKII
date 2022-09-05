using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class detalle_proveedor
    {
        public int id { get; set; }
        public string nit { get; set; }
        public string contrato { get; set; }
        public int estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
