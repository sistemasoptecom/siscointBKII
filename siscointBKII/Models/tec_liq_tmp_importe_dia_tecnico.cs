using System;

namespace siscointBKII.Models
{
    public class tec_liq_tmp_importe_dia_tecnico
    {
        public Int64 id { get; set; }
        public string periodo_comision { get; set; }
        public Int32 cod_semana_comision { get; set; }
        public string cod_ciudad { get; set; }
        public DateTime dia_comision { get; set; }
        public string cedula_tec_lider { get; set; }
        public string cedula_tec_aux { get; set; }
        public string tipo { get; set; }
        public Int32 estado { get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
