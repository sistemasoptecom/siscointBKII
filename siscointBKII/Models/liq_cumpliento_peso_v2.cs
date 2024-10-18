using System;

namespace siscointBKII.Models
{
    public class liq_cumpliento_peso_v2
    {
        public Int64 id { get; set; }
        public string descripcion_producto { get; set; }
        public string peso {  get; set; }
        public double homologa_peso { get; set; }
        public Int32 codigo_tipo_esquema { get; set; }  
        public Int32 estado {  get; set; } 
        public string usuario { get; set; } 
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }    
    }
}
