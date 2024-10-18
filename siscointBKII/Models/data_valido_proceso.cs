using System;

namespace siscointBKII.Models
{
    public class data_valido_proceso
    {
        public Int64 id { get; set; }
        public string proceso { get; set; }
        public Int32 cantidad { get; set; }
        public Int32 estado { get; set; }
        public string usuario { get; set; }
        public DateTime fecha_inicio_proceso { get; set; }
        public DateTime? fecha_fin_proceso { get; set; } 
        public Int32 consecutivo_lote { get; set; } 
        public string nombre_tabla { get; set; }    
    }
}
