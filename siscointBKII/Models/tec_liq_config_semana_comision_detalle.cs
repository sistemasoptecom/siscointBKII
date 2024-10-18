using System;

namespace siscointBKII.Models
{
    public class tec_liq_config_semana_comision_detalle
    {
        public Int64 id { get; set; }
        public Int64 cod_semana_comision { get; set; }
        public string cod_ciudad { get; set; }
        public string periodo_comision { get; set; } 
        public DateTime dia_comision { get; set; }
        public string usuario {  get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
