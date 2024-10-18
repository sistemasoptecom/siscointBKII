using System;

namespace siscointBKII.Models
{
    public class liq_pap
    {
        public Int64 id { get; set; }
        public Int32 cod_liq_pap { get; set; }
        public Int32 valor_mega { get; set; }
        public Int32 valor_nivel { get; set; }
        public Int64 valor { get; set; }
        public string cumplimiento { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public Int32 nivel_cumplimiento { get; set; }
        public Int32 codigo_liq_esq { get; set; }
        public string homologa_cumplimiento { get; set; }
        public Int32 tipo_liquidador { get; set; }
        public string descripcion { get; set; }
    }
}
