using Microsoft.Graph;
using System;
namespace siscointBKII.Models
{
    public class liq_tipo_esquema
    {
        public Int64 id { get; set; }
        public string nombre_tipo_esquema { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; } 
        public Int32 codigo_valor { get; set; } 
        public Int32 esConfigurable { get; set; }
        public Int32 esImporteMetas { get; set; }   
        public string esquema {  get; set; }    

    }
}
