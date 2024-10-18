using System;

namespace siscointBKII.Models
{
    public class tec_liq_base_cierre_semanal
    {
        public Int64 id { get; set; }
        public string periodo_importe { get; set; }
        public string cod_ciudad {  get; set; }
        public Int32 cod_semana { get; set; }
        public string bucket_t1 { get; set; }
        public string tecnologia { get; set; }
        public string subtipo_orden {  get; set; }
        public string pet_atis {  get; set; }
        public Int32 cantidad_decos { get; set; }
        public string p {  get; set; }
        public DateTime fecha_comision {  get; set; }
        public Int32 mes_comision { get; set; }
        public Int32 dia_comision { get; set; }
        public string cedula_tecnico_1 { get; set; }
        public decimal puntos_tecnico_1 { get; set; }
        public string cedula_tecnico_2 { get; set; }
        public decimal puntos_tecnico_2 { get; set; }
        public decimal facturado {  get; set; }
        public string observaciones { get; set; }
        public Int32 estado {  get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}