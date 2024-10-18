using System;

namespace siscointBKII.Models
{
    public class liq_valores_megabytes
    {
        public Int64 id { get; set; } 
        public Int32 valor_mega { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha { get; set; } 
        public Int32 codigo_valor { get; set; } 
        public Int32 codigo_tipo_escala { get; set; }
        public Int32 homologa_valor_orden { get; set; }
        public Int32 calcula_mega { get; set; } 
    }
}
