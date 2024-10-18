using System;

namespace siscointBKII.Models
{
    public class liq_escala_altas
    {
        public Int64 id {  get; set; } 
        public Int32 numero_escala_alta { get; set; }
        public string nivel_escala { get; set; }
        public Int32  estado { get; set; }  
        public DateTime fecha_creacion { get; set; }
        public Int32 codigo_escala_altas { get; set; } 
        public Int32 rango_altas { get; set; } 
        public Int32 codigo_tipo_escala { get; set; }
    }
}
