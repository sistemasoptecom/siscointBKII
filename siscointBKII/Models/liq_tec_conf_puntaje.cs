using System;

namespace siscointBKII.Models
{
    public class liq_tec_conf_puntaje
    {
        public Int64 id {  get; set; }
        public DateTime fecha1 { get; set; }
        public DateTime fecha2 { get; set; }
        public string rango_fechas { get; set; }
        public Int32 puntos_semana { get; set; }
        public string mm_comision { get; set; }
        public string aaaa_comision { get; set; }   
        public string periodo_comision {  get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
