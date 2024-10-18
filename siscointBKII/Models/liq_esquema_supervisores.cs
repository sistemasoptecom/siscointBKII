using System;

namespace siscointBKII.Models
{
    public class liq_esquema_supervisores
    {
        public Int32 id {  get; set; }
        public string escala_meta_cumplimiento { get; set; }
        public string homologa_meta_cumplimiento { get; set; }
        public double valor {  get; set; }
        public Int32 codigo_tipo_escala {  get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }    
    }
}
