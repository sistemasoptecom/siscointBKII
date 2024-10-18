using System;

namespace siscointBKII.Models
{
    public class tec_liq_config_semana_comision
    {
        public Int64 id { get; set; }
        public Int64 numero_semana { get; set; }
        public string cod_ciudad {  get; set; }

        public string mm_comision {  get; set; }
        public string aaaa_comision { get; set; }
        public string periodo {  get; set; }
        public Int32 puntaje_semana { get; set; }
        public string usuario { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
