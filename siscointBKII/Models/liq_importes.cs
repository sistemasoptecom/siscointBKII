using System;

namespace siscointBKII.Models
{
    public class liq_importes
    {
        public Int64 id { get; set; }
        public string descripcion { get; set; }
        public Int32 estado { get; set; }   
        public DateTime fecha_creacion { get; set; }    
        public Int32 codigo_importe { get; set; }
        public Int32 orden_lista { get; set; }  
    }
}
