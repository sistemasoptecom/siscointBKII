using System;

namespace siscointBKII.Models
{
    public class data_archivos
    {
        public Int64 id {  get; set; }
        public string nombre_archivo { get; set; }
        public string ruta { get; set; }
        public Int32 estado { get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public string categoria { get; set; }   
    }
}
