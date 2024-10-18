using System;

namespace siscointBKII.Models
{
    public class liq_esquema_movil
    {
        public Int64 id {  get; set; }
        public Int64 codigo_tipo_esc_movil { get; set; }
        public string cumplimiento { get; set; } 
        public string homologa_cumplimiento { get; set; }
        public string tipo_renta { get; set; }
        public string homologa_renta { get; set; }
        public Int32 nivel { get; set; }
        public double valor { get; set; } 
        public Int32 estado { get; set; } 
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public Int32 nivel_tipo_renta { get; set; }
        public Int32 codigo_tipo_esquema { get; set; }
    }
}
