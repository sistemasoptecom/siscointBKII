using System;

namespace siscointBKII.Models
{
    public class liq_esquema_call
    {
        public Int64 id {  get; set; }
        public Int64 codigo_esquema_call { get; set; }
        public string cumplimiento { get; set; }   
        public string homologa_cumplimiento { get; set;}
        public Int32 nivel { get; set; }
        public Int32 codigo_tipo_internet { get; set; }
        public double valor { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }    
    }
}
