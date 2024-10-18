using System;

namespace siscointBKII.Models
{
    public class tec_liq_conf_items_valores
    {
        public Int64 id { get; set; }
        public string cod_ciudad {  get; set; }
        public string codigo_orden {  get; set; }
        public decimal valor { get; set; }
        public double punto_bonificacion { get; set; }
        public Int32 productividad { get; set; }
        public bool esConfiguracionDeco { get; set; }
        public bool esPuntoAdicional { get; set; }
        public Int32 cantidad_deco { get; set; }
        public double valor_punto_adicional { get; set; }
        public string usuario {  get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
