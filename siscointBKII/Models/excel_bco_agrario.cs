using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class excel_bco_agrario
    {
        public Int64 id { get; set; }
        public DateTime fecha { get; set; }
        public string ruta { get; set; }
        public Int32 estado { get; set; }
        public string usuario { get; set; }
        public Int64 consecutivo { get; set; }
    }
}
