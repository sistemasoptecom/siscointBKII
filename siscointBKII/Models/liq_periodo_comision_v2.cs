using System;

namespace siscointBKII.Models
{
    public class liq_periodo_comision_v2
    {
        public Int64 id { get; set; }
        public string periodo { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public Int32 EsPublicado { get; set; }
        public Int32 EsCerrado { get; set; }    
    }
}
