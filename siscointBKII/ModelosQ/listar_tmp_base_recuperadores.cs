using Microsoft.Crm.Sdk.Messages;

namespace siscointBKII.ModelosQ
{
    public class listar_tmp_base_recuperadores
    {
        public string CEDULA_RECUPERADOR {  get; set; }
        public string NOMBRE_RECUPERADOR { get; set; }
        public string CEDULA_SUPERVISOR { get; set; }
        public string NOMBRE_SUPERVISOR { get; set; }
        public string ZONA {  get; set; }
        public string PERIODO { get; set; }
        public string ESQUEMA { get; set; }
        public string CIUDAD {  get; set; }
        public string TIPO_CALCULO_FIJA { get; set; }
        public string TIPO_CALCULO_MOVIL { get; set; }
    }
}
