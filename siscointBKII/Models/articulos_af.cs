using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class articulos_af
    {
        public int id { get; set; }
        public int codigo { get; set; }
        public string descripcion { get; set; }
        public string und { get; set; }
        public string cuenta { get; set; }
        public string tipo_iva { get; set; }
        public string grupo { get; set; }
        public int v_util { get; set; }
    }
}
