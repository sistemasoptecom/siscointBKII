using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;

namespace siscointBKII.ModelosQ
{
    public class listar_liq_tbl_pap
    {
        public Int64 id { get; set; }
        public string nivel_escala { get; set; }
        public Int32 rango_altas { get;set; }
        public Int32 valor_mega { get; set; }
        public Int64 valor { get; set; }
        public string cumplimiento { get; set; }
        public string tipo_renta { get; set; } 
        public string descripcion {  get; set; }

    }
}
