using System;

namespace siscointBKII.Models
{
    public class liq_tmp_otros_conceptos
    {
        public Int64 id {  get; set; }
        public string cedula_asesor {  get; set; }
        public string cedula_supervisor { get; set; }
        public string zona {  get; set; }  
        public string periodo { get; set; }
        public Int32 cod_tipo_esquema { get; set; }
        public string descripcion {  get; set; }
        public double total { get; set; } 
        public Int32 estado { get; set; }
        public Int32 EsProcesado { get; set; }  
        public Int32 lote_importe { get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public Int32 EsValido { get; set; }
    }
}
