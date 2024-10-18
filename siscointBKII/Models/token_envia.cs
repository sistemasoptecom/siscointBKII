using System;

namespace siscointBKII.Models
{
    public class token_envia
    {
        public Int64 id { get; set; }
        public Int32 token {  get; set; }
        public string cedula { get; set; }
        public string email { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_expiracion { get; set; }
        public Int32 estado { get; set; }
    }
}
