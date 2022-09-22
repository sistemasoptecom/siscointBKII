using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class pedidos
    {
        public int id { get; set; }
        public int nro_pedido { get; set; }
        public System.DateTime fecha { get; set; }
        public string usuario { get; set; }
        public string proveedor { get; set; }
        public string ccosto { get; set; }
        public string justificacion { get; set; }
        public string vlr_total { get; set; }
        public Nullable<int> vto_bueno_presu { get; set; }
        public Nullable<System.DateTime> fecha_vto_bueno { get; set; }
        public Nullable<int> vto_bueno_finan { get; set; }
        public Nullable<System.DateTime> fecha_vto_finan { get; set; }
        public string estado { get; set; }
        public Nullable<int> nro_srp { get; set; }
        public Nullable<int> nro_rp { get; set; }
        public string nro_contrato { get; set; }
        public string obs_compras { get; set; }
        public string pc { get; set; }
        public string st { get; set; }
        public string et { get; set; }
        public string cont { get; set; }
        public string ad { get; set; }
        public string asignado_a { get; set; }
        public string aprobado_jefe { get; set; }
    }
}
