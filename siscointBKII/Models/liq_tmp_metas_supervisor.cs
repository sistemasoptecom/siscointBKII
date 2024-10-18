using DocumentFormat.OpenXml.Office2010.Excel;
using System;

namespace siscointBKII.Models
{
    public class liq_tmp_metas_supervisor
    {
        public Int64 id { get; set; } 
        public Int32 mes_importe { get; set; } 
        public string cedula_supervisor { get; set; }
        public string zona {  get; set; }
        public Int32 numero_carta_meta_ftth { get; set; } 
        public Int32 numero_carta_meta_movil { get; set; }
        public Int32 numero_carta_meta_tv {  get; set; } 
        public Int32 numero_carta_meta_linea_basica { get; set; }
        public string periodo_importe { get; set; }
        public string usuario { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion {  get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
