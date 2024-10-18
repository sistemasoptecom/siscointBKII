using System;

namespace siscointBKII.Models
{
    public class token_autoriza
    {
        public Int64 Id { get; set; }
        public Int32? token {  get; set; }
        public string? proceso { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha_creacion { get; set; }
        public DateTime? fecha_expira {  get; set; }
        public Int32? estado { get; set; }
        public string correo_envia { get; set; }
    }
}
