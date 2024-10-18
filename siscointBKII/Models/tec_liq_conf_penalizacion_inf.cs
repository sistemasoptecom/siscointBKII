using System;

namespace siscointBKII.Models
{
    public class tec_liq_conf_penalizacion_inf
    {
        public Int64 id {  get; set; }
        public string porcentaje_infancia { get; set; }
        public string homologacion_infancia { get; set; }
        public string porcentaje_afectacion { get; set; }
        public string homologacion_afectacion { get; set; }
        public string cod_ciudad {  get; set; }
        public string usuario { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modifacion { get; set; }
    }
}
