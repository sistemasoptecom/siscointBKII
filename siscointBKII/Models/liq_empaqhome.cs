using System;

namespace siscointBKII.Models
{
    public class liq_empaqhome
    {
        public Int64 id {  get; set; } 
        public Int64 cod_liq_dtn { get; set; } 
        public Int32 tipo_empaquehome { get; set; }
        public double valor { get; set; }
        public string cumplimiento { get; set; } 
        public string homologa_cumplimieno { get; set; }
        public Int32 cod_tipo_esquema { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public Int32 codigo_nivel { get; set; }
    }
}
