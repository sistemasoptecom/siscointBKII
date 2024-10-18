using DocumentFormat.OpenXml.Office2010.Excel;
using System;

namespace siscointBKII.Models
{
    public class liq_consultas
    {
        public Int64 id {  get; set; }  
        public Int64 codigo { get; set; }
        public string descripcion { get; set; }
        public Int32 orden_lista { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public Int32 Tipo_proceso { get; set; }
    }
}
