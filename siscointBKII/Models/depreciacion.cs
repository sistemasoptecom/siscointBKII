using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class depreciacion
    {
        public int id { get; set; }
        public int id_pedido { get; set; }
        public string cod_art { get; set; }
        public string descripcion { get; set; }
        public Nullable<double> valor { get; set; }
        public string ano { get; set; }
        public Nullable<System.DateTime> fecha_ejecucion { get; set; }
        public Nullable<int> valor_ejecutado { get; set; }
        public Nullable<int> v_util { get; set; }
        public Nullable<int> v_util_restante { get; set; }
        public string ccosto { get; set; }
        public string cuenta { get; set; }
        public string rubro { get; set; }
        public System.DateTime fecha_ini_depre { get; set; }
        public Nullable<int> cuota { get; set; }
        public Nullable<int> inventario { get; set; }
        public string placa_af { get; set; }
    }
}
