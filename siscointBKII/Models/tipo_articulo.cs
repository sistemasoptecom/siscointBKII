using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class tipo_articulo
    {
        public int id { get; set; }
        public string descripcion { get; set; }
        public int estado { get; set; }
        public DateTime fechaCreacion { get; set; }
    }
}
