using System;

namespace siscointBKII.Models
{
    public class liq_esquema_recuperador
    {
        public Int32 id {  get; set; }
        public string escala_cumplimiento { get; set; } 
        public string homologa_cumplimiento { get; set; }   
        public float valor { get; set; }
        public Int32 estado { get; set; }   
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
