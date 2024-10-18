using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class tipo_reporte
    {
        public int id { get; set; }
        public int valor { get; set; }
        public string descripcion { get; set; }
        public int tipo_reporte_tabla { get; set; }
    }
}
