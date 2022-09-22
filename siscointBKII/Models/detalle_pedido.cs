using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class detalle_pedido
    {
        public int id { get; set; }
        public int id_pedido { get; set; }
        public string codigo_art { get; set; }
        public string descripcion { get; set; }
        public Nullable<int> cantidad { get; set; }
        public string und { get; set; }
        public Nullable<double> valor { get; set; }
        public Nullable<double> iva { get; set; }
        public Nullable<double> subtotal { get; set; }
        public Nullable<double> total { get; set; }
        public string cuenta { get; set; }
    }
}
