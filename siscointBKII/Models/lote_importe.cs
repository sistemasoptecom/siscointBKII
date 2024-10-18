using System;

namespace siscointBKII.Models
{
    public class lote_importe
    {
        public Int64 id { get; set; }
        public Int64 consecutivo_lote { get; set; }
        public string tipo_importe { get; set; } 
        public string ruta_archivo { get; set; }
        public string usuario { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
    }
}
