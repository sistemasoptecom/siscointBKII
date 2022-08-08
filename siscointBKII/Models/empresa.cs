using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class empresa
    {
        public int id { get; set; }
        public string nit { get; set; }
        public string nombre { get; set; }
        public string codigo { get; set; }
        public string ruta_logo { get; set; }
        public int estado { get; set; }
    }
}
