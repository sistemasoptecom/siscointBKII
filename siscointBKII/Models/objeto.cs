using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class objeto
    {
        public int id { get; set; }
        public int tipo { get; set; }
        public string af { get; set; }
        public string imei { get; set; }
        public string descripcion { get; set; }
        public string observacion { get; set; }
        public int estado { get; set; }
        public string factura { get; set; }
        public string linea { get; set; }
        public string linea_activa { get; set; }
        public string valor { get; set; }
        public string nuevo_imei { get; set; }
        public Nullable<System.DateTime> causacion { get; set; }
        public string centro_costo { get; set; }
        public Nullable<System.DateTime> fecha_estado { get; set; }
        public Nullable<int> tipo_articulo { get; set; }
        public string cod_articulo { get; set; }

    }
}
