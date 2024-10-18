using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.ModelosQ;
using siscointBKII.Models;
using System;

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImporteMetasController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;

        public ImporteMetasController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("ListarTipoEquema")]
        [Authorize]
        public IActionResult ListarTipoEquema(dynamic data_recibe)
        {
            List<liq_tipo_esquema> _liq_tipo_esquema = new List<liq_tipo_esquema>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                int parametro = Convert.ToInt32(datObject["parametro"]);
                string concatenateQuery = "";
                if (parametro == 1)
                {
                    concatenateQuery = " and esImporteMetas = 1";
                }
                string query = "select * from liq_tipo_esquema where estado = 1 and esConfigurable = 1"+ concatenateQuery;
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _liq_tipo_esquema.Add(new liq_tipo_esquema
                                {
                                    id = Convert.ToInt64(sdr["id"]),
                                    nombre_tipo_esquema = sdr["nombre_tipo_esquema"]+"",
                                    estado = Convert.ToInt32(sdr["estado"]),
                                    codigo_valor = Convert.ToInt32(sdr["codigo_valor"])
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar tipo esquema", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            

            return Ok(_liq_tipo_esquema);
        }
        #region valido procesos
        [HttpPost("validoEstadoProceso")]
        [Authorize]
        public async Task<IActionResult> validoEstadoProceso(dynamic proceso)
        {
            General.crearImprimeMensajeLog("Entro a la funcion", "validoEstadoProceso", _config.GetConnectionString("conexionDbPruebas"));
            var dataJson = System.Text.Json.JsonSerializer.Serialize(proceso);
            var datObject = JObject.Parse(dataJson);
            string _proceso = Convert.ToString(datObject["proceso"]);
            data_valido_proceso _data_valido_proceso = _context.data_valido_proceso.Where(x => x.proceso == _proceso
                                                                                                && x.estado == 0)
                                                                                         .OrderByDescending(x => x.id)
                                                                                         .FirstOrDefault();
            int valor = 0;
            Int32 Consecutivo_lote = 0;
            Int32 Total_registros = 0;
            decimal porcentaje_registros = 0;
            if(_data_valido_proceso != null)
            {
                //General.crearImprimeMensajeLog("Entro a la validacion", "validoEstadoProceso", _config.GetConnectionString("conexionDbPruebas"));
                valor = _data_valido_proceso.cantidad;
                Consecutivo_lote = _data_valido_proceso.consecutivo_lote;
                string nombre_tabla = _data_valido_proceso.nombre_tabla;
                string query = _data_valido_proceso.nombre_tabla;
                //string query = "select count(*) as total from '" + nombre_tabla + "' where lote_importe = '" + Consecutivo_lote + "' and estado = 1";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            
                            while (sdr.Read())
                            {
                                Total_registros = Convert.ToInt32(sdr["dato"]);
                            }

                        }
                        con.Close();
                    }
                }
                if(Total_registros > 0)
                {
                    //General.crearImprimeMensajeLog("Entro al if", "validoEstadoProceso", _config.GetConnectionString("conexionDbPruebas"));
                    decimal division = (decimal)Total_registros / valor;
                    porcentaje_registros = (division * 100);
                }

            }
            return Ok(porcentaje_registros);
        }
        [HttpPost("cancelarProceso")]
        [Authorize]
        public async Task<IActionResult> cancelarProceso(dynamic proceso)
        {
            General.crearImprimeMensajeLog("Entro a la funcion", "validoEstadoProceso", _config.GetConnectionString("conexionDbPruebas"));
            var dataJson = System.Text.Json.JsonSerializer.Serialize(proceso);
            var datObject = JObject.Parse(dataJson);
            string _proceso = Convert.ToString(datObject["proceso"]);
            data_valido_proceso _data_valido_proceso =  await _context.data_valido_proceso.Where(x => x.proceso == _proceso
                                                                                                && x.estado == 0)
                                                                                         .OrderByDescending(x => x.id)
                                                                                         .FirstOrDefaultAsync();
            string mensaje ="";
            if (_data_valido_proceso != null)
            {
                _data_valido_proceso.estado = 1;
                _data_valido_proceso.fecha_fin_proceso = DateTime.Now;
                _context.data_valido_proceso.Update(_data_valido_proceso);
                await _context.SaveChangesAsync();
                mensaje = "Ok";
            }
            return Ok(mensaje);
        }

        #endregion


        #region carta meta
        [HttpPost("procesarExcelCartaMetasAsesor")]
        [Authorize]
        public async Task<IActionResult> procesarExcelCartaMetasAsesor(dynamic data_recibe)
        {
            General.crearImprimeMensajeLog("Entro a la funcion", "procesarExcelCartaMetasAsesor", _config.GetConnectionString("conexionDbPruebas"));
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            Int32 codigoTipoEsquema = 0;
            int estadoTras = 0;
            int cuentaErrors = 0;
            string periodo_comision = "";
            List<object> arr = new List<object>();
            List<listar_liq_tmp_metas> _listar_liq_tmp_metas = new List<listar_liq_tmp_metas>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["meta"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                codigoTipoEsquema = Convert.ToInt32(datObject["codigo_tipo_esquema"]);
                
                _listar_liq_tmp_metas = JsonConvert.DeserializeObject<List<listar_liq_tmp_metas>>(base_);
                var procesoCartaMeta = _listar_liq_tmp_metas.Select(x => new { x.PERIODO }).Distinct().ToList();
                System.Boolean archivoValido = false;
                if(procesoCartaMeta.Count() == 1)
                {
                    //resultado = "ENTRO AL PRIMER IF DEL TRY";
                    archivoValido = true;
                    if (archivoValido)
                    {
                        //resultado = "ENTRO AL SEGUNDO IF DEL TRY";
                        //aqui ya validado
                        foreach (listar_liq_tmp_metas item in _listar_liq_tmp_metas)
                        {
                            Int32 TipoEsquemaValidate = 0;
                            liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                            if (_liq_tipo_pap != null)
                            {

                                if (_liq_tipo_pap.esquema.Equals(item.ESQUEMA))
                                {
                                    TipoEsquemaValidate = 1;
                                }
                            }
                            if (_liq_tipo_pymes != null)
                            {

                                if (_liq_tipo_pymes.esquema.Equals(item.ESQUEMA))
                                {
                                    TipoEsquemaValidate = 2;
                                }
                            }
                            if (_liq_tipo_call != null)
                            {

                                if (_liq_tipo_call.esquema.Equals(item.ESQUEMA))
                                {
                                    TipoEsquemaValidate = 3;
                                }
                            }
                            if(_liq_tipo_pap_ii != null)
                            {
                                if (_liq_tipo_pap_ii.esquema.Equals(item.ESQUEMA))
                                {
                                    TipoEsquemaValidate = 5;
                                }
                            }
                            
                            int Existe = _context.liq_tmp_metas.Where(x => x.cedula_asesor == item.CEDULA
                                                                      && x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                      && x.periodo_importe == item.PERIODO
                                                                      && x.cod_tipo_escala == TipoEsquemaValidate).Count();

                            //if((Existe == 0) && (TipoEsquemaValidate == codigoTipoEsquema))
                                //if ((Existe == 0) && (TipoEsquemaValidate == codigoTipoEsquema))
                                if(Existe == 0)
                                {
                                //resultado = "ENTRO AL TERCER IF DEL TRY";
                                string primer_nombre = "";
                                string segundo_nombre = "";
                                string primer_apellido = "";
                                string segundo_apellido = "";
                                string nombre_completo = "";
                                string supervisor_completo = "";
                                System.Boolean ExiteAsesor = validarExisteEmpleado(item.CEDULA);
                                System.Boolean ExisteSuper = validarExisteEmpleado(item.CEDULA_SUPERVISOR);
                                if (!ExiteAsesor)
                                {
                                    //General.CrearEmpleados(item.CEDULA, item.)
                                    string[] result = item.VENDEDOR.Split(" ");
                                    int count = result.Count();
                                    foreach(string s in result)
                                    {
                                        nombre_completo += s + " ";
                                    }
                                   
                                    //se crea el empleado 
                                    string mensaje1 = await General.crearEmpleadosV2(item.CEDULA,nombre_completo, item.CARGO,item.EMPRESA_CONTRATANTE, "COMERCIAL" , _config.GetConnectionString("conexionDbPruebas"));

                                }
                                if (!ExisteSuper)
                                {

                                    string[] result_2 = item.JEFE_INMEDIATO.Split(" ");
                                    int count2 = result_2.Count();
                                    foreach (string s in result_2)
                                    {
                                        supervisor_completo += s + " ";
                                    }
                                   
                                    //se crea el empleado

                                    //General.crearImprimeMensajeLog("Entro a mirar el empleado ")
                                   // string _mensaje_ = "supervisor es : " + item.CEDULA + " y el nombre es : " + supervisor_completo + " y el cargo es " + item.CARGO;
                                    //General.crearImprimeMensajeLog(_mensaje_, "procesarExcelCartaMetasAsesor", _config.GetConnectionString("conexionDbPruebas"));
                                    string mensaje2 = await General.crearEmpleadosV2(item.CEDULA_SUPERVISOR, supervisor_completo, item.CARGO, item.EMPRESA_SUPERVISOR , "COMERCIAL", _config.GetConnectionString("conexionDbPruebas"));
                                    //General.CrearEmpleados(item.CEDULA_SUPERVISOR, primer_nombre, segundo_nombre, primer_apellido, segundo_apellido, item.CARGO, _config.GetConnectionString("conexionDbPruebas"));
                                }

                                //se procede con la validacion de la lista que no existan los repetidos
                                var duplicados = _listar_liq_tmp_metas.GroupBy(x => x.CEDULA)
                                                                       .Where(g => g.Count() > 1)
                                                                       .Select(x => x.Key)
                                                                       .ToList();

                                var duplicados_periodo = _listar_liq_tmp_metas.GroupBy(x => x.PERIODO)
                                                                       .Where(g => g.Count() == 0)
                                                                       .Select(x => x.Key)
                                                                       .ToList();
                                if (duplicados.Count == 0
                                    && duplicados_periodo.Count == 0)
                                {

                                    //resultado = "ENTRO AL CUARTO IF DEL TRY 1";
                                    //aqui empieza el proceso
                                    //insertamos los datos del excel en la tabla temporal de las carta metas
                                    liq_tmp_metas _liq_tmp_metas = new liq_tmp_metas();
                                    string[] arr_mes_perido = item.PERIODO.Split('-');

                                    int perido = Convert.ToInt32(arr_mes_perido[1]);
                                    _liq_tmp_metas.mes_importe_liq = perido;
                                    _liq_tmp_metas.cedula_asesor = item.CEDULA;
                                    _liq_tmp_metas.cod_tipo_escala = TipoEsquemaValidate;
                                    if (Microsoft.VisualBasic.Information.IsNumeric(item.CARTA_META_FIJA))
                                    {
                                        _liq_tmp_metas.numero_carta_meta_ftth = Convert.ToInt32(item.CARTA_META_FIJA);
                                    }
                                    if (Microsoft.VisualBasic.Information.IsNumeric(item.CARTA_META_MOVIL))
                                    {
                                        _liq_tmp_metas.numero_carta_meta_movil = Convert.ToInt32(item.CARTA_META_MOVIL);
                                    }
                                    if (Microsoft.VisualBasic.Information.IsNumeric(item.CARTA_META_TV))
                                    {
                                        _liq_tmp_metas.numero_carta_meta_tv = Convert.ToInt32(item.CARTA_META_TV);
                                    }
                                    _liq_tmp_metas.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                    _liq_tmp_metas.fecha_ingreso = DateTime.ParseExact(item.FECHA_DE_INGRESO, "dd/MM/yyyy", null);
                                    _liq_tmp_metas.fecha_retiro = DateTime.ParseExact(item.RETIRO, "dd/MM/yyyy", null);
                                    //_liq_tmp_metas.fecha_ingreso = DateTime.Parse(item.FECHA_DE_INGRESO);
                                    //_liq_tmp_metas.fecha_retiro = DateTime.Parse(item.RETIRO);
                                    _liq_tmp_metas.zona = item.ZONA;
                                    _liq_tmp_metas.activo = item.ACTIVO;
                                    _liq_tmp_metas.periodo_importe = item.PERIODO;
                                    _liq_tmp_metas.estado = 1;
                                    _liq_tmp_metas.usuario = usuario_;
                                    _liq_tmp_metas.fecha_creacion = DateTime.Now;
                                    _liq_tmp_metas.fecha_modificacion = DateTime.Now;
                                    _liq_tmp_metas.empresa_contratante = item.EMPRESA_CONTRATANTE;
                                    _liq_tmp_metas.empresa_supervisor = item.EMPRESA_SUPERVISOR;
                                    _liq_tmp_metas.ciudad = item.CIUDAD;
                                    Int32 tipo_liquidador = 0;
                                    Int32.TryParse(item.TIPO_LIQUIDADOR, out tipo_liquidador);
                                    _liq_tmp_metas.tipo_liquidador = tipo_liquidador;
                                    _liq_tmp_metas.nombre_liquidador = item.NOMBRE_LIQUIDADOR;

                                    //resultado = "ENTRO AL CUARTO IF DEL TRY 2";
                                    var a = _context.liq_tmp_metas.Add(_liq_tmp_metas).State;
                                    if (a == Microsoft.EntityFrameworkCore.EntityState.Added)
                                    {
                                        //resultado = "ENTRO AL QUINTO IF DEL TRY";
                                        _context.liq_tmp_metas.Add(_liq_tmp_metas);
                                        //_context.SaveChanges();
                                        //el siguiente paso
                                        // se crea el esquema de la comision 
                                        liq_comision_asesor _liq_comision_asesor = new liq_comision_asesor();
                                        _liq_comision_asesor.mes_comision = perido;
                                        _liq_comision_asesor.codigo_tipo_escala = TipoEsquemaValidate;
                                        _liq_comision_asesor.cedula_asesor = item.CEDULA;
                                        _liq_comision_asesor.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                        if (Microsoft.VisualBasic.Information.IsNumeric(item.CARTA_META_FIJA))
                                        {
                                            _liq_comision_asesor.meta_asesor = Convert.ToInt32(item.CARTA_META_FIJA);
                                        }
                                        if (Microsoft.VisualBasic.Information.IsNumeric(item.CARTA_META_MOVIL))
                                        {
                                            _liq_comision_asesor.meta_asesor_2 = Convert.ToInt32(item.CARTA_META_MOVIL);
                                        }
                                        if (Microsoft.VisualBasic.Information.IsNumeric(item.CARTA_META_TV))
                                        {
                                            _liq_comision_asesor.meta_asesor_3 = Convert.ToInt32(item.CARTA_META_TV);
                                        }

                                        _liq_comision_asesor.periodo = item.PERIODO;
                                        _liq_comision_asesor.zona = item.ZONA;
                                        _liq_comision_asesor.ciudad = item.CIUDAD;
                                        _liq_comision_asesor.tipo_liquidador = tipo_liquidador;
                                        _liq_comision_asesor.nombre_liquidador = item.NOMBRE_LIQUIDADOR;
                                        _liq_comision_asesor.estado = 1;
                                        _liq_comision_asesor.EsAsesorValido = true;
                                        _liq_comision_asesor.usuario = usuario_;
                                        _liq_comision_asesor.fecha_creacion = DateTime.Now;
                                        _liq_comision_asesor.fecha_modificacion = DateTime.Now;
                                        
                                        var b = _context.liq_comision_asesor.Add(_liq_comision_asesor).State;
                                        if (b == Microsoft.EntityFrameworkCore.EntityState.Added)
                                        {
                                            _context.liq_comision_asesor.Add(_liq_comision_asesor);
                                            //_context.SaveChanges();
                                        }

                                    }

                                }
                                else
                                {
                                    //notificar el resultado en un alert
                                    resultado += " EN EL ARCHIVO DE EXCEL CUENTA CON CEDULAS REPETIDAS DE LOS ASESORES, SUPERVISORES O EL PERIODO";
                                    cuentaErrors++;
                                    estadoTras = 1;
                                }
                                _context.SaveChanges();
                            }

                            


                           
                        }
                        if (cuentaErrors == 0)
                        {

                            resultado += " CARTA META IMPORTADA DE FORMA EXITOSA!";
                            //validamos que si exite un periodo activo no lo cree nuevamente
                            //var _perido_comison;
                            var _perido_comison = _listar_liq_tmp_metas.Select(x => x.PERIODO).Distinct().First();
                            if (_perido_comison != null)
                            {
                                int cuentaPerido = _context.liq_periodo_comision_v2.Where(x => x.periodo.Contains(_perido_comison.ToString())).Count();
                                if (cuentaPerido == 0)
                                {
                                    //se crea el perido
                                    General.crearPeriodo(_perido_comison.ToString(), _config.GetConnectionString("conexionDbPruebas"));
                                }
                            }


                            //_context.SaveChanges();
                        }
                        else
                        {
                            resultado += " ERROR AL PROCESAR LA CARTA META";
                            estadoTras = 1;
                        }
                    }
                    else
                    {
                        resultado = "EL ARCHIVO A PROCESAR NO PERTENECE AL ESQUEMA EN EL EXCEL";
                        estadoTras = 0;
                    }
                }
                else
                {
                    resultado = "EXISTEN VARIOS TIPOS DE PERIODOS POR FAVOR VALIDAR";
                    estadoTras = 0;
                }
                //ANTES DE INSERTAR VERIFICAR QUE EL ARCHIVO SEA AL DEL TIPO DE ESQUEMA
                //aqui tengo el array
                

            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                resultado = e.Message + " " + e.Source + " " + e.StackTrace + " " + methodName;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Carta Metas", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
                //eliminar los registros por lotes del importe
            }
            //validar El estado de la trasaccion
           
            arr.Add(resultado);
            arr.Add(estadoTras);

            return Ok(arr);
        }

        //CARTA META PARA LOS SUPERVISORES
        [HttpPost("procesarExcelCartaMetasSuper")]
        [Authorize]
        public IActionResult procesarExcelCartaMetasSuper(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            Int32 codigoTipoEsquema = 0;
            int estadoTras = 0;
            int cuentaErrors = 0;
            string periodo_comision = "";
            try
            {
                List<listar_liq_tmp_metas_supervisor> _listar_liq_tmp_metas_super = new List<listar_liq_tmp_metas_supervisor>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["meta"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                codigoTipoEsquema = Convert.ToInt32(datObject["codigo_tipo_esquema"]);
                _listar_liq_tmp_metas_super = JsonConvert.DeserializeObject<List<listar_liq_tmp_metas_supervisor>>(base_);
                var procesoCartaMeta = _listar_liq_tmp_metas_super.Select(x => new { x.MES_PERIODO }).Distinct().ToList();
                System.Boolean archivoValido = false;
                if (procesoCartaMeta.Count() == 1)
                {
                    periodo_comision = procesoCartaMeta.Select(x => x.MES_PERIODO).First();
                    archivoValido = true;
                    if (archivoValido)
                    {
                        //General.crearImprimeMensajeLog("Entro a la funcion", "procesarExcelCartaMetasSuper", _config.GetConnectionString("conexionDbPruebas"));
                        //aqui ya validado
                        foreach (listar_liq_tmp_metas_supervisor item in _listar_liq_tmp_metas_super)
                        {
                            Int32 TipoEsquemaValidate = 0;
                            liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();

                            if (_liq_tipo_pap != null)
                            {

                                if (_liq_tipo_pap.esquema.Equals(item.ESQUEMA_COMISION))
                                {
                                    TipoEsquemaValidate = 1;
                                }
                            }
                            if (_liq_tipo_pymes != null)
                            {

                                if (_liq_tipo_pymes.esquema.Equals(item.ESQUEMA_COMISION))
                                {
                                    TipoEsquemaValidate = 2;
                                }
                            }
                            if (_liq_tipo_call != null)
                            {

                                if (_liq_tipo_call.esquema.Equals(item.ESQUEMA_COMISION))
                                {
                                    TipoEsquemaValidate = 3;
                                }
                            }
                            if (_liq_tipo_pap_ii != null)
                            {
                                if (_liq_tipo_pap_ii.esquema.Equals(item.ESQUEMA_COMISION))
                                {
                                    TipoEsquemaValidate = 5;
                                }
                            }

                            int Existe = _context.liq_comision_supervisor.Where(x => x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                && x.periodo == item.MES_PERIODO
                                                                                && x.codigo_tipo_esquema == TipoEsquemaValidate
                                                                                && x.estado == 1).Count();

                            if (Existe == 0)
                            {
                               
                                //validamos que la cedula del supervisor tenga asesores con el periodo
                                int TieneAsesoresPeriodo = _context.liq_comision_asesor.Where(x => x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                              && x.periodo == periodo_comision
                                                                                              && x.estado == 1).Count();
                                //seguimos
                                if (TieneAsesoresPeriodo > 0)
                                {
                                    liq_tmp_metas_supervisor _liq_tmp_metas_super_e = new liq_tmp_metas_supervisor();
                                    string[] arr_mes_perido = item.MES_PERIODO.Split('-');
                                    int perido = Convert.ToInt32(arr_mes_perido[1]);
                                    _liq_tmp_metas_super_e.mes_importe = perido;
                                    _liq_tmp_metas_super_e.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                    _liq_tmp_metas_super_e.zona = item.ZONA;
                                    if (item.META_FTTH != null)
                                    {
                                        _liq_tmp_metas_super_e.numero_carta_meta_ftth = Convert.ToInt32(item.META_FTTH);
                                    }
                                    if (item.META_MOVIL != null)
                                    {
                                        _liq_tmp_metas_super_e.numero_carta_meta_movil = Convert.ToInt32(item.META_MOVIL);
                                    }
                                    if (item.META_LB != null)
                                    {
                                        _liq_tmp_metas_super_e.numero_carta_meta_linea_basica = Convert.ToInt32(item.META_LB);
                                    }
                                    if (item.META_TV != null)
                                    {
                                        _liq_tmp_metas_super_e.numero_carta_meta_tv = Convert.ToInt32(item.META_TV);
                                    }
                                    _liq_tmp_metas_super_e.periodo_importe = item.MES_PERIODO;
                                    _liq_tmp_metas_super_e.usuario = usuario_;
                                    _liq_tmp_metas_super_e.estado = 1;
                                    _liq_tmp_metas_super_e.fecha_creacion = DateTime.Now;
                                    _liq_tmp_metas_super_e.fecha_modificacion = DateTime.Now;
                                    var a = _context.liq_tmp_metas_supervisor.Add(_liq_tmp_metas_super_e).State;
                                    if (a == Microsoft.EntityFrameworkCore.EntityState.Added)
                                    {
                                      
                                        //aqui el calculo para el esquema de los supervisores ... continuara ...
                                        _context.liq_tmp_metas_supervisor.Add(_liq_tmp_metas_super_e);
                                        liq_comision_supervisor _liq_comision_supervisor_e = new liq_comision_supervisor();
                                        _liq_comision_supervisor_e.mes_comision = perido;
                                        _liq_comision_supervisor_e.periodo = item.MES_PERIODO;
                                        _liq_comision_supervisor_e.codigo_tipo_esquema = TipoEsquemaValidate;
                                        _liq_comision_supervisor_e.cedula_supervisor = item.CEDULA_SUPERVISOR;

                                        double cantidad_ftth_asesor_megas = 0;
                                        _liq_comision_supervisor_e.numero_meta_ftth = _liq_tmp_metas_super_e.numero_carta_meta_ftth;
                                        cantidad_ftth_asesor_megas = _context.liq_tmp_base_cierre.Where(x =>
                                                                                                        x.velocidad > 0
                                                                                                        && x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                                        && x.periodo == item.MES_PERIODO
                                                                                                        && x.cod_tipo_esquema == TipoEsquemaValidate
                                                                                                        && x.estado == 1)
                                                                                                  .Select(x => x.unidad)
                                                                                                  .Sum();

                                        //string msj_cantidad_ftth_asesor_megas = "la cantidad de asesores megas del super "+item.CEDULA_SUPERVISOR+" es "+ cantidad_ftth_asesor_megas;
                                        //General.crearImprimeMensajeLog(msj_cantidad_ftth_asesor_megas, "procesarExcelCartaMetasSuper", _config.GetConnectionString("conexionDbPruebas"));

                                        _liq_comision_supervisor_e.numero_cumplimiento_asesor_ftth = (Int32)cantidad_ftth_asesor_megas;
                                        double aux_homologa_porcentaje_ftth = 0;
                                        //aux_homologa_porcentaje_ftth = (double)((double)(_liq_comision_supervisor_e.numero_meta_ftth /
                                        //                                        cantidad_ftth_asesor_megas) * 100);
                                        aux_homologa_porcentaje_ftth = (double)((double)(cantidad_ftth_asesor_megas / _liq_comision_supervisor_e.numero_meta_ftth) * 100);
                                        //string msj_aux_homologa_porcentaje_ftth = "el valor porcentajes ftth es del super " + item.CEDULA_SUPERVISOR + " es " + aux_homologa_porcentaje_ftth;
                                        //General.crearImprimeMensajeLog(msj_aux_homologa_porcentaje_ftth, "procesarExcelCartaMetasSuper", _config.GetConnectionString("conexionDbPruebas"));

                                        _liq_comision_supervisor_e.homologa_porcentaje_ftth = aux_homologa_porcentaje_ftth;
                                        _liq_comision_supervisor_e.porcentaje_cumplimiento_asesor_ftth = aux_homologa_porcentaje_ftth + " % ";

                                        double homologa_peso_fthh = 0;
                                        homologa_peso_fthh = _context.liq_cumpliento_peso_v2.Where(x => x.descripcion_producto == "FTTH"
                                                                                                   && x.codigo_tipo_esquema == TipoEsquemaValidate
                                                                                                   && x.estado == 1)
                                                                                            .Select(x => x.homologa_peso)
                                                                                            .FirstOrDefault();

                                        double aux_homologa_porcetaje_peso_ftth_t = 0;
                                        aux_homologa_porcetaje_peso_ftth_t = aux_homologa_porcentaje_ftth * (homologa_peso_fthh / 100);

                                        //string msj_homologa_peso_fthh = "el peso ftth es del super " + item.CEDULA_SUPERVISOR + " es " + homologa_peso_fthh;
                                       
                                        string peso_ftth = "";
                                        peso_ftth = _context.liq_cumpliento_peso_v2.Where(x => x.descripcion_producto == "FTTH"
                                                                                          && x.codigo_tipo_esquema == TipoEsquemaValidate
                                                                                          && x.estado == 1)
                                                                                   .Select(x => x.peso)
                                                                                   .FirstOrDefault();
                                        _liq_comision_supervisor_e.homologa_peso_ftth = aux_homologa_porcetaje_peso_ftth_t;
                                        _liq_comision_supervisor_e.peso_cumpliento_ftth = aux_homologa_porcetaje_peso_ftth_t + " % " + " : " + peso_ftth;


                                        if (TipoEsquemaValidate == 1 || TipoEsquemaValidate == 5)
                                        {

                                            //validar si con todas las altas de los moviles
                                            _liq_comision_supervisor_e.numero_meta_movil = _liq_tmp_metas_super_e.numero_carta_meta_movil;
                                            double numero_cumplimiento_asesor_movil = 0;
                                            numero_cumplimiento_asesor_movil = _context.liq_comision_asesor.Where(x => x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                                                  && x.periodo == item.MES_PERIODO
                                                                                                                  && x.estado == 1)
                                                                                                           .Select(x => x.numero_plan_movil)
                                                                                                           .Sum();

                                           
                                            _liq_comision_supervisor_e.numero_cumpliento_asesor_movil = (Int32)numero_cumplimiento_asesor_movil;
                                            double porcentaje_cumplimiento_movil = 0;
                                            if (numero_cumplimiento_asesor_movil > 0)
                                            {
                                                //porcentaje_cumplimiento_movil = (double)(_liq_comision_supervisor_e.numero_meta_movil /
                                                //                                     numero_cumplimiento_asesor_movil);
                                                porcentaje_cumplimiento_movil = (double)((double)(numero_cumplimiento_asesor_movil / _liq_comision_supervisor_e.numero_meta_movil) * 100);
                                            }
                                            else
                                            {
                                                porcentaje_cumplimiento_movil = 0;
                                            }

                                           
                                            _liq_comision_supervisor_e.homologa_porcentaje_movil = porcentaje_cumplimiento_movil;
                                            _liq_comision_supervisor_e.porcentaje_cumplimiento_asesor_movil = porcentaje_cumplimiento_movil + " % ";

                                            double peso_cumpliento_movil = _context.liq_cumpliento_peso_v2.Where(x => x.descripcion_producto == "MOVIL"
                                                                                                                 && x.codigo_tipo_esquema == TipoEsquemaValidate
                                                                                                                 && x.estado == 1)
                                                                                                          .Select(x => x.homologa_peso)
                                                                                                          .FirstOrDefault();
                                            double homologa_peso_movil = porcentaje_cumplimiento_movil * (peso_cumpliento_movil / 100);

                                           
                                            string peso_movil = "";
                                            peso_movil = _context.liq_cumpliento_peso_v2.Where(x => x.descripcion_producto == "MOVIL"
                                                                                               && x.codigo_tipo_esquema == TipoEsquemaValidate
                                                                                               && x.estado == 1)
                                                                                        .Select(x => x.peso)
                                                                                        .FirstOrDefault();

                                            _liq_comision_supervisor_e.homolog_peso_movil = homologa_peso_movil;
                                            _liq_comision_supervisor_e.peso_cumplimiento_movil = homologa_peso_movil + " % " + " : " + peso_movil;

                                        }

                                        double total_homologa_cumplimiento = 0;
                                        total_homologa_cumplimiento = (_liq_comision_supervisor_e.homologa_peso_ftth +
                                                                       _liq_comision_supervisor_e.homolog_peso_movil +
                                                                       _liq_comision_supervisor_e.homologa_peso_tv);

                                        
                                        _liq_comision_supervisor_e.total_homologa_cumplimiento = total_homologa_cumplimiento;

                                        _liq_comision_supervisor_e.total_porcentaje_cumplimiento = total_homologa_cumplimiento + " % ";
                                        double valor_comision = 0;
                                        List<liq_esquema_supervisores> _Comision_Supervisors = new List<liq_esquema_supervisores>();
                                        _Comision_Supervisors = _context.liq_esquema_supervisores.Where(x => x.codigo_tipo_escala == TipoEsquemaValidate
                                                                                                       && x.estado == 1).ToList();

                                        valor_comision = proceso_comision_supervisor(_Comision_Supervisors, total_homologa_cumplimiento);

                                       

                                        _liq_comision_supervisor_e.comision = valor_comision;
                                        
                                        //validamos en la tabla de aceleracion o desaleracion 
                                        List<liq_super_esquema_acelerador> _liq_super_esquema_acelerador = new List<liq_super_esquema_acelerador>();
                                        _liq_super_esquema_acelerador = _context.liq_super_esquema_acelerador.Where(x => x.codigo_tipo_esquema == TipoEsquemaValidate
                                                                                                                    && x.estado == 1).ToList();

                                        string valor_factor_mult = "";
                                        string aceleracion_desalerelacion = "";
                                        double valor_num_factor_mult = 0;

                                        //aqui sacamos la homologacion de las cumplimiento
                                        double homologa_cumpliemiento_asesores = 0;
                                        double numero_asesores_validos = _context.liq_comision_asesor.Where(x => x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                                            && x.periodo == item.MES_PERIODO
                                                                                                            && x.estado == 1
                                                                                                            && x.EsAsesorValido).Count();

                                        double numero_asesores_cumplen = _context.liq_comision_asesor.Where(x => x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                                            && x.periodo == item.MES_PERIODO
                                                                                                            && x.estado == 1
                                                                                                            && x.asesor_cumple == 1).Count();
                                       
                                        homologa_cumpliemiento_asesores = (double)((double)(numero_asesores_cumplen / numero_asesores_validos) * 100);

                                        _liq_comision_supervisor_e.numero_asesores_validos = numero_asesores_validos;
                                        _liq_comision_supervisor_e.numero_cumplimiento_asesores = numero_asesores_cumplen;
                                        _liq_comision_supervisor_e.homologa_cumpliento_asesores = homologa_cumpliemiento_asesores;
                                        _liq_comision_supervisor_e.cumplimiento_asesores = homologa_cumpliemiento_asesores + " % ";

                                        //string[] recibe_esquema_acelerador = new string[2];
                                        if (_liq_super_esquema_acelerador.Count > 0)
                                        {
                                            
                                            string[] recibe_esquema_acelerador = proceso_liq_esquema_acelerador(_liq_super_esquema_acelerador, homologa_cumpliemiento_asesores);
                                            valor_factor_mult = recibe_esquema_acelerador[0];
                                            aceleracion_desalerelacion = recibe_esquema_acelerador[1];

                                           
                                        }
                                       
                                        if (!string.IsNullOrEmpty(valor_factor_mult))
                                        {
                                            if (valor_factor_mult.Contains("-"))
                                            {
                                                string aux_valor_factor_mult = valor_factor_mult.Substring(1, 4);
                                                valor_num_factor_mult = Double.Parse(aux_valor_factor_mult);
                                            }
                                            
                                            valor_num_factor_mult = Double.Parse(valor_factor_mult);
                                        }
                                        
                                        //valor_num_factor_mult = Convert.ToDouble(valor_factor_mult);
                                        double descuento_comision = valor_comision * (valor_num_factor_mult/100);
                                        double total_comision = valor_comision + (descuento_comision);
                                        _liq_comision_supervisor_e.aceleracion_desaceleracion = aceleracion_desalerelacion;
                                        _liq_comision_supervisor_e.total_comision = total_comision;
                                        _liq_comision_supervisor_e.usuario = usuario_;
                                        _liq_comision_supervisor_e.estado = 1;
                                        _liq_comision_supervisor_e.fecha_creacion = DateTime.Now;
                                        _liq_comision_supervisor_e.fecha_modificacion = DateTime.Now;

                                        _context.liq_comision_supervisor.Add(_liq_comision_supervisor_e);
                                        _context.SaveChanges();
                                    }
                                }
                            }
                        }
                        resultado = "PROCESADO CORRECTAMENTE ";
                    }
                    else
                    {
                        resultado = "ARCHIVO INVALIDO";
                    }
                }
                else
                {
                    resultado = "EXITE VARIOS PERIODOS";
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                string msj7 = "Entro al exeption la exepcion es : " + e.Message;
                General.crearImprimeMensajeLog(msj7, "procesarExcelCartaMetasSuper Excep", _config.GetConnectionString("conexionDbPruebas"));
                string msj6 = "Entro al exeption la exepcion es : " + e.StackTrace;
                General.crearImprimeMensajeLog(msj6, "procesarExcelCartaMetasSuper Excep", _config.GetConnectionString("conexionDbPruebas"));

                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Carta Metas Super", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }

        //CARTA META DE LOS ANALISTA 
        [HttpPost("procesarExcelCartaMetaRecuperador")]
        [Authorize]
        public IActionResult procesarExcelCartaMetaRecuperador(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            Int32 codigoTipoEsquema = 0;
            int estadoTras = 0;
            int cuentaErrors = 0;
            string periodo_comision = "";
            try
            {
                List<listar_tmp_base_recuperadores> _listar_tmp_base_recuperadores = new List<listar_tmp_base_recuperadores>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["data"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                codigoTipoEsquema = Convert.ToInt32(datObject["codigo_tipo_esquema"]);
                _listar_tmp_base_recuperadores = JsonConvert.DeserializeObject<List<listar_tmp_base_recuperadores>>(base_);
                var procesarCartaMeta = _listar_tmp_base_recuperadores.Select(x => new { x.PERIODO }).Distinct().ToList();
                System.Boolean archivoValido = false;
                if(procesarCartaMeta.Count()  == 1)
                {
                    periodo_comision = procesarCartaMeta.Select(x => x.PERIODO).First();
                    archivoValido = true;
                    if (archivoValido)
                    {
                        //AQUI PROCESO LA DATA EN EXCEL
                        foreach(listar_tmp_base_recuperadores item in _listar_tmp_base_recuperadores)
                        {
                            Int32 TipoEsquemaValidate = 0;
                            liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();

                            if (_liq_tipo_pap != null)
                            {

                                if (_liq_tipo_pap.esquema.Equals(item.ESQUEMA))
                                {
                                    TipoEsquemaValidate = 1;
                                }
                            }
                            if (_liq_tipo_pymes != null)
                            {

                                if (_liq_tipo_pymes.esquema.Equals(item.ESQUEMA))
                                {
                                    TipoEsquemaValidate = 2;
                                }
                            }
                            if (_liq_tipo_call != null)
                            {

                                if (_liq_tipo_call.esquema.Equals(item.ESQUEMA))
                                {
                                    TipoEsquemaValidate = 3;
                                }
                            }
                            if (_liq_tipo_pap_ii != null)
                            {
                                if (_liq_tipo_pap_ii.esquema.Equals(item.ESQUEMA))
                                {
                                    TipoEsquemaValidate = 5;
                                }
                            }
                            
                            int Existe  = _context.liq_tmp_recuperadores.Where(x => x.cedula_recuperador == item.CEDULA_RECUPERADOR
                                                                               && x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                               && x.periodo == item.PERIODO
                                                                               && x.codigo_tipo_esquema == TipoEsquemaValidate
                                                                               && x.ciudad == item.CIUDAD).Count();
                            
                            if(Existe == 0)
                            {
                                System.Boolean tipo_calculo_fija = false;
                                System.Boolean tipo_calculo_movil = false;
                                double numero_meta_fijas = 0;
                                double numero_altas_fijas = 0;
                                double ejecucion_fijas = 0;
                                //string ejecucion_string = "";
                                double numero_vendedores = 0;
                                double numero_vendedores_cumplen = 0;
                                double ejecucion_vendedores_cumplen = 0;
                                //string porcentaje_ejecucion_vendedores = "";
                                double numero_meta_movil = 0;
                                double numero_altas_movil = 0;
                                double ejecucion_altas_movil = 0;
                                //string porcentaje_ejecucion_movil = "";

                                //se valide que el supervisor 

                                int tienePeriodoSupervisor = _context.liq_comision_supervisor.Where(x => x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                                    && x.periodo == item.PERIODO
                                                                                                    && x.codigo_tipo_esquema == TipoEsquemaValidate
                                                                                                    && x.estado == 1).Count();
                                if (tienePeriodoSupervisor > 0)
                                {
                                    liq_tmp_recuperadores _liq_tmp_recuperadores_e = new liq_tmp_recuperadores();
                                    _liq_tmp_recuperadores_e.cedula_recuperador = item.CEDULA_RECUPERADOR;
                                    _liq_tmp_recuperadores_e.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                    _liq_tmp_recuperadores_e.zona = item.ZONA;
                                    _liq_tmp_recuperadores_e.codigo_tipo_esquema = TipoEsquemaValidate;
                                    _liq_tmp_recuperadores_e.periodo = item.PERIODO;
                                    _liq_tmp_recuperadores_e.ciudad = item.CIUDAD;
                                    if (item.TIPO_CALCULO_FIJA == "1")
                                    {
                                        tipo_calculo_fija = true;
                                    }
                                    if (item.TIPO_CALCULO_MOVIL == "1")
                                    {
                                        tipo_calculo_movil = true;
                                    }
                                    _liq_tmp_recuperadores_e.tipo_caluclo_fija = tipo_calculo_fija;
                                    _liq_tmp_recuperadores_e.tipo_calculo_movil = tipo_calculo_movil;
                                    //calculo apunte a la comision del periodo del metas supervisor
                                    liq_comision_supervisor _liq_comision_supervisor_e = _context.liq_comision_supervisor.Where(x => x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                                                                && x.periodo == item.PERIODO
                                                                                                                                && x.estado == 1).FirstOrDefault();
                                    if(_liq_comision_supervisor_e != null)
                                    {
                                        //altas
                                        numero_meta_fijas = (double)_liq_comision_supervisor_e.numero_meta_ftth;
                                        numero_altas_fijas = (double)_liq_comision_supervisor_e.numero_cumplimiento_asesor_ftth;
                                        ejecucion_fijas = (numero_altas_fijas / numero_meta_fijas) * 100;
                                        _liq_tmp_recuperadores_e.numero_meta_fija = numero_meta_fijas;
                                        _liq_tmp_recuperadores_e.numero_altas_fija = numero_altas_fijas;
                                        _liq_tmp_recuperadores_e.ejecucion_fijas = ejecucion_fijas;
                                        _liq_tmp_recuperadores_e.porcentaje_ejecucion_fija = ejecucion_fijas + " % ";

                                        //vendores
                                        numero_vendedores = (double)_liq_comision_supervisor_e.numero_asesores_validos;
                                        numero_vendedores_cumplen = (double)_liq_comision_supervisor_e.numero_cumplimiento_asesores;
                                        ejecucion_vendedores_cumplen = (numero_vendedores_cumplen / numero_vendedores) * 100;
                                        _liq_tmp_recuperadores_e.numero_vendedores = numero_vendedores;
                                        _liq_tmp_recuperadores_e.vendendores_cumplen = numero_vendedores_cumplen;
                                        _liq_tmp_recuperadores_e.ejecucion_vendedores_cumplen = ejecucion_vendedores_cumplen;
                                        _liq_tmp_recuperadores_e.porcentaje_ejecucion_vendores = ejecucion_vendedores_cumplen + " % ";

                                        //movil
                                        numero_meta_movil = (double)_liq_comision_supervisor_e.numero_meta_movil;
                                        numero_altas_movil = (double)_liq_comision_supervisor_e.numero_cumpliento_asesor_movil;
                                        ejecucion_altas_movil = (numero_altas_movil / numero_meta_movil) * 100;
                                        _liq_tmp_recuperadores_e.numero_meta_movil = numero_meta_movil;
                                        _liq_tmp_recuperadores_e.numero_altas_movil = numero_altas_movil;
                                        _liq_tmp_recuperadores_e.ejecucion_altas_movil = ejecucion_altas_movil;
                                        _liq_tmp_recuperadores_e.porcentaje_ejecucion_movil = ejecucion_altas_movil + " % ";
                                    }
                                    _liq_tmp_recuperadores_e.usuario = nombreUsuario_;
                                    _liq_tmp_recuperadores_e.estado = 1;
                                    _liq_tmp_recuperadores_e.fecha_creacion = DateTime.Now;
                                    _liq_tmp_recuperadores_e.fecha_modificacion = DateTime.Now;
                                    //aqui siguiente paso
                                    // se llena la tabla temporal
                                    _context.liq_tmp_recuperadores.Add(_liq_tmp_recuperadores_e);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    resultado = "SUPERVISOR : " + item.CEDULA_SUPERVISOR + " CON EL PERIODO : " + item.PERIODO + " NO TIENE UN CARGUE METAS";
                                }
                            }
                        }
                        //aqui el proceso 
                        string msg = validarSaldosRecuperadores(periodo_comision, usuario_);
                    }
                    else
                    {
                        resultado = "ARCHIVO INVALIDO";
                    }
                }
                else
                {
                    resultado = "EXISTES VARIOS PERIDOS";
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Carta Metas Analista", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = "";
            return Ok(json);
        }

        #endregion

        #region base cierre

        [HttpPost("procesarExcelBaseCierrePap")]
        [Authorize]
        public async Task<IActionResult> procesarExcelBaseCierrePap(dynamic data_recibe)
        {
            General.crearImprimeMensajeLog("Entro a la funcion", "procesarExcelPenalizacionesMegasPap", _config.GetConnectionString("conexionDbPruebas"));
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int tipo_esquema = 0;
            string periodo = "";
            string mensaje = "";
            try
            {
                List<listar_tmp_base_cierre> _listar_tmp_base_cierre = new List<listar_tmp_base_cierre>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["baseCierre"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);
                _listar_tmp_base_cierre = JsonConvert.DeserializeObject<List<listar_tmp_base_cierre>>(base_);
                System.Boolean EsArchivoValido = false;
                var validar_base_cierre_periodo = _listar_tmp_base_cierre.Select(x => new { x.MES_LIQUIDACION }).Distinct().ToList();
                if(validar_base_cierre_periodo.Count() == 1)
                {
                    foreach(var i in validar_base_cierre_periodo)
                    {
                        if(i.MES_LIQUIDACION == periodo)
                        {
                            EsArchivoValido = true;
                        }
                    }
                    if (EsArchivoValido)
                    {
                        //crear un distinct de empleado, supervisor y periodo
                        //int baseCierreSinMetas = 0;
                        //string tipo_esquema_validate = "";
                        //switch (tipo_esquema)
                        //{
                        //    case 1:
                        //        tipo_esquema_validate = "PAP";
                        //        break;
                        //    case 2:
                        //        tipo_esquema_validate = "PYMES";
                        //        break;
                        //    case 3:
                        //        tipo_esquema_validate = "CALL OUT";
                        //        break;
                        //    case 5:
                        //        tipo_esquema_validate = "PAP II";
                        //        break;
                        //}
                        //var validar_base_cierre_metas = _listar_tmp_base_cierre.Select(x => new { x.CEDULA_ASESOR, x.DOCUMENTO_SUPERVISOR, x.MES_LIQUIDACION, x.TIPO_ESQUEMA })
                        //                                                       .Distinct().Where(x => x.MES_LIQUIDACION == periodo
                        //                                                                         && x.TIPO_ESQUEMA == tipo_esquema_validate).ToList();

                        System.Boolean tienePap = false;
                        System.Boolean tienePapII = false;
                        System.Boolean tienePymes = false;
                        System.Boolean tieneCall = false;
                        Int32 codigoTipoEsquemaPap = 0;
                        Int32 codigoTipoEsquemaPapII = 0;
                        Int32 codigoTipoEsquemaPymes = 0;
                        Int32 codigoTipoEsquemaCall = 0;
                        var validarTiposEsquemasExcel = _listar_tmp_base_cierre.Select(x => new { x.TIPO_ESQUEMA }).Distinct().ToList();
                        
                        foreach(var item in validarTiposEsquemasExcel)
                        {
                            if(item.TIPO_ESQUEMA == "PAP")
                            {
                                tienePap = true;
                                codigoTipoEsquemaPap = 1;
                            }
                            if(item.TIPO_ESQUEMA == "PAP II")
                            {
                                tienePapII = true;
                                codigoTipoEsquemaPapII = 5;
                            }
                            if(item.TIPO_ESQUEMA == "PYMES")
                            {
                                tienePymes = true;
                                codigoTipoEsquemaPymes = 2;
                            }
                            if(item.TIPO_ESQUEMA == "CALL OUT")
                            {
                                tieneCall = true;
                                codigoTipoEsquemaCall = 3;
                            }
                        }
                        
                        
                        
                       
                        if (_listar_tmp_base_cierre.Count > 0)
                        {
                            Int64 consecutivo_lote = consecutivo_lote_importe();
                            Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                            string subQuery = "select count(*) as dato from liq_tmp_base_cierre where lote_importe = "+ _consecutivo_lote +" and estado = 1";
                            General.crearDataValidoProceso("procesarExcelBaseCierrePap", _listar_tmp_base_cierre.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            foreach (listar_tmp_base_cierre item in _listar_tmp_base_cierre)
                            {

                                //validamos el tipo esquema 
                                Int32 TipoEsquemaValidate = 0;
                                liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                                if (_liq_tipo_pap != null)
                                {

                                    if (_liq_tipo_pap.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 1;
                                    }
                                }
                                if (_liq_tipo_pymes != null)
                                {

                                    if (_liq_tipo_pymes.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 2;
                                    }
                                }
                                if (_liq_tipo_call != null)
                                {

                                    if (_liq_tipo_call.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 3;
                                    }
                                }
                                if (_liq_tipo_pap_ii != null)
                                {

                                    if (_liq_tipo_pap_ii.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 5;
                                    }
                                }

                                int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                                                                && x.cod_peticion == item.ID_PETICION
                                                                                && x.periodo == periodo
                                                                                && x.cod_tipo_esquema == TipoEsquemaValidate
                                                                                && x.num_doc_cliente == item.NUM_DOCUMENTO_CLIENTE
                                                                                && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                && x.cedula_supervisor == item.DOCUMENTO_SUPERVISOR
                                                                                && x.estado == 1
                                                                                && x.unidad > 0
                                                                                && x.EsIngresado == 1).Count();
                                if (Existe == 0)
                                {
                                        
                                        //validamos que para este metodo solo inserten las unidades que son mayores a cero
                                        Int32 unidad = Convert.ToInt32(item.UNIDAD);
                                        if (unidad > 0)
                                        {
                                            liq_tmp_base_cierre _liq_tmp_base_cierre = new liq_tmp_base_cierre();
                                            _liq_tmp_base_cierre.producto = item.PRODUCTO;
                                            _liq_tmp_base_cierre.cedula_asesor = item.CEDULA_ASESOR;
                                            _liq_tmp_base_cierre.mes_seg = Convert.ToInt32(item.MES_SEG);
                                            _liq_tmp_base_cierre.unidad = Convert.ToInt32(item.UNIDAD);
                                            _liq_tmp_base_cierre.cod_peticion = item.ID_PETICION;
                                            System.Boolean tieneNumero = EsSoloNumero(item.VELOCIDAD);
                                            double mega_auxiliar = 0;
                                            if (tieneNumero)
                                            {
                                                Int32 valor_longitud_cadena = item.VELOCIDAD.Length;
                                                valor_longitud_cadena = valor_longitud_cadena - 3;
                                                string velocidad_aux = item.VELOCIDAD.Insert(valor_longitud_cadena, ",");
                                                //mensaje += "la longitud es : "+valor_longitud_cadena+" y el string es : "+velocidad_aux;
                                                string[] sub_velocidad = velocidad_aux.Split(',');
                                                mega_auxiliar = Convert.ToDouble(sub_velocidad[0]);
                                                  
                                            }
                                                
                                            //sigo VALIDACION TIPO ESQUEMA COMISION
                                            _liq_tmp_base_cierre.cod_tipo_esquema = TipoEsquemaValidate;

                                            //VALIDAMOS LAS MEGAS DE
                                            int valor_primera_mega = 0;
                                            int valor_segunda_mega = 0;
                                            int valor_tercera_mega = 0;
                                            int valor_cuarta_mega = 0;
                                            int valor_quinta_mega = 0;

                                            if(TipoEsquemaValidate == 1 || TipoEsquemaValidate == 5 || TipoEsquemaValidate == 2)
                                            {
                                                liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                                && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_1 != null)
                                                {
                                                    valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                                    //
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_2 != null)
                                                {
                                                    valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_3 != null)
                                                {
                                                    valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_4 != null)
                                                {
                                                    valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                                                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();

                                                if (_Valores_Megabytes_5 != null)
                                                {
                                                    valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                                }

                                                if (TipoEsquemaValidate == 1 || TipoEsquemaValidate == 5)
                                                {
                                                    //aqui la primera validacion del megabite 1
                                                    if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                                    {
                                                        //mensaje += "AQUI ENTRO EN EL PRIMER IF";
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;

                                                    }
                                                    if ((mega_auxiliar >= valor_segunda_mega) &&
                                                        (mega_auxiliar) < valor_segunda_mega + 99)
                                                    {
                                                        //mensaje += "AQUI ENTRO EN EL SEGUNDO IF";
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;

                                                    }
                                                    if ((mega_auxiliar >= valor_tercera_mega) &&
                                                        (mega_auxiliar) < valor_tercera_mega + 99)
                                                    {
                                                        //mensaje += "AQUI ENTRO EN EL TERCER  IF";
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;

                                                    }
                                                    if (mega_auxiliar >= valor_cuarta_mega)
                                                    {
                                                        //mensaje += "AQUI ENTRO EN EL CUARTO IF";
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;

                                                    }
                                                }
                                                if (TipoEsquemaValidate == 2)
                                                {
                                                    //aqui la primera validacion del megabite 1
                                                    if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_1.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_segunda_mega) &&
                                                        (mega_auxiliar) < valor_segunda_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_2.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_tercera_mega) &&
                                                        (mega_auxiliar) < valor_tercera_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_3.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_cuarta_mega) &&
                                                        (mega_auxiliar) < valor_cuarta_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_4.valor_mega;
                                                    }
                                                    if (mega_auxiliar >= valor_quinta_mega)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_5.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_5.valor_mega;
                                                    }
                                                }

                                            }
                                            else if(TipoEsquemaValidate == 3)
                                            {
                                                liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                                 && x.calcula_mega == 1
                                                                                                                                 && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_1 != null)
                                                {
                                                    valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_2 != null)
                                                {
                                                    valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_3 != null)
                                                {
                                                    valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_4 != null)
                                                {
                                                    valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                                }

                                                //liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                                //                                                                                  && x.calcula_mega == 1
                                                //                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                //if (_Valores_Megabytes_5 != null)
                                                //{
                                                //    valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                                //}
                                                //valido
                                                if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                                {
                                                    if (valor_primera_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;
                                                    }

                                                }
                                                else if ((mega_auxiliar >= valor_segunda_mega) &&
                                                    (mega_auxiliar) < valor_segunda_mega + 99)
                                                {
                                                    if (valor_segunda_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;
                                                    }

                                                }
                                                else if ((mega_auxiliar >= valor_tercera_mega) &&
                                                   (mega_auxiliar) < valor_tercera_mega + 99)
                                                {
                                                    if (valor_tercera_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;
                                                    }

                                                }
                                                else if (mega_auxiliar >= valor_cuarta_mega)
                                                {
                                                    if (valor_cuarta_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;
                                                    }
                                                }
                                            }
                                            

                                           

       
                                            _liq_tmp_base_cierre.empaqhomo = item.EMPAQHOMO;
                                            _liq_tmp_base_cierre.num_doc_cliente = item.NUM_DOCUMENTO_CLIENTE;
                                            _liq_tmp_base_cierre.cedula_supervisor = item.DOCUMENTO_SUPERVISOR;
                                            _liq_tmp_base_cierre.observacion = item.OBSERVACION;
                                            _liq_tmp_base_cierre.migracion_otro = item.MIGRACION_OTRO;
                                            _liq_tmp_base_cierre.periodo = periodo;
                                            _liq_tmp_base_cierre.lote_importe = (Int32)consecutivo_lote;
                                            _liq_tmp_base_cierre.estado = 1;
                                            _liq_tmp_base_cierre.EsProcesado = 0;
                                            _liq_tmp_base_cierre.EsIngresado = 1;

                                            _liq_tmp_base_cierre.usuario = usuario_;
                                            System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.DOCUMENTO_SUPERVISOR, periodo, TipoEsquemaValidate);
                                            if (EsValido)
                                            {
                                                _liq_tmp_base_cierre.EsValido = 1;
                                            }
                                            else
                                            {
                                                _liq_tmp_base_cierre.EsValido = 0;
                                            //aqui realizamos una validacion
                                            //validamos que el asesor no se encuentre en una carta meta actual

                                            //System.Boolean EsAsesorValido = validoAsesorEnCartaMeta(item.CEDULA_ASESOR, periodo, TipoEsquemaValidate);
                                            //    System.Boolean EsAsesorNoSuper = validoAsesorNoSupervisor(item.CEDULA_ASESOR, periodo, TipoEsquemaValidate);
                                            //    if (EsAsesorValido && EsAsesorNoSuper)
                                            //    {
                                            //        //procedemos a la creacion de la carta meta
                                            //        string nombre_completo = "";
                                            //        string supervisor_completo = "";

                                            //        System.Boolean ExiteAsesor = validarExisteEmpleado(item.CEDULA_ASESOR);
                                            //        System.Boolean ExisteSuper = validarExisteEmpleado(item.DOCUMENTO_SUPERVISOR);
                                            //        if (!ExiteAsesor)
                                            //        {
                                            //            string[] result = item.VENDEDOR.Split(" ");
                                            //            int count = result.Count();
                                            //            foreach (string s in result)
                                            //            {
                                            //                nombre_completo += s + " ";
                                            //            }
                                            //            string cargo1 = "ASESOR VENTAS";
                                            //            string mensaje1 = await General.crearEmpleadosV2(item.CEDULA_ASESOR, nombre_completo, cargo1,"2", _config.GetConnectionString("conexionDbPruebas"));
                                            //        }

                                            //        if (!ExisteSuper)
                                            //        {
                                            //            string[] result_2 = item.NOMBRE_SUPERVISOR.Split(" ");
                                            //            int count2 = result_2.Count();
                                            //            foreach (string s in result_2)
                                            //            {
                                            //                supervisor_completo += s + " ";
                                            //            }
                                            //            string cargo2 = "SUPERVISOR VENTAS";
                                            //            string mensaje2 = await General.crearEmpleadosV2(item.DOCUMENTO_SUPERVISOR, supervisor_completo, cargo2,"2", _config.GetConnectionString("conexionDbPruebas"));
                                            //        }

                                            //    }
                                            //    else
                                            //    {
                                            //        _liq_tmp_base_cierre.EsValido = 0;
                                            //    }
                                            }
                                            _liq_tmp_base_cierre.tipo_campana = item.TIPO_CAMPANA;
                                            _liq_tmp_base_cierre.fecha_creacion = DateTime.Now;
                                            _liq_tmp_base_cierre.fecha_modificacion = DateTime.Now;


                                            _context.liq_tmp_base_cierre.Add(_liq_tmp_base_cierre);
                                            await _context.SaveChangesAsync();
                                               
                                        }
                                        
                                        

                                }

                            }
                            string msj = "";
                            //valida el tipo proceso
                            if (tienePap)
                            {
                                General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PAP", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                msj = await procesar_base_cierre_carta_meta_pap(periodo, codigoTipoEsquemaPap, 1);
                            }
                            if (tienePapII)
                            {
                                General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PAP II", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                msj = await procesar_base_cierre_carta_meta_pap(periodo, codigoTipoEsquemaPapII, 1);
                            }
                            if (tienePymes)
                            {
                                General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PYMES", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                msj = await procesar_base_cierre_carta_meta_pymes(periodo, 1);
                            }
                            if (tieneCall)
                            {
                                General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACIONES CALL", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                msj = await procesar_base_cierre_carta_meta_call_v2(periodo, 1);
                            }
                            //if (tipo_esquema == 1)
                            //{
                            //    General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PAP", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                  
                            //    msj = await procesar_base_cierre_carta_meta_pap(periodo, tipo_esquema, 1);
                            //    //mensaje += auxmensaje;
                            //}
                            //else if(tipo_esquema == 5)
                            //{
                            //    string msj_ = "Entro aqui PAP II y el consecutivo es : " + consecutivo_lote;
                            //    General.crearImprimeMensajeLog(msj_, "procesarExcelPenalizacionesMegasPap", _config.GetConnectionString("conexionDbPruebas"));
                            //    General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PAP II", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));

                            //    msj = await procesar_base_cierre_carta_meta_pap(periodo,tipo_esquema, 1);
                            //}
                                
                            mensaje += "PROCESADO DE FORMA CORRECTA";
                        }
                        

                    }
                 
                }
                else
                {
                    mensaje = "EL ARCHIVO PRESENTA VARIOS PERIODOS ";
                }
                
                
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Base Cierre", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }

        [HttpPost("ReprocesarEsquema")]
        [Authorize]
        public async Task<IActionResult> ReprocesarEsquema(dynamic data_recibe)
        {
            Int32 TipoEsquema = 0;
            Int32 TipoProceso = 0;
            string periodo = "";
            string mensaje = "";
            
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                TipoEsquema = Convert.ToInt32(datObject["TipoEsquema"]);
                TipoProceso = Convert.ToInt32(datObject["TipoProceso"]);
                periodo = Convert.ToString(datObject["periodo"]);

                switch (TipoEsquema)
                {
                    case 1:
                        mensaje += await procesar_base_cierre_carta_meta_pap(periodo, TipoEsquema, TipoProceso);
                        break;
                    case 2:
                        mensaje += await procesar_base_cierre_carta_meta_pymes(periodo, TipoProceso);
                        break;
                    case 3:
                        mensaje += await procesar_base_cierre_carta_meta_call_v2(periodo, TipoProceso);
                        break;
                    case 5:
                        mensaje += await procesar_base_cierre_carta_meta_pap(periodo, TipoEsquema, TipoProceso);
                        break;
                }

                
                if (string.IsNullOrEmpty(mensaje))
                {
                    mensaje += "PROCESO REALIZADO DE FORMA CORRECTA";
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "ReprocesarEsquemaPap", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(mensaje);
        }

        [HttpPost("reprocesarBaseCierreCartaMetaPap")]
        [Authorize]
        public async Task<IActionResult> reprocesarBaseCierreCartaMetaPap(dynamic data_recibe)
        {
            Int32 tipo_esquema = 0;
            Int32 TipoProceso = 0;
            string cedula_supervisor_ = "";
            string periodo = "";
            string mensaje = "";
            string json = "";

            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);
                TipoProceso = Convert.ToInt32(datObject["TipoProceso"]);
                cedula_supervisor_ = Convert.ToString(datObject["cedula_supervisor"]);
                periodo = Convert.ToString(datObject["periodo"]);
                //mensaje = "";
                List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
                _carta_metas = await _context.liq_tmp_metas.Where(x => x.cedula_supervisor == cedula_supervisor_
                                                                   && x.periodo_importe == periodo
                                                                   && x.cod_tipo_escala == tipo_esquema
                                                                   && x.estado == 1).ToListAsync();
                int tam_carta_metas = _carta_metas.Count();
                General.crearImprimeMensajeLog("El tamaño de la lista es : " + tam_carta_metas, "reprocesarBaseCierreCartaMetaPap", _config.GetConnectionString("conexionDbPruebas"));
                //se establece el liq_pap
                List<listar_pap_resumida> _listar_pap_resumidas_ = listar_pap_resumidas();
                List<liq_pap> _liq_pap_l = new List<liq_pap>();
                Int32 valor_mega1 = 0;
                Int32 valor_mega2 = 0;
                Int32 valor_mega3 = 0;
                Int32 valor_mega4 = 0;
                if (tipo_esquema == 1)
                {
                    _liq_pap_l = _context.liq_pap.Where(x => x.codigo_liq_esq == 1).ToList();
                    valor_mega1 = ObtnerVelocidadMega(1);
                    valor_mega2 = ObtnerVelocidadMega(2);
                    valor_mega3 = ObtnerVelocidadMega(3);
                    valor_mega4 = ObtnerVelocidadMega(4);
                }
                else if (tipo_esquema == 5)
                {
                    _liq_pap_l = _context.liq_pap.Where(x => x.codigo_liq_esq == 5).ToList();
                    valor_mega1 = ObtnerVelocidadMega(19);
                    valor_mega2 = ObtnerVelocidadMega(20);
                    valor_mega3 = ObtnerVelocidadMega(21);
                    valor_mega4 = ObtnerVelocidadMega(22);
                }


                List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_escala_altas == tipo_esquema).ToList();
                List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                List<liq_empaqhome> _liq_empaqhome_ = _context.liq_empaqhome.Where(x => x.cod_tipo_esquema == tipo_esquema).ToList();
                if (_carta_metas.Count > 0)
                {
                    foreach (liq_tmp_metas item in _carta_metas)
                    {

                        Int32 sumasUnidades = sumarTotalUnidadesAsesor(item.cedula_asesor, item.cedula_supervisor, item.periodo_importe, TipoProceso, item.cod_tipo_escala);
                        if (sumasUnidades > 0)
                        {
                            string cedula_asesor = "";
                            string cedula_supervisor = "";
                            Int32 codigo_tipo_esquema = 0;
                            cedula_asesor = item.cedula_asesor;
                            cedula_supervisor = item.cedula_supervisor;
                            codigo_tipo_esquema = item.cod_tipo_escala;
                            liq_comision_asesor _liq_comision_asesor_e = await _context.liq_comision_asesor.Where(x => x.cedula_asesor == cedula_asesor
                                                                                                          && x.cedula_supervisor == cedula_supervisor
                                                                                                          && x.periodo == periodo
                                                                                                          && x.codigo_tipo_escala == codigo_tipo_esquema
                                                                                                          && x.estado == 1).FirstOrDefaultAsync();

                            int rango_altas = 0;
                            liq_escala_altas _Escala_Altas = _context.liq_escala_altas.Where(x => x.numero_escala_alta == sumasUnidades).FirstOrDefault();
                            if (_Escala_Altas != null)
                            {
                                rango_altas = _Escala_Altas.rango_altas;
                            }
                            else
                            {
                                int max_numero_escala_alta = _context.liq_escala_altas.Select(x => x.numero_escala_alta).Max();
                                if (sumasUnidades > max_numero_escala_alta)
                                {
                                    rango_altas = _context.liq_escala_altas.Select(x => x.rango_altas).Max();
                                }
                            }
                            if (_liq_comision_asesor_e != null)
                            {
                                string data_mensaje_es = _liq_comision_asesor_e.cedula_asesor + " periodo es : " + periodo + " y el tipo proceso : " + TipoProceso + " ";
                                General.crearImprimeMensajeLog(data_mensaje_es, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));
                                double porcentaje_asesor_e = 0;
                                double porcentaje_aux_e = 0;
                                int meta_asesor_e = _liq_comision_asesor_e.meta_asesor;
                                porcentaje_aux_e = (double)sumasUnidades / meta_asesor_e;
                                porcentaje_asesor_e = (porcentaje_aux_e * 100);
                                Int32 portecentaje_asesor_e_i = Convert.ToInt32(porcentaje_asesor_e);
                                _liq_comision_asesor_e.cumplimiento_asesor = portecentaje_asesor_e_i;
                                //string auxmensaje = "";
                                Int32 tabla_cumplimiento = ObtnerNivelCumplimientoPap(_listar_pap_resumidas_, portecentaje_asesor_e_i);
                                //mensaje = auxmensaje;
                                //aqui validar si el cumplimiento de las unidades es mayor a la meta

                                if (sumasUnidades > _liq_comision_asesor_e.meta_asesor)
                                {
                                    _liq_comision_asesor_e.asesor_cumple = 1;
                                }
                                else
                                {
                                    _liq_comision_asesor_e.asesor_cumple = 0;
                                }
                                //traemos por group by la tabla  para el comparativo
                                _liq_comision_asesor_e.nivel = tabla_cumplimiento;
                                Int32 cantidad_velocidad_1 = 0;
                                Int32 cantidad_velocidad_2 = 0;
                                Int32 cantidad_velocidad_3 = 0;
                                Int32 cantidad_velocidad_4 = 0;

                                int[] arr_cantidades_velocidad = new int[4];
                                arr_cantidades_velocidad = await calcularCantidadesMegasPap(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                           valor_mega1, valor_mega2, valor_mega3, valor_mega4);
                                cantidad_velocidad_1 = arr_cantidades_velocidad[0];
                                cantidad_velocidad_2 = arr_cantidades_velocidad[1];
                                cantidad_velocidad_3 = arr_cantidades_velocidad[2];
                                cantidad_velocidad_4 = arr_cantidades_velocidad[3];
                                _liq_comision_asesor_e.numero_cant_megas_1 = cantidad_velocidad_1;
                                _liq_comision_asesor_e.numero_cant_megas_2 = cantidad_velocidad_2;
                                _liq_comision_asesor_e.numero_cant_megas_3 = cantidad_velocidad_3;
                                _liq_comision_asesor_e.numero_cant_megas_4 = cantidad_velocidad_4;

                                //setiar los valores mega

                                _liq_comision_asesor_e.nombre_mega_1 = valor_mega1 + "";
                                _liq_comision_asesor_e.nombre_mega_2 = valor_mega2 + "";
                                _liq_comision_asesor_e.nombre_mega_3 = valor_mega3 + "";
                                _liq_comision_asesor_e.nombre_mega_4 = valor_mega4 + "";

                                double valor_mega_1 = 0;
                                double valor_mega_2 = 0;
                                double valor_mega_3 = 0;
                                double valor_mega_4 = 0;

                                double[] arr_valores_megas = new double[4];
                                //aqui definir por el liquidador el tipo de liquidacion
                                arr_valores_megas = await calcularValorMegasPap(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                      tabla_cumplimiento, sumasUnidades, valor_mega1, valor_mega2, valor_mega3, valor_mega4, item.tipo_liquidador);
                                valor_mega_1 = arr_valores_megas[0];
                                valor_mega_2 = arr_valores_megas[1];
                                valor_mega_3 = arr_valores_megas[2];
                                valor_mega_4 = arr_valores_megas[3];
                                if (valor_mega1 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_1 = valor_mega_1;
                                    _liq_comision_asesor_e.total_valor_mega_1 = valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1;

                                }
                                if (valor_mega_2 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_2 = valor_mega_2;
                                    _liq_comision_asesor_e.total_valor_mega_2 = valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2;
                                }
                                if (valor_mega3 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_3 = valor_mega_3;
                                    _liq_comision_asesor_e.total_valor_mega_3 = valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3;
                                }
                                if (valor_mega4 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_4 = valor_mega_4;
                                    _liq_comision_asesor_e.total_valor_mega_4 = valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4;
                                }


                                double subTotalValorMegas = ((valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1) +
                                                             (valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2) +
                                                             (valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3) +
                                                             (valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4));
                                Int32 total_naked = 0;
                                total_naked = await sumarTotalNaked(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                                _liq_comision_asesor_e.numero_naked = total_naked;
                                double valor_naked = 0;
                                valor_naked = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 3, _liq_empaqhome_);
                                double subTotalNaked = 0;
                                if (valor_naked > 0)
                                {
                                    _liq_comision_asesor_e.valor_naked = valor_naked;
                                    subTotalNaked = (total_naked * valor_naked);
                                    _liq_comision_asesor_e.total_valor_naked = subTotalNaked;
                                }


                                Int32 total_duos = 0;
                                total_duos = await sumarTotalDuos(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                                _liq_comision_asesor_e.numero_duos = total_duos;
                                double valor_duos = 0;
                                valor_duos = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 1, _liq_empaqhome_);
                                double subTotalDuos = 0;
                                if (valor_duos > 0)
                                {
                                    _liq_comision_asesor_e.valor_duos = valor_duos;
                                    subTotalDuos = (total_duos * valor_duos);
                                    _liq_comision_asesor_e.total_valor_duos = subTotalDuos;
                                }

                                //aqui calcular los naked para los pap
                                // y los duos los pap
                                // en el esquema de los duos trios y naked se evalua por el cumplimiento de las cantidades ftth??

                                _liq_comision_asesor_e.sub_total_comision = subTotalValorMegas + subTotalNaked + subTotalDuos;
                                _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.sub_total_comision;


                                _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                int sa = await _context.SaveChangesAsync();
                                if (sa > 0)
                                {
                                    (from lq_ in _context.liq_tmp_base_cierre
                                     where lq_.velocidad > 0 && lq_.cedula_asesor == item.cedula_asesor
                                     && lq_.cedula_supervisor == item.cedula_supervisor
                                     && lq_.periodo == periodo && lq_.estado == 1
                                     && lq_.EsProcesado == 0
                                     select lq_).ToList()
                                    .ForEach(x => x.EsProcesado = 1);
                                    _context.SaveChanges();

                                }
                                General.recalcular_subtotales(cedula_asesor, periodo, _config.GetConnectionString("conexionDbPruebas"));
                            }
                        }
                    }
                }
                mensaje = "REPROCESADO CORRECTAMENTE";
                json = JsonConvert.SerializeObject(mensaje);
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Carta Metas Super", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            
            return Ok(json);
        }

        [HttpPost("procesarExcelBaseCierrePyme")]
        [Authorize]
        public async Task<IActionResult> procesarExcelBaseCierrePyme(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int tipo_esquema = 0;
            string periodo = "";
            string mensaje = "";
            try
            {
                List<listar_tmp_base_cierre> _listar_tmp_base_cierre = new List<listar_tmp_base_cierre>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["baseCierre"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);
                _listar_tmp_base_cierre = JsonConvert.DeserializeObject<List<listar_tmp_base_cierre>>(base_);
                System.Boolean EsArchivoValido = false;
                var validar_base_cierre_periodo = _listar_tmp_base_cierre.Select(x => new { x.MES_LIQUIDACION }).Distinct().ToList();
                if (validar_base_cierre_periodo.Count() == 1)
                {
                    foreach (var i in validar_base_cierre_periodo)
                    {
                        if (i.MES_LIQUIDACION == periodo)
                        {
                            EsArchivoValido = true;
                        }
                    }
                    if (EsArchivoValido)
                    {
                        //crear un distinct de empleado, supervisor y periodo
                        int baseCierreSinMetas = 0;
                        string tipo_esquema_validate = "";
                        switch (tipo_esquema)
                        {
                            case 1:
                                tipo_esquema_validate = "PAP";
                                break;
                            case 2:
                                tipo_esquema_validate = "PYMES";
                                break;
                            case 3:
                                tipo_esquema_validate = "CALL OUT";
                                break;
                        }
                        var validar_base_cierre_metas = _listar_tmp_base_cierre.Select(x => new { x.CEDULA_ASESOR, x.DOCUMENTO_SUPERVISOR, x.MES_LIQUIDACION, x.TIPO_ESQUEMA })
                                                                               .Distinct().Where(x => x.MES_LIQUIDACION == periodo
                                                                                                 && x.TIPO_ESQUEMA == tipo_esquema_validate).ToList();

                        //foreach (var i in validar_base_cierre_metas)
                        //{
                        //    liq_tmp_metas _liq_tmp_metas_e = _context.liq_tmp_metas.Where(x => x.cedula_asesor == i.CEDULA_ASESOR
                        //                                                                  && x.cedula_supervisor == i.DOCUMENTO_SUPERVISOR
                        //                                                                  && x.periodo_importe == i.MES_LIQUIDACION).FirstOrDefault();
                        //    if (_liq_tmp_metas_e == null)
                        //    {
                        //        List<listar_tmp_base_cierre> _listar_tmp_base_cierre_respon = _listar_tmp_base_cierre.Where(x => x.CEDULA_ASESOR == i.CEDULA_ASESOR
                        //                                                                                                           && x.DOCUMENTO_SUPERVISOR == i.DOCUMENTO_SUPERVISOR
                        //                                                                                                           && x.MES_LIQUIDACION == i.MES_LIQUIDACION).ToList();
                        //        baseCierreSinMetas++;
                        //        //List<listar_tmp_base_cierre_respon> _listar_tmp_base_cierre_respon_aux = new List<listar_tmp_base_cierre_respon>();
                        //        //_listar_tmp_base_cierre_respon_aux.Add(_listar_tmp_base_cierre_respon);
                        //    }
                        //}
                        if (baseCierreSinMetas == 0)
                        {
                            if (_listar_tmp_base_cierre.Count > 0)
                            {
                                Int64 consecutivo_lote = consecutivo_lote_importe();
                                Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                                string subQuery = "select count(*) as dato from liq_tmp_base_cierre where lote_importe = " + _consecutivo_lote + " and estado = 1";
                                General.crearDataValidoProceso("procesarExcelBaseCierrePyme", _listar_tmp_base_cierre.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                foreach (listar_tmp_base_cierre item in _listar_tmp_base_cierre)
                                {



                                    //validamos el tipo esquema 
                                    Int32 TipoEsquemaValidate = 0;
                                    liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                                    liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                                    liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                                    if (_liq_tipo_pap != null)
                                    {

                                        if (_liq_tipo_pap.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 1;
                                        }
                                    }
                                    if (_liq_tipo_pymes != null)
                                    {

                                        if (_liq_tipo_pymes.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 2;
                                        }
                                    }
                                    if (_liq_tipo_call != null)
                                    {

                                        if (_liq_tipo_call.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 3;
                                        }
                                    }

                                    //int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                    //                                                && x.cod_peticion == item.ID_PETICION
                                    //                                                && x.periodo == periodo
                                    //                                                && x.cod_tipo_esquema == TipoEsquemaValidate
                                    //                                                && x.estado == 1).Count();

                                    int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                                                                    && x.cod_peticion == item.ID_PETICION
                                                                                    && x.periodo == periodo
                                                                                    && x.cod_tipo_esquema == TipoEsquemaValidate
                                                                                    && x.num_doc_cliente == item.NUM_DOCUMENTO_CLIENTE
                                                                                    && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                    && x.cedula_supervisor == item.DOCUMENTO_SUPERVISOR
                                                                                    && x.estado == 1
                                                                                    && x.EsIngresado == 1).Count();
                                    if (Existe == 0)
                                    {
                                        if (TipoEsquemaValidate == 2)
                                        {
                                            //validamos que para este metodo solo inserten las unidades que son mayores a cero
                                            Int32 unidad = Convert.ToInt32(item.UNIDAD);
                                            if (unidad > 0)
                                            {
                                                liq_tmp_base_cierre _liq_tmp_base_cierre = new liq_tmp_base_cierre();
                                                _liq_tmp_base_cierre.producto = item.PRODUCTO;
                                                _liq_tmp_base_cierre.cedula_asesor = item.CEDULA_ASESOR;
                                                _liq_tmp_base_cierre.mes_seg = Convert.ToInt32(item.MES_SEG);
                                                _liq_tmp_base_cierre.unidad = Convert.ToInt32(item.UNIDAD);
                                                _liq_tmp_base_cierre.cod_peticion = item.ID_PETICION;
                                                System.Boolean tieneNumero = EsSoloNumero(item.VELOCIDAD);
                                                double mega_auxiliar = 0;
                                                if (tieneNumero)
                                                {
                                                    
                                                    Int32 valor_longitud_cadena = item.VELOCIDAD.Length;
                                                    valor_longitud_cadena = valor_longitud_cadena - 3;
                                                    string velocidad_aux = item.VELOCIDAD.Insert(valor_longitud_cadena, ",");
                                                    //mensaje += "la longitud es : "+valor_longitud_cadena+" y el string es : "+velocidad_aux;
                                                    string[] sub_velocidad = velocidad_aux.Split(',');
                                                    mega_auxiliar = Convert.ToDouble(sub_velocidad[0]);
                                                    //string _msj = "la mega auxiliar es : "+mega_auxiliar;
                                                    //General.crearImprimeMensajeLog(_msj, "procesarExcelBaseCierrePyme", _config.GetConnectionString("conexionDbPruebas"));
                                                }

                                                //sigo VALIDACION TIPO ESQUEMA COMISION
                                                _liq_tmp_base_cierre.cod_tipo_esquema = TipoEsquemaValidate;

                                                //VALIDAMOS LAS MEGAS DE
                                                int valor_primera_mega = 0;
                                                int valor_segunda_mega = 0;
                                                int valor_tercera_mega = 0;
                                                int valor_cuarta_mega = 0;
                                                int valor_quinta_mega = 0;
                                                liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_1 != null)
                                                {
                                                    valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                                    //
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_2 != null)
                                                {
                                                    valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_3 != null)
                                                {
                                                    valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_4 != null)
                                                {
                                                    valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();

                                                if (_Valores_Megabytes_5 != null)
                                                {
                                                    valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                                }

                                                //if (TipoEsquemaValidate == 1)
                                                //{
                                                //    //aqui la primera validacion del megabite 1
                                                //    if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                //        && (mega_auxiliar < valor_primera_mega + 99))
                                                //    {
                                                //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;

                                                //    }
                                                //    else if ((mega_auxiliar >= valor_segunda_mega) &&
                                                //        (mega_auxiliar) < valor_segunda_mega + 99)
                                                //    {
                                                //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;

                                                //    }
                                                //    else if ((mega_auxiliar >= valor_tercera_mega) &&
                                                //       (mega_auxiliar) < valor_tercera_mega + 99)
                                                //    {
                                                //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;

                                                //    }
                                                //    else if (mega_auxiliar >= valor_cuarta_mega)
                                                //    {
                                                //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;

                                                //    }
                                                //}
                                                if (TipoEsquemaValidate == 2)
                                                {
                                                    //aqui la primera validacion del megabite 1
                                                    if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_1.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_segunda_mega) &&
                                                        (mega_auxiliar) < valor_segunda_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_2.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_tercera_mega) &&
                                                       (mega_auxiliar) < valor_tercera_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_3.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_cuarta_mega) &&
                                                       (mega_auxiliar) < valor_cuarta_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_4.valor_mega;
                                                    }
                                                    if (mega_auxiliar >= valor_quinta_mega)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_5.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_5.valor_mega;
                                                    }
                                                }

                                                _liq_tmp_base_cierre.empaqhomo = item.EMPAQHOMO;
                                                _liq_tmp_base_cierre.num_doc_cliente = item.NUM_DOCUMENTO_CLIENTE;
                                                _liq_tmp_base_cierre.cedula_supervisor = item.DOCUMENTO_SUPERVISOR;
                                                _liq_tmp_base_cierre.observacion = item.OBSERVACION;
                                                _liq_tmp_base_cierre.migracion_otro = item.MIGRACION_OTRO;
                                                _liq_tmp_base_cierre.periodo = periodo;
                                                _liq_tmp_base_cierre.lote_importe = (Int32)consecutivo_lote;
                                                _liq_tmp_base_cierre.estado = 1;
                                                _liq_tmp_base_cierre.EsProcesado = 0;
                                                _liq_tmp_base_cierre.EsIngresado = 1;
                                                _liq_tmp_base_cierre.usuario = usuario_;
                                                System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.DOCUMENTO_SUPERVISOR, periodo, TipoEsquemaValidate);
                                                if (EsValido)
                                                {
                                                    _liq_tmp_base_cierre.EsValido = 1;
                                                }
                                                else
                                                {
                                                    _liq_tmp_base_cierre.EsValido = 0;
                                                }
                                                _liq_tmp_base_cierre.fecha_creacion = DateTime.Now;
                                                _liq_tmp_base_cierre.fecha_modificacion = DateTime.Now;
                                                _context.liq_tmp_base_cierre.Add(_liq_tmp_base_cierre);
                                                await _context.SaveChangesAsync();
                                            }
                                        }


                                    }

                                }
                                string msj = "";
                                //valida el tipo proceso
                                //if (tipo_esquema == 1)
                                //{
                                //    General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PAP", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                //    //string auxmensaje = "";
                                //    msj = await procesar_base_cierre_carta_meta_pap(periodo, 1);
                                //    //mensaje += auxmensaje;
                                //}
                                if (tipo_esquema == 2)
                                {
                                    General.crearImprimeMensajeLog("Entro al if ", "procesarExcelBaseCierrePyme", _config.GetConnectionString("conexionDbPruebas"));
                                    General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PYMES", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                    msj = await procesar_base_cierre_carta_meta_pymes(periodo, 1);
                                }
                                mensaje = "PROCESADO DE FORMA CORRECTA";
                            }
                        }

                    }
                    else
                    {
                        mensaje = "EL ARCHIVO BASE CIERRE TIENE ASESORES QUE AUN NO TIENE METAS";
                    }
                }
                else
                {
                    mensaje = "EL ARCHIVO PRESENTA VARIOS PERIODOS ";
                }


            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Base Cierre", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }
        [HttpPost("reprocesarBaseCierreCartaMetaPymes")]
        [Authorize]
        public async Task<IActionResult> reprocesarBaseCierreCartaMetaPymes(dynamic data_recibe)
        {
            Int32 codigo_tipo_esquema = 0;
            Int32 TipoProceso = 0;
            string periodo = "";
            string cedula_supervisor = "";
            string mensaje = "";
            string json = "";
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                codigo_tipo_esquema = Convert.ToInt32(datObject["codigo_tipo_esquema"]);
                TipoProceso = Convert.ToInt32(datObject["TipoProceso"]);
                periodo = Convert.ToString(datObject["periodo"]);
                cedula_supervisor = Convert.ToString(datObject["cedula_supervisor"]);
                List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
                _carta_metas = _context.liq_tmp_metas.Where(x => x.periodo_importe == periodo
                                                            && x.cod_tipo_escala == codigo_tipo_esquema
                                                            && x.cedula_supervisor == cedula_supervisor
                                                            && x.estado == 1).ToList();

                int tam_carta_metas = _carta_metas.Count();
                General.crearImprimeMensajeLog("El tamaño de la lista es : " + tam_carta_metas, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));

                //se establece el liq_pap
                List<listar_pap_resumida> _listar_pap_resumidas_ = listar_pap_resumidas();
                List<liq_pap> _liq_pap_l = _context.liq_pap.Where(x => x.codigo_liq_esq == 2).ToList();
                Int32 valor_mega1 = ObtnerVelocidadMega(5);
                Int32 valor_mega2 = ObtnerVelocidadMega(6);
                Int32 valor_mega3 = ObtnerVelocidadMega(7);
                Int32 valor_mega4 = ObtnerVelocidadMega(8);
                Int32 valor_mega5 = ObtnerVelocidadMega(9);
                List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_escala_altas == codigo_tipo_esquema).ToList();
                List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema).ToList();
                List<liq_empaqhome> _liq_empaqhome_ = _context.liq_empaqhome.Where(x => x.cod_tipo_esquema == codigo_tipo_esquema).ToList();
                if (_carta_metas.Count > 0)
                {
                    foreach (liq_tmp_metas item in _carta_metas)
                    {
                        Int32 sumasUnidades = sumarTotalUnidadesAsesor(item.cedula_asesor, item.cedula_supervisor, item.periodo_importe, TipoProceso, item.cod_tipo_escala);
                        if (sumasUnidades > 0)
                        {
                            liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                                                          && x.cedula_supervisor == item.cedula_supervisor
                                                                                                          && x.periodo == periodo
                                                                                                          && x.codigo_tipo_escala == item.cod_tipo_escala
                                                                                                          && x.estado == 1).FirstOrDefault();
                            int rango_altas = 0;
                            liq_escala_altas _Escala_Altas = _context.liq_escala_altas.Where(x => x.numero_escala_alta == sumasUnidades).FirstOrDefault();
                            if (_Escala_Altas != null)
                            {
                                rango_altas = _Escala_Altas.rango_altas;
                            }
                            else
                            {
                                int max_numero_escala_alta = _context.liq_escala_altas.Select(x => x.numero_escala_alta).Max();
                                if (sumasUnidades > max_numero_escala_alta)
                                {
                                    rango_altas = _context.liq_escala_altas.Select(x => x.rango_altas).Max();
                                }
                            }
                            if (_liq_comision_asesor_e != null)
                            {
                                string data_mensaje_es = _liq_comision_asesor_e.cedula_asesor + " periodo es : " + periodo + " y el tipo proceso : " + TipoProceso + " ";
                                General.crearImprimeMensajeLog(data_mensaje_es, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));

                                double porcentaje_asesor_e = 0;
                                double porcentaje_aux_e = 0;
                                int meta_asesor_e = _liq_comision_asesor_e.meta_asesor;
                                porcentaje_aux_e = (double)sumasUnidades / meta_asesor_e;
                                porcentaje_asesor_e = (porcentaje_aux_e * 100);
                                Int32 portecentaje_asesor_e_i = Convert.ToInt32(porcentaje_asesor_e);
                                _liq_comision_asesor_e.cumplimiento_asesor = portecentaje_asesor_e_i;
                                //string auxmensaje = "";
                                int tabla_cumplimiento = ObtnerNivelCumplimientoPap(_listar_pap_resumidas_, portecentaje_asesor_e_i);
                                if (sumasUnidades > _liq_comision_asesor_e.meta_asesor)
                                {
                                    _liq_comision_asesor_e.asesor_cumple = 1;
                                }
                                else
                                {
                                    _liq_comision_asesor_e.asesor_cumple = 0;
                                }
                                //traemos por group by la tabla  para el comparativo
                                _liq_comision_asesor_e.nivel = tabla_cumplimiento;
                                Int32 cantidad_velocidad_1 = 0;
                                Int32 cantidad_velocidad_2 = 0;
                                Int32 cantidad_velocidad_3 = 0;
                                Int32 cantidad_velocidad_4 = 0;
                                Int32 cantidad_velocidad_5 = 0;
                                int[] arr_cantidades_velocidad = new int[5];
                                arr_cantidades_velocidad = await calcularCantidadesMegasPymes(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                          valor_mega1, valor_mega2, valor_mega3, valor_mega4, valor_mega5);
                                cantidad_velocidad_1 = arr_cantidades_velocidad[0];
                                cantidad_velocidad_2 = arr_cantidades_velocidad[1];
                                cantidad_velocidad_3 = arr_cantidades_velocidad[2];
                                cantidad_velocidad_4 = arr_cantidades_velocidad[3];
                                cantidad_velocidad_5 = arr_cantidades_velocidad[4];
                                _liq_comision_asesor_e.numero_cant_megas_1 = cantidad_velocidad_1;
                                _liq_comision_asesor_e.numero_cant_megas_2 = cantidad_velocidad_2;
                                _liq_comision_asesor_e.numero_cant_megas_3 = cantidad_velocidad_3;
                                _liq_comision_asesor_e.numero_cant_megas_4 = cantidad_velocidad_4;
                                _liq_comision_asesor_e.numero_cant_megas_5 = cantidad_velocidad_5;
                                double valor_mega_1 = 0;
                                double valor_mega_2 = 0;
                                double valor_mega_3 = 0;
                                double valor_mega_4 = 0;
                                double valor_mega_5 = 0;

                                //setiar los valores mega

                                _liq_comision_asesor_e.nombre_mega_1 = valor_mega1 + "";
                                _liq_comision_asesor_e.nombre_mega_2 = valor_mega2 + "";
                                _liq_comision_asesor_e.nombre_mega_3 = valor_mega3 + "";
                                _liq_comision_asesor_e.nombre_mega_4 = valor_mega4 + "";
                                _liq_comision_asesor_e.nombre_mega_5 = valor_mega5 + "";

                                double[] arr_valores_megas = new double[5];
                                arr_valores_megas = await calcularValorMegasPymes(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                      tabla_cumplimiento, sumasUnidades, valor_mega1, valor_mega2, valor_mega3, valor_mega4, valor_mega5);
                                valor_mega_1 = arr_valores_megas[0];
                                valor_mega_2 = arr_valores_megas[1];
                                valor_mega_3 = arr_valores_megas[2];
                                valor_mega_4 = arr_valores_megas[3];
                                valor_mega_5 = arr_valores_megas[4];
                                if (valor_mega1 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_1 = valor_mega_1;
                                    _liq_comision_asesor_e.total_valor_mega_1 = valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1;
                                }
                                if (valor_mega_2 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_2 = valor_mega_2;
                                    _liq_comision_asesor_e.total_valor_mega_2 = valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2;
                                }
                                if (valor_mega3 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_3 = valor_mega_3;
                                    _liq_comision_asesor_e.total_valor_mega_3 = valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3;
                                }
                                if (valor_mega4 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_4 = valor_mega_4;
                                    _liq_comision_asesor_e.total_valor_mega_4 = valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4;
                                }
                                if (valor_mega5 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_5 = valor_mega_5;
                                    _liq_comision_asesor_e.total_valor_mega_5 = valor_mega_5 * _liq_comision_asesor_e.numero_cant_megas_5;
                                }
                                double subTotalValorMegas = ((valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1) +
                                                             (valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2) +
                                                             (valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3) +
                                                             (valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4) +
                                                             (valor_mega_5 * _liq_comision_asesor_e.numero_cant_megas_5));
                                Int32 total_duos = 0;
                                total_duos = await sumarTotalDuos(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                                _liq_comision_asesor_e.numero_duos = total_duos;
                                double valor_duos = 0;
                                valor_duos = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 1, _liq_empaqhome_);
                                double subTotalDuos = 0;
                                if (valor_duos > 0)
                                {
                                    _liq_comision_asesor_e.valor_duos = valor_duos;
                                    subTotalDuos = (total_duos * valor_duos);
                                    _liq_comision_asesor_e.total_valor_duos = subTotalDuos;
                                }

                                Int32 total_trios = 0;
                                total_trios = await sumarTotalTrios(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                                _liq_comision_asesor_e.numero_trios = total_trios;
                                double valor_trios = 0;
                                valor_trios = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 2, _liq_empaqhome_);
                                double subTotalTrios = 0;
                                if (valor_trios > 0)
                                {
                                    _liq_comision_asesor_e.valor_trios = valor_trios;
                                    subTotalTrios = (total_trios * valor_trios);
                                    _liq_comision_asesor_e.total_valor_trios = subTotalTrios;
                                }

                                _liq_comision_asesor_e.sub_total_comision = subTotalValorMegas + subTotalTrios + subTotalDuos;
                                _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.sub_total_comision;
                                _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                int sa = await _context.SaveChangesAsync();
                                if (sa > 0)
                                {
                                    (from lq_ in _context.liq_tmp_base_cierre
                                     where lq_.velocidad > 0 && lq_.cedula_asesor == item.cedula_asesor
                                     && lq_.cedula_supervisor == item.cedula_supervisor
                                     && lq_.periodo == periodo && lq_.estado == 1
                                     && lq_.EsProcesado == 0
                                     select lq_).ToList()
                                    .ForEach(x => x.EsProcesado = 1);
                                    _context.SaveChanges();

                                }
                                General.recalcular_subtotales(item.cedula_asesor, periodo, _config.GetConnectionString("conexionDbPruebas"));
                            }
                        }
                    }

                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Carta Metas Super", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }

        //excepcion base cierre call
        [HttpPost("procesarExcelBaseCierreCall")]
        [Authorize]
        public async Task<IActionResult> procesarExcelBaseCierreCall(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int tipo_esquema = 0;
            string periodo = "";
            string mensaje = "";

            try
            {
                List<listar_tmp_base_cierre> _listar_tmp_base_cierre = new List<listar_tmp_base_cierre>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["baseCierre"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);

                _listar_tmp_base_cierre = JsonConvert.DeserializeObject<List<listar_tmp_base_cierre>>(base_);
                System.Boolean EsArchivoValido = false;
                var validar_base_cierre_periodo = _listar_tmp_base_cierre.Select(x => new { x.MES_LIQUIDACION }).Distinct().ToList();
                if (validar_base_cierre_periodo.Count() == 1)
                {
                    foreach (var i in validar_base_cierre_periodo)
                    {
                        if (i.MES_LIQUIDACION == periodo)
                        {
                            EsArchivoValido = true;
                        }
                    }

                    if (EsArchivoValido)
                    {
                        int baseCierreSinMetas = 0;
                        string tipo_esquema_validate = "";
                        switch (tipo_esquema)
                        {
                            case 1:
                                tipo_esquema_validate = "PAP";
                                break;
                            case 2:
                                tipo_esquema_validate = "PYMES";
                                break;
                            case 3:
                                tipo_esquema_validate = "CALL OUT";
                                break;
                        }
                        var validar_base_cierre_metas = _listar_tmp_base_cierre.Select(x => new { x.CEDULA_ASESOR, x.DOCUMENTO_SUPERVISOR, x.MES_LIQUIDACION, x.TIPO_ESQUEMA })
                                                                               .Distinct().Where(x => x.MES_LIQUIDACION == periodo
                                                                                                 && x.TIPO_ESQUEMA == tipo_esquema_validate).ToList();

                        //foreach (var i in validar_base_cierre_metas)
                        //{
                        //    liq_tmp_metas _liq_tmp_metas_e = _context.liq_tmp_metas.Where(x => x.cedula_asesor == i.CEDULA_ASESOR
                        //                                                                  && x.cedula_supervisor == i.DOCUMENTO_SUPERVISOR
                        //                                                                  && x.periodo_importe == i.MES_LIQUIDACION).FirstOrDefault();
                        //    if (_liq_tmp_metas_e == null)
                        //    {
                        //        List<listar_tmp_base_cierre> _listar_tmp_base_cierre_respon = _listar_tmp_base_cierre.Where(x => x.CEDULA_ASESOR == i.CEDULA_ASESOR
                        //                                                                                                           && x.DOCUMENTO_SUPERVISOR == i.DOCUMENTO_SUPERVISOR
                        //                                                                                                           && x.MES_LIQUIDACION == i.MES_LIQUIDACION).ToList();
                        //        baseCierreSinMetas++;
                        //        //List<listar_tmp_base_cierre_respon> _listar_tmp_base_cierre_respon_aux = new List<listar_tmp_base_cierre_respon>();
                        //        //_listar_tmp_base_cierre_respon_aux.Add(_listar_tmp_base_cierre_respon);
                        //    }
                        //}
                        if (baseCierreSinMetas == 0)
                        {
                            if (_listar_tmp_base_cierre.Count > 0)
                            {
                                Int64 consecutivo_lote = consecutivo_lote_importe();
                                Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                                string subQuery = "select count(*) as dato from liq_tmp_base_cierre where lote_importe = " + _consecutivo_lote + " and estado = 1";
                                General.crearDataValidoProceso("procesarExcelBaseCierreCall", _listar_tmp_base_cierre.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                foreach (listar_tmp_base_cierre item in _listar_tmp_base_cierre)
                                {
                                    //validamos el tipo esquema 
                                    Int32 TipoEsquemaValidate = 0;
                                    liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                                    liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                                    liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                                    if (_liq_tipo_pap != null)
                                    {

                                        if (_liq_tipo_pap.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 1;
                                        }
                                    }
                                    if (_liq_tipo_pymes != null)
                                    {

                                        if (_liq_tipo_pymes.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 2;
                                        }
                                    }
                                    if (_liq_tipo_call != null)
                                    {

                                        if (_liq_tipo_call.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 3;
                                        }
                                    }
                                    //int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                    //                                                && x.cod_peticion == item.ID_PETICION
                                    //                                                && x.periodo == periodo
                                    //                                                && x.cod_tipo_esquema == TipoEsquemaValidate
                                    //                                                && x.estado == 1).Count();
                                    int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                                                                    && x.cod_peticion == item.ID_PETICION
                                                                                    && x.periodo == periodo
                                                                                    && x.cod_tipo_esquema == TipoEsquemaValidate
                                                                                    && x.num_doc_cliente == item.NUM_DOCUMENTO_CLIENTE
                                                                                    && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                    && x.cedula_supervisor == item.DOCUMENTO_SUPERVISOR
                                                                                    && x.estado == 1
                                                                                    && x.EsIngresado == 1).Count();
                                    if (Existe == 0)
                                    {
                                        //validar que solo corresponda al tipo de esquema
                                        if (TipoEsquemaValidate == 3)
                                        {
                                            Int32 unidad = Convert.ToInt32(item.UNIDAD);
                                            if (unidad > 0)
                                            {
                                                liq_tmp_base_cierre _liq_tmp_base_cierre = new liq_tmp_base_cierre();
                                                _liq_tmp_base_cierre.producto = item.PRODUCTO;
                                                _liq_tmp_base_cierre.cedula_asesor = item.CEDULA_ASESOR;
                                                _liq_tmp_base_cierre.mes_seg = Convert.ToInt32(item.MES_SEG);
                                                _liq_tmp_base_cierre.unidad = Convert.ToInt32(item.UNIDAD);
                                                _liq_tmp_base_cierre.cod_peticion = item.ID_PETICION;
                                                System.Boolean tieneNumero = EsSoloNumero(item.VELOCIDAD);
                                                double mega_auxiliar = 0;
                                                if (tieneNumero)
                                                {

                                                    Int32 valor_longitud_cadena = item.VELOCIDAD.Length;
                                                    valor_longitud_cadena = valor_longitud_cadena - 3;
                                                    string velocidad_aux = item.VELOCIDAD.Insert(valor_longitud_cadena, ",");
                                                    //mensaje += "la longitud es : "+valor_longitud_cadena+" y el string es : "+velocidad_aux;
                                                    string[] sub_velocidad = velocidad_aux.Split(',');
                                                    mega_auxiliar = Convert.ToDouble(sub_velocidad[0]);
                                                }
                                                _liq_tmp_base_cierre.cod_tipo_esquema = TipoEsquemaValidate;
                                                int valor_primera_mega = 0;
                                                int valor_segunda_mega = 0;
                                                int valor_tercera_mega = 0;
                                                int valor_cuarta_mega = 0;
                                                int valor_quinta_mega = 0;
                                                //validar aqui mediante un parametro
                                                liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_1 != null)
                                                {
                                                    valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_2 != null)
                                                {
                                                    valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_3 != null)
                                                {
                                                    valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_4 != null)
                                                {
                                                    valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                                }

                                                //liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                                //                                                                                  && x.calcula_mega == 1
                                                //                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                //if (_Valores_Megabytes_5 != null)
                                                //{
                                                //    valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                                //}
                                                //valido
                                                if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                                {
                                                    if (valor_primera_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;
                                                    }

                                                }
                                                else if ((mega_auxiliar >= valor_segunda_mega) &&
                                                    (mega_auxiliar) < valor_segunda_mega + 99)
                                                {
                                                    if (valor_segunda_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;
                                                    }

                                                }
                                                else if ((mega_auxiliar >= valor_tercera_mega) &&
                                                   (mega_auxiliar) < valor_tercera_mega + 99)
                                                {
                                                    if (valor_tercera_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;
                                                    }

                                                }
                                                else if (mega_auxiliar >= valor_cuarta_mega)
                                                {
                                                    if (valor_cuarta_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;
                                                    }
                                                }
                                                //else if (mega_auxiliar >= valor_quinta_mega)
                                                //{
                                                //    if (valor_quinta_mega > 0)
                                                //    {
                                                //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_5.valor_mega;
                                                //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_5.valor_mega;
                                                //    }
                                                //}
                                                _liq_tmp_base_cierre.empaqhomo = item.EMPAQHOMO;
                                                _liq_tmp_base_cierre.num_doc_cliente = item.NUM_DOCUMENTO_CLIENTE;
                                                _liq_tmp_base_cierre.cedula_supervisor = item.DOCUMENTO_SUPERVISOR;
                                                _liq_tmp_base_cierre.observacion = item.OBSERVACION;
                                                _liq_tmp_base_cierre.migracion_otro = item.MIGRACION_OTRO;
                                                _liq_tmp_base_cierre.periodo = periodo;
                                                _liq_tmp_base_cierre.lote_importe = (Int32)consecutivo_lote;
                                                _liq_tmp_base_cierre.estado = 1;
                                                _liq_tmp_base_cierre.EsProcesado = 0;
                                                _liq_tmp_base_cierre.EsIngresado = 1;
                                                _liq_tmp_base_cierre.usuario = usuario_;
                                                System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.DOCUMENTO_SUPERVISOR, periodo, TipoEsquemaValidate);
                                                if (EsValido)
                                                {
                                                    _liq_tmp_base_cierre.EsValido = 1;
                                                }
                                                else
                                                {
                                                    _liq_tmp_base_cierre.EsValido = 0;
                                                }
                                                _liq_tmp_base_cierre.fecha_creacion = DateTime.Now;
                                                _liq_tmp_base_cierre.fecha_modificacion = DateTime.Now;
                                                _liq_tmp_base_cierre.tipo_campana = item.TIPO_CAMPANA;
                                                _context.liq_tmp_base_cierre.Add(_liq_tmp_base_cierre);
                                                await _context.SaveChangesAsync();

                                            }
                                        }

                                    }
                                }

                                General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACIONES PYMES", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                string msj = "";
                                msj = await procesar_base_cierre_carta_meta_call_v2(periodo, 1);
                                if (string.IsNullOrEmpty(msj))
                                {
                                    mensaje = "PROCESADO DE FORMA CORRECTA";
                                }
                            }
                        }
                        else
                        {
                            mensaje = "EL ARCHIVO BASE CIERRE TIENE ASESORES QUE AUN NO TIENE METAS";
                        }

                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                string msj_exp = sf.GetMethod().Name + " " + e.Message + " " + e.Source + " " + e.StackTrace;

                General.crearImprimeMensajeLog(msj_exp, "Error log call", _config.GetConnectionString("conexionDbPruebas"));
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Base Cierre Call", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }


            //string json = "";
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }
        [HttpPost("reprocesarExcelBaseCierreCall")]
        [Authorize]
        public async Task<IActionResult> reprocesarExcelBaseCierreCall(dynamic data_recibe)
        {
            Int32 tipo_esquema = 0;
            Int32 TipoProceso = 0;
            string cedula_supervisor_ = "";
            string periodo = "";
            string mensaje = "";
            string json = "";
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);
                TipoProceso = Convert.ToInt32(datObject["TipoProceso"]);
                cedula_supervisor_ = Convert.ToString(datObject["cedula_supervisor"]);
                periodo = Convert.ToString(datObject["periodo"]);

                List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
                _carta_metas = await _context.liq_tmp_metas.Where(x => x.cedula_supervisor == cedula_supervisor_
                                                                   && x.periodo_importe == periodo
                                                                   && x.cod_tipo_escala == tipo_esquema
                                                                   && x.estado == 1).ToListAsync();
                int tam_carta_metas = _carta_metas.Count();
                General.crearImprimeMensajeLog("El tamaño de la lista es : " + tam_carta_metas, "procesar_base_cierre_carta_meta_call", _config.GetConnectionString("conexionDbPruebas"));
                List<listar_pap_resumida> _listar_pap_resumidas_ = listar_call_resumidas_v2();

                List<liq_pap> _liq_pap = _context.liq_pap.Where(x => x.codigo_liq_esq == tipo_esquema
                                                                && x.estado == 1).ToList();

                List<liq_empaqhome> _liq_empaqhome_ = _context.liq_empaqhome.Where(x => x.cod_tipo_esquema == tipo_esquema).ToList();

                Int32 valor_mega1 = ObtnerVelocidadMega(10);
                Int32 valor_mega2 = ObtnerVelocidadMega(11);
                Int32 valor_mega3 = ObtnerVelocidadMega(12);
                Int32 valor_mega4 = ObtnerVelocidadMega(13);
                Int32 valor_mega5 = 0;
                List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                if (_carta_metas.Count > 0)
                {
                    foreach (liq_tmp_metas item in _carta_metas)
                    {
                        Int32 sumasUnidades = sumarTotalUnidadesAsesorCall(item.cedula_asesor, item.cedula_supervisor, item.periodo_importe, TipoProceso, item.cod_tipo_escala);
                        if (sumasUnidades > 0)
                        {
                            liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                                                          && x.cedula_supervisor == item.cedula_supervisor
                                                                                                          && x.periodo == periodo
                                                                                                          && x.codigo_tipo_escala == item.cod_tipo_escala
                                                                                                          && x.estado == 1).FirstOrDefault();
                            if (_liq_comision_asesor_e != null)
                            {
                                string data_mensaje_es = _liq_comision_asesor_e.cedula_asesor + " periodo es : " + periodo + " y el tipo proceso : " + TipoProceso + " ";
                                General.crearImprimeMensajeLog(data_mensaje_es, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));

                                double porcentaje_asesor_e = 0;
                                double porcentaje_aux_e = 0;
                                int meta_asesor_e = _liq_comision_asesor_e.meta_asesor;
                                _liq_comision_asesor_e.total_venta_alta_velocidad = sumasUnidades;
                                porcentaje_aux_e = (double)sumasUnidades / meta_asesor_e;
                                porcentaje_asesor_e = (porcentaje_aux_e * 100);
                                Int32 portecentaje_asesor_e_i = Convert.ToInt32(porcentaje_asesor_e);
                                _liq_comision_asesor_e.cumplimiento_asesor = portecentaje_asesor_e_i;


                                int tabla_cumplimiento = ObtnerNivelCumplimientoPap(_listar_pap_resumidas_, portecentaje_asesor_e_i);
                                if (sumasUnidades > _liq_comision_asesor_e.meta_asesor)
                                {
                                    _liq_comision_asesor_e.asesor_cumple = 1;
                                }
                                else
                                {
                                    _liq_comision_asesor_e.asesor_cumple = 0;
                                }
                                _liq_comision_asesor_e.nivel = tabla_cumplimiento;
                                Int32 cantidad_velocidad_1 = 0;
                                Int32 cantidad_velocidad_2 = 0;
                                Int32 cantidad_velocidad_3 = 0;
                                Int32 cantidad_velocidad_4 = 0;

                                int[] arr_cantidades_velocidad = new int[4];
                                arr_cantidades_velocidad = await calcularCantidadesMegasCallV2(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                           valor_mega1, valor_mega2, valor_mega3, valor_mega4);

                                cantidad_velocidad_1 = arr_cantidades_velocidad[0];
                                cantidad_velocidad_2 = arr_cantidades_velocidad[1];
                                cantidad_velocidad_3 = arr_cantidades_velocidad[2];
                                cantidad_velocidad_4 = arr_cantidades_velocidad[3];

                                _liq_comision_asesor_e.numero_cant_megas_1 = cantidad_velocidad_1;
                                _liq_comision_asesor_e.numero_cant_megas_2 = cantidad_velocidad_2;
                                _liq_comision_asesor_e.numero_cant_megas_3 = cantidad_velocidad_3;
                                _liq_comision_asesor_e.numero_cant_megas_4 = cantidad_velocidad_4;


                                _liq_comision_asesor_e.nombre_mega_1 = valor_mega1 + "";
                                _liq_comision_asesor_e.nombre_mega_2 = valor_mega2 + "";
                                _liq_comision_asesor_e.nombre_mega_3 = valor_mega3 + "";
                                _liq_comision_asesor_e.nombre_mega_4 = valor_mega4 + "";


                                double valor_mega_1 = 0;
                                double valor_mega_2 = 0;
                                double valor_mega_3 = 0;
                                double valor_mega_4 = 0;
                                //aqui lo bueno
                                double[] arr_valores_megas = new double[5];

                                arr_valores_megas = await calcularValorMegasCallV2(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                      tabla_cumplimiento, sumasUnidades, valor_mega1, valor_mega2, valor_mega3, valor_mega4);
                                valor_mega_1 = arr_valores_megas[0];
                                valor_mega_2 = arr_valores_megas[1];
                                valor_mega_3 = arr_valores_megas[2];
                                valor_mega_4 = arr_valores_megas[3];

                                if (valor_mega1 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_1 = valor_mega_1;
                                    _liq_comision_asesor_e.total_valor_mega_1 = valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1;

                                }
                                if (valor_mega_2 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_2 = valor_mega_2;
                                    _liq_comision_asesor_e.total_valor_mega_2 = valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2;
                                }
                                if (valor_mega3 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_3 = valor_mega_3;
                                    _liq_comision_asesor_e.total_valor_mega_3 = valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3;
                                }
                                if (valor_mega4 > 0)
                                {
                                    _liq_comision_asesor_e.valor_mega_4 = valor_mega_4;
                                    _liq_comision_asesor_e.total_valor_mega_4 = valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4;
                                }

                                double subTotalValorMegas = ((valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1) +
                                                             (valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2) +
                                                             (valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3) +
                                                             (valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4)
                                                            );

                                Int32 numero_ventas_c2c = 0;

                                numero_ventas_c2c = await numeroVentasAltaVelocidad(item.cedula_asesor, item.cedula_supervisor, periodo, "VENTA EFECTIVA C2C", item.cod_tipo_escala, TipoProceso);
                                _liq_comision_asesor_e.numero_venta_c2c = numero_ventas_c2c;
                                double valor_venta_c2c = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 4, _liq_empaqhome_);
                                double subTotalC2c = 0;
                                if (valor_venta_c2c > 0)
                                {
                                    _liq_comision_asesor_e.valor_venta_c2c = valor_venta_c2c;
                                    subTotalC2c = (numero_ventas_c2c * valor_venta_c2c);
                                    _liq_comision_asesor_e.total_venta_c2c = subTotalC2c;
                                }
                                Int32 numero_ventas_venta_base = 0;
                                numero_ventas_venta_base = await numeroVentasAltaVelocidad(item.cedula_asesor, item.cedula_supervisor, periodo, "VENTA EFECTIVA MOVISTAR BASE", item.cod_tipo_escala, TipoProceso);
                                _liq_comision_asesor_e.numero_venta_base = numero_ventas_venta_base;
                                double valor_venta_base = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 5, _liq_empaqhome_);
                                double subTotalVentaBase = 0;
                                if (valor_venta_base > 0)
                                {
                                    _liq_comision_asesor_e.valor_venta_base = valor_venta_base;
                                    subTotalVentaBase = (numero_ventas_venta_base * valor_venta_base);
                                    _liq_comision_asesor_e.total_venta_base = subTotalVentaBase;
                                }
                                Int32 total_naked = 0;
                                total_naked = await sumarTotalNaked(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                                _liq_comision_asesor_e.numero_naked = total_naked;
                                double valor_naked = 0;
                                valor_naked = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 3, _liq_empaqhome_);
                                double subTotalNaked = 0;
                                if (valor_naked > 0)
                                {
                                    _liq_comision_asesor_e.valor_naked = valor_naked;
                                    subTotalNaked = (total_naked * valor_naked);
                                    _liq_comision_asesor_e.total_valor_naked = subTotalNaked;
                                }

                                int total_duos = 0;
                                total_duos = await sumarTotalDuosCall(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                                _liq_comision_asesor_e.numero_duos = total_duos;
                                double valor_duos = 0;
                                valor_duos = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 1, _liq_empaqhome_);
                                double subTotalDuos = 0;
                                if (valor_duos > 0)
                                {
                                    _liq_comision_asesor_e.valor_duos = valor_duos;
                                    subTotalDuos = (total_duos * valor_duos);
                                    _liq_comision_asesor_e.total_valor_duos = subTotalDuos;
                                }
                                _liq_comision_asesor_e.sub_total_comision = subTotalValorMegas + subTotalC2c + subTotalVentaBase + subTotalNaked + subTotalDuos;
                                _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.sub_total_comision;
                                _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                int sa = await _context.SaveChangesAsync();
                                if (sa > 0)
                                {
                                    (from lq_ in _context.liq_tmp_base_cierre
                                     where lq_.velocidad > 0 && lq_.cedula_asesor == item.cedula_asesor
                                     && lq_.cedula_supervisor == item.cedula_supervisor
                                     && lq_.periodo == periodo && lq_.estado == 1
                                     && lq_.EsProcesado == 0
                                     select lq_).ToList()
                                    .ForEach(x => x.EsProcesado = 1);
                                    _context.SaveChanges();

                                }
                                General.recalcular_subtotales(item.cedula_asesor, periodo, _config.GetConnectionString("conexionDbPruebas"));
                            }
                        }
                    }
                }
                mensaje = "REPROCESADO CORRECTAMENTE";
                json = JsonConvert.SerializeObject(mensaje);
            }
            catch(Exception e)
            {

            }
            return Ok(json);
        }

        #endregion

        #region penalizacionesMega
        [HttpPost("procesarExcelPenalizacionesMegasPap")]
        [Authorize]
        public async Task<IActionResult> procesarExcelPenalizacionesMegasPap(dynamic data_recibe)
        {
            General.crearImprimeMensajeLog("Entro a la funcion", "procesarExcelPenalizacionesMegasPap", _config.GetConnectionString("conexionDbPruebas"));
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int tipo_esquema = 0;
            string periodo = "";
            string mensaje = "";
            try
            {
                List<listar_tmp_base_cierre> _listar_tmp_base_cierre = new List<listar_tmp_base_cierre>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["baseCierre"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);
                _listar_tmp_base_cierre = JsonConvert.DeserializeObject<List<listar_tmp_base_cierre>>(base_);
                System.Boolean EsArchivoValido = false;

                var validar_base_cierre_periodo = _listar_tmp_base_cierre.Select(x => new { x.MES_LIQUIDACION }).Distinct().ToList();
                if (validar_base_cierre_periodo.Count() == 1)
                {
                    //if(validar_base_cierre_periodo.Select(x =>x.MES_LIQUIDACION))
                    string mes_liq = validar_base_cierre_periodo.Select(x => x.MES_LIQUIDACION).First();
                    if (mes_liq.Equals(periodo))
                    {
                        EsArchivoValido = true;
                    }
                    if (EsArchivoValido)
                    {
                        //crear un distinct de empleado, supervisor y periodo
                        //int baseCierreSinMetas = 0;
                        //string tipo_esquema_validate = "";
                        //switch (tipo_esquema)
                        //{
                        //    case 1:
                        //        tipo_esquema_validate = "PAP";
                        //        break;
                        //    case 2:
                        //        tipo_esquema_validate = "PYMES";
                        //        break;
                        //    case 3:
                        //        tipo_esquema_validate = "CALL OUT";
                        //        break;
                        //    case 5:
                        //        tipo_esquema_validate = "PAP II";
                        //        break;

                        //}
                        //var validar_base_cierre_metas = _listar_tmp_base_cierre.Select(x => new { x.CEDULA_ASESOR, x.DOCUMENTO_SUPERVISOR, x.MES_LIQUIDACION, x.TIPO_ESQUEMA })
                        //                                                       .Distinct().Where(x => x.MES_LIQUIDACION == periodo
                        //                                                                         && x.TIPO_ESQUEMA == tipo_esquema_validate).ToList();

                        //foreach (var i in validar_base_cierre_metas)
                        //{
                        //    liq_tmp_metas _liq_tmp_metas_e = _context.liq_tmp_metas.Where(x => x.cedula_asesor == i.CEDULA_ASESOR
                        //                                                                  && x.cedula_supervisor == i.DOCUMENTO_SUPERVISOR
                        //                                                                  && x.periodo_importe == i.MES_LIQUIDACION).FirstOrDefault();
                        //    if (_liq_tmp_metas_e == null)
                        //    {
                        //        List<listar_tmp_base_cierre> _listar_tmp_base_cierre_respon = _listar_tmp_base_cierre.Where(x => x.CEDULA_ASESOR == i.CEDULA_ASESOR
                        //                                                                                                           && x.DOCUMENTO_SUPERVISOR == i.DOCUMENTO_SUPERVISOR
                        //                                                                                                           && x.MES_LIQUIDACION == i.MES_LIQUIDACION).ToList();
                        //        baseCierreSinMetas++;
                        //        //List<listar_tmp_base_cierre_respon> _listar_tmp_base_cierre_respon_aux = new List<listar_tmp_base_cierre_respon>();
                        //        //_listar_tmp_base_cierre_respon_aux.Add(_listar_tmp_base_cierre_respon);
                        //    }
                        //}

                        System.Boolean tienePap = false;
                        System.Boolean tienePapII = false;
                        System.Boolean tienePymes = false;
                        System.Boolean tieneCall = false;
                        Int32 codigoTipoEsquemaPap = 0;
                        Int32 codigoTipoEsquemaPapII = 0;
                        Int32 codigoTipoEsquemaPymes = 0;
                        Int32 codigoTipoEsquemaCall = 0;
                        var validarTiposEsquemasExcel = _listar_tmp_base_cierre.Select(x => new { x.TIPO_ESQUEMA }).Distinct().ToList();

                        foreach (var item in validarTiposEsquemasExcel)
                        {
                            if (item.TIPO_ESQUEMA == "PAP")
                            {
                                tienePap = true;
                                codigoTipoEsquemaPap = 1;
                            }
                            if (item.TIPO_ESQUEMA == "PAP II")
                            {
                                tienePapII = true;
                                codigoTipoEsquemaPapII = 5;
                            }
                            if (item.TIPO_ESQUEMA == "PYMES")
                            {
                                tienePymes = true;
                                codigoTipoEsquemaPymes = 2;
                            }
                            if (item.TIPO_ESQUEMA == "CALL OUT")
                            {
                                tieneCall = true;
                                codigoTipoEsquemaCall = 3;
                            }
                        }

                        if (_listar_tmp_base_cierre.Count > 0)
                        {
                            Int64 consecutivo_lote = consecutivo_lote_importe();
                            Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                            string subQuery = "select count(*) as dato from liq_tmp_base_cierre where lote_importe = " + _consecutivo_lote + " and estado = 1";
                            General.crearDataValidoProceso("procesarExcelPenalizacionesMegasPap", _listar_tmp_base_cierre.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            foreach (listar_tmp_base_cierre item in _listar_tmp_base_cierre)
                            {
                                //validamos el tipo esquema 
                                Int32 TipoEsquemaValidate = 0;
                                liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                                if (_liq_tipo_pap != null)
                                {

                                    if (_liq_tipo_pap.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 1;
                                    }
                                }
                                if (_liq_tipo_pymes != null)
                                {

                                    if (_liq_tipo_pymes.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 2;
                                    }
                                }
                                if (_liq_tipo_call != null)
                                {

                                    if (_liq_tipo_call.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 3;
                                    }
                                }
                                if (_liq_tipo_pap_ii != null)
                                {

                                    if (_liq_tipo_pap_ii.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 5;
                                    }
                                }


                                int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                                                                && x.cod_peticion == item.ID_PETICION
                                                                                && x.periodo == periodo
                                                                                && x.cod_tipo_esquema == TipoEsquemaValidate
                                                                                && x.num_doc_cliente == item.NUM_DOCUMENTO_CLIENTE
                                                                                && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                && x.cedula_supervisor == item.DOCUMENTO_SUPERVISOR
                                                                                && x.estado == 1
                                                                                && x.unidad < 0
                                                                                && x.EsIngresado == 1).Count();

                                if (Existe == 0)
                                {
                                  
                                    //validamos que para este metodo solo inserten las unidades que son mayores a cero
                                    Int32 unidad = Convert.ToInt32(item.UNIDAD);
                                    if (unidad < 0)
                                    {
                                        liq_tmp_base_cierre _liq_tmp_base_cierre = new liq_tmp_base_cierre();
                                        _liq_tmp_base_cierre.producto = item.PRODUCTO;
                                        _liq_tmp_base_cierre.cedula_asesor = item.CEDULA_ASESOR;
                                        _liq_tmp_base_cierre.mes_seg = Convert.ToInt32(item.MES_SEG);
                                        _liq_tmp_base_cierre.unidad = Convert.ToInt32(item.UNIDAD);
                                        _liq_tmp_base_cierre.cod_peticion = item.ID_PETICION;
                                        System.Boolean tieneNumero = EsSoloNumero(item.VELOCIDAD);
                                        double mega_auxiliar = 0;
                                        if (tieneNumero)
                                        {
                                            Int32 valor_longitud_cadena = item.VELOCIDAD.Length;
                                            valor_longitud_cadena = valor_longitud_cadena - 3;
                                            string velocidad_aux = item.VELOCIDAD.Insert(valor_longitud_cadena, ",");
                                            //mega_auxiliar = Convert.ToDouble(velocidad_aux);
                                            string[] sub_velocidad = velocidad_aux.Split(',');
                                            mega_auxiliar = Convert.ToDouble(sub_velocidad[0]);
                                        }

                                        //sigo VALIDACION TIPO ESQUEMA COMISION
                                        _liq_tmp_base_cierre.cod_tipo_esquema = TipoEsquemaValidate;

                                        //VALIDAMOS LAS MEGAS DE
                                        int valor_primera_mega = 0;
                                        int valor_segunda_mega = 0;
                                        int valor_tercera_mega = 0;
                                        int valor_cuarta_mega = 0;
                                        int valor_quinta_mega = 0;

                                        if (TipoEsquemaValidate == 1 || TipoEsquemaValidate == 5 || TipoEsquemaValidate == 2)
                                        {
                                            liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                            && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            if (_Valores_Megabytes_1 != null)
                                            {
                                                valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                                //
                                            }
                                            liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                                && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            if (_Valores_Megabytes_2 != null)
                                            {
                                                valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                            }
                                            liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                                && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            if (_Valores_Megabytes_3 != null)
                                            {
                                                valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                            }
                                            liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                                && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            if (_Valores_Megabytes_4 != null)
                                            {
                                                valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                            }

                                            liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                                                                                                                && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();

                                            if (_Valores_Megabytes_5 != null)
                                            {
                                                valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                            }

                                            if (TipoEsquemaValidate == 1 || TipoEsquemaValidate == 5)
                                            {
                                                //aqui la primera validacion del megabite 1
                                                if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                    && (mega_auxiliar < valor_primera_mega + 99))
                                                {
                                                    //mensaje += "AQUI ENTRO EN EL PRIMER IF";
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;

                                                }
                                                if ((mega_auxiliar >= valor_segunda_mega) &&
                                                    (mega_auxiliar) < valor_segunda_mega + 99)
                                                {
                                                    //mensaje += "AQUI ENTRO EN EL SEGUNDO IF";
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;

                                                }
                                                if ((mega_auxiliar >= valor_tercera_mega) &&
                                                    (mega_auxiliar) < valor_tercera_mega + 99)
                                                {
                                                    //mensaje += "AQUI ENTRO EN EL TERCER  IF";
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;

                                                }
                                                if (mega_auxiliar >= valor_cuarta_mega)
                                                {
                                                    //mensaje += "AQUI ENTRO EN EL CUARTO IF";
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;

                                                }
                                            }
                                            if (TipoEsquemaValidate == 2)
                                            {
                                                //aqui la primera validacion del megabite 1
                                                if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                    && (mega_auxiliar < valor_primera_mega + 99))
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_1.valor_mega;
                                                }
                                                if ((mega_auxiliar >= valor_segunda_mega) &&
                                                    (mega_auxiliar) < valor_segunda_mega + 99)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_2.valor_mega;
                                                }
                                                if ((mega_auxiliar >= valor_tercera_mega) &&
                                                    (mega_auxiliar) < valor_tercera_mega + 99)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_3.valor_mega;
                                                }
                                                if ((mega_auxiliar >= valor_cuarta_mega) &&
                                                    (mega_auxiliar) < valor_cuarta_mega + 99)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_4.valor_mega;
                                                }
                                                if (mega_auxiliar >= valor_quinta_mega)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_5.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_5.valor_mega;
                                                }
                                            }

                                        }
                                        else if (TipoEsquemaValidate == 3)
                                        {
                                            liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                             && x.calcula_mega == 1
                                                                                                                             && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            if (_Valores_Megabytes_1 != null)
                                            {
                                                valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                            }
                                            liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                              && x.calcula_mega == 1
                                                                                                                              && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            if (_Valores_Megabytes_2 != null)
                                            {
                                                valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                            }

                                            liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                              && x.calcula_mega == 1
                                                                                                                              && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            if (_Valores_Megabytes_3 != null)
                                            {
                                                valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                            }

                                            liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                              && x.calcula_mega == 1
                                                                                                                              && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            if (_Valores_Megabytes_4 != null)
                                            {
                                                valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                            }

                                            //liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                            //                                                                                  && x.calcula_mega == 1
                                            //                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                            //if (_Valores_Megabytes_5 != null)
                                            //{
                                            //    valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                            //}
                                            //valido
                                            if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                    && (mega_auxiliar < valor_primera_mega + 99))
                                            {
                                                if (valor_primera_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;
                                                }

                                            }
                                            else if ((mega_auxiliar >= valor_segunda_mega) &&
                                                (mega_auxiliar) < valor_segunda_mega + 99)
                                            {
                                                if (valor_segunda_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;
                                                }

                                            }
                                            else if ((mega_auxiliar >= valor_tercera_mega) &&
                                               (mega_auxiliar) < valor_tercera_mega + 99)
                                            {
                                                if (valor_tercera_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;
                                                }

                                            }
                                            else if (mega_auxiliar >= valor_cuarta_mega)
                                            {
                                                if (valor_cuarta_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;
                                                }
                                            }
                                        }

                                        //liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                        //                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                        //if (_Valores_Megabytes_1 != null)
                                        //{
                                        //    valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                        //    //
                                        //}
                                        //liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                        //                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                        //if (_Valores_Megabytes_2 != null)
                                        //{
                                        //    valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                        //}
                                        //liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                        //                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                        //if (_Valores_Megabytes_3 != null)
                                        //{
                                        //    valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                        //}
                                        //liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                        //                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                        //if (_Valores_Megabytes_4 != null)
                                        //{
                                        //    valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                        //}

                                        //liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                        //                                                                                    && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();

                                        //if (_Valores_Megabytes_5 != null)
                                        //{
                                        //    valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                        //}

                                        //if (TipoEsquemaValidate == 1 || TipoEsquemaValidate == 5)
                                        //{
                                        //    //aqui la primera validacion del megabite 1
                                        //    if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                        //        && (mega_auxiliar < valor_primera_mega + 99))
                                        //    {
                                        //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                        //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;

                                        //    }
                                        //    if ((mega_auxiliar >= valor_segunda_mega) &&
                                        //        (mega_auxiliar) < valor_segunda_mega + 99)
                                        //    {
                                        //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                        //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;

                                        //    }
                                        //    if ((mega_auxiliar >= valor_tercera_mega) &&
                                        //        (mega_auxiliar) < valor_tercera_mega + 99)
                                        //    {
                                        //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                        //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;

                                        //    }
                                        //    if (mega_auxiliar >= valor_cuarta_mega)
                                        //    {
                                        //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                        //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;

                                        //    }
                                        //}

                                        _liq_tmp_base_cierre.empaqhomo = item.EMPAQHOMO;
                                        _liq_tmp_base_cierre.num_doc_cliente = item.NUM_DOCUMENTO_CLIENTE;
                                        _liq_tmp_base_cierre.cedula_supervisor = item.DOCUMENTO_SUPERVISOR;
                                        _liq_tmp_base_cierre.observacion = item.OBSERVACION;
                                        _liq_tmp_base_cierre.migracion_otro = item.MIGRACION_OTRO;
                                        _liq_tmp_base_cierre.periodo = periodo;
                                        _liq_tmp_base_cierre.lote_importe = (Int32)consecutivo_lote;
                                        _liq_tmp_base_cierre.estado = 1;
                                        _liq_tmp_base_cierre.EsProcesado = 0;
                                        _liq_tmp_base_cierre.EsIngresado = 1;
                                        _liq_tmp_base_cierre.usuario = usuario_;
                                        System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.DOCUMENTO_SUPERVISOR, periodo, TipoEsquemaValidate);
                                        if (EsValido)
                                        {
                                            _liq_tmp_base_cierre.EsValido = 1;
                                        }
                                        else
                                        {
                                            _liq_tmp_base_cierre.EsValido = 0;
                                        }
                                        _liq_tmp_base_cierre.fecha_creacion = DateTime.Now;
                                        _liq_tmp_base_cierre.fecha_modificacion = DateTime.Now;
                                        _context.liq_tmp_base_cierre.Add(_liq_tmp_base_cierre);
                                        await _context.SaveChangesAsync();
                                    }
                                    
                                        
                                }

                            }
                            string msj = "";
                            //valida el tipo proceso
                            //if (tipo_esquema == 1)
                            //{
                            //    General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACIONES PAP", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            //    //string auxmensaje = "";
                            //    msj = await procesar_base_cierre_carta_meta_pap(periodo,tipo_esquema, 0);

                            //}
                            //else if (tipo_esquema == 5)
                            //{
                            //    General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PAP II", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));

                            //    msj = await procesar_base_cierre_carta_meta_pap(periodo, tipo_esquema, 0);
                            //}
                            ////else if (tipo_esquema == 2)
                            ////{
                            ////    General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACIONES PYMES", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            ////    procesar_base_cierre_carta_meta_pymes(periodo, 0);
                            ////}
                            ///

                            if (tienePap)
                            {
                                General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PAP", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                msj = await procesar_base_cierre_carta_meta_pap(periodo, codigoTipoEsquemaPap, 0);
                            }
                            if (tienePapII)
                            {
                                General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PAP II", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                msj = await procesar_base_cierre_carta_meta_pap(periodo, codigoTipoEsquemaPapII, 0);
                            }
                            if (tienePymes)
                            {
                                General.crearLoteImporte(consecutivo_lote, "IMP. BASE CIERRE PYMES", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                msj = await procesar_base_cierre_carta_meta_pymes(periodo, 0);
                            }
                            if (tieneCall)
                            {
                                General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACIONES CALL", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                msj = await procesar_base_cierre_carta_meta_call_v2(periodo, 0);
                            }

                            mensaje = "PROCESADO DE FORMA CORRECTA";
                        }
                      


                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe penalizaciones", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }

        [HttpPost("procesarExcelPenalizacionesMegasPyme")]
        [Authorize]
        public async Task<IActionResult> procesarExcelPenalizacionesMegasPyme(dynamic data_recibe)
        {
            General.crearImprimeMensajeLog("Entro a la funcion", "procesarExcelPenalizacionesMegasPyme", _config.GetConnectionString("conexionDbPruebas"));
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int tipo_esquema = 0;
            string periodo = "";
            string mensaje = "";
            try
            {
                List<listar_tmp_base_cierre> _listar_tmp_base_cierre = new List<listar_tmp_base_cierre>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["baseCierre"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);
                _listar_tmp_base_cierre = JsonConvert.DeserializeObject<List<listar_tmp_base_cierre>>(base_);
                System.Boolean EsArchivoValido = false;

                var validar_base_cierre_periodo = _listar_tmp_base_cierre.Select(x => new { x.MES_LIQUIDACION }).Distinct().ToList();
                if (validar_base_cierre_periodo.Count() == 1)
                {
                    //if(validar_base_cierre_periodo.Select(x =>x.MES_LIQUIDACION))
                    string mes_liq = validar_base_cierre_periodo.Select(x => x.MES_LIQUIDACION).First();
                    if (mes_liq.Equals(periodo))
                    {
                        EsArchivoValido = true;
                    }
                    if (EsArchivoValido)
                    {
                        //crear un distinct de empleado, supervisor y periodo
                        int baseCierreSinMetas = 0;
                        string tipo_esquema_validate = "";
                        switch (tipo_esquema)
                        {
                            case 1:
                                tipo_esquema_validate = "PAP";
                                break;
                            case 2:
                                tipo_esquema_validate = "PYMES";
                                break;
                            case 3:
                                tipo_esquema_validate = "CALL OUT";
                                break;
                        }
                        var validar_base_cierre_metas = _listar_tmp_base_cierre.Select(x => new { x.CEDULA_ASESOR, x.DOCUMENTO_SUPERVISOR, x.MES_LIQUIDACION, x.TIPO_ESQUEMA })
                                                                               .Distinct().Where(x => x.MES_LIQUIDACION == periodo
                                                                                                 && x.TIPO_ESQUEMA == tipo_esquema_validate).ToList();

                        //foreach (var i in validar_base_cierre_metas)
                        //{
                        //    liq_tmp_metas _liq_tmp_metas_e = _context.liq_tmp_metas.Where(x => x.cedula_asesor == i.CEDULA_ASESOR
                        //                                                                  && x.cedula_supervisor == i.DOCUMENTO_SUPERVISOR
                        //                                                                  && x.periodo_importe == i.MES_LIQUIDACION).FirstOrDefault();
                        //    if (_liq_tmp_metas_e == null)
                        //    {
                        //        List<listar_tmp_base_cierre> _listar_tmp_base_cierre_respon = _listar_tmp_base_cierre.Where(x => x.CEDULA_ASESOR == i.CEDULA_ASESOR
                        //                                                                                                           && x.DOCUMENTO_SUPERVISOR == i.DOCUMENTO_SUPERVISOR
                        //                                                                                                           && x.MES_LIQUIDACION == i.MES_LIQUIDACION).ToList();
                        //        baseCierreSinMetas++;
                        //        //List<listar_tmp_base_cierre_respon> _listar_tmp_base_cierre_respon_aux = new List<listar_tmp_base_cierre_respon>();
                        //        //_listar_tmp_base_cierre_respon_aux.Add(_listar_tmp_base_cierre_respon);
                        //    }
                        //}

                        if (baseCierreSinMetas == 0)
                        {
                            if (_listar_tmp_base_cierre.Count > 0)
                            {
                                Int64 consecutivo_lote = consecutivo_lote_importe();
                                Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                                string subQuery = "select count(*) as dato from liq_tmp_base_cierre where lote_importe = " + _consecutivo_lote + " and estado = 1";
                                General.crearDataValidoProceso("procesarExcelPenalizacionesMegasPyme", _listar_tmp_base_cierre.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                foreach (listar_tmp_base_cierre item in _listar_tmp_base_cierre)
                                {
                                    //validamos el tipo esquema 
                                    Int32 TipoEsquemaValidate = 0;
                                    liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                                    liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                                    liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                                    if (_liq_tipo_pap != null)
                                    {

                                        if (_liq_tipo_pap.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 1;
                                        }
                                    }
                                    if (_liq_tipo_pymes != null)
                                    {

                                        if (_liq_tipo_pymes.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 2;
                                        }
                                    }
                                    if (_liq_tipo_call != null)
                                    {

                                        if (_liq_tipo_call.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 3;
                                        }
                                    }

                                    
                                    int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                                                                    && x.cod_peticion == item.ID_PETICION
                                                                                    && x.periodo == periodo
                                                                                    && x.cod_tipo_esquema == TipoEsquemaValidate
                                                                                    && x.num_doc_cliente == item.NUM_DOCUMENTO_CLIENTE
                                                                                    && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                    && x.cedula_supervisor == item.DOCUMENTO_SUPERVISOR
                                                                                    && x.estado == 1
                                                                                    && x.EsIngresado == 1).Count();

                                    if (Existe == 0)
                                    {
                                        if (TipoEsquemaValidate == 2)
                                        {
                                            //validamos que para este metodo solo inserten las unidades que son mayores a cero
                                            Int32 unidad = Convert.ToInt32(item.UNIDAD);
                                            if (unidad < 0)
                                            {
                                                liq_tmp_base_cierre _liq_tmp_base_cierre = new liq_tmp_base_cierre();
                                                _liq_tmp_base_cierre.producto = item.PRODUCTO;
                                                _liq_tmp_base_cierre.cedula_asesor = item.CEDULA_ASESOR;
                                                _liq_tmp_base_cierre.mes_seg = Convert.ToInt32(item.MES_SEG);
                                                _liq_tmp_base_cierre.unidad = Convert.ToInt32(item.UNIDAD);
                                                _liq_tmp_base_cierre.cod_peticion = item.ID_PETICION;
                                                System.Boolean tieneNumero = EsSoloNumero(item.VELOCIDAD);
                                                double mega_auxiliar = 0;
                                                if (tieneNumero)
                                                {
                                                   
                                                    Int32 valor_longitud_cadena = item.VELOCIDAD.Length;
                                                    valor_longitud_cadena = valor_longitud_cadena - 3;
                                                    string velocidad_aux = item.VELOCIDAD.Insert(valor_longitud_cadena, ",");
                                                    //mensaje += "la longitud es : "+valor_longitud_cadena+" y el string es : "+velocidad_aux;
                                                    string[] sub_velocidad = velocidad_aux.Split(',');
                                                    mega_auxiliar = Convert.ToDouble(sub_velocidad[0]);
                                                }

                                                //sigo VALIDACION TIPO ESQUEMA COMISION
                                                _liq_tmp_base_cierre.cod_tipo_esquema = TipoEsquemaValidate;

                                                //VALIDAMOS LAS MEGAS DE
                                                int valor_primera_mega = 0;
                                                int valor_segunda_mega = 0;
                                                int valor_tercera_mega = 0;
                                                int valor_cuarta_mega = 0;
                                                int valor_quinta_mega = 0;
                                                liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_1 != null)
                                                {
                                                    valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                                    //
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_2 != null)
                                                {
                                                    valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_3 != null)
                                                {
                                                    valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_4 != null)
                                                {
                                                    valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();

                                                if (_Valores_Megabytes_5 != null)
                                                {
                                                    valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                                }

                                                
                                                if (TipoEsquemaValidate == 2)
                                                {
                                                    //aqui la primera validacion del megabite 1
                                                    if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_1.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_segunda_mega) &&
                                                        (mega_auxiliar) < valor_segunda_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_2.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_tercera_mega) &&
                                                       (mega_auxiliar) < valor_tercera_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_3.valor_mega;
                                                    }
                                                    if ((mega_auxiliar >= valor_cuarta_mega) &&
                                                       (mega_auxiliar) < valor_cuarta_mega + 99)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_4.valor_mega;
                                                    }
                                                    if (mega_auxiliar >= valor_quinta_mega)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_5.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_5.valor_mega;
                                                    }
                                                }
                                                _liq_tmp_base_cierre.empaqhomo = item.EMPAQHOMO;
                                                _liq_tmp_base_cierre.num_doc_cliente = item.NUM_DOCUMENTO_CLIENTE;
                                                _liq_tmp_base_cierre.cedula_supervisor = item.DOCUMENTO_SUPERVISOR;
                                                _liq_tmp_base_cierre.observacion = item.OBSERVACION;
                                                _liq_tmp_base_cierre.migracion_otro = item.MIGRACION_OTRO;
                                                _liq_tmp_base_cierre.periodo = periodo;
                                                _liq_tmp_base_cierre.lote_importe = (Int32)consecutivo_lote;
                                                _liq_tmp_base_cierre.estado = 1;
                                                _liq_tmp_base_cierre.EsProcesado = 0;
                                                _liq_tmp_base_cierre.EsIngresado = 1;
                                                _liq_tmp_base_cierre.usuario = usuario_;
                                                System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.DOCUMENTO_SUPERVISOR, periodo, TipoEsquemaValidate);
                                                if (EsValido)
                                                {
                                                    _liq_tmp_base_cierre.EsValido = 1;
                                                }
                                                else
                                                {
                                                    _liq_tmp_base_cierre.EsValido = 0;
                                                }
                                                _liq_tmp_base_cierre.fecha_creacion = DateTime.Now;
                                                _liq_tmp_base_cierre.fecha_modificacion = DateTime.Now;
                                                _context.liq_tmp_base_cierre.Add(_liq_tmp_base_cierre);
                                                await _context.SaveChangesAsync();
                                            }
                                        }

                                    }

                                }
                                string msj = "";
                                //valida el tipo proceso
                                //if (tipo_esquema == 1)
                                //{
                                //    General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACIONES PAP", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                //    //string auxmensaje = "";
                                //    procesar_base_cierre_carta_meta_pap(periodo, 0);
                                //    //mensaje += auxmensaje;
                                //}
                                if (tipo_esquema == 2)
                                {

                                    General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACIONES PYMES", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                    msj = await procesar_base_cierre_carta_meta_pymes(periodo, 0);
                                }
                                mensaje = "PROCESADO DE FORMA CORRECTA";
                            }
                        }
                        else
                        {
                            mensaje = "EL ARCHIVO BASE CIERRE TIENE ASESORES QUE AUN NO TIENE METAS";
                        }


                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe penalizaciones", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }

        //call 
        [HttpPost("procesarPenalizacionesCall")]
        [Authorize]
        public async Task<IActionResult> procesarPenalizacionesCall(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int tipo_esquema = 0;
            string periodo = "";
            string mensaje = "";

            try
            {
                List<listar_tmp_base_cierre> _listar_tmp_base_cierre = new List<listar_tmp_base_cierre>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["baseCierre"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);

                _listar_tmp_base_cierre = JsonConvert.DeserializeObject<List<listar_tmp_base_cierre>>(base_);
                System.Boolean EsArchivoValido = false;
                var validar_base_cierre_periodo = _listar_tmp_base_cierre.Select(x => new { x.MES_LIQUIDACION }).Distinct().ToList();
                if (validar_base_cierre_periodo.Count() == 1)
                {
                    foreach (var i in validar_base_cierre_periodo)
                    {
                        if (i.MES_LIQUIDACION == periodo)
                        {
                            EsArchivoValido = true;
                        }
                    }

                    if (EsArchivoValido)
                    {
                        //crear un distinct de empleado, supervisor y periodo
                        int baseCierreSinMetas = 0;
                        string tipo_esquema_validate = "";
                        switch (tipo_esquema)
                        {
                            case 1:
                                tipo_esquema_validate = "PAP";
                                break;
                            case 2:
                                tipo_esquema_validate = "PYMES";
                                break;
                            case 3:
                                tipo_esquema_validate = "CALL OUT";
                                break;
                        }
                        var validar_base_cierre_metas = _listar_tmp_base_cierre.Select(x => new { x.CEDULA_ASESOR, x.DOCUMENTO_SUPERVISOR, x.MES_LIQUIDACION, x.TIPO_ESQUEMA })
                                                                               .Distinct().Where(x => x.MES_LIQUIDACION == periodo
                                                                                                 && x.TIPO_ESQUEMA == tipo_esquema_validate).ToList();

                        //foreach (var i in validar_base_cierre_metas)
                        //{
                        //    liq_tmp_metas _liq_tmp_metas_e = _context.liq_tmp_metas.Where(x => x.cedula_asesor == i.CEDULA_ASESOR
                        //                                                                  && x.cedula_supervisor == i.DOCUMENTO_SUPERVISOR
                        //                                                                  && x.periodo_importe == i.MES_LIQUIDACION).FirstOrDefault();
                        //    if (_liq_tmp_metas_e == null)
                        //    {
                        //        List<listar_tmp_base_cierre> _listar_tmp_base_cierre_respon = _listar_tmp_base_cierre.Where(x => x.CEDULA_ASESOR == i.CEDULA_ASESOR
                        //                                                                                                           && x.DOCUMENTO_SUPERVISOR == i.DOCUMENTO_SUPERVISOR
                        //                                                                                                           && x.MES_LIQUIDACION == i.MES_LIQUIDACION).ToList();
                        //        baseCierreSinMetas++;
                        //        //List<listar_tmp_base_cierre_respon> _listar_tmp_base_cierre_respon_aux = new List<listar_tmp_base_cierre_respon>();
                        //        //_listar_tmp_base_cierre_respon_aux.Add(_listar_tmp_base_cierre_respon);
                        //    }
                        //}

                        if(baseCierreSinMetas == 0)
                        {
                            if (_listar_tmp_base_cierre.Count > 0)
                            {
                                Int64 consecutivo_lote = consecutivo_lote_importe();
                                Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                                string subQuery = "select count(*) as dato from liq_tmp_base_cierre where lote_importe = " + _consecutivo_lote + " and estado = 1";
                                General.crearDataValidoProceso("procesarPenalizacionesCall", _listar_tmp_base_cierre.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                foreach (listar_tmp_base_cierre item in _listar_tmp_base_cierre)
                                {
                                    //validamos el tipo esquema 
                                    Int32 TipoEsquemaValidate = 0;
                                    liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                                    liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                                    liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                                    if (_liq_tipo_pap != null)
                                    {

                                        if (_liq_tipo_pap.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 1;
                                        }
                                    }
                                    if (_liq_tipo_pymes != null)
                                    {

                                        if (_liq_tipo_pymes.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 2;
                                        }
                                    }
                                    if (_liq_tipo_call != null)
                                    {

                                        if (_liq_tipo_call.nombre_tipo_esquema.Contains(item.TIPO_ESQUEMA))
                                        {
                                            TipoEsquemaValidate = 3;
                                        }
                                    }
                                    //int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                    //                                                && x.cod_peticion == item.ID_PETICION
                                    //                                                && x.periodo == periodo
                                    //                                                && x.cod_tipo_esquema == TipoEsquemaValidate
                                    //                                                && x.estado == 1).Count();
                                    int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                                                                    && x.cod_peticion == item.ID_PETICION
                                                                                    && x.periodo == periodo
                                                                                    && x.cod_tipo_esquema == TipoEsquemaValidate
                                                                                    && x.num_doc_cliente == item.NUM_DOCUMENTO_CLIENTE
                                                                                    && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                    && x.cedula_supervisor == item.DOCUMENTO_SUPERVISOR
                                                                                    && x.estado == 1
                                                                                    && x.EsIngresado == 1).Count();
                                    if (Existe == 0)
                                    {
                                        if(TipoEsquemaValidate == 3)
                                        {
                                            Int32 unidad = Convert.ToInt32(item.UNIDAD);
                                            if (unidad < 0)
                                            {
                                                liq_tmp_base_cierre _liq_tmp_base_cierre = new liq_tmp_base_cierre();
                                                _liq_tmp_base_cierre.producto = item.PRODUCTO;
                                                _liq_tmp_base_cierre.cedula_asesor = item.CEDULA_ASESOR;
                                                _liq_tmp_base_cierre.mes_seg = Convert.ToInt32(item.MES_SEG);
                                                _liq_tmp_base_cierre.unidad = Convert.ToInt32(item.UNIDAD);
                                                _liq_tmp_base_cierre.cod_peticion = item.ID_PETICION;
                                                System.Boolean tieneNumero = EsSoloNumero(item.VELOCIDAD);
                                                double mega_auxiliar = 0;
                                                if (tieneNumero)
                                                {
                                                    
                                                    Int32 valor_longitud_cadena = item.VELOCIDAD.Length;
                                                    valor_longitud_cadena = valor_longitud_cadena - 3;
                                                    string velocidad_aux = item.VELOCIDAD.Insert(valor_longitud_cadena, ",");
                                                    //mensaje += "la longitud es : "+valor_longitud_cadena+" y el string es : "+velocidad_aux;
                                                    string[] sub_velocidad = velocidad_aux.Split(',');
                                                    mega_auxiliar = Convert.ToDouble(sub_velocidad[0]);
                                                }
                                                _liq_tmp_base_cierre.cod_tipo_esquema = TipoEsquemaValidate;
                                                int valor_primera_mega = 0;
                                                int valor_segunda_mega = 0;
                                                int valor_tercera_mega = 0;
                                                int valor_cuarta_mega = 0;
                                                int valor_quinta_mega = 0;
                                                //validar aqui mediante un parametro
                                                liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_1 != null)
                                                {
                                                    valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                                }
                                                liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_2 != null)
                                                {
                                                    valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_3 != null)
                                                {
                                                    valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                                }

                                                liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                                  && x.calcula_mega == 1
                                                                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                if (_Valores_Megabytes_4 != null)
                                                {
                                                    valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                                }
                                                //liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                                //                                                                                  && x.calcula_mega == 1
                                                //                                                                                  && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                                //if (_Valores_Megabytes_5 != null)
                                                //{
                                                //    valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                                //}
                                                //valido
                                                if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                                {
                                                    if (valor_primera_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;
                                                    }

                                                }
                                                else if ((mega_auxiliar >= valor_segunda_mega) &&
                                                    (mega_auxiliar) < valor_segunda_mega + 99)
                                                {
                                                    if (valor_segunda_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;
                                                    }

                                                }
                                                else if ((mega_auxiliar >= valor_tercera_mega) &&
                                                   (mega_auxiliar) < valor_tercera_mega + 99)
                                                {
                                                    if (valor_tercera_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;
                                                    }

                                                }
                                                else if (mega_auxiliar >= valor_cuarta_mega)
                                                {
                                                    if (valor_cuarta_mega > 0)
                                                    {
                                                        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;
                                                    }
                                                }
                                                //else if (mega_auxiliar >= valor_quinta_mega)
                                                //{
                                                //    if (valor_quinta_mega > 0)
                                                //    {
                                                //        _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_5.valor_mega;
                                                //        _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_5.valor_mega;
                                                //    }
                                                //}
                                                _liq_tmp_base_cierre.empaqhomo = item.EMPAQHOMO;
                                                _liq_tmp_base_cierre.num_doc_cliente = item.NUM_DOCUMENTO_CLIENTE;
                                                _liq_tmp_base_cierre.cedula_supervisor = item.DOCUMENTO_SUPERVISOR;
                                                _liq_tmp_base_cierre.observacion = item.OBSERVACION;
                                                _liq_tmp_base_cierre.migracion_otro = item.MIGRACION_OTRO;
                                                _liq_tmp_base_cierre.periodo = periodo;
                                                _liq_tmp_base_cierre.lote_importe = (Int32)consecutivo_lote;
                                                _liq_tmp_base_cierre.estado = 1;
                                                _liq_tmp_base_cierre.EsProcesado = 0;
                                                _liq_tmp_base_cierre.EsIngresado = 1;
                                                _liq_tmp_base_cierre.usuario = usuario_;
                                                System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.DOCUMENTO_SUPERVISOR, periodo, TipoEsquemaValidate);
                                                if (EsValido)
                                                {
                                                    _liq_tmp_base_cierre.EsValido = 1;
                                                }
                                                else
                                                {
                                                    _liq_tmp_base_cierre.EsValido = 0;
                                                }
                                                _liq_tmp_base_cierre.fecha_creacion = DateTime.Now;
                                                _liq_tmp_base_cierre.fecha_modificacion = DateTime.Now;
                                                _liq_tmp_base_cierre.tipo_campana = item.TIPO_CAMPANA;
                                                _context.liq_tmp_base_cierre.Add(_liq_tmp_base_cierre);
                                                await _context.SaveChangesAsync();

                                            }
                                        }
                                        
                                    }
                                }
                                //string msj = "";
                                General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACIONES CALL", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                //msj = await procesar_base_cierre_carta_meta_call(periodo, 0);
                                string msj = "";
                                msj = await procesar_base_cierre_carta_meta_call_v2(periodo, 0);
                            }
                        }
                        else
                        {
                            mensaje = "EL ARCHIVO BASE CIERRE TIENE ASESORES QUE AUN NO TIENE METAS";
                        }

                        
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Importe Base Cierre Call", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }


            string json = "";
            return Ok(json);
        }
        //
        #endregion

        #region migracion y nunca pago

        [HttpPost("procesarExcelAltasMigracion")]
        [Authorize]
        public IActionResult procesarExcelAltasMigracion(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int tipo_esquema = 0;
            string periodo = "";
            string mensaje = "";
            try
            {
                List<listar_tmp_base_cierre> _listar_tmp_base_cierre = new List<listar_tmp_base_cierre>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["altaMigracion"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);
                _listar_tmp_base_cierre = JsonConvert.DeserializeObject<List<listar_tmp_base_cierre>>(base_);

                System.Boolean EsArchivoValido = false;
                var validar_base_cierre_periodo = _listar_tmp_base_cierre.Select(x => new { x.MES_LIQUIDACION }).Distinct().ToList();
                if (validar_base_cierre_periodo.Count() == 1)
                {
                    //if(validar_base_cierre_periodo.Select(x =>x.MES_LIQUIDACION))
                    string mes_liq = validar_base_cierre_periodo.Select(x => x.MES_LIQUIDACION).First();
                    if (mes_liq.Equals(periodo))
                    {
                        EsArchivoValido = true;
                    }
                    if (EsArchivoValido)
                    {
                        System.Boolean tienePap = false;
                        System.Boolean tienePapII = false;
                        System.Boolean tienePymes = false;
                        System.Boolean tieneCall = false;
                        Int32 codigoTipoEsquemaPap = 0;
                        Int32 codigoTipoEsquemaPapII = 0;
                        Int32 codigoTipoEsquemaPymes = 0;
                        Int32 codigoTipoEsquemaCall = 0;
                        var validarTiposEsquemasExcel = _listar_tmp_base_cierre.Select(x => new { x.TIPO_ESQUEMA }).Distinct().ToList();

                        foreach (var item in validarTiposEsquemasExcel)
                        {
                            if (item.TIPO_ESQUEMA == "PAP")
                            {
                                tienePap = true;
                                codigoTipoEsquemaPap = 1;
                            }
                            if (item.TIPO_ESQUEMA == "PAP II")
                            {
                                tienePapII = true;
                                codigoTipoEsquemaPapII = 5;
                            }
                            if (item.TIPO_ESQUEMA == "PYMES")
                            {
                                tienePymes = true;
                                codigoTipoEsquemaPymes = 2;
                            }
                            if (item.TIPO_ESQUEMA == "CALL OUT")
                            {
                                tieneCall = true;
                                codigoTipoEsquemaCall = 3;
                            }
                        }

                        if (_listar_tmp_base_cierre.Count > 0)
                        {
                            Int64 consecutivo_lote = consecutivo_lote_importe();
                            Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                            string subQuery = "select count(*) as dato from liq_tmp_base_cierre where lote_importe = " + _consecutivo_lote + " and estado = 1";
                            General.crearDataValidoProceso("procesarExcelAltasMigracion", _listar_tmp_base_cierre.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            foreach (listar_tmp_base_cierre item in _listar_tmp_base_cierre)
                            {
                                General.crearImprimeMensajeLog(" Entro al foreach ", "procesarExcelAltasMigracion", _config.GetConnectionString("conexionDbPruebas"));
                                //validamos el tipo de esquema 
                                Int32 TipoEsquemaValidate = 0;
                                //sigo
                                liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                                liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                                if (_liq_tipo_pap != null)
                                {

                                    if (_liq_tipo_pap.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        //_liq_tmp_base_cierre.cod_tipo_esquema = 1;
                                        TipoEsquemaValidate = 1;
                                    }
                                }
                                if (_liq_tipo_pymes != null)
                                {

                                    if (_liq_tipo_pymes.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        // _liq_tmp_base_cierre.cod_tipo_esquema = 2;
                                        TipoEsquemaValidate = 2;
                                    }
                                }
                                if (_liq_tipo_call != null)
                                {

                                    if (_liq_tipo_call.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        //_liq_tmp_base_cierre.cod_tipo_esquema = 3;
                                        TipoEsquemaValidate = 3;
                                    }
                                }
                                if(_liq_tipo_pap_ii != null)
                                {
                                    if (_liq_tipo_pap_ii.esquema.Equals(item.TIPO_ESQUEMA))
                                    {
                                        TipoEsquemaValidate = 5;
                                    }
                                }


                                //int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                //                                                && x.cod_peticion == item.ID_PETICION
                                //                                                && x.periodo == periodo
                                //                                                && x.estado == 1).Count();
                                int Existe = _context.liq_tmp_base_cierre.Where(x => x.producto == item.PRODUCTO
                                                                                    && x.cod_peticion == item.ID_PETICION
                                                                                    && x.periodo == periodo
                                                                                    && x.cod_tipo_esquema == TipoEsquemaValidate
                                                                                    && x.num_doc_cliente == item.NUM_DOCUMENTO_CLIENTE
                                                                                    && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                    && x.cedula_supervisor == item.DOCUMENTO_SUPERVISOR
                                                                                    && x.estado == 1
                                                                                    && x.unidad == 0
                                                                                    && x.EsIngresado == 1).Count();
                                if (Existe == 0)
                                {
                                    General.crearImprimeMensajeLog("Validando si existe ", "procesarExcelAltasMigracion", _config.GetConnectionString("conexionDbPruebas"));
                                    //validamos que para este metodo solo inserten las unidades que son mayores a cero
                                    Int32 unidad = Convert.ToInt32(item.UNIDAD);
                                    if (unidad == 0)
                                    {
                                        General.crearImprimeMensajeLog("Insertado y validando en 0 ", "procesarExcelAltasMigracion", _config.GetConnectionString("conexionDbPruebas"));
                                        liq_tmp_base_cierre _liq_tmp_base_cierre = new liq_tmp_base_cierre();
                                        _liq_tmp_base_cierre.producto = item.PRODUCTO;
                                        _liq_tmp_base_cierre.cedula_asesor = item.CEDULA_ASESOR;
                                        _liq_tmp_base_cierre.mes_seg = Convert.ToInt32(item.MES_SEG);
                                        _liq_tmp_base_cierre.unidad = Convert.ToInt32(item.UNIDAD);
                                        _liq_tmp_base_cierre.cod_peticion = item.ID_PETICION;
                                        System.Boolean tieneNumero = EsSoloNumero(item.VELOCIDAD);
                                        double mega_auxiliar = 0;
                                        if (tieneNumero)
                                        {
                                           
                                            Int32 valor_longitud_cadena = item.VELOCIDAD.Length;
                                            valor_longitud_cadena = valor_longitud_cadena - 3;
                                            string velocidad_aux = item.VELOCIDAD.Insert(valor_longitud_cadena, ",");
                                            //mensaje += "la longitud es : "+valor_longitud_cadena+" y el string es : "+velocidad_aux;
                                            string[] sub_velocidad = velocidad_aux.Split(',');
                                            mega_auxiliar = Convert.ToDouble(sub_velocidad[0]);
                                        }

                                        int valor_primera_mega = 0;
                                        int valor_segunda_mega = 0;
                                        int valor_tercera_mega = 0;
                                        int valor_cuarta_mega = 0;
                                        int valor_quinta_mega = 0;
                                        liq_valores_megabytes _Valores_Megabytes_1 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 1
                                                                                                                          && x.calcula_mega == 1
                                                                                                                          && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                        if (_Valores_Megabytes_1 != null)
                                        {
                                            valor_primera_mega = _Valores_Megabytes_1.valor_mega;
                                        }
                                        liq_valores_megabytes _Valores_Megabytes_2 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 2
                                                                                                                          && x.calcula_mega == 1
                                                                                                                          && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                        if (_Valores_Megabytes_2 != null)
                                        {
                                            valor_segunda_mega = _Valores_Megabytes_2.valor_mega;
                                        }
                                        liq_valores_megabytes _Valores_Megabytes_3 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 3
                                                                                                                          && x.calcula_mega == 1
                                                                                                                          && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                        if (_Valores_Megabytes_3 != null)
                                        {
                                            valor_tercera_mega = _Valores_Megabytes_3.valor_mega;
                                        }
                                        liq_valores_megabytes _Valores_Megabytes_4 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 4
                                                                                                                          && x.calcula_mega == 1
                                                                                                                          && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();
                                        if (_Valores_Megabytes_4 != null)
                                        {
                                            valor_cuarta_mega = _Valores_Megabytes_4.valor_mega;
                                        }
                                        liq_valores_megabytes _Valores_Megabytes_5 = _context.liq_valores_megabytes.Where(x => x.homologa_valor_orden == 5
                                                                                                                          && x.calcula_mega == 1
                                                                                                                          && x.codigo_tipo_escala == TipoEsquemaValidate).FirstOrDefault();

                                        if (_Valores_Megabytes_5 != null)
                                        {
                                            valor_quinta_mega = _Valores_Megabytes_5.valor_mega;
                                        }

                                        //validamos con la primera
                                        if (TipoEsquemaValidate == 1 || TipoEsquemaValidate == 5)
                                        {
                                            if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                && (mega_auxiliar < valor_primera_mega + 99))
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_1.valor_mega;
                                            }
                                            else if ((mega_auxiliar >= valor_segunda_mega) &&
                                                (mega_auxiliar) < valor_segunda_mega + 99)
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_2.valor_mega;
                                            }
                                            else if ((mega_auxiliar >= valor_tercera_mega) &&
                                               (mega_auxiliar) < valor_tercera_mega + 99)
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_3.valor_mega;
                                            }
                                            else if (mega_auxiliar >= valor_cuarta_mega)
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_4.valor_mega;
                                            }
                                        }
                                        //validamos con el segundo
                                        if(TipoEsquemaValidate == 2)
                                        {
                                            if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_1.valor_mega;
                                            }
                                            else if ((mega_auxiliar >= valor_segunda_mega) &&
                                                (mega_auxiliar) < valor_segunda_mega + 99)
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_2.valor_mega;
                                            }
                                            else if ((mega_auxiliar >= valor_tercera_mega) &&
                                               (mega_auxiliar) < valor_tercera_mega + 99)
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_3.valor_mega;
                                            }
                                            else if ((mega_auxiliar >= valor_cuarta_mega) &&
                                               (mega_auxiliar) < valor_cuarta_mega + 99)
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_4.valor_mega;
                                            }
                                            else if (mega_auxiliar >= valor_quinta_mega)
                                            {
                                                _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_5.valor_mega;
                                                _liq_tmp_base_cierre.velocidad_pymes_rango = _Valores_Megabytes_5.valor_mega;
                                            }
                                        }
                                        //valido con la tercera
                                        if(TipoEsquemaValidate == 3)
                                        {
                                            if (mega_auxiliar > valor_primera_mega - valor_primera_mega
                                                        && (mega_auxiliar < valor_primera_mega + 99))
                                            {
                                                if (valor_primera_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_1.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_1.valor_mega;
                                                }

                                            }
                                            else if ((mega_auxiliar >= valor_segunda_mega) &&
                                                (mega_auxiliar) < valor_segunda_mega + 99)
                                            {
                                                if (valor_segunda_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_2.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_2.valor_mega;
                                                }

                                            }
                                            else if ((mega_auxiliar >= valor_tercera_mega) &&
                                               (mega_auxiliar) < valor_tercera_mega + 99)
                                            {
                                                if (valor_tercera_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_3.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_3.valor_mega;
                                                }

                                            }
                                            else if ((mega_auxiliar >= valor_cuarta_mega) &&
                                                   (mega_auxiliar) < valor_cuarta_mega + 99)
                                            {
                                                if (valor_cuarta_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_4.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_4.valor_mega;
                                                }
                                            }
                                            else if (mega_auxiliar >= valor_quinta_mega)
                                            {
                                                if (valor_quinta_mega > 0)
                                                {
                                                    _liq_tmp_base_cierre.velocidad = _Valores_Megabytes_5.valor_mega;
                                                    _liq_tmp_base_cierre.velocidad_ftth_rango = _Valores_Megabytes_5.valor_mega;
                                                }
                                            }
                                        }
                                        _liq_tmp_base_cierre.cod_tipo_esquema = TipoEsquemaValidate;
                                        _liq_tmp_base_cierre.empaqhomo = item.EMPAQHOMO;
                                        _liq_tmp_base_cierre.num_doc_cliente = item.NUM_DOCUMENTO_CLIENTE;
                                        _liq_tmp_base_cierre.cedula_supervisor = item.DOCUMENTO_SUPERVISOR;
                                        _liq_tmp_base_cierre.observacion = item.OBSERVACION;
                                        
                                        
                                        _liq_tmp_base_cierre.migracion_otro = item.MIGRACION_OTRO;
                                        _liq_tmp_base_cierre.periodo = periodo;
                                        _liq_tmp_base_cierre.lote_importe = (Int32)consecutivo_lote;
                                        _liq_tmp_base_cierre.estado = 1;
                                        _liq_tmp_base_cierre.EsProcesado = 0;
                                        _liq_tmp_base_cierre.EsIngresado = 1;
                                        _liq_tmp_base_cierre.usuario = usuario_;
                                        System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.DOCUMENTO_SUPERVISOR, periodo, TipoEsquemaValidate);
                                        if (EsValido)
                                        {
                                            _liq_tmp_base_cierre.EsValido = 1;
                                        }
                                        else
                                        {
                                            _liq_tmp_base_cierre.EsValido = 0;
                                        }
                                        _liq_tmp_base_cierre.fecha_creacion = DateTime.Now;
                                        _liq_tmp_base_cierre.fecha_modificacion = DateTime.Now;
                                        _context.liq_tmp_base_cierre.Add(_liq_tmp_base_cierre);
                                        _context.SaveChanges();
                                        General.crearImprimeMensajeLog("Insertado ...", "procesarExcelAltasMigracion", _config.GetConnectionString("conexionDbPruebas"));
                                    }
                                }
                            }
                            //

                            //VALIDAR CON LA CARTA METAS LOS INGRESOS TEMPORARES
                            if (tienePap)
                            {
                                procesar_base_cierre_alta_migracion(periodo, codigoTipoEsquemaPap);
                            }
                            if (tienePapII)
                            {
                                procesar_base_cierre_alta_migracion(periodo, codigoTipoEsquemaPapII);
                            }
                            if (tienePymes)
                            {
                                procesar_base_cierre_alta_migracion(periodo, codigoTipoEsquemaPymes);
                            }
                            if (tieneCall)
                            {
                                procesar_base_cierre_alta_migracion(periodo, codigoTipoEsquemaCall);
                            }
                            //procesar_base_cierre_alta_migracion(periodo, tipo_esquema);
                            //liq_tipo_esquema _esquema = _context.liq_tipo_esquema.Where(x => x.codigo_valor == tipo_esquema).FirstOrDefault();
                            //if(_esquema != null)
                            //{
                            //    string tipo_importe = "IMP. ALTAS MIGRACION " + _esquema.esquema;
                            //    General.crearLoteImporte(consecutivo_lote, tipo_importe, "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));

                            //}
                            mensaje = "PROCESADO DE FORMA CORRECTA";
                        }
                    }
                }

            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Altas Movil", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }

        [HttpPost("procesarExcelNuncaPagosMegas")]
        [Authorize]
        public IActionResult procesarExcelNuncaPagosMegas(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            string periodo = "";
            string mensaje = "";
            try
            {
                List<listar_liq_tmp_nunca_pagos_megas> _listar_tmp_nunca_pagos_megas = new List<listar_liq_tmp_nunca_pagos_megas>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["nuncaPagos"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);

                _listar_tmp_nunca_pagos_megas = JsonConvert.DeserializeObject<List<listar_liq_tmp_nunca_pagos_megas>>(base_);
                System.Boolean EsArchivoValido = false;
                var validar_nunca_pagos_mega = _listar_tmp_nunca_pagos_megas.Select(x => new { x.PERIODO }).Distinct().ToList();
                if (validar_nunca_pagos_mega.Count() == 1)
                {
                    string mes_liq = validar_nunca_pagos_mega.Select(x => x.PERIODO).First();
                    if (mes_liq.Equals(periodo))
                    {
                        EsArchivoValido = true;
                    }
                    if (EsArchivoValido)
                    {
                        if (_listar_tmp_nunca_pagos_megas.Count() > 0)
                        {
                            liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                            Int64 consecutivo_lote = consecutivo_lote_importe();
                            Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                            string subQuery = "select count(*) as dato from liq_tmp_nunca_pagos_megas where lote_importe = " + _consecutivo_lote + " and estado = 1";
                            General.crearDataValidoProceso("procesarExcelNuncaPagosMegas", _listar_tmp_nunca_pagos_megas.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            foreach (listar_liq_tmp_nunca_pagos_megas item in _listar_tmp_nunca_pagos_megas)
                            {

                                //validamos que en el excel este registro no exita en la base de datos
                                double total = Convert.ToDouble(item.TOTAL);
                                int Existe = _context.liq_tmp_nunca_pagos_megas.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                      && x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                      && x.periodo == item.PERIODO
                                                                                      && x.observacion == item.OBSERVACION
                                                                                      && x.total == total).Count();
                                if (Existe == 0)
                                {
                                    liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                                                
                                                                                                                && x.periodo == item.PERIODO
                                                                                                                && x.estado == 1).FirstOrDefault();
                                   
                                        //si todo Ok validamos que el valor sea menor al subtotal
                                        // se guarda el registro y Ok

                                        liq_tmp_nunca_pagos_megas _Liq_Tmp_Nunca_Pagos_Megas_e = new liq_tmp_nunca_pagos_megas();
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.cedula_asesor = item.CEDULA_ASESOR;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.zona = item.ZONA;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.periodo = item.PERIODO;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.tipo_operacion = "MEGAS";
                                        if (_liq_tipo_pap != null)
                                        {

                                            if (_liq_tipo_pap.esquema.Equals(item.ESQUEMA))
                                            {
                                                _Liq_Tmp_Nunca_Pagos_Megas_e.cod_tipo_esquema = 1;
                                            }
                                        }
                                        if (_liq_tipo_pymes != null)
                                        {

                                            if (_liq_tipo_pymes.esquema.Equals(item.ESQUEMA))
                                            {
                                                _Liq_Tmp_Nunca_Pagos_Megas_e.cod_tipo_esquema = 2;
                                            }
                                        }
                                        if (_liq_tipo_call != null)
                                        {

                                            if (_liq_tipo_call.esquema.Equals(item.ESQUEMA))
                                            {
                                                _Liq_Tmp_Nunca_Pagos_Megas_e.cod_tipo_esquema = 3;
                                            }
                                        }
                                        if(_liq_tipo_pap_ii != null)
                                        {
                                            if (_liq_tipo_pap_ii.esquema.Equals(item.ESQUEMA))
                                            {
                                                _Liq_Tmp_Nunca_Pagos_Megas_e.cod_tipo_esquema = 5;
                                            }
                                        }
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.observacion = item.OBSERVACION;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.total = total;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.estado = 1;
                                    if (_liq_comision_asesor_e != null)
                                    {
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.EsProcesado = 1;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.EsValido = 1;
                                    }
                                    else
                                    {
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.EsProcesado = 0;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.EsValido = 0;
                                    }
                                           
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.lote_importe = Convert.ToInt32(consecutivo_lote);
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.usuario = usuario_;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.fecha_creacion = DateTime.Now;
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.fecha_modificacion = DateTime.Now;
                                        DateTime _dt = DateTime.Now.AddMonths(-5);
                                        string periodo_ant_ = _dt.ToString("yyyy-mm");
                                        _Liq_Tmp_Nunca_Pagos_Megas_e.periodo_ant = periodo_ant_;
                                        var state = _context.liq_tmp_nunca_pagos_megas.Add(_Liq_Tmp_Nunca_Pagos_Megas_e).State;
                                        if (state == Microsoft.EntityFrameworkCore.EntityState.Added)
                                        {
                                            if(_liq_comision_asesor_e != null)
                                            {
                                                double valor_sub_total_comision = _liq_comision_asesor_e.sub_total_comision;
                                                if (item.CEDULA_ASESOR.Equals(_liq_comision_asesor_e.cedula_asesor) &&
                                                   item.PERIODO.Equals(_liq_comision_asesor_e.periodo))
                                                {
                                                    //validamos si tiene saldos pedientes
                                                    double valor_pendiente_np = validarSaldosPendientesNuncaPagos(item.CEDULA_ASESOR,"MEGAS");

                                                    total = total + valor_pendiente_np;
                                                    if (total <= _liq_comision_asesor_e.sub_total_comision)
                                                    {
                                                        _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.sub_total_comision - total;
                                                        _liq_comision_asesor_e.descripcion_nunca_pago = "Descuento Nunca pago periodo : " + periodo_ant_;
                                                        _liq_comision_asesor_e.total_nunca_pago = total;
                                                        saldarNuncaPagosPendientes(item.CEDULA_ASESOR, item.PERIODO, item.ZONA, valor_sub_total_comision, 0, "MEGAS", usuario_);
                                                    //_context.liq_comisi   on_asesor.Update(_liq_comision_asesor_e);
                                                }
                                                    else
                                                    {
                                                        if (total > _liq_comision_asesor_e.sub_total_comision)
                                                        {
                                                            double resisual = total - _liq_comision_asesor_e.sub_total_comision;
                                                            _liq_comision_asesor_e.total_comision = 0;
                                                            
                                                            //el residual pendiente se guarda para la siguiente liquidacion
                                                            //liq_pendientes_nunca_pagos _liq_pendientes_nunca_pagos_e = new liq_pendientes_nunca_pagos();
                                                            //_liq_pendientes_nunca_pagos_e.cedula_asesor = item.CEDULA_ASESOR;
                                                            //_liq_pendientes_nunca_pagos_e.zona_asesor = item.ZONA;
                                                            //_liq_pendientes_nunca_pagos_e.periodo_np = item.PERIODO;
                                                            //_liq_pendientes_nunca_pagos_e.valor_pendiente = resisual;
                                                            //_liq_pendientes_nunca_pagos_e.pendiente = 1;
                                                            //_liq_pendientes_nunca_pagos_e.estado = 1;
                                                            //_liq_pendientes_nunca_pagos_e.usuario = usuario_;
                                                            //_liq_pendientes_nunca_pagos_e.tipo_operacion = "MEGAS";
                                                            //_liq_pendientes_nunca_pagos_e.fecha_creacion = DateTime.Now;
                                                            //_liq_pendientes_nunca_pagos_e.fecha_modificacion = DateTime.Now;
                                                            //_context.liq_pendientes_nunca_pagos.Add(_liq_pendientes_nunca_pagos_e);

                                                            if (resisual > 0)
                                                            {
                                                                //damos el paz y salvo del nunca pago marcandolo como pendiente 0
                                                                saldarNuncaPagosPendientes(item.CEDULA_ASESOR, item.PERIODO, item.ZONA,valor_sub_total_comision, resisual, "MEGAS", usuario_);

                                                            }
                                                        }
                                                        //aqui hago el update
                                                        _liq_comision_asesor_e.descripcion_nunca_pago = "Descuento Nunca pago periodo : " + periodo_ant_;
                                                        _liq_comision_asesor_e.total_nunca_pago = total;

                                                        //_context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                                    }
                                                    _context.liq_comision_asesor.Update(_liq_comision_asesor_e);

                                                }
                                            }
                                            
                                            _context.SaveChanges();

                                        }
                                    
                                }
                                //aqui el resultado de los saldos pendientes
                                General.recalcular_saldos(periodo, _config.GetConnectionString("conexionDbPruebas"));
                                resultado = "ARCHIVO PROCESADO DE FORMA EXITOSA";
                            }
                        }
                        else
                        {
                            resultado = "EL ARCHIVO NO CORRESPONDE AL PERIODO " + periodo;
                        }
                    }
                    else
                    {
                        resultado = "EL ARCHIVO DE EXCEL PRESENTA VARIOS PERIODOS";
                    }

                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel nunca pagos", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }

        #endregion

        #region movil

        [HttpPost("procesarExcelAltasMovil")]
        [Authorize]
        public IActionResult procesarExcelAltasMovil(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            string periodo = "";
            string mensaje = "";
            System.Boolean EsArchivoValido = false;
            try
            {
                
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["altasMovil"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);
                //validamos que primero se haya importado el base cierre de las altas ftth por el periodo
                int count_base_cierre = _context.liq_tmp_base_cierre.Where(x => x.periodo == periodo).Count(); 
                
                if(count_base_cierre > 0)
                {
                    List<listar_tmp_altas_movil> _listar_tmp_altas_movil = new List<listar_tmp_altas_movil>();
                    _listar_tmp_altas_movil = JsonConvert.DeserializeObject<List<listar_tmp_altas_movil>>(base_);
                    var cuenta_periodo_importe = _listar_tmp_altas_movil.Select(x => new {x.PERIODO}).Distinct().ToList();
                    //var distincLiqEmpHome = _liq_empaqhome_.Select(x => new {x.homologa_cumplimieno, x.codigo_nivel }).Distinct().ToList();

                    if (cuenta_periodo_importe.Count() == 1)
                    {
                        foreach(var i in cuenta_periodo_importe)
                        {
                            if(i.PERIODO == periodo)
                            {
                                EsArchivoValido = true;
                            }
                        }
                        if (EsArchivoValido)
                        {
                            Int64 consecutivo_lote = consecutivo_lote_importe();
                            Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                            string subQuery = "select count(*) as dato from liq_tmp_altas_movil where lote_importe = " + _consecutivo_lote + " and estado = 1";
                            General.crearDataValidoProceso("procesarExcelAltasMovil", _listar_tmp_altas_movil.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                            foreach (listar_tmp_altas_movil item in _listar_tmp_altas_movil)
                            {
                                //validamos que el asesor cumpla con el 80% de las carta metas para su proceso en la liquidacion



                                Int32 TipoEsquemaValidate = 0;

                                        //General.crearDataValidoProceso("procesarExcelAltasMovil", _listar_tmp_altas_movil.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                                        //validamos que no se repitan en las altas de los moviles
                                        int Existe = _context.liq_tmp_altas_movil.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                        && x.cedula_cliente == item.CEDULA_CLIENTE
                                                                                        && x.unidad > 0
                                                                                        && x.celular == item.CELULAR
                                                                                        && x.imei == item.IMEI
                                                                                        && x.estado == 1
                                                                                        && x.periodo == item.PERIODO
                                                                                        && x.EsProcesado == 1).Count();
                                        if(Existe == 0)
                                        {
                                            int unidad = Convert.ToInt32(item.UNIDAD);
                                            if(unidad > 0)
                                            {
                                                liq_tmp_altas_movil _liq_tmp_altas_movil_e = new liq_tmp_altas_movil();
                                                _liq_tmp_altas_movil_e.cedula_asesor = item.CEDULA_ASESOR;
                                                _liq_tmp_altas_movil_e.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                                _liq_tmp_altas_movil_e.unidad = Convert.ToInt32(item.UNIDAD);
                                                _liq_tmp_altas_movil_e.valor = Convert.ToDouble(item.MONTO);
                                                _liq_tmp_altas_movil_e.periodo = periodo;
                                                _liq_tmp_altas_movil_e.observacion = item.OBSERVACIONES;

                                                _liq_tmp_altas_movil_e.estado = 1;
                                                _liq_tmp_altas_movil_e.EsProcesado = 0;
                                                _liq_tmp_altas_movil_e.lote_importe = (Int32)consecutivo_lote;
                                                _liq_tmp_altas_movil_e.usuario = usuario_;
                                        
                                        
                                        _liq_tmp_altas_movil_e.fecha_creacion = DateTime.Now;
                                                _liq_tmp_altas_movil_e.fecha_modificacion = DateTime.Now;
                                                _liq_tmp_altas_movil_e.cedula_cliente = item.CEDULA_CLIENTE;
                                                _liq_tmp_altas_movil_e.imei = item.IMEI;
                                        _liq_tmp_altas_movil_e.celular = item.CELULAR;
                                                if (_liq_tipo_pap != null)
                                                {

                                                    if (_liq_tipo_pap.esquema.Equals(item.ESQUEMA))
                                                    {
                                                        _liq_tmp_altas_movil_e.codigo_tipo_escala = 1;
                                                        TipoEsquemaValidate = 1;
                                                    }
                                                }
                                                if (_liq_tipo_pap_ii != null)
                                                {

                                                    if (_liq_tipo_pap_ii.esquema.Equals(item.ESQUEMA))
                                                    {
                                                        _liq_tmp_altas_movil_e.codigo_tipo_escala = 5;
                                                        TipoEsquemaValidate = 5;
                                                    }
                                                }
                                                if (_liq_tipo_pymes != null)
                                                {

                                                    if (_liq_tipo_pymes.esquema.Equals(item.ESQUEMA))
                                                    {
                                                        _liq_tmp_altas_movil_e.codigo_tipo_escala = 2;
                                                        TipoEsquemaValidate = 2;
                                                    }
                                                }
                                                if (_liq_tipo_call != null)
                                                {

                                                    if (_liq_tipo_call.esquema.Equals(item.ESQUEMA))
                                                    {
                                                        _liq_tmp_altas_movil_e.codigo_tipo_escala = 3;
                                                        TipoEsquemaValidate = 3;
                                                    }
                                                }

                                        System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.CEDULA_SUPERVISOR, periodo, TipoEsquemaValidate);
                                        if (EsValido)
                                        {
                                            _liq_tmp_altas_movil_e.EsValido = 1;
                                        }
                                        else
                                        {
                                            _liq_tmp_altas_movil_e.EsValido = 0;
                                        }
                                        _context.liq_tmp_altas_movil.Add(_liq_tmp_altas_movil_e);
                                                _context.SaveChanges();
                                            }
                                            
                                        }
                                        
                                   
                                
                                
                            }
                            General.crearLoteImporte(consecutivo_lote, "IMP. ALTAS MOVIL", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            //AQUI PROCESAR CON EL LIQUIDADOR
                            //parame tro 1 para las positivas
                            procesar_altas_movil_pap(periodo,1);
                            resultado = "ARCHIVO PROCESADO DE FORMA CORRECTA";
                        }
                        else
                        {
                            resultado = "EL PERIODO "+ periodo+" NO CORRESPONDE AL DEL ARCHIVO";
                        }
                    }
                    else
                    {
                        resultado = "EL ARCHIVO PRESENTA VARIOS PERIODOS O EN BLANCO";
                    }
                    

                }
                else
                {
                    resultado = "NO SE PUEDE IMPORTAR EL ARCHIVO DE ALTAS MOVILES SIN CARGAR EL BASE CIERRE";
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Altas Movil", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }

        [HttpPost("reprocesarExcelAltasMovil")]
        [Authorize]
        public IActionResult reprocesarExcelAltasMovil(dynamic data_recibe)
        {
            Int32 tipo_esquema = 0;
            Int32 TipoProceso = 0;
            string cedula_supervisor_ = "";
            string periodo = "";
            string mensaje = "";
            string json = "";
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                tipo_esquema = Convert.ToInt32(datObject["tipo_esquema"]);
                TipoProceso = Convert.ToInt32(datObject["TipoProceso"]);
                cedula_supervisor_ = Convert.ToString(datObject["cedula_supervisor"]);
                periodo = Convert.ToString(datObject["periodo"]);

                List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
                _carta_metas = _context.liq_tmp_metas.Where(x => x.periodo_importe == periodo
                                                            && x.cedula_supervisor == cedula_supervisor_
                                                            && x.cod_tipo_escala == tipo_esquema
                                                            && x.estado == 1)
                                                      .ToList();
                if (_carta_metas.Count() > 0)
                {
                    foreach (liq_tmp_metas item in _carta_metas)
                    {
                        Int32 UnidadesMovil = sumarTotalUnidadesAsesorMovil(item.cedula_asesor,
                                                                            item.cedula_supervisor,
                                                                            item.periodo_importe,
                                                                            TipoProceso,
                                                                            item.cod_tipo_escala);
                        if (UnidadesMovil > 0)
                        {

                            List<liq_esquema_movil> _liq_esquema_movil = _context.liq_esquema_movil.Where(x => x.estado == 1
                                                                                                          && x.codigo_tipo_esquema == item.cod_tipo_escala)
                                                                                                   .ToList();
                            liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.cedula_asesor

                                                                                                          && x.periodo == periodo
                                                                                                          && x.codigo_tipo_escala == item.cod_tipo_escala
                                                                                                          && x.estado == 1).FirstOrDefault();
                            //validar con una parametro el cumpliemiento del asesor
                            if (_liq_comision_asesor_e.cumplimiento_asesor >= 80)
                            {
                                _liq_comision_asesor_e.numero_plan_movil = UnidadesMovil;
                                double[] arrTotalMovil = new double[2];
                                arrTotalMovil = calcularValorPlanMovil(item.cedula_asesor,
                                                                       item.cedula_supervisor,
                                                                       item.periodo_importe,
                                                                       _liq_comision_asesor_e.cumplimiento_asesor,
                                                                       _liq_esquema_movil,
                                                                       TipoProceso,
                                                                       item.cod_tipo_escala);
                                _liq_comision_asesor_e.valor_plan_movil = arrTotalMovil[0];
                                _liq_comision_asesor_e.total_plan_movil = arrTotalMovil[1];
                                _liq_comision_asesor_e.sub_total_comision = _liq_comision_asesor_e.sub_total_comision + arrTotalMovil[1];
                                _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.total_comision + arrTotalMovil[1];
                                _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                int rs = _context.SaveChanges();
                                if (rs > 0)
                                {
                                    (from lq_ in _context.liq_tmp_altas_movil
                                     where lq_.cedula_asesor == item.cedula_asesor
                                     && lq_.cedula_supervisor == item.cedula_supervisor
                                     && lq_.EsProcesado == 0 && lq_.estado == 1
                                     select lq_).ToList()
                                     .ForEach(x => x.EsProcesado = 1);
                                    _context.SaveChanges();
                                }

                            }
                            General.recalcular_subtotales(item.cedula_asesor, periodo, _config.GetConnectionString("conexionDbPruebas"));
                        }
                    }
                }
                mensaje = "REPROCESADO CORRECTAMENTE";
                json = JsonConvert.SerializeObject(mensaje);
            }
            catch(Exception e)
            {

            }
            return Ok(json);
        }

        [HttpPost("procesarExcelPenalizacionMovil")]
        [Authorize]
        public IActionResult procesarExcelPenalizacionMovil(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            string periodo = "";
            string mensaje = "";
            System.Boolean EsArchivoValido = false;
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["penalizacionMovil"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);

                int count_base_cierre = _context.liq_tmp_base_cierre.Where(x => x.periodo == periodo).Count();

                if(count_base_cierre > 0)
                {
                    List<listar_tmp_altas_movil> _listar_tmp_altas_movil = new List<listar_tmp_altas_movil>();
                    _listar_tmp_altas_movil = JsonConvert.DeserializeObject<List<listar_tmp_altas_movil>>(base_);
                    var cuenta_periodo_importe = _listar_tmp_altas_movil.Select(x => new { x.PERIODO }).Distinct().ToList();
                    if (cuenta_periodo_importe.Count() == 1)
                    {
                        string mes_liq = _listar_tmp_altas_movil.Select(x => x.PERIODO).First();
                        if (mes_liq.Equals(periodo))
                        {
                            EsArchivoValido = true;
                        }

                        if (EsArchivoValido)
                        {
                            Int64 consecutivo_lote = consecutivo_lote_importe();
                            Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                            string subQuery = "select count(*) as dato from liq_tmp_altas_movil where lote_importe = " + _consecutivo_lote + " and estado = 1";
                            General.crearDataValidoProceso("procesarExcelPenalizacionMovil", _listar_tmp_altas_movil.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                            foreach (listar_tmp_altas_movil item in _listar_tmp_altas_movil)
                            {
                                //validamos que el asesor cumpla con el 80% de las carta metas para su proceso en la liquidacion
                                //validamos que ya haya tenido unidades
                                Int32 TipoEsquemaValidate = 0;
                                List<liq_tmp_altas_movil> _liq_tmp_altas_movil = _context.liq_tmp_altas_movil.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                                                    && x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                                                    && x.periodo == item.PERIODO).ToList();
                                //if(_liq_tmp_altas_movil.Count() > 0)
                                //{
                                    //liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                                                
                                    //                                                                            && x.periodo == periodo
                                    //                                                                            && x.estado == 1).FirstOrDefault();


                                    int Existe = _context.liq_tmp_altas_movil.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                        && x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                        && x.unidad  < 0
                                                                                        && x.celular == item.CELULAR
                                                                                        && x.imei == item.IMEI
                                                                                        && x.estado == 1
                                                                                        && x.periodo == item.PERIODO
                                                                                        && x.EsProcesado == 1).Count();

                                    if(Existe == 0)
                                    {
                                        Int32 unidadConvert = Convert.ToInt32(item.UNIDAD);
                                        if (unidadConvert < 0)
                                        {

                                           
                                               
                                                    liq_tmp_altas_movil _liq_tmp_altas_movil_e = new liq_tmp_altas_movil();
                                                    _liq_tmp_altas_movil_e.cedula_asesor = item.CEDULA_ASESOR;
                                                    _liq_tmp_altas_movil_e.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                                    _liq_tmp_altas_movil_e.unidad = Convert.ToInt32(item.UNIDAD);
                                                    _liq_tmp_altas_movil_e.valor = Convert.ToDouble(item.MONTO);
                                                    _liq_tmp_altas_movil_e.periodo = periodo;
                                                    _liq_tmp_altas_movil_e.observacion = item.OBSERVACIONES;

                                                    _liq_tmp_altas_movil_e.estado = 1;
                                                    _liq_tmp_altas_movil_e.EsProcesado = 0;
                                                    _liq_tmp_altas_movil_e.lote_importe = (Int32)consecutivo_lote;
                                                    _liq_tmp_altas_movil_e.usuario = usuario_;
                                                    _liq_tmp_altas_movil_e.fecha_creacion = DateTime.Now;
                                                    _liq_tmp_altas_movil_e.fecha_modificacion = DateTime.Now;
                                                    _liq_tmp_altas_movil_e.cedula_cliente = item.CEDULA_CLIENTE;
                                                    _liq_tmp_altas_movil_e.imei = item.IMEI;
                                                    _liq_tmp_altas_movil_e.celular = item.CELULAR;
                                                    if (_liq_tipo_pap != null)
                                                    {

                                                        if (_liq_tipo_pap.nombre_tipo_esquema.Contains(item.ESQUEMA))
                                                        {
                                                            _liq_tmp_altas_movil_e.codigo_tipo_escala = 1;
                                                            TipoEsquemaValidate = 1;
                                                        }
                                                    }
                                                    if (_liq_tipo_pap_ii != null)
                                                    {

                                                        if (_liq_tipo_pap_ii.esquema.Equals(item.ESQUEMA))
                                                        {
                                                            _liq_tmp_altas_movil_e.codigo_tipo_escala = 5;
                                                            TipoEsquemaValidate = 5;
                                                        }
                                                    }
                                                    if (_liq_tipo_pymes != null)
                                                    {

                                                        if (_liq_tipo_pymes.nombre_tipo_esquema.Contains(item.ESQUEMA))
                                                        {
                                                            _liq_tmp_altas_movil_e.codigo_tipo_escala = 2;
                                                            TipoEsquemaValidate = 2;
                                                        }
                                                    }
                                                    if (_liq_tipo_call != null)
                                                    {

                                                        if (_liq_tipo_call.nombre_tipo_esquema.Contains(item.ESQUEMA))
                                                        {
                                                            _liq_tmp_altas_movil_e.codigo_tipo_escala = 3;
                                                            TipoEsquemaValidate = 3;
                                                        }
                                                    }
                                            System.Boolean EsValido = validoCartaMetaProceso(item.CEDULA_ASESOR, item.CEDULA_SUPERVISOR, periodo, TipoEsquemaValidate);
                                            if (EsValido)
                                            {
                                                _liq_tmp_altas_movil_e.EsValido = 1;
                                            }
                                            else
                                            {
                                                _liq_tmp_altas_movil_e.EsValido = 0;
                                            }
                                            _context.liq_tmp_altas_movil.Add(_liq_tmp_altas_movil_e);
                                                    _context.SaveChanges();
                                                
                                            
                                        }
                                    }

                                    
                                   
                                


                                

                            }
                            General.crearLoteImporte(consecutivo_lote, "IMP. PENALIZACION MOVIL", "N/A", usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            //AQUI PROCESAR CON EL LIQUIDADOR
                            //parame tro 1 para las positivas
                            procesar_altas_movil_pap(periodo, 0);
                            resultado = "ARCHIVO PROCESADO DE FORMA CORRECTA";
                        }
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel Penalizacion Movil", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok();
        }

        [HttpPost("procesarNuncaPagosMovil")]
        [Authorize]
        public IActionResult procesarNuncaPagosMovil(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            string periodo = "";
            string mensaje = "";
            try
            {
                List<listar_liq_tmp_nunca_pagos_megas> _listar_tmp_nunca_pagos_movil = new List<listar_liq_tmp_nunca_pagos_megas>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["nuncaPagosMovil"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);

                _listar_tmp_nunca_pagos_movil = JsonConvert.DeserializeObject<List<listar_liq_tmp_nunca_pagos_megas>>(base_);
                System.Boolean EsArchivoValido = false;
                var validar_nunca_pagos_mega = _listar_tmp_nunca_pagos_movil.Select(x => new { x.PERIODO }).Distinct().ToList();
                if (validar_nunca_pagos_mega.Count() == 1)
                {
                    string mes_liq = validar_nunca_pagos_mega.Select(x => x.PERIODO).First();
                    if (mes_liq.Equals(periodo))
                    {
                        EsArchivoValido = true;
                    }
                    if (EsArchivoValido)
                    {
                        if (_listar_tmp_nunca_pagos_movil.Count() > 0)
                        {
                            liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                            Int64 consecutivo_lote = consecutivo_lote_importe();
                            Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);
                            string subQuery = "select count(*) as dato from liq_tmp_nunca_pagos_megas where lote_importe = " + _consecutivo_lote + " and estado = 1";
                            General.crearDataValidoProceso("procesarNuncaPagosMovil", _listar_tmp_nunca_pagos_movil.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            foreach (listar_liq_tmp_nunca_pagos_megas item in _listar_tmp_nunca_pagos_movil)
                            {
                                //validamos que en el excel este registro no exita en la base de datos
                                double total = Convert.ToDouble(item.TOTAL);
                                int Existe = _context.liq_tmp_nunca_pagos_megas.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                      && x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                      && x.periodo == item.PERIODO
                                                                                      && x.observacion == item.OBSERVACION
                                                                                      && x.tipo_operacion == item.TIPO_OPERACION
                                                                                      && x.total == total).Count();
                                if (Existe == 0)
                                {
                                    liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                                                
                                                                                                                && x.periodo == item.PERIODO
                                                                                                                && x.estado == 1).FirstOrDefault();
                                    //si todo Ok validamos que el valor sea menor al subtotal
                                    // se guarda el registro y Ok
                                    liq_tmp_nunca_pagos_megas _Liq_Tmp_Nunca_Pagos_Movil_e = new liq_tmp_nunca_pagos_megas();
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.cedula_asesor = item.CEDULA_ASESOR;
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.zona = item.ZONA;
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.periodo = item.PERIODO;
                                    if (_liq_tipo_pap != null)
                                    {

                                        if (_liq_tipo_pap.esquema.Equals(item.ESQUEMA))
                                        {
                                            _Liq_Tmp_Nunca_Pagos_Movil_e.cod_tipo_esquema = 1;
                                        }
                                    }
                                    if (_liq_tipo_pymes != null)
                                    {

                                        if (_liq_tipo_pymes.esquema.Equals(item.ESQUEMA))
                                        {
                                            _Liq_Tmp_Nunca_Pagos_Movil_e.cod_tipo_esquema = 2;
                                        }
                                    }
                                    if (_liq_tipo_call != null)
                                    {

                                        if (_liq_tipo_call.esquema.Equals(item.ESQUEMA))
                                        {
                                            _Liq_Tmp_Nunca_Pagos_Movil_e.cod_tipo_esquema = 3;
                                        }
                                    }
                                    if (_liq_tipo_pap_ii != null)
                                    {
                                        if (_liq_tipo_pap_ii.esquema.Equals(item.ESQUEMA))
                                        {
                                            _Liq_Tmp_Nunca_Pagos_Movil_e.cod_tipo_esquema = 5;
                                        }
                                    }
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.observacion = item.OBSERVACION;
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.total = total;
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.estado = 1;
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.tipo_operacion = "MOVIL";
                                    if (_liq_comision_asesor_e != null)
                                    {
                                        _Liq_Tmp_Nunca_Pagos_Movil_e.EsProcesado = 1;
                                    }
                                    else
                                    {
                                        _Liq_Tmp_Nunca_Pagos_Movil_e.EsProcesado = 0;
                                    }
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.lote_importe = Convert.ToInt32(consecutivo_lote);
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.usuario = usuario_;
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.fecha_creacion = DateTime.Now;
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.fecha_modificacion = DateTime.Now;
                                    DateTime _dt = DateTime.Now.AddMonths(-5);
                                    string periodo_ant_ = _dt.ToString("yyyy-mm");
                                    _Liq_Tmp_Nunca_Pagos_Movil_e.periodo_ant = periodo_ant_;
                                    var state = _context.liq_tmp_nunca_pagos_megas.Add(_Liq_Tmp_Nunca_Pagos_Movil_e).State;
                                    if (state == Microsoft.EntityFrameworkCore.EntityState.Added)
                                    {
                                        if (_liq_comision_asesor_e != null)
                                        {
                                            double valor_sub_total_comision = _liq_comision_asesor_e.sub_total_comision;
                                            //aqui validamos que la comision sea mayor a cero
                                            if(valor_sub_total_comision > 0)
                                            {
                                                if (item.CEDULA_ASESOR.Equals(_liq_comision_asesor_e.cedula_asesor) &&
                                                   item.PERIODO.Equals(_liq_comision_asesor_e.periodo))
                                                {
                                                    double valor_pendiente_np = validarSaldosPendientesNuncaPagos(item.CEDULA_ASESOR,"MOVIL");
                                                    total = total + valor_pendiente_np;
                                                    if (total <= _liq_comision_asesor_e.sub_total_comision)
                                                    {
                                                        _liq_comision_asesor_e.sub_total_comision = _liq_comision_asesor_e.sub_total_comision - total;
                                                        _liq_comision_asesor_e.descripcion_nunca_pago_movil = "Descuento Nunca pago periodo : " + periodo_ant_;
                                                        _liq_comision_asesor_e.total_nunca_pago_movil = total;

                                                        //_context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                                        if (valor_pendiente_np > 0)
                                                        {
                                                            //damos el paz y salvo del nunca pago marcandolo como pendiente 0
                                                            saldarNuncaPagosPendientes(item.CEDULA_ASESOR, item.PERIODO,item.ZONA, valor_sub_total_comision,total,"MOVIL",usuario_);

                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                liq_pendientes_nunca_pagos _liq_pendientes_nunca_pagos_e = new liq_pendientes_nunca_pagos();
                                                _liq_pendientes_nunca_pagos_e.cedula_asesor = item.CEDULA_ASESOR;
                                                _liq_pendientes_nunca_pagos_e.zona_asesor = item.ZONA;
                                                _liq_pendientes_nunca_pagos_e.periodo_np = item.PERIODO;
                                                _liq_pendientes_nunca_pagos_e.valor_pendiente = total;
                                                _liq_pendientes_nunca_pagos_e.pendiente = 1;
                                                _liq_pendientes_nunca_pagos_e.estado = 1;
                                                _liq_pendientes_nunca_pagos_e.usuario = usuario_;
                                                _liq_pendientes_nunca_pagos_e.fecha_creacion = DateTime.Now;
                                                _liq_pendientes_nunca_pagos_e.fecha_modificacion = DateTime.Now;
                                                _liq_pendientes_nunca_pagos_e.tipo_operacion = "MOVIL";
                                                _context.liq_pendientes_nunca_pagos.Add(_liq_pendientes_nunca_pagos_e);
                                            }
                                            _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                        }
                                    }
                                    _context.SaveChanges();
                                    General.recalcular_subtotales(item.CEDULA_ASESOR, item.PERIODO, _config.GetConnectionString("conexionDbPruebas"));
                                }
                            }
                            //aqui el procedimiento para el recalculo de los saldos
                            General.recalcular_saldos(periodo, _config.GetConnectionString("conexionDbPruebas"));
                            resultado = "ARCHIVO PROCESADO";
                        }
                        else
                        {
                            resultado = "EL ARCHIVO NO CORRESPONDE AL PERIODO " + periodo;
                        }
                    }
                    else
                    {
                        resultado = "EL ARCHIVO DE EXCEL PRESENTA VARIOS PERIODOS";
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel nunca pagos", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }

        #endregion

        #region otros metodos
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpPost("procesarExcelOtrosConceptos")]
        [Authorize]
        public IActionResult procesarExcelOtrosConceptos(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            string periodo = "";
            string mensaje = "";
            try
            {
                List<listar_liq_tmp_nunca_pagos_megas> _listar_tmp_otros_conceptos = new List<listar_liq_tmp_nunca_pagos_megas>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["otrosConceptos"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);

                _listar_tmp_otros_conceptos = JsonConvert.DeserializeObject<List<listar_liq_tmp_nunca_pagos_megas>>(base_);
                System.Boolean EsArchivoValido = false;
                var validar_nunca_pagos_mega = _listar_tmp_otros_conceptos.Select(x => new { x.PERIODO }).Distinct().ToList();
                if (validar_nunca_pagos_mega.Count() == 1)
                {
                    string mes_liq = validar_nunca_pagos_mega.Select(x => x.PERIODO).First();
                    if (mes_liq.Equals(periodo))
                    {
                        EsArchivoValido = true;
                    }
                    if (EsArchivoValido)
                    {
                        if (validar_nunca_pagos_mega.Count() > 0)
                        {
                            liq_tipo_esquema _liq_tipo_pap = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 1).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pymes = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 2).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_call = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 3).FirstOrDefault();
                            liq_tipo_esquema _liq_tipo_pap_ii = _context.liq_tipo_esquema.Where(x => x.codigo_valor == 5).FirstOrDefault();
                            Int64 consecutivo_lote = consecutivo_lote_importe();
                            Int32 _consecutivo_lote = Convert.ToInt32(consecutivo_lote);

                            string subQuery = "select count(*) as dato from liq_tmp_otros_conceptos where lote_importe = " + _consecutivo_lote + " and estado = 1";
                            General.crearDataValidoProceso("procesarExcelOtrosConceptos", _listar_tmp_otros_conceptos.Count(), _consecutivo_lote, subQuery, usuario_, _config.GetConnectionString("conexionDbPruebas"));
                            foreach (listar_liq_tmp_nunca_pagos_megas item in _listar_tmp_otros_conceptos)
                            {
                                double total = 0;
                                var format = new NumberFormatInfo();
                                if (item.TOTAL.Contains("-"))
                                {
                                    format.NegativeSign = "-";
                                    total = Double.Parse(item.TOTAL, format);

                                }
                                else
                                {
                                    format.PositiveSign = "+"; 
                                    total = Double.Parse(item.TOTAL, format);
                                }
                                int Existe = _context.liq_tmp_otros_conceptos.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                      && x.cedula_supervisor == item.CEDULA_SUPERVISOR
                                                                                      && x.periodo == item.PERIODO
                                                                                      && x.descripcion == item.OBSERVACION
                                                                                      && x.total == total).Count();
                                if(Existe == 0)
                                {
                                    liq_tmp_otros_conceptos _liq_tmp_otros_conceptos_e = new liq_tmp_otros_conceptos();
                                    _liq_tmp_otros_conceptos_e.cedula_asesor = item.CEDULA_ASESOR;
                                    _liq_tmp_otros_conceptos_e.cedula_supervisor = item.CEDULA_SUPERVISOR;
                                    _liq_tmp_otros_conceptos_e.zona = item.ZONA;
                                    _liq_tmp_otros_conceptos_e.periodo = item.PERIODO;
                                    if (_liq_tipo_pap != null)
                                    {

                                        if (_liq_tipo_pap.esquema.Equals(item.ESQUEMA))
                                        {
                                            _liq_tmp_otros_conceptos_e.cod_tipo_esquema = 1;
                                        }
                                    }
                                    if (_liq_tipo_pymes != null)
                                    {

                                        if (_liq_tipo_pymes.esquema.Equals(item.ESQUEMA))
                                        {
                                            _liq_tmp_otros_conceptos_e.cod_tipo_esquema = 2;
                                        }
                                    }
                                    if (_liq_tipo_call != null)
                                    {

                                        if (_liq_tipo_call.esquema.Equals(item.ESQUEMA))
                                        {
                                            _liq_tmp_otros_conceptos_e.cod_tipo_esquema = 3;
                                        }
                                    }
                                    if (_liq_tipo_pap_ii != null)
                                    {
                                        if (_liq_tipo_pap_ii.esquema.Equals(item.ESQUEMA))
                                        {
                                            _liq_tmp_otros_conceptos_e.cod_tipo_esquema = 5;
                                        }
                                    }
                                    _liq_tmp_otros_conceptos_e.descripcion = item.OBSERVACION;
                                    _liq_tmp_otros_conceptos_e.total = total;
                                    _liq_tmp_otros_conceptos_e.estado = 1;
                                    _liq_tmp_otros_conceptos_e.EsProcesado = 0;
                                    
                                    _liq_tmp_otros_conceptos_e.lote_importe = Convert.ToInt32(consecutivo_lote);
                                    _liq_tmp_otros_conceptos_e.usuario = usuario_;
                                    _liq_tmp_otros_conceptos_e.fecha_creacion = DateTime.Now;
                                    _liq_tmp_otros_conceptos_e.fecha_modificacion = DateTime.Now;

                                    

                                    var state = _context.liq_tmp_otros_conceptos.Add(_liq_tmp_otros_conceptos_e).State;
                                    if (state == Microsoft.EntityFrameworkCore.EntityState.Added)
                                    {
                                        liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                                                    && x.periodo == item.PERIODO
                                                                                                                    && x.estado == 1).FirstOrDefault();
                                        if(_liq_comision_asesor_e != null)
                                        {
                                            double aux_subTotal = _liq_comision_asesor_e.sub_total_comision;
                                            if(total > 0)
                                            {
                                                _liq_comision_asesor_e.sub_total_comision = _liq_comision_asesor_e.sub_total_comision + (total);
                                               
                                            }
                                            else
                                            {
                                                if((_liq_comision_asesor_e.sub_total_comision + (total)) > 0)
                                                {
                                                    _liq_comision_asesor_e.sub_total_comision = _liq_comision_asesor_e.sub_total_comision + (total);
                                                }
                                            }
                                            _liq_comision_asesor_e.total_otros_conceptos = total;
                                            _liq_comision_asesor_e.descripcion_otros_conceptos = item.OBSERVACION;
                                            _context.liq_comision_asesor.Update(_liq_comision_asesor_e);

                                        }
                                        _context.SaveChanges();
                                        General.recalcular_subtotales(item.CEDULA_ASESOR, item.PERIODO, _config.GetConnectionString("conexionDbPruebas"));
                                    }
                                }
                            }
                            //aqui proceso el procedimiento almacenado
                            General.recalcular_saldos(periodo, _config.GetConnectionString("conexionDbPruebas"));
                            resultado = "ARCHIVO PROCESADO";
                        }
                        else
                        {
                            resultado = "EL ARCHIVO NO CORRESPONDE AL PERIODO " + periodo;
                        }
                    }
                    else
                    {
                        resultado = "EL ARCHIVO DE EXCEL PRESENTA VARIOS PERIODOS";
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "procesarExcelOtrosConceptos", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }


        [HttpPost("procesarExcelNoPrecesadosMegas")]
        [Authorize]
        public async Task<IActionResult> procesarExcelNoPrecesadosMegas(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int estadoTras = 0;
            int cuentaErrors = 0;
            List<listar_tmp_base_cierre_v2> _liq_tmp_base_cierre_ = new List<listar_tmp_base_cierre_v2>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["baseCierre"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                _liq_tmp_base_cierre_ = JsonConvert.DeserializeObject<List<listar_tmp_base_cierre_v2>>(base_);
                var procesoCartaMeta = _liq_tmp_base_cierre_.Select(x => new { x.periodo }).Distinct().ToList();
               
                
                var validoNombresVaciosAsesor = _liq_tmp_base_cierre_.Select(x => new { x.nombre_asesor }).Distinct().ToList();
                var validoNombresVaciosSuper = _liq_tmp_base_cierre_.Select(x => new {x.nombre_supervisor}).Distinct().ToList();
                System.Boolean nombreVacioAsesor = false;
                System.Boolean nombreVacioSuper = false;
                //validar los tipos de operaciones pertenecientes los no procesados

                //nombreVacioAsesor = validoNombresVaciosAsesor.Select(x => x.nombre_asesor).Contains("#N/A");
                var valido_aux_asesor = validoNombresVaciosAsesor.Where(x => string.IsNullOrEmpty(x.nombre_asesor)).ToList();
                var valido_aux_super = validoNombresVaciosSuper.Where(x => string.IsNullOrEmpty(x.nombre_supervisor)).ToList();
                if (valido_aux_asesor.Count > 0)
                {
                    nombreVacioAsesor = true;
                }
                if(valido_aux_super.Count > 0)
                {
                    nombreVacioSuper = true;
                }

            
                System.Boolean archivoValido = false;
                if (procesoCartaMeta.Count() == 1 
                    && !nombreVacioAsesor 
                    && !nombreVacioSuper)
                {
                    archivoValido = true;
                    if (archivoValido)
                    {
                        foreach(listar_tmp_base_cierre_v2 item in _liq_tmp_base_cierre_)
                        {
                            int Existe = _context.liq_tmp_metas.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                      && x.cedula_supervisor == item.cedula_supervisor
                                                                      && x.periodo_importe == item.periodo
                                                                      && x.cod_tipo_escala == item.cod_tipo_esquema).Count();
                            //valido que el supervisor exita en 
                            int Existe_sup_cm = _context.liq_tmp_metas.Where(x => x.cedula_supervisor == item.cedula_supervisor
                                                                             && x.periodo_importe == item.periodo
                                                                             && x.cod_tipo_escala == item.cod_tipo_esquema).Count();
                            if(Existe == 0 && Existe_sup_cm > 0)
                            {
                                
                                string nombre_completo = "";
                                string supervisor_completo = "";

                                System.Boolean ExiteAsesor = validarExisteEmpleado(item.cedula_asesor);
                                System.Boolean ExisteSuper = validarExisteEmpleado(item.cedula_supervisor);
                                if (!ExiteAsesor)
                                {
                                    string[] result = item.nombre_asesor.Split(" ");
                                    int count = result.Count();
                                    foreach (string s in result)
                                    {
                                        nombre_completo += s + " ";
                                    }
                                    string cargo1 = "ASESOR VENTAS";
                                    string mensaje1 = await General.crearEmpleadosV2(item.cedula_asesor, nombre_completo, cargo1," ","COMERCIAL", _config.GetConnectionString("conexionDbPruebas"));
                                }

                                if (!ExisteSuper)
                                {
                                    string[] result_2 = item.cedula_supervisor.Split(" ");
                                    int count2 = result_2.Count();
                                    foreach (string s in result_2)
                                    {
                                        supervisor_completo += s + " ";
                                    }
                                    string cargo2 = "SUPERVISOR VENTAS";
                                    string mensaje2 = await General.crearEmpleadosV2(item.cedula_supervisor, supervisor_completo, cargo2," ","COMERCIAL", _config.GetConnectionString("conexionDbPruebas"));
                                }

                                liq_tmp_metas _liq_tmp_metas = new liq_tmp_metas();
                                string[] arr_mes_perido = item.periodo.Split('-');
                                int perido = Convert.ToInt32(arr_mes_perido[1]);
                                _liq_tmp_metas.mes_importe_liq = perido;
                                _liq_tmp_metas.cedula_asesor = item.cedula_asesor;
                                _liq_tmp_metas.cod_tipo_escala = item.cod_tipo_esquema;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_tmp_metas.numero_carta_meta_ftth = 100;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_tmp_metas.numero_carta_meta_movil = 100;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_tmp_metas.numero_carta_meta_tv = 100;
                                _liq_tmp_metas.cedula_supervisor = item.cedula_supervisor;
                                string zona_aux = _context.liq_tmp_metas.Where(x => x.cedula_supervisor == item.cedula_supervisor
                                                                               && x.periodo_importe == item.periodo
                                                                               && x.cod_tipo_escala == item.cod_tipo_esquema).Select(x => x.zona).FirstOrDefault();
                                _liq_tmp_metas.zona = zona_aux;
                                _liq_tmp_metas.activo = "INACTIVO";
                                _liq_tmp_metas.periodo_importe = item.periodo;
                                _liq_tmp_metas.estado = 1;
                                _liq_tmp_metas.usuario = usuario_;
                                _liq_tmp_metas.fecha_creacion = DateTime.Now;
                                _liq_tmp_metas.fecha_modificacion = DateTime.Now;
                                var a = _context.liq_tmp_metas.Add(_liq_tmp_metas).State;
                                if (a == Microsoft.EntityFrameworkCore.EntityState.Added)
                                {
                                    _context.liq_tmp_metas.Add(_liq_tmp_metas);
                                    //_context.SaveChanges();
                                    //el siguiente paso
                                    // se crea el esquema de la comision 
                                    liq_comision_asesor _liq_comision_asesor = new liq_comision_asesor();
                                    _liq_comision_asesor.mes_comision = perido;
                                    _liq_comision_asesor.codigo_tipo_escala = item.cod_tipo_esquema;
                                    _liq_comision_asesor.cedula_asesor = item.cedula_asesor;
                                    _liq_comision_asesor.cedula_supervisor = item.cedula_supervisor;
                                    //valor quemado que sea un parametro desde la base de datos
                                    _liq_comision_asesor.meta_asesor = 100;
                                    //valor quemado que sea un parametro desde la base de datos
                                    _liq_comision_asesor.meta_asesor_2 = 100;
                                    //valor quemado que sea un parametro desde la base de datos
                                    _liq_comision_asesor.meta_asesor_3 = 100;
                                    _liq_comision_asesor.periodo = item.periodo;
                                    string zona_aux_2 = _context.liq_tmp_metas.Where(x => x.cedula_supervisor == item.cedula_supervisor
                                                                              && x.periodo_importe == item.periodo
                                                                              && x.cod_tipo_escala == item.cod_tipo_esquema).Select(x => x.zona).FirstOrDefault();
                                    _liq_comision_asesor.zona = zona_aux_2;
                                    _liq_comision_asesor.estado = 1;
                                    _liq_comision_asesor.usuario = usuario_;
                                    _liq_comision_asesor.fecha_creacion = DateTime.Now;
                                    _liq_comision_asesor.fecha_modificacion = DateTime.Now;
                                    var b = _context.liq_comision_asesor.Add(_liq_comision_asesor).State;
                                    if (b == Microsoft.EntityFrameworkCore.EntityState.Added)
                                    {
                                        _context.liq_comision_asesor.Add(_liq_comision_asesor);
                                        //valido ahora el codigo de peticion
                                        List<liq_tmp_base_cierre> _liq_tmp_base_cierre_v = _context.liq_tmp_base_cierre.Where(x => x.cod_peticion == item.cod_peticion).ToList();
                                        if(_liq_tmp_base_cierre_v.Count() > 0)
                                        {
                                            foreach(liq_tmp_base_cierre itB in _liq_tmp_base_cierre_v)
                                            {
                                                itB.EsValido = 1;
                                                _context.Update(itB);
                                            }
                                        }
                                        //_context.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                resultado += " EN EL ARCHIVO DE EXCEL CUENTA CON CEDULAS REPETIDAS DE LOS ASESORES, SUPERVISORES O EL PERIODO";
                                cuentaErrors++;
                                estadoTras = 1;
                            }
                            _context.SaveChanges();
                        }
                        //valido que procesos va tomar
                        //System.Boolean tieneAltas = false;
                        //System.Boolean tienePenalizaciones = false;
                        //System.Boolean tieneMigracion = false;
                        System.Boolean EsPapAlta = false;
                        System.Boolean EsPapBaja = false;
                        System.Boolean EsPapMigracion = false;
                        System.Boolean EsPapIIAlta = false;
                        System.Boolean EsPapIIBaja = false;
                        System.Boolean EsPapIIMigracion = false;
                        System.Boolean EsPymeAlta = false;
                        System.Boolean EsPymeBaja = false;
                        System.Boolean EsPymeMigracion = false;
                        System.Boolean EsCallAlta = false;
                        System.Boolean EsCallBaja = false;
                        System.Boolean EsCallMigracion = false;
                        string periodo_ = "";
                        if (procesoCartaMeta.Count == 1)
                        {
                            periodo_ = procesoCartaMeta.Select(x => x.periodo).ToString();
                            List<listar_no_precesados_mega_resumida> _listar_no_procesados_mega = validoNoProcesadosMega(periodo_);
                            foreach(listar_no_precesados_mega_resumida item in _listar_no_procesados_mega)
                            {
                                if(item.cod_tipo_esquema == 1 && item.unidad == 1)
                                {
                                    EsPapAlta = true;
                                }
                                if(item.cod_tipo_esquema == 1 && item.unidad == -1)
                                {
                                    EsPapBaja = true;
                                }
                                if(item.cod_tipo_esquema == 1 && item.unidad == 0)
                                {
                                    EsPapMigracion = true;
                                }
                                if(item.cod_tipo_esquema == 2 && item.unidad == 1)
                                {
                                    EsPymeAlta = true;
                                }
                                if (item.cod_tipo_esquema == 2 && item.unidad == -1)
                                {
                                    EsPymeBaja = true;
                                }
                                if (item.cod_tipo_esquema == 2 && item.unidad == 0)
                                {
                                    EsPymeMigracion = true;
                                }
                                if (item.cod_tipo_esquema == 3 && item.unidad == 1)
                                {
                                    EsCallAlta = true;
                                }
                                if (item.cod_tipo_esquema == 3 && item.unidad == -1)
                                {
                                    EsCallBaja = true;
                                }
                                if (item.cod_tipo_esquema == 3 && item.unidad == 0)
                                {
                                    EsCallMigracion = true;
                                }
                                if (item.cod_tipo_esquema == 5 && item.unidad == 1)
                                {
                                    EsPapIIAlta = true;
                                }
                                if (item.cod_tipo_esquema == 5 && item.unidad == -1)
                                {
                                    EsPapIIBaja = true;
                                }
                                if (item.cod_tipo_esquema == 5 && item.unidad == 0)
                                {
                                    EsPapIIMigracion = true;
                                }
                            }
                            //all rigth
                            string msj = "";
                            if(EsPapAlta)
                            {
                                msj += await procesar_base_cierre_carta_meta_pap(periodo_, 1, 1);
                            }
                            if(EsPapBaja)
                            {
                                msj += await procesar_base_cierre_carta_meta_pap(periodo_, 1, 0);
                            }
                            if(EsPapMigracion)
                            {
                                procesar_base_cierre_alta_migracion(periodo_, 1);
                            }
                            if(EsPymeAlta)
                            {
                                msj += await procesar_base_cierre_carta_meta_pymes(periodo_, 1);
                            }
                            if(EsPymeBaja)
                            {
                                msj += await procesar_base_cierre_carta_meta_pymes(periodo_, 0);
                            }
                            if(EsPymeMigracion)
                            {
                                procesar_base_cierre_alta_migracion(periodo_, 2);
                            }
                            if(EsCallAlta)
                            {
                                msj += await procesar_base_cierre_carta_meta_call_v2(periodo_, 1);
                            }
                            if(EsCallBaja)
                            {
                                msj += await procesar_base_cierre_carta_meta_call_v2(periodo_, 0);
                            }
                            if(EsCallMigracion)
                            {
                                procesar_base_cierre_alta_migracion(periodo_, 3);
                            }
                            if (EsPapIIAlta)
                            {
                                msj += await procesar_base_cierre_carta_meta_pap(periodo_, 5, 1);
                            }
                            if (EsPapIIBaja)
                            {
                                msj += await procesar_base_cierre_carta_meta_pap(periodo_, 5, 0);
                            }
                            if (EsPapIIMigracion)
                            {
                                procesar_base_cierre_alta_migracion(periodo_, 5);
                            }
                        }
                    }
                }
                else
                {
                    resultado = "EL ARCHIVO DE EXCEL PRESENTA CAMPOS VACIOS EN LOS NOMBRES";
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "procesarExcelNoPrecesados", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }

        [HttpPost("procesarExcelNoPrecesadosMovil")]
        [Authorize]
        public async Task<IActionResult> procesarExcelNoPrecesadosMovil(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            int estadoTras = 0;
            int cuentaErrors = 0;
            List<listar_tmp_altas_movil_v2> _listar_tmp_altas_movil_v2 = new List<listar_tmp_altas_movil_v2>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["procesar"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                _listar_tmp_altas_movil_v2 = JsonConvert.DeserializeObject<List<listar_tmp_altas_movil_v2>>(base_);
                var procesoCartaMeta = _listar_tmp_altas_movil_v2.Select(x => new {x.periodo}).Distinct().ToList(); 
                
                var validoNombreVaciosAsesor = _listar_tmp_altas_movil_v2.Select(x => new {x.nombre_asesor}).Distinct().ToList();
                var validoNombreVaciosSuper = _listar_tmp_altas_movil_v2.Select(x => new {x.nombre_supervisor}).Distinct().ToList();

                System.Boolean nombreVacioAsesor = false;
                System.Boolean nombreVacioSuper = false;

                foreach(var itN in validoNombreVaciosAsesor)
                {
                    if (string.IsNullOrEmpty(itN.nombre_asesor))
                    {
                        nombreVacioAsesor = true;
                    }
                }

                foreach(var itS in validoNombreVaciosSuper)
                {
                    if (string.IsNullOrEmpty(itS.nombre_supervisor))
                    {
                        nombreVacioSuper = true;    
                    }
                }
                System.Boolean archivoValido = false;
                if (procesoCartaMeta.Count() == 1
                    && !nombreVacioAsesor
                    && !nombreVacioSuper)
                {
                    archivoValido = true;
                    if (archivoValido)
                    {
                        foreach(listar_tmp_altas_movil_v2 item in _listar_tmp_altas_movil_v2)
                        {
                            int Existe = _context.liq_tmp_metas.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                      && x.cedula_supervisor == item.cedula_supervisor
                                                                      && x.periodo_importe == item.periodo
                                                                      && x.cod_tipo_escala == item.codigo_tipo_escala).Count();

                            //valido que el supervisor exita en 
                            int Existe_sup_cm = _context.liq_tmp_metas.Where(x => x.cedula_supervisor == item.cedula_supervisor
                                                                             && x.periodo_importe == item.periodo
                                                                             && x.cod_tipo_escala == item.codigo_tipo_escala).Count();

                            if (Existe == 0 && Existe_sup_cm > 0)
                            {
                                string nombre_completo = "";
                                string supervisor_completo = "";

                                System.Boolean ExiteAsesor = validarExisteEmpleado(item.cedula_asesor);
                                System.Boolean ExisteSuper = validarExisteEmpleado(item.cedula_supervisor);

                                if (!ExiteAsesor)
                                {
                                    string[] result = item.nombre_asesor.Split(" ");
                                    int count = result.Count();
                                    foreach (string s in result)
                                    {
                                        nombre_completo += s + " ";
                                    }
                                    string cargo1 = "ASESOR VENTAS";
                                    string mensaje1 = await General.crearEmpleadosV2(item.cedula_asesor, nombre_completo, cargo1," ","COMERCIAL", _config.GetConnectionString("conexionDbPruebas"));
                                }

                                if (!ExisteSuper)
                                {
                                    string[] result_2 = item.cedula_supervisor.Split(" ");
                                    int count2 = result_2.Count();
                                    foreach (string s in result_2)
                                    {
                                        supervisor_completo += s + " ";
                                    }
                                    string cargo2 = "SUPERVISOR VENTAS";
                                    string mensaje2 = await General.crearEmpleadosV2(item.cedula_supervisor, supervisor_completo, cargo2, " ", "COMERCIAL", _config.GetConnectionString("conexionDbPruebas"));
                                }

                                liq_tmp_metas _liq_tmp_metas = new liq_tmp_metas();
                                string[] arr_mes_perido = item.periodo.Split('-');
                                int perido = Convert.ToInt32(arr_mes_perido[1]);
                                _liq_tmp_metas.mes_importe_liq = perido;
                                _liq_tmp_metas.cedula_asesor = item.cedula_asesor;
                                _liq_tmp_metas.cod_tipo_escala = item.codigo_tipo_escala;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_tmp_metas.numero_carta_meta_ftth = 100;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_tmp_metas.numero_carta_meta_movil = 100;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_tmp_metas.numero_carta_meta_tv = 100;
                                _liq_tmp_metas.cedula_supervisor = item.cedula_supervisor;
                                _liq_tmp_metas.zona = "N/A";
                                _liq_tmp_metas.activo = "INACTIVO";
                                _liq_tmp_metas.periodo_importe = item.periodo;
                                _liq_tmp_metas.estado = 1;
                                _liq_tmp_metas.usuario = usuario_;
                                _liq_tmp_metas.fecha_creacion = DateTime.Now;
                                _liq_tmp_metas.fecha_modificacion = DateTime.Now;
                                var a = _context.liq_tmp_metas.Add(_liq_tmp_metas).State;
                                if (a == Microsoft.EntityFrameworkCore.EntityState.Added)
                                {
                                    _context.liq_tmp_metas.Add(_liq_tmp_metas);
                                    liq_comision_asesor _liq_comision_asesor = new liq_comision_asesor();
                                    _liq_comision_asesor.mes_comision = perido;
                                    _liq_comision_asesor.codigo_tipo_escala = item.codigo_tipo_escala;
                                    _liq_comision_asesor.cedula_asesor = item.cedula_asesor;
                                    _liq_comision_asesor.cedula_supervisor = item.cedula_supervisor;
                                    //valor quemado que sea un parametro desde la base de datos
                                    _liq_comision_asesor.meta_asesor = 100;
                                    //valor quemado que sea un parametro desde la base de datos
                                    _liq_comision_asesor.meta_asesor_2 = 100;
                                    //valor quemado que sea un parametro desde la base de datos
                                    _liq_comision_asesor.meta_asesor_3 = 100;
                                    _liq_comision_asesor.periodo = item.periodo;
                                    _liq_comision_asesor.zona = "N/A";
                                    _liq_comision_asesor.estado = 1;
                                    _liq_comision_asesor.usuario = usuario_;
                                    _liq_comision_asesor.fecha_creacion = DateTime.Now;
                                    _liq_comision_asesor.fecha_modificacion = DateTime.Now;
                                    var b = _context.liq_comision_asesor.Add(_liq_comision_asesor).State;
                                    if (b == Microsoft.EntityFrameworkCore.EntityState.Added)
                                    {
                                        _context.liq_comision_asesor.Add(_liq_comision_asesor);
                                        //valido si tiene pendiente
                                    }
                                }
                            }
                            else
                            {
                                resultado += " EN EL ARCHIVO DE EXCEL CUENTA CON CEDULAS REPETIDAS DE LOS ASESORES, SUPERVISORES O EL PERIODO";
                                cuentaErrors++;
                                estadoTras = 1;
                            }
                            _context.SaveChanges();
                        }
                        System.Boolean EsPapAlta = false;
                        System.Boolean EsPapBaja = false;
                        System.Boolean EsPapIIAlta = false;
                        System.Boolean EsPapIIBaja = false;

                        string periodo_ = "";
                        if (procesoCartaMeta.Count == 1)
                        {
                            periodo_ = procesoCartaMeta.Select(x => x.periodo).ToString();
                            List<listar_no_precesados_mega_resumida> _listar_no_procesados_movil = validoNoProcesadosMovil(periodo_);
                            foreach(listar_no_precesados_mega_resumida item in _listar_no_procesados_movil)
                            {
                                if(item.cod_tipo_esquema ==1 && item.unidad == 1)
                                {
                                    EsPapAlta = true;
                                }
                                if(item.cod_tipo_esquema == 1 && item.unidad == -1)
                                {
                                    EsPapBaja = true;    
                                }
                                if(item.cod_tipo_esquema == 5 && item.unidad == 1)
                                {
                                    EsPapIIAlta = true;
                                }
                                if(item.cod_tipo_esquema == 5 && item.unidad == -1)
                                {
                                    EsPapIIBaja = true; 
                                }
                            }

                            if (EsPapAlta)
                            {
                                procesar_altas_movil_pap(periodo_, 1);
                            }
                            if (EsPapBaja)
                            {
                                procesar_altas_movil_pap(periodo_, 0);
                            }
                            if (EsPapIIAlta)
                            {
                                procesar_altas_movil_pap(periodo_, 1);
                            }
                            if (EsPapIIBaja)
                            {
                                procesar_altas_movil_pap(periodo_, 0);
                            }
                        }
                    }
                    resultado = "PROCESADO DE FORMA CORRECTA";
                }
                else
                {
                    resultado = "EL ARCHIVO DE EXCEL PRESENTA CAMPOS VACIOS EN LOS NOMBRES";
                }


            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "procesarExcelNoPrecesadosMovil", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }

        [HttpPost("procesarExcelNoPrecesadosNuncaPagos")]
        [Authorize]
        public async Task<IActionResult> procesarExcelNoPrecesadosNuncaPagos(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            string periodo = "";
            string mensaje = "";

            try
            {
                List<listar_tmp_nunca_pagos_megas_v2> _listar_tmp_nunca_pagos_megas_v2 = new List<listar_tmp_nunca_pagos_megas_v2>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["nuncaPagosMovil"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                periodo = Convert.ToString(datObject["periodo"]);

                _listar_tmp_nunca_pagos_megas_v2 = JsonConvert.DeserializeObject<List<listar_tmp_nunca_pagos_megas_v2>>(base_);
               
                var validar_nunca_pagos_mega = _listar_tmp_nunca_pagos_megas_v2.Select(x => new { x.periodo }).Distinct().ToList();

                var validarNombresVaciosAsesor = _listar_tmp_nunca_pagos_megas_v2.Select(x => new {x.nombre_asesor}).Distinct().ToList();
                var validarNombresVaciosSuper = _listar_tmp_nunca_pagos_megas_v2.Select(x => new { x.nombre_supervisor }).Distinct().ToList();
                System.Boolean nombreVacioAsesor = false;
                System.Boolean nombreVacioSuper = false;

                foreach (var itN in validarNombresVaciosAsesor)
                {
                    if (string.IsNullOrEmpty(itN.nombre_asesor))
                    {
                        nombreVacioAsesor = true;
                    }
                }

                foreach (var itS in validarNombresVaciosSuper)
                {
                    if (string.IsNullOrEmpty(itS.nombre_supervisor))
                    {
                        nombreVacioSuper = true;
                    }
                }

                System.Boolean EsArchivoValido = false;
                if(validar_nunca_pagos_mega.Count() == 1
                    && !nombreVacioAsesor
                    && !nombreVacioSuper)
                {
                    EsArchivoValido = true;
                    foreach(listar_tmp_nunca_pagos_megas_v2 item in _listar_tmp_nunca_pagos_megas_v2)
                    {
                        int Existe = _context.liq_tmp_metas.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                  && x.cedula_supervisor == item.cedula_supervisor
                                                                  && x.periodo_importe == item.periodo
                                                                  && x.cod_tipo_escala == item.cod_tipo_esquema).Count();
                        if(Existe == 0)
                        {
                            string nombre_completo = "";
                            string supervisor_completo = "";

                            System.Boolean ExiteAsesor = validarExisteEmpleado(item.cedula_asesor);
                            System.Boolean ExisteSuper = validarExisteEmpleado(item.cedula_supervisor);
                            if (!ExiteAsesor)
                            {
                                string[] result = item.nombre_asesor.Split(" ");
                                int count = result.Count();
                                foreach (string s in result)
                                {
                                    nombre_completo += s + " ";
                                }
                                string cargo1 = "ASESOR VENTAS";
                                string mensaje1 = await General.crearEmpleadosV2(item.cedula_asesor, nombre_completo, cargo1, " ","COMERCIAL", _config.GetConnectionString("conexionDbPruebas"));
                            }

                            if (!ExisteSuper)
                            {
                                string[] result_2 = item.cedula_supervisor.Split(" ");
                                int count2 = result_2.Count();
                                foreach (string s in result_2)
                                {
                                    supervisor_completo += s + " ";
                                }
                                string cargo2 = "SUPERVISOR VENTAS";
                                string mensaje2 = await General.crearEmpleadosV2(item.cedula_supervisor, supervisor_completo, cargo2," ", "COMERCIAL", _config.GetConnectionString("conexionDbPruebas"));
                            }

                            liq_tmp_metas _liq_tmp_metas = new liq_tmp_metas();
                            string[] arr_mes_perido = item.periodo.Split('-');
                            int perido = Convert.ToInt32(arr_mes_perido[1]);
                            _liq_tmp_metas.mes_importe_liq = perido;
                            _liq_tmp_metas.cedula_asesor = item.cedula_asesor;
                            _liq_tmp_metas.cod_tipo_escala = item.cod_tipo_esquema;
                            //valor quemado que sea un parametro desde la base de datos
                            _liq_tmp_metas.numero_carta_meta_ftth = 100;
                            //valor quemado que sea un parametro desde la base de datos
                            _liq_tmp_metas.numero_carta_meta_movil = 100;
                            //valor quemado que sea un parametro desde la base de datos
                            _liq_tmp_metas.numero_carta_meta_tv = 100;
                            _liq_tmp_metas.cedula_supervisor = item.cedula_supervisor;
                            _liq_tmp_metas.zona = "N/A";
                            _liq_tmp_metas.activo = "INACTIVO";
                            _liq_tmp_metas.periodo_importe = item.periodo;
                            _liq_tmp_metas.estado = 1;
                            _liq_tmp_metas.usuario = usuario_;
                            _liq_tmp_metas.fecha_creacion = DateTime.Now;
                            _liq_tmp_metas.fecha_modificacion = DateTime.Now;
                            var a = _context.liq_tmp_metas.Add(_liq_tmp_metas).State;
                            if (a == Microsoft.EntityFrameworkCore.EntityState.Added)
                            {
                                _context.liq_tmp_metas.Add(_liq_tmp_metas);
                                //el siguiente paso
                                // se crea el esquema de la comision 
                                liq_comision_asesor _liq_comision_asesor = new liq_comision_asesor();
                                _liq_comision_asesor.mes_comision = perido;
                                _liq_comision_asesor.codigo_tipo_escala = item.cod_tipo_esquema;
                                _liq_comision_asesor.cedula_asesor = item.cedula_asesor;
                                _liq_comision_asesor.cedula_supervisor = item.cedula_supervisor;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_comision_asesor.meta_asesor = 100;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_comision_asesor.meta_asesor_2 = 100;
                                //valor quemado que sea un parametro desde la base de datos
                                _liq_comision_asesor.meta_asesor_3 = 100;
                                _liq_comision_asesor.periodo = item.periodo;
                                _liq_comision_asesor.zona = "N/A";
                                _liq_comision_asesor.estado = 1;
                                _liq_comision_asesor.usuario = usuario_;
                                _liq_comision_asesor.fecha_creacion = DateTime.Now;
                                _liq_comision_asesor.fecha_modificacion = DateTime.Now;
                                var b = _context.liq_comision_asesor.Add(_liq_comision_asesor).State;
                                if (b == Microsoft.EntityFrameworkCore.EntityState.Added)
                                {
                                    _context.liq_comision_asesor.Add(_liq_comision_asesor);
                                    //que pasa si no tiene comision?
                                }
                            }
                        }
                    }
                }


            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "procesarExcelNoPrecesadosNP", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }


        [HttpGet("listarPeriodos")]
        [Authorize]
        public IActionResult listarPeriodos()
        {
            List<liq_periodo_comision_v2> _liq_periodo_comision_v2 = _context.liq_periodo_comision_v2.OrderByDescending(x => x.id)
                                                                                                     .Take(1)
                                                                                                     .ToList();
            return Ok(_liq_periodo_comision_v2);
        }
        [HttpGet("ListarPeriodosAll")]
        [Authorize]
        public IActionResult ListarPeriodosAll()
        {
            List<liq_periodo_comision_v2> _liq_periodo_comision_v2 = _context.liq_periodo_comision_v2.OrderByDescending(x => x.id)
                                                                                                     .ToList();
            return Ok(_liq_periodo_comision_v2);
        }
        [HttpGet("listarEsquemasXperiodo/{periodo}")]
        [Authorize]
        public IActionResult listarEsquemasXperiodo(string periodo)
        {
           List<listar_esquema_periodo> _Esquema_Periodos = new List<listar_esquema_periodo>();
            try
            {
                string query = "select  codigo_tipo_escala, periodo from liq_comision_asesor where periodo = '"+periodo+"'" +
                               " group by codigo_tipo_escala, periodo";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _Esquema_Periodos.Add(new listar_esquema_periodo
                                {
                                    esquema = Convert.ToInt32(sdr["codigo_tipo_escala"]),
                                    periodo = sdr["periodo"] + ""
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar esquema periodo", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }                              
           return Ok(_Esquema_Periodos);
        }
        [HttpPost("ListarSupervisoresXPeriodoEsquema")]
        [Authorize]
        public IActionResult ListarSupervisoresXPeriodoEsquema(dynamic data_recibe)
        {
            List<listar_supervisor_esq_perido> _listar_super_esquema_period = new List<listar_supervisor_esq_perido>();
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                Int32 codigo_tipo_esquema = Convert.ToInt32(datObejt["codigo_tipo_esquema"]);
                string periodo = Convert.ToString(datObejt["periodo"]);
                string query = "select lqa.cedula_supervisor, concat (e.nombre,' ', e.snombre, ' ', e.ppellido,' ' ,e.spellido) as nombre_supervisor, " +
                    "  lqa.zona, lqa.codigo_tipo_escala, lqa.periodo " +
                    " from liq_comision_asesor lqa inner join empleado e on lqa.cedula_supervisor = e.cedula_emp " +
                    "  where lqa.periodo = '"+ periodo + "' and lqa.codigo_tipo_escala = "+ codigo_tipo_esquema + " and lqa.estado = 1 " +
                    "  group by lqa.cedula_supervisor, e.nombre, e.snombre, e.ppellido, e.spellido, lqa.zona, lqa.codigo_tipo_escala, lqa.periodo";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _listar_super_esquema_period.Add(new listar_supervisor_esq_perido
                                {
                                    cedula_supervisor = sdr["cedula_supervisor"] + "",
                                    nombre_supervisor = sdr["nombre_supervisor"] + "",
                                    zona = sdr["zona"] + "",
                                    codigo_tipo_escala = Convert.ToInt32(sdr["codigo_tipo_escala"]),
                                    periodo = sdr["periodo"] + ""
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar super esquema peroido", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_listar_super_esquema_period);
        }
        [HttpPost("getSupervisores")]
        [Authorize]
        public IActionResult getSupervisores(dynamic data_recibe)
        {

            var result_comision = new Object();
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                Int32 codigo_tipo_esquema = Convert.ToInt32(datObejt["codigo_tipo_esquema"]);
                string periodo = Convert.ToString(datObejt["periodo"]);
                result_comision = (from em in _context.empleado
                                   join ca in _context.liq_tmp_metas_supervisor on em.cedula_emp equals ca.cedula_supervisor
                                   join dc in _context.liq_comision_supervisor on new { P = ca.periodo_importe, C = ca.cedula_supervisor }
                                                                               equals new { P = dc.periodo, C = dc.cedula_supervisor }
                                   where (dc.codigo_tipo_esquema == codigo_tipo_esquema && dc.periodo == periodo)
                                   select new
                                   {
                                       id = dc.id,
                                       cedula_supervisor = dc.cedula_supervisor,
                                       mes_comision = dc.mes_comision,
                                       periodo = dc.periodo,
                                       codigo_tipo_esquema = dc.codigo_tipo_esquema,
                                       numero_meta_ftth = dc.numero_meta_ftth,
                                       numero_cumplimiento_asesor_ftth = dc.numero_cumplimiento_asesor_ftth,
                                       porcentaje_cumplimiento_asesor_ftth = dc.porcentaje_cumplimiento_asesor_ftth,
                                       homologa_porcentaje_ftth = dc.homologa_porcentaje_ftth,
                                       peso_cumpliento_ftth = dc.peso_cumpliento_ftth,
                                       homologa_peso_ftth = dc.homologa_peso_ftth,
                                       numero_meta_movil = dc.numero_meta_movil,
                                       numero_cumpliento_asesor_movil = dc.numero_cumpliento_asesor_movil,
                                       porcentaje_cumplimiento_asesor_movil = dc.porcentaje_cumplimiento_asesor_movil,
                                       homologa_porcentaje_movil = dc.homologa_porcentaje_movil,
                                       peso_cumplimiento_movil = dc.peso_cumplimiento_movil,
                                       homolog_peso_movil = dc.homolog_peso_movil,
                                       numero_cumplimiento_asesor_lb = dc.numero_cumplimiento_asesor_lb,
                                       porcentaje_cumplimiento_asesor_lb = dc.porcentaje_cumplimiento_asesor_lb,
                                       homologa_porcentaje_lb = dc.homologa_porcentaje_lb,
                                       numero_meta_lb = dc.numero_meta_lb,
                                       peso_cumplimiento_lb = dc.peso_cumplimiento_lb,
                                       homologa_peso_cumplieminto_lb = dc.homologa_peso_cumplieminto_lb,
                                       numero_cumplimiento_asesor_tv = dc.numero_cumplimiento_asesor_tv,
                                       porcentaje_cumplimiento_asesor_tv = dc.porcentaje_cumplimiento_asesor_tv,
                                       homologa_porcentaje_tv = dc.homologa_porcentaje_tv,
                                       numero_meta_tv = dc.numero_meta_tv,
                                       peso_cumplimiento_tv = dc.peso_cumplimiento_tv,
                                       homologa_peso_tv = dc.homologa_peso_tv,
                                       comision = dc.comision,
                                       factor_acelearion_desaceleracion = dc.factor_acelearion_desaceleracion,
                                       homologa_factor_aceleracion_desaceleracion = dc.homologa_factor_aceleracion_desaceleracion,
                                       total_comision = dc.total_comision,
                                       total_porcentaje_cumplimiento = dc.total_porcentaje_cumplimiento,
                                       total_homologa_cumplimiento = dc.total_homologa_cumplimiento,
                                       aceleracion_desaceleracion = dc.aceleracion_desaceleracion,
                                       numero_asesores_validos = dc.numero_asesores_validos,
                                       numero_cumplimiento_asesores = dc.numero_cumplimiento_asesores,
                                       cumplimiento_asesores = dc.cumplimiento_asesores,
                                       homologa_cumpliento_asesores = dc.homologa_cumpliento_asesores,
                                       nombre_supervisor = General.getNombreCompletoEmpleado(dc.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                       nombre_esquema = General.getNombreTipoEsquema(dc.codigo_tipo_esquema, _config.GetConnectionString("conexionDbPruebas")),
                                       empresa_supervisor = em.empresa,
                                       zona = ca.zona

                                   });

            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "getSupervisores", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(result_comision);
        }

        [HttpPost("recalcularComisionSupervisor")]
        [Authorize]
        public IActionResult recalcularComisionSupervisor(dynamic data_recibe)
        {
            string periodo = "";
            string cedula_supervisor = "";
            Int32 codigo_tipo_esquema = 0;
            Int32 numero_meta_ftth = 0;
            Int32 numero_ejecucion_ftth = 0;
            Int32 numero_meta_movil = 0;
            Int32 numero_ejecucion_movil = 0;
            string usuario = "";

            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                //periodo = 
                periodo = Convert.ToString(datObject["periodo"]);
                usuario = Convert.ToString(datObject["usuario"]);
                cedula_supervisor = Convert.ToString(datObject["cedula_supervisor"]);
                codigo_tipo_esquema = Convert.ToInt32(datObject["codigo_tipo_esquema"]);
                numero_meta_ftth = Convert.ToInt32(datObject["numero_meta_ftth"]);
                numero_ejecucion_ftth = Convert.ToInt32(datObject["numero_ejecucion_ftth"]);
                numero_meta_movil = Convert.ToInt32(datObject["numero_meta_movil"]);
                numero_ejecucion_movil = Convert.ToInt32(datObject["numero_ejecucion_movil"]);

                //validamos en la comision del supervisor
                liq_comision_supervisor _liq_comision_supervisor_e = _context.liq_comision_supervisor.Where(x => x.cedula_supervisor == cedula_supervisor
                                                                                                            && x.periodo == periodo
                                                                                                            && x.codigo_tipo_esquema == codigo_tipo_esquema)
                                                                                                     .FirstOrDefault();
                if(_liq_comision_supervisor_e != null)
                {
                    double aux_homologa_porcentaje_ftth = 0;
                    aux_homologa_porcentaje_ftth = (double)((double)(numero_ejecucion_ftth / numero_meta_ftth) * 100);
                    _liq_comision_supervisor_e.numero_meta_ftth = numero_meta_ftth;
                    _liq_comision_supervisor_e.homologa_porcentaje_ftth = aux_homologa_porcentaje_ftth;
                    _liq_comision_supervisor_e.porcentaje_cumplimiento_asesor_ftth = aux_homologa_porcentaje_ftth + " % ";

                    double homologa_peso_fthh = 0;
                    homologa_peso_fthh = _context.liq_cumpliento_peso_v2.Where(x => x.descripcion_producto == "FTTH"
                                                                               && x.codigo_tipo_esquema == codigo_tipo_esquema
                                                                               && x.estado == 1)
                                                                        .Select(x => x.homologa_peso)
                                                                        .FirstOrDefault();
                    double aux_homologa_porcetaje_peso_ftth_t = 0;
                    aux_homologa_porcetaje_peso_ftth_t = aux_homologa_porcentaje_ftth * (homologa_peso_fthh / 100);
                    string peso_ftth = "";
                    peso_ftth = _context.liq_cumpliento_peso_v2.Where(x => x.descripcion_producto == "FTTH"
                                                                      && x.codigo_tipo_esquema == codigo_tipo_esquema
                                                                      && x.estado == 1)
                                                               .Select(x => x.peso)
                                                               .FirstOrDefault();
                    _liq_comision_supervisor_e.homologa_peso_ftth = aux_homologa_porcetaje_peso_ftth_t;
                    _liq_comision_supervisor_e.peso_cumpliento_ftth = aux_homologa_porcetaje_peso_ftth_t + " % " + " : " + peso_ftth;
                    double homologa_peso_movil = 0;
                    double homologa_peso_tv = 0;
                    if (codigo_tipo_esquema == 1 || codigo_tipo_esquema == 5)
                    {
                        _liq_comision_supervisor_e.numero_meta_movil = numero_meta_movil;
                        _liq_comision_supervisor_e.numero_cumpliento_asesor_movil = (Int32)numero_ejecucion_movil;
                        double porcentaje_cumplimiento_movil = 0;
                        porcentaje_cumplimiento_movil = (double)((double)(numero_ejecucion_movil / numero_meta_movil) * 100);
                        _liq_comision_supervisor_e.homologa_porcentaje_movil = porcentaje_cumplimiento_movil;
                        _liq_comision_supervisor_e.porcentaje_cumplimiento_asesor_movil = porcentaje_cumplimiento_movil + " % ";
                        double peso_cumpliento_movil = _context.liq_cumpliento_peso_v2.Where(x => x.descripcion_producto == "MOVIL"
                                                                                                                 && x.codigo_tipo_esquema == codigo_tipo_esquema
                                                                                                                 && x.estado == 1)
                                                                                                          .Select(x => x.homologa_peso)
                                                                                                          .FirstOrDefault();
                        homologa_peso_movil = porcentaje_cumplimiento_movil * (peso_cumpliento_movil / 100);
                        string peso_movil = "";
                        peso_movil = _context.liq_cumpliento_peso_v2.Where(x => x.descripcion_producto == "MOVIL"
                                                                           && x.codigo_tipo_esquema == codigo_tipo_esquema
                                                                           && x.estado == 1)
                                                                    .Select(x => x.peso)
                                                                    .FirstOrDefault();
                        _liq_comision_supervisor_e.homolog_peso_movil = homologa_peso_movil;
                        _liq_comision_supervisor_e.peso_cumplimiento_movil = homologa_peso_movil + " % " + " : " + peso_movil;
                    }
                    double total_homologa_cumplimiento = 0;
                    total_homologa_cumplimiento = (aux_homologa_porcetaje_peso_ftth_t + homologa_peso_movil + homologa_peso_tv);
                    _liq_comision_supervisor_e.total_homologa_cumplimiento = total_homologa_cumplimiento;
                    _liq_comision_supervisor_e.total_porcentaje_cumplimiento = total_homologa_cumplimiento + " % ";
                    double valor_comision = 0;
                    List<liq_esquema_supervisores> _Comision_Supervisors = new List<liq_esquema_supervisores>();
                    _Comision_Supervisors = _context.liq_esquema_supervisores.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema
                                                                                                      && x.estado == 1).ToList();
                    valor_comision = proceso_comision_supervisor(_Comision_Supervisors, total_homologa_cumplimiento);
                    _liq_comision_supervisor_e.comision = valor_comision;
                    List<liq_super_esquema_acelerador> _liq_super_esquema_acelerador = new List<liq_super_esquema_acelerador>();
                    _liq_super_esquema_acelerador = _context.liq_super_esquema_acelerador.Where(x => x.codigo_tipo_esquema == codigo_tipo_esquema
                                                                                                                    && x.estado == 1).ToList();

                    string valor_factor_mult = "";
                    string aceleracion_desalerelacion = "";
                    double valor_num_factor_mult = 0;

                    //aqui sacamos la homologacion de las cumplimiento
                    double homologa_cumpliemiento_asesores = 0;
                    double numero_asesores_validos = _context.liq_comision_asesor.Where(x => x.cedula_supervisor == cedula_supervisor
                                                                                        && x.periodo == periodo
                                                                                        && x.estado == 1
                                                                                        && x.EsAsesorValido).Count();

                    double numero_asesores_cumplen = _context.liq_comision_asesor.Where(x => x.cedula_supervisor == cedula_supervisor
                                                                                        && x.periodo == periodo
                                                                                        && x.estado == 1
                                                                                        && x.asesor_cumple == 1).Count();

                    homologa_cumpliemiento_asesores = (double)((double)(numero_asesores_cumplen / numero_asesores_validos) * 100);
                    _liq_comision_supervisor_e.numero_asesores_validos = numero_asesores_validos;
                    _liq_comision_supervisor_e.numero_cumplimiento_asesores = numero_asesores_cumplen;
                    _liq_comision_supervisor_e.homologa_cumpliento_asesores = homologa_cumpliemiento_asesores;
                    _liq_comision_supervisor_e.cumplimiento_asesores = homologa_cumpliemiento_asesores + " % ";

                    if (_liq_super_esquema_acelerador.Count > 0)
                    {

                        string[] recibe_esquema_acelerador = proceso_liq_esquema_acelerador(_liq_super_esquema_acelerador, homologa_cumpliemiento_asesores);
                        valor_factor_mult = recibe_esquema_acelerador[0];
                        aceleracion_desalerelacion = recibe_esquema_acelerador[1];


                    }

                    if (!string.IsNullOrEmpty(valor_factor_mult))
                    {
                        if (valor_factor_mult.Contains("-"))
                        {
                            string aux_valor_factor_mult = valor_factor_mult.Substring(1, 4);
                            valor_num_factor_mult = Double.Parse(aux_valor_factor_mult);
                        }

                        valor_num_factor_mult = Double.Parse(valor_factor_mult);
                    }
                    //valor_num_factor_mult = Convert.ToDouble(valor_factor_mult);
                    double descuento_comision = valor_comision * (valor_num_factor_mult/100);
                    double total_comision = valor_comision + (descuento_comision);
                    _liq_comision_supervisor_e.aceleracion_desaceleracion = aceleracion_desalerelacion;
                    _liq_comision_supervisor_e.total_comision = total_comision;
                    _liq_comision_supervisor_e.usuario = usuario;
                    _liq_comision_supervisor_e.estado = 1;
                    _liq_comision_supervisor_e.fecha_creacion = DateTime.Now;
                    _liq_comision_supervisor_e.fecha_modificacion = DateTime.Now;
                    _context.liq_comision_supervisor.Update(_liq_comision_supervisor_e);
                    _context.SaveChanges();
                }
                                                                                       
                

            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "recalcularComisionSupervisor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok();
        }

        [HttpPost("ListarAsesorSupervisoresXPeriodoEsquema")]
        [Authorize]
        public IActionResult ListarAsesorSupervisoresXPeriodoEsquema(dynamic data_recibe)
        {
            List<listar_asesor_super_esq_periodo> _listar_asesor_super_esp_periodo = new List<listar_asesor_super_esq_periodo>();
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                Int32 codigo_tipo_esquema = Convert.ToInt32(datObejt["codigo_tipo_esquema"]);
                string periodo = Convert.ToString(datObejt["periodo"]);
                string cedula_super = Convert.ToString(datObejt["cedula_super"]);
                string query = "select lqa.id, lqa.cedula_asesor, concat (e.nombre,' ', e.snombre, ' ', e.ppellido,' ' ,e.spellido) as asesor," +
                    "  lqa.meta_asesor, lqa.meta_asesor_2, lqa.meta_asesor_3, lqa.cumplimiento_asesor, lqa.tabla_cumplimiento, lqa.nivel, lqa.total_comision," +
                    "  lqa.numero_cant_megas_1, lqa.numero_cant_megas_2, lqa.numero_cant_megas_3, lqa.numero_cant_megas_4, lqa.zona, lqa.EsAsesorValido" +
                    "  from liq_comision_asesor lqa inner join  empleado e on lqa.cedula_asesor = e.cedula_emp" +
                    "  where lqa.periodo = '"+ periodo + "' and lqa.codigo_tipo_escala = "+ codigo_tipo_esquema + " " +
                    "  and lqa.estado = 1 and lqa.cedula_supervisor = '"+ cedula_super + "'";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _listar_asesor_super_esp_periodo.Add(new listar_asesor_super_esq_periodo
                                {
                                    id  = Convert.ToInt64(sdr["id"]),
                                    cedula_asesor = sdr["cedula_asesor"]+"",
                                    asesor = sdr["asesor"]+"",
                                    meta_asesor = Convert.ToInt32(sdr["meta_asesor"]),
                                    meta_asesor_2 = Convert.ToInt32(sdr["meta_asesor_2"]),
                                    meta_asesor_3 = Convert.ToInt32(sdr["meta_asesor_3"]),
                                    cumplimiento_asesor = Convert.ToInt32(sdr["cumplimiento_asesor"]),
                                    tabla_cumplimiento = sdr["tabla_cumplimiento"] + "",
                                    nivel = Convert.ToInt32(sdr["nivel"]),
                                    total_comision = Convert.ToDecimal(sdr["total_comision"]),
                                    numero_cant_megas_1 = Convert.ToInt32(sdr["numero_cant_megas_1"]),
                                    numero_cant_megas_2 = Convert.ToInt32(sdr["numero_cant_megas_2"]),
                                    numero_cant_megas_3 = Convert.ToInt32(sdr["numero_cant_megas_3"]),
                                    numero_cant_megas_4 = Convert.ToInt32(sdr["numero_cant_megas_3"]),
                                    zona  = Convert.ToString(sdr["zona"]),
                                    EsAsesorValido = Convert.ToBoolean(sdr["EsAsesorValido"]) 
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar asesor super esquema peroido", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_listar_asesor_super_esp_periodo);
        }
        [HttpPost("detalleLiquidadorComisionAsesor")]
        [Authorize]
        public IActionResult detalleLiquidadorComisionAsesor(dynamic data_recibe)
        {
            liq_comision_asesor _liq_comision_asesor = new liq_comision_asesor();
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                Int64 id = Convert.ToInt64(datObejt["id"]);
                _liq_comision_asesor = _context.liq_comision_asesor.Where(x => x.estado == 1 && x.id == id).FirstOrDefault(); 
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "detalle liq asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_liq_comision_asesor);
        }

        [HttpPost("detalleLiquidadorComisionSupervisor")]
        [Authorize]
        public IActionResult detalleLiquidadorComisionSupervisor(dynamic data_recibe)
        {
            liq_comision_supervisor _liq_comision_supervisor_e = new liq_comision_supervisor();
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                //Int64 id = Convert.ToInt64(datObejt["id"]);
                string cedula_supervisor = Convert.ToString(datObejt["cedula_supervisor"]);
                string periodo = Convert.ToString(datObejt["periodo"]);
                _liq_comision_supervisor_e = _context.liq_comision_supervisor.Where(x => x.cedula_supervisor == cedula_supervisor 
                                                                                    && x.periodo == periodo).FirstOrDefault();
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "detalle liq supervisor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_liq_comision_supervisor_e);
        }

        [HttpGet("ListarTipoImporte")]
        [Authorize]
        public IActionResult ListarTipoImporte()
        {
            List<liq_importes> _liq_importes = _context.liq_Importes.Where(x => x.estado == 1).OrderBy(x => x.orden_lista).ToList();           
            return Ok(_liq_importes);
        }

        //[HttpPost("listarLiqComision")]

        [HttpPost("ListarLiquidadorSupervisorAsesor")]
        [Authorize]
        public IActionResult ListarLiquidadorSupervisorAsesor(dynamic data_recibe)
        {
            //en este metodo validar con el esquema para traer el excel mas detallado
            var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
            var datObejt = JObject.Parse(data);
            string periodo = Convert.ToString(datObejt["periodo"]);
            string cedula_super = Convert.ToString(datObejt["cedula_supervisor"]);
            var listLiquidador = (from liqCom in _context.liq_comision_asesor
                                 join emplA in _context.empleado on liqCom.cedula_asesor equals emplA.cedula_emp
                                 join emplS in _context.empleado on liqCom.cedula_supervisor equals emplS.cedula_emp
                                 join liqTipEsq in _context.liq_tipo_esquema on liqCom.codigo_tipo_escala equals liqTipEsq.codigo_valor
                                 where liqTipEsq.esImporteMetas == 1
                                       && liqCom.cedula_supervisor == cedula_super
                                       && liqCom.periodo == periodo
                                       && liqCom.estado == 1
                                 select new { nombre_asesor = emplA.nombre +" "+emplA.snombre+" "+emplA.ppellido+" "+emplA.spellido,
                                              cedula_asero = liqCom.cedula_asesor,
                                              nombre_supervisor = emplS.nombre+" "+emplS.snombre+" "+emplS.ppellido+" "+emplS.spellido,
                                              cedula_supervisor = liqCom.cedula_supervisor,
                                              zona = liqCom.zona,
                                              meta_ftth = liqCom.meta_asesor,
                                              asesor_cumple = liqCom.asesor_cumple,
                                              cumplimiento_asesor = liqCom.cumplimiento_asesor,
                                              tabla_cumpliemiento = liqCom.nivel,
                                              esquema = liqTipEsq.nombre_tipo_esquema,
                                              periodo = liqCom.periodo,
                                              mega_1 = liqCom.nombre_mega_1,
                                              cantidad_1 = liqCom.numero_cant_megas_1,
                                              valor_1 = liqCom.valor_mega_1,
                                              total_1 = liqCom.total_valor_mega_1,
                                              mega_2 = liqCom.nombre_mega_2,
                                              cantidad_2 = liqCom.numero_cant_megas_2,
                                              valor_2 = liqCom.valor_mega_2,
                                              total_2 = liqCom.total_valor_mega_2,
                                              mega_3 = liqCom.nombre_mega_3,
                                              cantidad_3 = liqCom.numero_cant_megas_3,
                                              valor_3 = liqCom.valor_mega_3,
                                              total_3 = liqCom.total_valor_mega_3,
                                              mega_4 = liqCom.nombre_mega_4,
                                              cantidad_4 = liqCom.numero_cant_megas_4,
                                              valor_4 = liqCom.valor_mega_4,
                                              total_4 = liqCom.total_valor_mega_4,
                                              mega_5 = liqCom.nombre_mega_5,
                                              cantidad_5 = liqCom.numero_cant_megas_5,
                                              valor_5 = liqCom.valor_mega_5,
                                              total_5 = liqCom.total_valor_mega_5,
                                              mega_6 = liqCom.nombre_mega_6,
                                              cantidad_6 = liqCom.numero_cant_mega_6,
                                              valor_6 = liqCom.valor_mega_6,
                                              total_6 = liqCom.total_valor_mega_6,
                                              numero_duos = liqCom.numero_duos,
                                              valor_duos = liqCom.valor_duos,
                                              total_duos = liqCom.total_valor_duos,
                                              numero_naked = liqCom.numero_naked,
                                              valor_naked = liqCom.valor_naked,
                                              total_naked = liqCom.total_valor_naked,
                                              numero_trios = liqCom.numero_trios,
                                              valor_trios = liqCom.valor_trios,
                                              total_trios = liqCom.total_valor_trios,
                                              numero_migracion = liqCom.numero_migracion,
                                              valor_migracion = liqCom.valor_migracion,
                                              total_migracion = liqCom.total_migracion,
                                              numero_plan_movil = liqCom.numero_plan_movil,
                                              valor_plan_movil = liqCom.valor_plan_movil,
                                              total_plan_movil = liqCom.total_plan_movil,
                                              total_nunca_pago_movil = liqCom.total_nunca_pago_movil,
                                              descripcion_nunca_pago_movil = liqCom.descripcion_nunca_pago_movil != null ? liqCom.descripcion_nunca_pago_movil : "N/a",
                                              total_otros_conceptos = liqCom.total_otros_conceptos,
                                              descripcion_otros_conceptos = liqCom.descripcion_otros_conceptos != null ? liqCom.descripcion_otros_conceptos : "N/a",
                                              sub_total_comision = liqCom.sub_total_comision,
                                              
                                              descripcion_nunca_pagos = liqCom.descripcion_nunca_pago != null ? liqCom.descripcion_nunca_pago : "N/a",
                                              total_nunca_pago = liqCom.total_nunca_pago,
                                              total_comision = liqCom.total_comision
                                              }).ToList(); 

            return Ok(listLiquidador);
        }

        [HttpPost("ListarLiqComisionAsesorSupervisor")]
        [Authorize]
        public IActionResult ListarLiqComisionAsesorSupervisor(dynamic data_recibe)
        {
            List<listar_liq_comision_asesor_sup> _listar_liq_comision_asesor_sup = new List<listar_liq_comision_asesor_sup>();
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                string periodo = Convert.ToString(datObejt["periodo"]);
                string cedula_super = Convert.ToString(datObejt["cedula_super"]);
                string query = "select *,(select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido) " +
                               " from empleado where cedula_emp = cedula_asesor) as nombreAsesor, " +
                               " (select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido) from empleado " +
                               " where cedula_emp = cedula_supervisor) as nombreSupervisor " +
                               " from liq_comision_asesor where cedula_supervisor = '"+ cedula_super + "' and periodo = '"+ periodo + "'";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _listar_liq_comision_asesor_sup.Add(new listar_liq_comision_asesor_sup
                                {
                                    id = Convert.ToInt64(sdr["id"]),
                                    mes_comision = Convert.ToInt32(sdr["mes_comision"]),
                                    codigo_tipo_escala = Convert.ToInt32(sdr["codigo_tipo_escala"]),
                                    cedula_asesor = sdr["cedula_asesor"]+"",
                                    cedula_supervisor = sdr["cedula_supervisor"] + "",
                                    meta_asesor = Convert.ToInt32(sdr["meta_asesor"]),
                                    cumplimiento_asesor = Convert.ToInt32(sdr["cumplimiento_asesor"]),
                                    tabla_cumplimiento = sdr["tabla_cumplimiento"] + "",
                                    nivel = Convert.ToInt32(sdr["nivel"]),
                                    numero_cant_megas_1 = Convert.ToInt32(sdr["numero_cant_megas_1"]),
                                    numero_cant_megas_2 = Convert.ToInt32(sdr["numero_cant_megas_2"]),
                                    numero_cant_megas_3 = Convert.ToInt32(sdr["numero_cant_megas_3"]),
                                    numero_cant_megas_4 = Convert.ToInt32(sdr["numero_cant_megas_4"]),
                                    numero_duos = Convert.ToInt32(sdr["numero_duos"]),
                                    numero_naked = Convert.ToInt32(sdr["numero_naked"]),
                                    sub_total_comision = Convert.ToDouble(sdr["sub_total_comision"]),
                                    numero_migracion = Convert.ToInt32(sdr["numero_migracion"]),
                                    total_migracion = Convert.ToDouble(sdr["total_migracion"]),
                                    numero_plan_movil = Convert.ToInt32(sdr["numero_plan_movil"]),
                                    total_plan_movil = Convert.ToDouble(sdr["total_plan_movil"]),
                                    ajustes = Convert.ToDouble(sdr["ajustes"]),
                                    descripcion_nunca_pago = sdr["descripcion_nunca_pago"] + "",
                                    total_nunca_pago = Convert.ToDouble(sdr["total_nunca_pago"]),
                                    total_comision = Convert.ToDouble(sdr["total_comision"]),
                                    meta_asesor_2 = Convert.ToInt32(sdr["meta_asesor_2"]),
                                    periodo = sdr["periodo"] + "",
                                    valor_mega_1 = Convert.ToDouble(sdr["valor_mega_1"]),
                                    valor_mega_2 = Convert.ToDouble(sdr["valor_mega_2"]),
                                    valor_mega_3 = Convert.ToDouble(sdr["valor_mega_3"]),
                                    valor_mega_4 = Convert.ToDouble(sdr["valor_mega_4"]),
                                    total_valor_mega_1 = Convert.ToDouble(sdr["total_valor_mega_1"]),
                                    total_valor_mega_2 = Convert.ToDouble(sdr["total_valor_mega_2"]),
                                    total_valor_mega_3 = Convert.ToDouble(sdr["total_valor_mega_3"]),
                                    total_valor_mega_4 = Convert.ToDouble(sdr["total_valor_mega_4"]),
                                    numero_trios = Convert.ToInt32(sdr["numero_trios"]),
                                    valor_duos = Convert.ToDouble(sdr["valor_duos"]),
                                    total_valor_duos = Convert.ToDouble(sdr["total_valor_duos"]),
                                    valor_trios = Convert.ToDouble(sdr["valor_trios"]),
                                    total_valor_trios = Convert.ToDouble(sdr["total_valor_trios"]),
                                    valor_naked = Convert.ToDouble(sdr["total_valor_trios"]),
                                    total_valor_naked = Convert.ToDouble(sdr["total_valor_naked"]),
                                    zona = sdr["zona"] + "",
                                    nombreAsesor = sdr["nombreAsesor"] + "",
                                    nombreSupervisor = sdr["nombreSupervisor"] + ""
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception e ) 
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "detalle lista liq asesor super", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_listar_liq_comision_asesor_sup);
        }

        //[HttpPut("actualizar")]

        [HttpPut("actualizarLiqComisionAsesor/{id}")]
        [Authorize]
        public IActionResult actualizarLiqComisionAsesor(int id, [FromBody] liq_comision_asesor _liq_comision_asesor_e)
        {
            string mensaje = "";
            try
            {
                if(id == _liq_comision_asesor_e.id)
                {
                    _liq_comision_asesor_e.fecha_modificacion = DateTime.Now;
                    _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                    int rs = _context.SaveChanges();
                    if(rs > 0)
                    {
                        mensaje = "ACTUALIZADO DE FORMA CORRECTA";
                    }
                }
            }
            catch(Exception e)
            {
                //log de errores s
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Actualizar liq_comision", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }

        [HttpGet("listarArchivosLiquidador")]
        [Authorize]
        public IActionResult listarArchivosLiquidador()
        {
            List<data_archivos> _data_archivos = _context.data_Archivos.Where(x => x.estado == 1).ToList();
            return Ok(_data_archivos);
        }

        [HttpGet("DescargarArchivoExcel/{id}")]
        [Authorize]
        public async Task<IActionResult> DescargarArchivoExcel(int id)
        {
            General.crearImprimeMensajeLog("Entro a la funcion", "procesarExcelCartaMetasAsesor", _config.GetConnectionString("conexionDbPruebas"));
            data_archivos _data_archivos = _context.data_Archivos.Where(x => x.id == id && x.estado == 1).FirstOrDefault();
            string ruta = "";
            if (_data_archivos != null)
            {
                ruta = _data_archivos.ruta;
                //string direcorty = Directory.GetCurrentDirectory();
                
            }
            var workingDirectory = Environment.CurrentDirectory;
            var file = $"{workingDirectory}\\{ruta}";
            //var path = Path.Combine(Directory.GetCurrentDirectory(), ruta);
            if (!System.IO.File.Exists(file))
                return NotFound();

            Byte[] bytes = await System.IO.File.ReadAllBytesAsync(file);


            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
        }

        [HttpPost("EnviarTokenAutorizacion")]
        [Authorize]
        public ActionResult EnviarTokenAutorizacion(dynamic data_recibe)
        {
            string mensaje = "";
            string cuerpoMensaje = "";
            string asunto = "";
            string usuario_ = "";
            string proceso_ = "";
            string correo_saliente = "";
            Int32 myRandomNo = 0;
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                usuario_ = Convert.ToString(datObject["usuario"]);
                proceso_ = Convert.ToString(datObject["proceso"]);
                correo_saliente = Convert.ToString(datObject["correo"]);
                Random rnd = new Random();
                myRandomNo = Convert.ToInt32(rnd.Next(10000000, 99999999));
                //validamos que el random no este en base  datos
                token_autoriza _token_autoriza_e = _context.token_autoriza.Where(x => x.token == myRandomNo
                                                                                 && x.estado == 1).FirstOrDefault();

                if(_token_autoriza_e == null)
                {
                    System.Boolean EnviaCorreo = true;
                    cuerpoMensaje = "ENVIO TOKEN DE AUTORIOZACION : \n" + myRandomNo;
                    asunto = "TOKEN DE AUTORIOZACION";
                   
                    EnviaCorreo = General.enviarCorreo(asunto, cuerpoMensaje, correo_saliente, _config.GetConnectionString("conexionDbPruebas"));
                    if (EnviaCorreo && myRandomNo > 0)
                    {
                        token_autoriza token_Autoriza_e = new token_autoriza();
                        token_Autoriza_e.token = myRandomNo;
                        token_Autoriza_e.proceso = proceso_;
                        token_Autoriza_e.usuario = usuario_;
                        token_Autoriza_e.fecha_creacion = DateTime.Now;
                        token_Autoriza_e.fecha_expira = DateTime.Now;
                        token_Autoriza_e.estado = 1;
                        _context.token_autoriza.Add(token_Autoriza_e);
                        _context.SaveChanges();

                        //Reviso el token activo por el tipo Proceoso
                        List<token_autoriza> _token_autoriza = _context.token_autoriza.Where(x => x.proceso == proceso_
                                                                                             && x.estado == 1).ToList();
                        if(_token_autoriza.Count > 1)
                        {
                            Int64 max = _token_autoriza.Max(y => y.Id);
                            foreach (var item in _token_autoriza.Select((value, i) => (value, i)).OrderBy(x => x.value.Id))
                            {
                                //escojo el maximo 
                                
                                if(max != item.value.Id)
                                {
                                    item.value.estado = 0;
                                    item.value.fecha_expira = DateTime.Now;
                                    _context.token_autoriza.Update(item.value);
                                    _context.SaveChanges();
                                }
                               
                            }
                        }
                        mensaje = "TOKEN ENVIADO AL CORREO";
                    }
                    else
                    {
                        mensaje = "ERROR AL ENVIAR EL CORREO ";
                    }
                    
                }
                else
                {
                    mensaje = "BEDE GENERAR NUEVAMENTE";
                }
            }
            catch(Exception e) { }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }
        [HttpPost("ValidarTokenAutorizacion")]
        [Authorize]
        public IActionResult ValidarTokenAutorizacion(dynamic data_recibe)
        {
            string mensaje = "";
            string usuario_ = "";
            string proceso_ = "";
            string prop2 = "";
            string prop3 = "";
            Int32 token = 0;
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                usuario_ = Convert.ToString(datObject["usuario"]);
                //proceso_ = Convert.ToString(datObject["proceso"]);
                token = Convert.ToInt32(datObject["token"]);
                proceso_ = Convert.ToString(datObject["prop1"]);
                prop2 = Convert.ToString(datObject["prop2"]);
                prop3 = Convert.ToString(datObject["prop3"]);

                //validamos el token para inactivarlo
                token_autoriza _token_autoriza_e = _context.token_autoriza.Where(x => x.token == token
                                                                                 && x.proceso == proceso_
                                                                                 && x.usuario == usuario_
                                                                                 && x.estado == 1).FirstOrDefault();
                if (_token_autoriza_e != null)
                {
                    _token_autoriza_e.estado = 0;
                    _token_autoriza_e.fecha_expira = DateTime.Now;

                    _context.token_autoriza.Update(_token_autoriza_e);
                    int rs =  _context.SaveChanges();
                    if(rs > 0)
                    {
                        switch (proceso_)
                        {
                            case "CerrarPeriodo":
                                liq_periodo_comision_v2 _liq_periodo_com_v2_e = new liq_periodo_comision_v2();
                                _liq_periodo_com_v2_e = _context.liq_periodo_comision_v2.Where(x => x.periodo == prop2).FirstOrDefault();
                                if(_liq_periodo_com_v2_e != null)
                                {
                                    int valueInt = Convert.ToInt32(prop3);
                                    System.Boolean estado = Convert.ToBoolean(valueInt);
                                    //Int32 setEstado = 0;
                                    if (estado)
                                    {
                                        //setEstado = 1;
                                        _liq_periodo_com_v2_e.estado = 0;
                                    }
                                    else
                                    {
                                        //setEstado = 0;
                                        _liq_periodo_com_v2_e.estado = 1;
                                    }
                                    _context.liq_periodo_comision_v2.Update(_liq_periodo_com_v2_e);
                                    int rs2 = _context.SaveChanges();
                                    if (rs2 > 0)
                                    {
                                        mensaje = "PERIODO CERRADO DE FORMA CORRECTA";
                                    }
                                }
                                
                                //liq_periodo_comision_v2 _liq_periodo_comision_v2_e = _
                                break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Inactivar Token", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }
        [HttpPost("validarEstadoPeriodo")]
        [Authorize]
        public IActionResult validarEstadoPeriodo(dynamic data_recibe)
        {
            Int32 EstadoPerido = 0;
            string periodo = "";
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                periodo = Convert.ToString(datObject["periodo"]);
                liq_periodo_comision_v2 _liq_periodo_comision_v2_e = _context.liq_periodo_comision_v2.Where(x => x.periodo == periodo).FirstOrDefault();
                if (_liq_periodo_comision_v2_e != null)
                {
                    EstadoPerido = _liq_periodo_comision_v2_e.estado;
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "validarEstadoPeriodo", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(EstadoPerido);
            return Ok(json);
        }

        [HttpPost("ListarComisionSupervisorV2")]
        [Authorize]
        public IActionResult ListarComisionSupervisorV2(dynamic data_recibe)
        {
            var listarComisionSupervisores = new object();
            string periodo = "";
            int codigo_tipo_esquema = 0;
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                periodo = Convert.ToString(datObejt["periodo"]);
                codigo_tipo_esquema = Convert.ToInt32(datObejt["codigo_tipo_esquema"]);
                listarComisionSupervisores = (from liqComS in _context.liq_comision_supervisor
                                              join emplS in _context.empleado on liqComS.cedula_supervisor equals emplS.cedula_emp
                                              join liqTipEsq in _context.liq_tipo_esquema on liqComS.codigo_tipo_esquema equals liqTipEsq.codigo_valor
                                              where liqComS.codigo_tipo_esquema == codigo_tipo_esquema
                                                    && liqComS.periodo == periodo
                                                    && liqComS.estado == 1
                                              select new 
                                              {   nombre_supervisor = emplS.nombre + " " + emplS.snombre + " " + emplS.ppellido + " " + emplS.spellido,
                                                  cedula_supervisor = liqComS.cedula_supervisor,
                                                  mes_comision  = liqComS.mes_comision,
                                                  periodo = liqComS.periodo,
                                                  codigo_tipo_esquema = liqComS.codigo_tipo_esquema,
                                                  nombre_esquema = liqTipEsq.nombre_tipo_esquema,
                                                  numero_meta_ftth = liqComS.numero_meta_ftth,
                                                  
                                                  numero_cumplimiento_asesor_ftth = liqComS.numero_cumplimiento_asesor_ftth,
                                                  porcentaje_cumplimiento_asesor_ftth = liqComS.porcentaje_cumplimiento_asesor_ftth,
                                                  homologa_porcentaje_ftth = Math.Round(liqComS.homologa_porcentaje_ftth,2),
                                                  peso_cumpliento_ftth = liqComS.peso_cumpliento_ftth,
                                                  homologa_peso_ftth = Math.Round(liqComS.homologa_peso_ftth,2),
                                                  numero_meta_movil = liqComS.numero_meta_movil,
                                                  numero_cumpliento_asesor_movil = liqComS.numero_cumpliento_asesor_movil,
                                                  porcentaje_cumplimiento_asesor_movil = liqComS.porcentaje_cumplimiento_asesor_movil,
                                                  homologa_porcentaje_movil = Math.Round(liqComS.homologa_porcentaje_movil,2),
                                                  peso_cumplimiento_movil = liqComS.peso_cumplimiento_movil,
                                                  homolog_peso_movil = Math.Round(liqComS.homolog_peso_movil,2),

                                                  total_porcentaje_cumplimiento = liqComS.total_porcentaje_cumplimiento,
                                                  total_homologa_cumplimiento = Math.Round(liqComS.total_homologa_cumplimiento, 2),

                                                  comision = liqComS.comision,
                                                  numero_asesores_valido = liqComS.numero_asesores_validos,
                                                  numero_asesores_cumplen = liqComS.numero_cumplimiento_asesores,
                                                  cumplimiento_asesores = liqComS.cumplimiento_asesores,
                                                  aceleracion_desaceleracion = liqComS.aceleracion_desaceleracion,

                                                  //factor_acelearion_desaceleracion = liqComS.factor_acelearion_desaceleracion,
                                                  //homologa_factor_aceleracion_desaceleracion = Math.Round(liqComS.homologa_factor_aceleracion_desaceleracion, 2),

                                                  total_comision = liqComS.total_comision,
                                                  estado = liqComS.estado,
                                                  usuario = liqComS.usuario,
                                                  fecha_creacion = liqComS.fecha_creacion,
                                                  fecha_modificacion = liqComS.fecha_modificacion,
                                                  
                                                  
                                              }).ToList();
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "ListarComisionSupervisorV2", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(listarComisionSupervisores);
        }



        #endregion

        //METODOS
        #region metodos ftth pap
        public async Task<string> procesar_base_cierre_carta_meta_pap(string perido, int tipo_esquema, int TipoProceso)
        {
            string mensaje = "";
            try
            {
                var distictSuperPeriodoEsquema = _context.liq_tmp_metas.Select(x => new { x.cedula_supervisor, 
                                                                                          x.periodo_importe, 
                                                                                          x.cod_tipo_escala, 
                                                                                          x.estado })
                                                                        .Distinct().Where(y => y.periodo_importe == perido 
                                                                                           && y.cod_tipo_escala == tipo_esquema
                                                                                           && y.estado == 1).ToList();

                foreach(var i in distictSuperPeriodoEsquema)
                {
                    List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
                    _carta_metas = await _context.liq_tmp_metas.Where(x => x.periodo_importe == i.periodo_importe
                                                                && x.cod_tipo_escala == i.cod_tipo_escala
                                                                && x.cedula_supervisor == i.cedula_supervisor
                                                                && x.estado == 1).ToListAsync();
                    int tam_carta_metas = _carta_metas.Count();
                    General.crearImprimeMensajeLog("El tamaño de la lista es : " + tam_carta_metas, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));
                    //se establece el liq_pap
                    List<listar_pap_resumida> _listar_pap_resumidas_ = listar_pap_resumidas();
                    List<liq_pap> _liq_pap_l = new List<liq_pap>();
                    Int32 valor_mega1 = 0;
                    Int32 valor_mega2 = 0;
                    Int32 valor_mega3 = 0;
                    Int32 valor_mega4 = 0;
                    if (tipo_esquema == 1)
                    {
                        _liq_pap_l = _context.liq_pap.Where(x => x.codigo_liq_esq == 1).ToList();
                        valor_mega1 = ObtnerVelocidadMega(1);
                        valor_mega2 = ObtnerVelocidadMega(2);
                        valor_mega3 = ObtnerVelocidadMega(3);
                        valor_mega4 = ObtnerVelocidadMega(4);
                    }
                    else if (tipo_esquema == 5)
                    {
                        _liq_pap_l = _context.liq_pap.Where(x => x.codigo_liq_esq == 5).ToList();
                        valor_mega1 = ObtnerVelocidadMega(19);
                        valor_mega2 = ObtnerVelocidadMega(20);
                        valor_mega3 = ObtnerVelocidadMega(21);
                        valor_mega4 = ObtnerVelocidadMega(22);
                    }


                    List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_escala_altas == tipo_esquema).ToList();
                    List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                    List<liq_empaqhome> _liq_empaqhome_ = _context.liq_empaqhome.Where(x => x.cod_tipo_esquema == tipo_esquema).ToList();
                    if (_carta_metas.Count > 0)
                    {
                        foreach (liq_tmp_metas item in _carta_metas)
                        {
                            
                            Int32 sumasUnidades = sumarTotalUnidadesAsesor(item.cedula_asesor, item.cedula_supervisor, item.periodo_importe, TipoProceso, item.cod_tipo_escala);
                          
                            if (sumasUnidades > 0  && item.numero_carta_meta_ftth > 0)
                            {
                                //string 
                                //string data_mensaje_es_2 = "la suma del  El asesor es : " + item.cedula_asesor + " es : "+ sumasUnidades+" y el supervisor es  : " + item.cedula_supervisor + " periodo es : " + perido + " y el tipo proceso : " + TipoProceso + " ";
                                //General.crearImprimeMensajeLog(data_mensaje_es_2, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));
                                string cedula_asesor = "";
                                string cedula_supervisor = "";
                                Int32 codigo_tipo_esquema = 0;
                                cedula_asesor = item.cedula_asesor;
                                cedula_supervisor = item.cedula_supervisor;
                                codigo_tipo_esquema = item.cod_tipo_escala;
                                liq_comision_asesor _liq_comision_asesor_e = await _context.liq_comision_asesor.Where(x => x.cedula_asesor == cedula_asesor
                                                                                                              && x.cedula_supervisor == cedula_supervisor
                                                                                                              && x.periodo == perido
                                                                                                              && x.codigo_tipo_escala == codigo_tipo_esquema
                                                                                                              && x.estado == 1).FirstOrDefaultAsync();



                                int rango_altas = 0;
                                liq_escala_altas _Escala_Altas = _context.liq_escala_altas.Where(x => x.numero_escala_alta == sumasUnidades).FirstOrDefault();
                                if (_Escala_Altas != null)
                                {
                                    rango_altas = _Escala_Altas.rango_altas;
                                }
                                else
                                {
                                    int max_numero_escala_alta = _context.liq_escala_altas.Select(x => x.numero_escala_alta).Max();
                                    if (sumasUnidades > max_numero_escala_alta)
                                    {
                                        rango_altas = _context.liq_escala_altas.Select(x => x.rango_altas).Max();
                                    }
                                }
                                if (_liq_comision_asesor_e != null)
                                {
                                    string data_mensaje_es = "El asesor es : " + _liq_comision_asesor_e.cedula_asesor + " y el supervisor es  : " + _liq_comision_asesor_e.cedula_supervisor + " periodo es : " + perido + " y el tipo proceso : " + TipoProceso + " ";
                                    General.crearImprimeMensajeLog(data_mensaje_es, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));
                                    double porcentaje_asesor_e = 0;
                                    double porcentaje_aux_e = 0;
                                    int meta_asesor_e = _liq_comision_asesor_e.meta_asesor;
                                    porcentaje_aux_e = (double)sumasUnidades / meta_asesor_e;
                                    porcentaje_asesor_e = (porcentaje_aux_e * 100);
                                    Int32 portecentaje_asesor_e_i = Convert.ToInt32(porcentaje_asesor_e);
                                    _liq_comision_asesor_e.cumplimiento_asesor = portecentaje_asesor_e_i;
                                    //string auxmensaje = "";
                                    Int32 tabla_cumplimiento = ObtnerNivelCumplimientoPap(_listar_pap_resumidas_, portecentaje_asesor_e_i);
                                    //mensaje = auxmensaje;
                                    //aqui validar si el cumplimiento de las unidades es mayor a la meta

                                    if (sumasUnidades >= _liq_comision_asesor_e.meta_asesor)
                                    {
                                        _liq_comision_asesor_e.asesor_cumple = 1;
                                    }
                                    else
                                    {
                                        _liq_comision_asesor_e.asesor_cumple = 0;
                                    }
                                    //traemos por group by la tabla  para el comparativo
                                    _liq_comision_asesor_e.nivel = tabla_cumplimiento;
                                    Int32 cantidad_velocidad_1 = 0;
                                    Int32 cantidad_velocidad_2 = 0;
                                    Int32 cantidad_velocidad_3 = 0;
                                    Int32 cantidad_velocidad_4 = 0;

                                    int[] arr_cantidades_velocidad = new int[4];
                                    arr_cantidades_velocidad = await calcularCantidadesMegasPap(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala,
                                                               valor_mega1, valor_mega2, valor_mega3, valor_mega4);
                                    cantidad_velocidad_1 = arr_cantidades_velocidad[0];
                                    cantidad_velocidad_2 = arr_cantidades_velocidad[1];
                                    cantidad_velocidad_3 = arr_cantidades_velocidad[2];
                                    cantidad_velocidad_4 = arr_cantidades_velocidad[3];
                                    _liq_comision_asesor_e.numero_cant_megas_1 = cantidad_velocidad_1;
                                    _liq_comision_asesor_e.numero_cant_megas_2 = cantidad_velocidad_2;
                                    _liq_comision_asesor_e.numero_cant_megas_3 = cantidad_velocidad_3;
                                    _liq_comision_asesor_e.numero_cant_megas_4 = cantidad_velocidad_4;

                                    //setiar los valores mega

                                    _liq_comision_asesor_e.nombre_mega_1 = valor_mega1 + "";
                                    _liq_comision_asesor_e.nombre_mega_2 = valor_mega2 + "";
                                    _liq_comision_asesor_e.nombre_mega_3 = valor_mega3 + "";
                                    _liq_comision_asesor_e.nombre_mega_4 = valor_mega4 + "";

                                    double valor_mega_1 = 0;
                                    double valor_mega_2 = 0;
                                    double valor_mega_3 = 0;
                                    double valor_mega_4 = 0;

                                    //aqui
                                    double[] arr_valores_megas = new double[4];
                                    arr_valores_megas = await calcularValorMegasPap(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala,
                                                          tabla_cumplimiento, sumasUnidades, valor_mega1, valor_mega2, valor_mega3, valor_mega4,item.tipo_liquidador);
                                    valor_mega_1 = arr_valores_megas[0];
                                    valor_mega_2 = arr_valores_megas[1];
                                    valor_mega_3 = arr_valores_megas[2];
                                    valor_mega_4 = arr_valores_megas[3];
                                    if (valor_mega1 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_1 = valor_mega_1;
                                        _liq_comision_asesor_e.total_valor_mega_1 = valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1;

                                    }
                                    if (valor_mega_2 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_2 = valor_mega_2;
                                        _liq_comision_asesor_e.total_valor_mega_2 = valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2;
                                    }
                                    if (valor_mega3 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_3 = valor_mega_3;
                                        _liq_comision_asesor_e.total_valor_mega_3 = valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3;
                                    }
                                    if (valor_mega4 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_4 = valor_mega_4;
                                        _liq_comision_asesor_e.total_valor_mega_4 = valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4;
                                    }


                                    double subTotalValorMegas = ((valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1) +
                                                                 (valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2) +
                                                                 (valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3) +
                                                                 (valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4));
                                    Int32 total_naked = 0;
                                    total_naked = await sumarTotalNaked(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala, TipoProceso);
                                    _liq_comision_asesor_e.numero_naked = total_naked;
                                    double valor_naked = 0;
                                    valor_naked = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 3, _liq_empaqhome_);
                                    double subTotalNaked = 0;
                                    if (valor_naked > 0)
                                    {
                                        _liq_comision_asesor_e.valor_naked = valor_naked;
                                        subTotalNaked = (total_naked * valor_naked);
                                        _liq_comision_asesor_e.total_valor_naked = subTotalNaked;
                                    }


                                    Int32 total_duos = 0;
                                    total_duos = await sumarTotalDuos(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala, TipoProceso);
                                    _liq_comision_asesor_e.numero_duos = total_duos;
                                    double valor_duos = 0;
                                    valor_duos = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 1, _liq_empaqhome_);
                                    double subTotalDuos = 0;
                                    if (valor_duos > 0)
                                    {
                                        _liq_comision_asesor_e.valor_duos = valor_duos;
                                        subTotalDuos = (total_duos * valor_duos);
                                        _liq_comision_asesor_e.total_valor_duos = subTotalDuos;
                                    }

                                    //aqui calcular los naked para los pap
                                    // y los duos los pap
                                    // en el esquema de los duos trios y naked se evalua por el cumplimiento de las cantidades ftth??

                                    _liq_comision_asesor_e.sub_total_comision = subTotalValorMegas + subTotalNaked + subTotalDuos;
                                    _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.sub_total_comision;


                                    _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                    int sa = await _context.SaveChangesAsync();
                                    if (sa > 0)
                                    {
                                        (from lq_ in _context.liq_tmp_base_cierre
                                         where lq_.velocidad > 0 && lq_.cedula_asesor == item.cedula_asesor
                                         && lq_.cedula_supervisor == item.cedula_supervisor
                                         && lq_.periodo == perido && lq_.estado == 1
                                         && lq_.EsProcesado == 0
                                         select lq_).ToList()
                                        .ForEach(x => x.EsProcesado = 1);
                                        _context.SaveChanges();

                                    }
                                    General.recalcular_subtotales(cedula_asesor, perido, _config.GetConnectionString("conexionDbPruebas"));
                                }
                            }
                        }
                    }
                }
     
            }
            catch (Exception e)
            {
                //log de errores s
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "procesar_base_cierre_carta_meta_pap", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            //mensaje = "";
            
            return mensaje;
        }

        

        public void procesar_base_cierre_alta_migracion(string perido, Int32 tipoEsquema)
        {
            //List<liq_pap> _liq_pap_l = _context.liq_pap.Where(x => x.codigo_liq_esq == tipoEsquema).ToList();
            //Int32 valor_mega1 = ObtnerVelocidadMega(1);
            //Int32 valor_mega2 = ObtnerVelocidadMega(2);
            //Int32 valor_mega3 = ObtnerVelocidadMega(3);
            //Int32 valor_mega4 = ObtnerVelocidadMega(4);
            //List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_escala_altas == tipoEsquema).ToList();
            //List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == tipoEsquema).ToList();
            //List<liq_pap> _liq_pap = _context.liq_pap.Where(x => x.codigo_liq_esq == tipoEsquema
            //                                                && x.estado == 1).ToList();
            

            List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
            _carta_metas = _context.liq_tmp_metas.Where(x => x.periodo_importe == perido
                                                        && x.cod_tipo_escala == tipoEsquema
                                                        && x.estado == 1).ToList();
            if(_carta_metas.Count() > 0)
            {
                foreach (liq_tmp_metas item in _carta_metas)
                {
                    //validamos en este procedimiento que el cumplimiento sea mayor a 80
                    liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                                                    && x.cedula_supervisor == item.cedula_supervisor
                                                                                                    && x.periodo == perido
                                                                                                    && x.codigo_tipo_escala == tipoEsquema
                                                                                                    && x.estado == 1).FirstOrDefault();
                    if(_liq_comision_asesor_e != null)
                    {
                        if(_liq_comision_asesor_e.cumplimiento_asesor >= 80)
                        {
                            Int32 totalMigracionAltas = 0;
                            totalMigracionAltas = calcularTotalAltasMigracion(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala);
                            _liq_comision_asesor_e.numero_migracion = totalMigracionAltas;
                            Int32 total_ftth = (_liq_comision_asesor_e.numero_cant_megas_1 + 
                                                _liq_comision_asesor_e.numero_cant_megas_2 + 
                                                _liq_comision_asesor_e.numero_cant_megas_3 + 
                                                _liq_comision_asesor_e.numero_cant_megas_4 +
                                                _liq_comision_asesor_e.numero_cant_megas_5);
                            //valido el total de las migraciones
                            double[] arr_total_migracion = new double[2];
                            arr_total_migracion = calcularValorMigraciones(item.cedula_asesor,
                                                                                  item.cedula_supervisor,
                                                                                  perido,
                                                                                  item.cod_tipo_escala,
                                                                                  _liq_comision_asesor_e.nivel,
                                                                                  total_ftth);
                            _liq_comision_asesor_e.valor_migracion = arr_total_migracion[0];
                            _liq_comision_asesor_e.total_migracion = arr_total_migracion[1];
                            _liq_comision_asesor_e.sub_total_comision = (_liq_comision_asesor_e.sub_total_comision + arr_total_migracion[1]);
                            _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                            int sa = _context.SaveChanges();
                            if (sa > 0)
                            {
                                (from lq_ in _context.liq_tmp_base_cierre
                                 where lq_.unidad == 0 && lq_.cedula_asesor == item.cedula_asesor
                                 && lq_.cedula_supervisor == item.cedula_supervisor
                                 && lq_.periodo == perido && lq_.estado == 1
                                 && lq_.EsProcesado == 0
                                 select lq_).ToList()
                                .ForEach(x => x.EsProcesado = 1);
                                _context.SaveChanges();

                            }
                            General.recalcular_subtotales(item.cedula_asesor, perido, _config.GetConnectionString("conexionDbPruebas"));
                        }
                    }
                }
            }

        }

       
        public async Task<double[]> calcularValorMegasPap(string cedula_asesor,
                                          string cedula_supervisor,
                                          string periodo,
                                          Int32 tipo_esquema,
                                          Int32 nivel_cumplimiento,
                                          Int32 cantidadFtth,
                                          Int32 valor_velocidad1,
                                          Int32 valor_velocidad2,
                                          Int32 valor_velocidad3,
                                          Int32 valor_velocidad4,
                                          Int32 tipo_liquidador
                                         )

        {
            
            double[] arr_valores_mega = new double[4];
            double valor_mega_1 = 0;
            double valor_mega_2 = 0;
            double valor_mega_3 = 0;
            double valor_mega_4 = 0;
            try
            {
                List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                //validamos sin el supervisor
                string query = "select velocidad, isnull(sum(unidad),0) as unidades " +
                                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                                " and periodo = '" + periodo + "' and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                                " and velocidad > 0  group by velocidad order by velocidad ";
                //string query = "select velocidad, sum(unidad) as unidades " +
                //                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                //                " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                //                " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                //                " and velocidad > 0  group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                Int32 velocidad = Convert.ToInt32(sdr["velocidad"]);
                                //Int32 unidades_escala_altas = Convert.ToInt32(sdr["unidades"]);
                                //liq_valores_megabytes _liq_valores_mega_ = _context.liq_valores_megabytes.Where(x => x.valor_mega == velocidad).FirstOrDefault();
                                if (_liq_valores_megas_ != null && _liq_escalas_altas != null)
                                {
                                    //Int32 codigo_valor = _liq_valores_mega_.codigo_valor;
                                    Int32 codigo_valor_mega = 0;
                                    if (velocidad > 0)
                                    {
                                        var val_valor_mega = _liq_valores_megas_.Find(x => x.valor_mega == velocidad);
                                        if (val_valor_mega != null)
                                        {
                                            codigo_valor_mega = val_valor_mega.codigo_valor;
                                        }
                                        else
                                        {
                                            Int32 max_velocidad = _liq_valores_megas_.Select(x => x.valor_mega).Max();
                                            if (velocidad > max_velocidad)
                                            {
                                                codigo_valor_mega = _liq_valores_megas_.Select(x => x.codigo_valor).Max();
                                            }

                                        }
                                    }

                                    Int32 rango_altas = 0;
                                    if (cantidadFtth > 0)
                                    {
                                        var val_rango_altas = _liq_escalas_altas.Find(x => x.numero_escala_alta == cantidadFtth);
                                        if (val_rango_altas != null)
                                        {
                                            rango_altas = val_rango_altas.rango_altas;
                                        }
                                        else
                                        {
                                            Int32 max_rango_altas = _liq_escalas_altas.Select(x => x.numero_escala_alta).Max();
                                            if (cantidadFtth > max_rango_altas)
                                            {
                                                rango_altas = _liq_escalas_altas.Select(x => x.rango_altas).Max();
                                                //rango_altas = _liq_escalas_altas.Select(x => x.rango_altas).Max();
                                            }

                                        }
                                    }
                                    //codigo valor mega y el rango altas se procede a buscar el valor que corresponda por linq 
                                    liq_pap _liq_pap_ = _context.liq_pap.Where(x => x.nivel_cumplimiento == nivel_cumplimiento
                                                                               && x.valor_nivel == rango_altas
                                                                               && x.valor_mega == codigo_valor_mega
                                                                               && x.codigo_liq_esq == tipo_esquema
                                                                               && x.tipo_liquidador == tipo_liquidador
                                                                               && x.estado == 1).FirstOrDefault();
                                    if (_liq_pap_ != null)
                                    {
                                        if (velocidad == valor_velocidad1)
                                        {
                                            valor_mega_1 = _liq_pap_.valor;
                                            arr_valores_mega[0] = valor_mega_1;
                                        }
                                        else if (velocidad == valor_velocidad2)
                                        {
                                            valor_mega_2 = _liq_pap_.valor;
                                            arr_valores_mega[1] = valor_mega_2;
                                        }
                                        else if (velocidad == valor_velocidad3)
                                        {
                                            valor_mega_3 = _liq_pap_.valor;
                                            arr_valores_mega[2] = valor_mega_3;
                                        }
                                        else if (velocidad >= valor_velocidad4)
                                        {
                                            valor_mega_4 = _liq_pap_.valor;
                                            arr_valores_mega[3] = valor_mega_4;
                                        }
                                    }
                                }


                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "valores mega Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
                
            }
            return arr_valores_mega;
        }
        public async Task<int[]> calcularCantidadesMegasPap(string cedula_asesor,
                                                      string cedula_supervisor,
                                                      string periodo,
                                                      Int32 tipo_esquema,
                                                      Int32 valor_velocidad1,
                                                      Int32 valor_velocidad2,
                                                      Int32 valor_velocidad3,
                                                      Int32 valor_velocidad4
                                                      
                                                      )
        {
            
            int[] array_cantidades = new int[4];
            try
            {
                Int32 cantidad_velocidad_1 = 0;
                Int32 cantidad_velocidad_2 = 0;
                Int32 cantidad_velocidad_3 = 0;
                Int32 cantidad_velocidad_4 = 0;

                //modificar la consulta validar sin el supervisor
                string query = "select velocidad, isnull(sum(unidad),0) as unidades " +
                                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                                " and periodo = '" + periodo + "' and cod_tipo_esquema = " + tipo_esquema + " " +
                                "and estado = 1  and velocidad > 0 group by velocidad order by velocidad ";
                //string query = "select velocidad, sum(unidad) as unidades " +
                //                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                //                " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                //                " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                //                " and velocidad > 0 group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                //aqui validar
                                Int32 velocidad = Convert.ToInt32(sdr["velocidad"]);
                                if (velocidad == valor_velocidad1)
                                {
                                    cantidad_velocidad_1 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[0] = cantidad_velocidad_1;
                                }
                                else if (velocidad == valor_velocidad2)
                                {
                                    cantidad_velocidad_2 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[1] = cantidad_velocidad_2;
                                }
                                else if (velocidad == valor_velocidad3)
                                {
                                    cantidad_velocidad_3 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[2] = cantidad_velocidad_3;
                                }
                                else if (velocidad >= valor_velocidad4)
                                {
                                    cantidad_velocidad_4 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[3] = cantidad_velocidad_4;
                                }
                            }
                        }
                        con.Close();
                    }
                }

            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Cantidad Unidades Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return array_cantidades;
        }

        public Int32 calcularTotalAltasMigracion(string cedula_asesor, 
                                                 string cedula_supervisor, 
                                                 string periodo, 
                                                 Int32 tipo_esquema)
        {
            Int32 totalCantidad = 0;
            try
            {
                string query = "select velocidad, count(unidad) as unidades " +
                               " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                               " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                               " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                               " and velocidad > 0 and unidad = 0 group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                totalCantidad = totalCantidad +( Convert.ToInt32(sdr["unidades"]));
                            }
                        }

                        con.Close();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Cantidad Unidades Altas Migracion", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return totalCantidad;
        }

        public double[] calcularValorMigraciones(string cedula_asesor, 
                                                 string cedula_supervisor, 
                                                 string periodo, 
                                                 
                                                 Int32 codigo_tipo_esquema,
                                                 Int32 nivel_tabla,
                                                 Int32 total_megas
                                                 
                                                )
        {
            double[] arr_valores_migracion = new double[2];
            double acumValorMigracion = 0;
            double acumCantValorMigracion = 0;
            List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema).ToList();
            List<liq_valores_megabytes> _liq_valores_megabytes = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema).ToList();
            try
            {
                string query = "select velocidad, count(unidad) as unidades " +
                               " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                               " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                               " and cod_tipo_esquema = " + codigo_tipo_esquema + " and estado = 1 " +
                               " and velocidad > 0 and unidad = 0 group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                Int32 codigo_valor_mega = 0;
                                Int32 valor_megaBytes = Convert.ToInt32(sdr["velocidad"]);
                                Int32 unidades_mega = Convert.ToInt32(sdr["unidades"]);
                                var valores_mega_e = _liq_valores_megabytes.Find(x => x.valor_mega == valor_megaBytes);
                                //var val_valor_mega = _liq_valores_megas_.Find(x => x.valor_mega == velocidad);
                                if (valores_mega_e != null)
                                {
                                    codigo_valor_mega = valores_mega_e.codigo_valor;
                                }
                                else
                                {
                                    Int32 max_velocidad = _liq_valores_megabytes.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema && x.calcula_mega == 1).Select(x => x.valor_mega).Max();
                                    if (valor_megaBytes > max_velocidad)
                                    {
                                        codigo_valor_mega = _liq_valores_megabytes.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema && x.calcula_mega == 1).Select(x => x.codigo_valor).Max();
                                    }

                                }
                                if(codigo_tipo_esquema == 1 || codigo_tipo_esquema == 2)
                                {
                                    //aqui las suma 
                                    Int32 rango_altas = 0;
                                    if (total_megas > 0)
                                    {
                                        var val_rango_altas = _liq_escalas_altas.Find(x => x.numero_escala_alta == total_megas && x.codigo_tipo_escala == codigo_tipo_esquema);
                                        if (val_rango_altas != null)
                                        {
                                            rango_altas = val_rango_altas.rango_altas;
                                        }
                                        else
                                        {
                                            Int32 max_rango_altas = _liq_escalas_altas.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema).Select(x => x.numero_escala_alta).Max();
                                            if (total_megas > max_rango_altas)
                                            {
                                                rango_altas = _liq_escalas_altas.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema).Select(x => x.rango_altas).Max();
                                                // rango_altas = _liq_escalas_altas.Select(x => x.rango_altas).Max();
                                            }

                                        }
                                    }
                                    //
                                    liq_pap _liq_pap_ = _context.liq_pap.Where(x => x.nivel_cumplimiento == nivel_tabla
                                                                                   && x.valor_nivel == rango_altas
                                                                                   && x.valor_mega == codigo_valor_mega
                                                                                   && x.estado == 1).FirstOrDefault();

                                    if (_liq_pap_ != null)
                                    {
                                        acumValorMigracion = acumValorMigracion + _liq_pap_.valor;
                                        acumCantValorMigracion = acumCantValorMigracion + (Convert.ToInt32(sdr["unidades"]) * _liq_pap_.valor);
                                    }
                                }else if(codigo_tipo_esquema == 3)
                                {
                                    liq_esquema_call _liq_esquema_call_ = _context.liq_esquema_call.Where(x => x.codigo_tipo_internet == codigo_valor_mega
                                                                                                          && x.nivel == nivel_tabla).FirstOrDefault();
                                    if (_liq_esquema_call_ != null) 
                                    {
                                        acumValorMigracion = acumValorMigracion + _liq_esquema_call_.valor;
                                        acumCantValorMigracion = acumCantValorMigracion + (Convert.ToInt32(sdr["unidades"]) * _liq_esquema_call_.valor);
                                    }
                                }
                                

                            }
                            arr_valores_migracion[0] = acumValorMigracion;
                            arr_valores_migracion[1] = acumCantValorMigracion;
                        }
                        con.Close();
                    }

                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Calcular valores migracion", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }

            return arr_valores_migracion;
        }

        public Int32 ObtnerNivelCumplimientoPap(List<listar_pap_resumida> _list_pap_resumida, Int32 porcentaje_asesor)
        {
            string mensaje = "";
            Int32 nivel_cumpliento = 0;
            //General.crearImprimeMensajeLog(porcentaje_asesor + "", "ObtnerNivelCumplimientoPap", _config.GetConnectionString("conexionDbPruebas"));
            int tamano_Lista = _list_pap_resumida.Count;
            //General.crearImprimeMensajeLog("El tamaño de la Lista es : " + tamano_Lista + "", "ObtnerNivelCumplimientoPap", _config.GetConnectionString("conexionDbPruebas"));
            foreach (listar_pap_resumida i in _list_pap_resumida)
            {
                string[] arr_homologa_cump = i.homologa_cumplimiento.Split(',');
                Int32 rango_uno = Convert.ToInt32(arr_homologa_cump[0]);
                Int32 rango_dos = Convert.ToInt32(arr_homologa_cump[1]);
                mensaje = "El rango uno es : " + rango_uno + " rango dos es : " + rango_dos;

                //General.crearImprimeMensajeLog(mensaje, "ObtnerNivelCumplimientoPap", _config.GetConnectionString("conexionDbPruebas"));

                if (porcentaje_asesor >= rango_uno && porcentaje_asesor <= rango_dos)
                {
                    nivel_cumpliento = i.nivel_cumplimiento;
                }
            }
            return nivel_cumpliento;
        }

        public Int32 ObtnerVelocidadMega(Int32 codigo_valor)
        {
            Int32 velocidadMega = 0;
            liq_valores_megabytes _Valores_Megabytes_ = _context.liq_valores_megabytes.Where(x => x.codigo_valor == codigo_valor
                                                                                               && x.estado == 1).FirstOrDefault();
            if (_Valores_Megabytes_ != null)
            {
                velocidadMega = _Valores_Megabytes_.valor_mega;
            }
            return velocidadMega;
        }


        public Int32 sumarTotalUnidadesAsesorCall(string cedula_asesor, string cedulaSuper, string periodo, Int32 TipoProceso, Int32 TipoEsquema)
        {
            Int32 sumar = 0;
            try
            {

                //string query = "";
                string unidad = " ";
                if (TipoProceso == 1)
                {
                    unidad = " and unidad > 0";
                }

               
                string query = "select isnull(sum(unidad),0) as suma from liq_tmp_base_cierre  where cedula_asesor = '"+ cedula_asesor + "' " +
                               "and cedula_supervisor = '"+ cedulaSuper + "' and periodo = '"+ periodo + "' and cod_tipo_esquema = '"+ TipoEsquema + "' " +
                               "and velocidad > 0 and producto = 'Broadband Service' and empaqhomo in ('BA','DUO','TRIO') " +
                               "and tipo_campana in ('VENTA EFECTIVA C2C','VENTA EFECTIVA MOVISTAR BASE') "+unidad;


                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                sumar = Convert.ToInt32(sdr["suma"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Sumar Unidades Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return sumar;
        }
        public Int32 sumarTotalUnidadesAsesor(string cedula_asesor, string cedulaSuper, string periodo, Int32 TipoProceso, Int32 TipoEsquema)
        {
            
            Int32 sumar = 0;
            try
            {
               
                //string query = "";
                string unidad = " ";
                if (TipoProceso == 1)
                {
                    //validamos sin el supervisor
                    //unidad = " and cedula_supervisor = '" + cedulaSuper + "' and unidad > 0 ";
                    unidad = " and unidad > 0 ";
                }
                //validar futuramente para con un parametro el cual decida si se le aplica la reversion 
                string query = "SELECT isnull(sum(unidad),0) as suma  FROM liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                               " and periodo = '" + periodo + "' and cod_tipo_esquema = '"+ TipoEsquema+"' and velocidad > 0 "+unidad;


                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                sumar = Convert.ToInt32(sdr["suma"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                string aux_mensaje = "Sumar Unidades Asesor la cedula supervisor" + cedulaSuper;
                General.CrearLogError(sf.GetMethod().Name, aux_mensaje, e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
                
            }
            return sumar;
        }
        public List<listar_pap_resumida> listar_pap_resumidas()
        {
            List<listar_pap_resumida> _listar_pap_resumida = new List<listar_pap_resumida>();
            try
            {
                string query = "select cumplimiento,nivel_cumplimiento ,homologa_cumplimiento " +
                               " from liq_pap where codigo_liq_esq in (1,2)  group by cumplimiento, nivel_cumplimiento, homologa_cumplimiento " +
                               " order by nivel_cumplimiento";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _listar_pap_resumida.Add(new listar_pap_resumida
                                {
                                    cumplimiento = sdr["cumplimiento"] + "",
                                    homologa_cumplimiento = sdr["homologa_cumplimiento"] + "",
                                    nivel_cumplimiento = Convert.ToInt32(sdr["nivel_cumplimiento"])
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar pap resumida", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return _listar_pap_resumida;
        }
        public async Task<Int32> sumarTotalNaked(string cedula_asesor, string cedula_supervisor, string periodo, Int32 cod_tipo_esquema, Int32 tipoProceso)
        {
            
            Int32 total_naked = 0;
            try
            {
                string cuentaUnidad = "";
                if(tipoProceso == 1)
                {
                    cuentaUnidad = " and unidad > 0";
                }
                //validamos sin el supervisor
                string query = "select isnull(sum(unidad),0) as cantidad from liq_tmp_base_cierre " +
                               "  where cedula_asesor = '" + cedula_asesor + "' " +
                               "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                               "  and empaqhomo = 'BA' and producto = 'Broadband Service'" +
                               "  and estado = 1 " + cuentaUnidad;
                //string query = "select sum(unidad) as cantidad from liq_tmp_base_cierre " +
                //               "  where cedula_asesor = '" + cedula_asesor + "' and cedula_supervisor = '" + cedula_supervisor + "'" +
                //               "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                //               "  and empaqhomo = 'BA' and producto = 'Broadband Service'" +
                //               "  and estado = 1 " + cuentaUnidad;
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                total_naked = Convert.ToInt32(sdr["cantidad"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Sumar cantidad naked", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
                
            }
            return total_naked;
        }
        public async Task<Int32> sumarTotalDuos(string cedula_asesor, string cedula_supervisor, string periodo, Int32 cod_tipo_esquema, Int32 tipoProceso)
        {
            
            Int32 total_duos = 0;
            try
            {
                string cuentaUnidad = "";
                if(tipoProceso == 1)
                {
                    cuentaUnidad = " and unidad > 0";
                }
                string query = "select isnull(sum(unidad),0) as cantidad from liq_tmp_base_cierre " +
                              "  where cedula_asesor = '" + cedula_asesor + "' " +
                              "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                              "  and empaqhomo = 'DUO' and producto = 'Fixed Line Voice'" +
                              "  and estado = 1 " + cuentaUnidad;
                //string query = "select sum(unidad) as cantidad from liq_tmp_base_cierre " +
                //              "  where cedula_asesor = '" + cedula_asesor + "' and cedula_supervisor = '" + cedula_supervisor + "'" +
                //              "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                //              "  and empaqhomo = 'DUO' and producto = 'Fixed Line Voice'" +
                //              "  and estado = 1 "+ cuentaUnidad;
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                total_duos = Convert.ToInt32(sdr["cantidad"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Sumar cantidad duos", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
                
            }
            return total_duos;
        }

        public async Task<double> calcularValorEmpaqHomo(Int32 cod_tipo_esquema, Int32 porcentaje_asesor, Int32 tipo_empaqhome ,List<liq_empaqhome> _liq_empaqhome_)
        {
            
            double valor_naked = 0;
            try
            {
                Int32 codigo_nivel = 0;
                
                var distincLiqEmpHome = _liq_empaqhome_.Select(x => new {x.homologa_cumplimieno, x.codigo_nivel }).Distinct().ToList();
                foreach(var i in distincLiqEmpHome)
                {
                    string[] homologa_cump = i.homologa_cumplimieno.Split(',');
                    Int32 rango_uno = Convert.ToInt32(homologa_cump[0]);
                    Int32 rango_dos = Convert.ToInt32(homologa_cump[1]);
                    if (porcentaje_asesor >= rango_uno && porcentaje_asesor <= rango_dos)
                    {
                        codigo_nivel = i.codigo_nivel;
                    }
                }
                liq_empaqhome _liq_empaqhome_e_ = await _context.liq_empaqhome.Where(x => x.cod_tipo_esquema == cod_tipo_esquema
                                                                               && x.codigo_nivel == codigo_nivel
                                                                               && x.tipo_empaquehome == tipo_empaqhome
                                                                               && x.estado == 1).FirstOrDefaultAsync();
                if(_liq_empaqhome_e_ != null)
                {
                    valor_naked = _liq_empaqhome_e_.valor;
                }
                else
                {
                    valor_naked = 0;
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "valor Empq Home", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
                
            }
            return valor_naked;
        }
        #endregion
        #region metodos movil
        public void procesar_altas_movil_pap(string perido, int tipoProceso)
        {

            List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
            _carta_metas = _context.liq_tmp_metas.Where(x => x.periodo_importe == perido
                                                        && x.estado == 1 ).ToList();

            

            if (_carta_metas.Count() > 0)
            {
                foreach (liq_tmp_metas item in _carta_metas)
                {
                    Int32 UnidadesMovil = sumarTotalUnidadesAsesorMovil(item.cedula_asesor, 
                                                                        item.cedula_supervisor, 
                                                                        item.periodo_importe, 
                                                                        tipoProceso, 
                                                                        item.cod_tipo_escala);
                    if (UnidadesMovil > 0)
                    {

                        List<liq_esquema_movil> _liq_esquema_movil = _context.liq_esquema_movil.Where(x => x.estado == 1
                                                                                                      && x.codigo_tipo_esquema == item.cod_tipo_escala)
                                                                                               .ToList();
                        liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                                                      
                                                                                                      && x.periodo == perido
                                                                                                      && x.codigo_tipo_escala == item.cod_tipo_escala
                                                                                                      && x.estado == 1).FirstOrDefault();
                        //validar con una parametro el cumpliemiento del asesor
                        if (_liq_comision_asesor_e.cumplimiento_asesor >= 80)
                        {
                            _liq_comision_asesor_e.numero_plan_movil = UnidadesMovil;
                            double[] arrTotalMovil = new double[2];
                            arrTotalMovil = calcularValorPlanMovil(item.cedula_asesor,
                                                                   item.cedula_supervisor,
                                                                   item.periodo_importe,
                                                                   _liq_comision_asesor_e.cumplimiento_asesor,
                                                                   _liq_esquema_movil,
                                                                   tipoProceso,
                                                                   item.cod_tipo_escala);
                            _liq_comision_asesor_e.valor_plan_movil = arrTotalMovil[0];
                            _liq_comision_asesor_e.total_plan_movil = arrTotalMovil[1];
                            _liq_comision_asesor_e.sub_total_comision = _liq_comision_asesor_e.sub_total_comision + arrTotalMovil[1];
                            _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.total_comision + arrTotalMovil[1];
                            _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                            int rs = _context.SaveChanges();
                            if(rs > 0)
                            {
                                (from lq_ in _context.liq_tmp_altas_movil
                                 where lq_.cedula_asesor == item.cedula_asesor
                                 && lq_.cedula_supervisor == item.cedula_supervisor
                                 && lq_.EsProcesado == 0 && lq_.estado == 1
                                 select lq_).ToList()
                                 .ForEach(x => x.EsProcesado = 1);
                                _context.SaveChanges();
                            }
                           
                        }
                        General.recalcular_subtotales(item.cedula_asesor, perido, _config.GetConnectionString("conexionDbPruebas"));
                    }
                }
            }
        }

        public Int32 sumarTotalUnidadesAsesorMovil(string cedulaEmpleado, string cedulaSupervisor, string periodo, int TipoProceso, Int32 codigo_tipo_esquema)
        {
            Int32 sumaUnidadesMovil = 0;
            string unidad = "";
            
            try
            {
                if (TipoProceso == 1)
                {
                    unidad = " and unidad > 0";
                }
                string query = "select isnull(sum(unidad),0) as suma from liq_tmp_altas_movil  " +
                               " where cedula_asesor = '"+cedulaEmpleado+"' and periodo = '"+ periodo + "'  " +
                               " and estado = 1 and codigo_tipo_escala = '"+ codigo_tipo_esquema + "' " + unidad;
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                sumaUnidadesMovil = Convert.ToInt32(sdr["suma"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Sumar Unidad Movil", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return sumaUnidadesMovil;
        }

        public double[] calcularValorPlanMovil(string cedulaAsesor, 
                                               string cedulaSupervisor, 
                                               string periodo, 
                                               Int32 cumpliemientoAsesor, 
                                               List<liq_esquema_movil> _liq_esquema_movil,
                                               Int32 TipoProceso,
                                               Int32 codigo_tipo_esquema)
        {
            double[] arr_valores_plan_movil = new double[2];
            string unidades = "";
            try
            {
                if(TipoProceso == 1)
                {
                    unidades = " and unidad > 0 ";
                }
                string query = "select isnull(sum(unidad),0) as suma, valor from liq_tmp_altas_movil " +
                               " where cedula_asesor = '"+ cedulaAsesor + "' and periodo = '"+ periodo + "'  " +
                               " and estado = 1 and codigo_tipo_escala = '"+ codigo_tipo_esquema + "' " + unidades+" group by valor";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            double acumCantValorMovil = 0;
                            double acumValorMovil = 0;
                            double valorMovil = 0;
                            var distincCumpliento = _liq_esquema_movil.Select(x => new { x.cumplimiento, x.homologa_cumplimiento, x.nivel }).Distinct().ToList();
                            var distincRenta = _liq_esquema_movil.Select(y => new { y.tipo_renta, y.homologa_renta, y.nivel_tipo_renta }).Distinct().ToList();
                            while (sdr.Read())
                            {
                                //AQUI SE OBTIENE EL NIVEL DE CUMPLIEMIENTO
                                Int32 nivel_cumplimiento = 0;
                                foreach(var i in distincCumpliento)
                                {
                                    string[] homologa_cumpliento_s = i.homologa_cumplimiento.Split(',');
                                    Int32 rango_uno = Convert.ToInt32(homologa_cumpliento_s[0]);
                                    Int32 rango_dos = Convert.ToInt32(homologa_cumpliento_s[1]);
                                    if(cumpliemientoAsesor >= rango_uno && cumpliemientoAsesor <= rango_dos)
                                    {
                                        nivel_cumplimiento = i.nivel;
                                    }
                                }

                                //AQUI SE OBTIENE EL NIVEL DE RENTA
                                Int32 nivel_tipo_renta = 0;
                                valorMovil = Convert.ToDouble(sdr["valor"]);
                                foreach (var j in distincRenta)
                                {
                                    string[] homologa_renta_s = j.homologa_renta.Split(',');
                                    double rango_uno = Convert.ToDouble(homologa_renta_s[0]);
                                    double rango_dos = Convert.ToDouble(homologa_renta_s[1]);
                                    if(valorMovil >= rango_uno && valorMovil <= rango_dos)
                                    {
                                        nivel_tipo_renta = j.nivel_tipo_renta;
                                    }
                                }
                                if(nivel_cumplimiento > 0 && nivel_tipo_renta > 0)
                                {
                                    //se valida
                                    liq_esquema_movil _liq_esquema_movil_e = _context.liq_esquema_movil.Where(x => x.nivel == nivel_cumplimiento
                                                                                                              && x.nivel_tipo_renta == nivel_tipo_renta
                                                                                                              && x.codigo_tipo_esquema == codigo_tipo_esquema)
                                                                                                       .FirstOrDefault();
                                    if(_liq_esquema_movil_e != null)
                                    {
                                        acumValorMovil =  _liq_esquema_movil_e.valor;
                                        acumCantValorMovil = acumCantValorMovil + (Convert.ToInt32(sdr["suma"]) * _liq_esquema_movil_e.valor);
                                        //guardar en una tabla temporal este apuntador de pagos
                                    }
                                }
                                
                            }
                            arr_valores_plan_movil[0] = acumValorMovil;
                            arr_valores_plan_movil[1] = acumCantValorMovil;
                        }
                        con.Close();
                        
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Calcular Unidad Movil", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return arr_valores_plan_movil;
        }
        #endregion

        #region metodos ftth pymee
        public async Task<string> procesar_base_cierre_carta_meta_pymes(string perido, int TipoProceso)
        {

            string mensaje = "";
            try
            {
                var distictSuperPeriodoEsquema = _context.liq_tmp_metas.Select(x => new {
                                                                                            x.cedula_supervisor,
                                                                                            x.periodo_importe,
                                                                                            x.cod_tipo_escala,
                                                                                            x.estado
                                                                                        })
                                                                        .Distinct().Where(y => y.periodo_importe == perido
                                                                                           && y.cod_tipo_escala == 2
                                                                                           && y.estado == 1).ToList();


                foreach(var i in distictSuperPeriodoEsquema)
                {
                    List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
                    _carta_metas = _context.liq_tmp_metas.Where(x => x.periodo_importe == i.periodo_importe
                                                                && x.cod_tipo_escala == i.cod_tipo_escala
                                                                && x.cedula_supervisor == i.cedula_supervisor
                                                                && x.estado == 1).ToList();

                    int tam_carta_metas = _carta_metas.Count();
                    General.crearImprimeMensajeLog("El tamaño de la lista es : " + tam_carta_metas, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));

                    //se establece el liq_pap
                    List<listar_pap_resumida> _listar_pap_resumidas_ = listar_pap_resumidas();
                    List<liq_pap> _liq_pap_l = _context.liq_pap.Where(x => x.codigo_liq_esq == 2).ToList();
                    Int32 valor_mega1 = ObtnerVelocidadMega(5);
                    Int32 valor_mega2 = ObtnerVelocidadMega(6);
                    Int32 valor_mega3 = ObtnerVelocidadMega(7);
                    Int32 valor_mega4 = ObtnerVelocidadMega(8);
                    Int32 valor_mega5 = ObtnerVelocidadMega(9);
                    List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_escala_altas == 2).ToList();
                    List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == 2).ToList();
                    List<liq_empaqhome> _liq_empaqhome_ = _context.liq_empaqhome.Where(x => x.cod_tipo_esquema == 2).ToList();
                    if (_carta_metas.Count > 0)
                    {
                        foreach (liq_tmp_metas item in _carta_metas)
                        {
                            Int32 sumasUnidades = sumarTotalUnidadesAsesor(item.cedula_asesor, item.cedula_supervisor, item.periodo_importe, TipoProceso, item.cod_tipo_escala);
                            if (sumasUnidades > 0 && item.numero_carta_meta_ftth > 0)
                            {
                                liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                                                              && x.cedula_supervisor == item.cedula_supervisor
                                                                                                              && x.periodo == perido
                                                                                                              && x.codigo_tipo_escala == item.cod_tipo_escala
                                                                                                              && x.estado == 1).FirstOrDefault();
                                int rango_altas = 0;
                                liq_escala_altas _Escala_Altas = _context.liq_escala_altas.Where(x => x.numero_escala_alta == sumasUnidades).FirstOrDefault();
                                if (_Escala_Altas != null)
                                {
                                    rango_altas = _Escala_Altas.rango_altas;
                                }
                                else
                                {
                                    int max_numero_escala_alta = _context.liq_escala_altas.Select(x => x.numero_escala_alta).Max();
                                    if (sumasUnidades > max_numero_escala_alta)
                                    {
                                        rango_altas = _context.liq_escala_altas.Select(x => x.rango_altas).Max();
                                    }
                                }
                                if (_liq_comision_asesor_e != null)
                                {
                                    string data_mensaje_es = _liq_comision_asesor_e.cedula_asesor + " periodo es : " + perido + " y el tipo proceso : " + TipoProceso + " ";
                                    General.crearImprimeMensajeLog(data_mensaje_es, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));

                                    double porcentaje_asesor_e = 0;
                                    double porcentaje_aux_e = 0;
                                    int meta_asesor_e = _liq_comision_asesor_e.meta_asesor;
                                    porcentaje_aux_e = (double)sumasUnidades / meta_asesor_e;
                                    porcentaje_asesor_e = (porcentaje_aux_e * 100);
                                    Int32 portecentaje_asesor_e_i = Convert.ToInt32(porcentaje_asesor_e);
                                    _liq_comision_asesor_e.cumplimiento_asesor = portecentaje_asesor_e_i;
                                    //string auxmensaje = "";
                                    int tabla_cumplimiento = ObtnerNivelCumplimientoPap(_listar_pap_resumidas_, portecentaje_asesor_e_i);
                                    if (sumasUnidades > _liq_comision_asesor_e.meta_asesor)
                                    {
                                        _liq_comision_asesor_e.asesor_cumple = 1;
                                    }
                                    else
                                    {
                                        _liq_comision_asesor_e.asesor_cumple = 0;
                                    }
                                    //traemos por group by la tabla  para el comparativo
                                    _liq_comision_asesor_e.nivel = tabla_cumplimiento;
                                    Int32 cantidad_velocidad_1 = 0;
                                    Int32 cantidad_velocidad_2 = 0;
                                    Int32 cantidad_velocidad_3 = 0;
                                    Int32 cantidad_velocidad_4 = 0;
                                    Int32 cantidad_velocidad_5 = 0;
                                    int[] arr_cantidades_velocidad = new int[5];
                                    arr_cantidades_velocidad = await calcularCantidadesMegasPymes(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala,
                                                              valor_mega1, valor_mega2, valor_mega3, valor_mega4, valor_mega5);
                                    cantidad_velocidad_1 = arr_cantidades_velocidad[0];
                                    cantidad_velocidad_2 = arr_cantidades_velocidad[1];
                                    cantidad_velocidad_3 = arr_cantidades_velocidad[2];
                                    cantidad_velocidad_4 = arr_cantidades_velocidad[3];
                                    cantidad_velocidad_5 = arr_cantidades_velocidad[4];
                                    _liq_comision_asesor_e.numero_cant_megas_1 = cantidad_velocidad_1;
                                    _liq_comision_asesor_e.numero_cant_megas_2 = cantidad_velocidad_2;
                                    _liq_comision_asesor_e.numero_cant_megas_3 = cantidad_velocidad_3;
                                    _liq_comision_asesor_e.numero_cant_megas_4 = cantidad_velocidad_4;
                                    _liq_comision_asesor_e.numero_cant_megas_5 = cantidad_velocidad_5;
                                    double valor_mega_1 = 0;
                                    double valor_mega_2 = 0;
                                    double valor_mega_3 = 0;
                                    double valor_mega_4 = 0;
                                    double valor_mega_5 = 0;

                                    //setiar los valores mega

                                    _liq_comision_asesor_e.nombre_mega_1 = valor_mega1 + "";
                                    _liq_comision_asesor_e.nombre_mega_2 = valor_mega2 + "";
                                    _liq_comision_asesor_e.nombre_mega_3 = valor_mega3 + "";
                                    _liq_comision_asesor_e.nombre_mega_4 = valor_mega4 + "";
                                    _liq_comision_asesor_e.nombre_mega_5 = valor_mega5 + "";

                                    double[] arr_valores_megas = new double[5];
                                    arr_valores_megas = await calcularValorMegasPymes(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala,
                                                          tabla_cumplimiento, sumasUnidades, valor_mega1, valor_mega2, valor_mega3, valor_mega4, valor_mega5);
                                    valor_mega_1 = arr_valores_megas[0];
                                    valor_mega_2 = arr_valores_megas[1];
                                    valor_mega_3 = arr_valores_megas[2];
                                    valor_mega_4 = arr_valores_megas[3];
                                    valor_mega_5 = arr_valores_megas[4];
                                    if (valor_mega1 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_1 = valor_mega_1;
                                        _liq_comision_asesor_e.total_valor_mega_1 = valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1;
                                    }
                                    if (valor_mega_2 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_2 = valor_mega_2;
                                        _liq_comision_asesor_e.total_valor_mega_2 = valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2;
                                    }
                                    if (valor_mega3 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_3 = valor_mega_3;
                                        _liq_comision_asesor_e.total_valor_mega_3 = valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3;
                                    }
                                    if (valor_mega4 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_4 = valor_mega_4;
                                        _liq_comision_asesor_e.total_valor_mega_4 = valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4;
                                    }
                                    if (valor_mega5 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_5 = valor_mega_5;
                                        _liq_comision_asesor_e.total_valor_mega_5 = valor_mega_5 * _liq_comision_asesor_e.numero_cant_megas_5;
                                    }
                                    double subTotalValorMegas = ((valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1) +
                                                                 (valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2) +
                                                                 (valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3) +
                                                                 (valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4) +
                                                                 (valor_mega_5 * _liq_comision_asesor_e.numero_cant_megas_5));
                                    Int32 total_duos = 0;
                                    total_duos = await sumarTotalDuos(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala, TipoProceso);
                                    _liq_comision_asesor_e.numero_duos = total_duos;
                                    double valor_duos = 0;
                                    valor_duos = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 1, _liq_empaqhome_);
                                    double subTotalDuos = 0;
                                    if (valor_duos > 0)
                                    {
                                        _liq_comision_asesor_e.valor_duos = valor_duos;
                                        subTotalDuos = (total_duos * valor_duos);
                                        _liq_comision_asesor_e.total_valor_duos = subTotalDuos;
                                    }

                                    Int32 total_trios = 0;
                                    total_trios = await sumarTotalTrios(item.cedula_asesor, item.cedula_supervisor, perido, item.cod_tipo_escala, TipoProceso);
                                    _liq_comision_asesor_e.numero_trios = total_trios;
                                    double valor_trios = 0;
                                    valor_trios = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 2, _liq_empaqhome_);
                                    double subTotalTrios = 0;
                                    if (valor_trios > 0)
                                    {
                                        _liq_comision_asesor_e.valor_trios = valor_trios;
                                        subTotalTrios = (total_trios * valor_trios);
                                        _liq_comision_asesor_e.total_valor_trios = subTotalTrios;
                                    }

                                    _liq_comision_asesor_e.sub_total_comision = subTotalValorMegas + subTotalTrios + subTotalDuos;
                                    _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.sub_total_comision;
                                    _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                    int sa = await _context.SaveChangesAsync();
                                    if (sa > 0)
                                    {
                                        (from lq_ in _context.liq_tmp_base_cierre
                                         where lq_.velocidad > 0 && lq_.cedula_asesor == item.cedula_asesor
                                         && lq_.cedula_supervisor == item.cedula_supervisor
                                         && lq_.periodo == perido && lq_.estado == 1
                                         && lq_.EsProcesado == 0
                                         select lq_).ToList()
                                        .ForEach(x => x.EsProcesado = 1);
                                        _context.SaveChanges();

                                    }
                                    General.recalcular_subtotales(item.cedula_asesor, perido, _config.GetConnectionString("conexionDbPruebas"));
                                }
                            }
                        }

                    }
                }

                
            }
            catch (Exception e)
            {
                //log de errores s
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "procesar_base_cierre_carta_meta_pap", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }

            
            return mensaje;
        }

        public async Task<int[]> calcularCantidadesMegasPymes(string cedula_asesor,
                                                      string cedula_supervisor,
                                                      string periodo,
                                                      Int32 tipo_esquema,
                                                      Int32 valor_velocidad1,
                                                      Int32 valor_velocidad2,
                                                      Int32 valor_velocidad3,
                                                      Int32 valor_velocidad4,
                                                      Int32 valor_velocidad5  
                                                      )
        {

            int[] array_cantidades = new int[5];
            try
            {
                Int32 cantidad_velocidad_1 = 0;
                Int32 cantidad_velocidad_2 = 0;
                Int32 cantidad_velocidad_3 = 0;
                Int32 cantidad_velocidad_4 = 0;
                Int32 cantidad_velocidad_5 = 0;

                string query = "select velocidad, isnull(sum(unidad),0) as unidades " +
                                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                                " and periodo = '" + periodo + "' and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                                " and velocidad > 0 group by velocidad order by velocidad ";

                //string query = "select velocidad, sum(unidad) as unidades " +
                //                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                //                " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                //                " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                //                " and velocidad > 0 group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                //aqui validar
                                Int32 velocidad = Convert.ToInt32(sdr["velocidad"]);
                                if (velocidad == valor_velocidad1)
                                {
                                    cantidad_velocidad_1 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[0] = cantidad_velocidad_1;
                                }
                                else if (velocidad == valor_velocidad2)
                                {
                                    cantidad_velocidad_2 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[1] = cantidad_velocidad_2;
                                }
                                else if (velocidad == valor_velocidad3)
                                {
                                    cantidad_velocidad_3 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[2] = cantidad_velocidad_3;
                                }
                                else if (velocidad == valor_velocidad4)
                                {
                                    cantidad_velocidad_4 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[3] = cantidad_velocidad_4;
                                }else if(velocidad >= valor_velocidad5)
                                {
                                    cantidad_velocidad_5 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[4] = cantidad_velocidad_5;
                                }
                            }
                        }
                        con.Close();
                    }
                }

            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Cantidad Unidades Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return array_cantidades;
        }

        public async Task<double[]> calcularValorMegasPymes(string cedula_asesor,
                                          string cedula_supervisor,
                                          string periodo,
                                          Int32 tipo_esquema,
                                          Int32 nivel_cumplimiento,
                                          Int32 cantidadFtth,
                                          Int32 valor_velocidad1,
                                          Int32 valor_velocidad2,
                                          Int32 valor_velocidad3,
                                          Int32 valor_velocidad4,
                                          Int32 valor_velocidad5
                                         )

        {

            double[] arr_valores_mega = new double[5];
            double valor_mega_1 = 0;
            double valor_mega_2 = 0;
            double valor_mega_3 = 0;
            double valor_mega_4 = 0;
            double valor_mega_5 = 0;
            try
            {
                List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                string query = "select velocidad, isnull(sum(unidad),0) as unidades " +
                                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                                " and periodo = '" + periodo + "' and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                                " and velocidad > 0  group by velocidad order by velocidad ";

                //string query = "select velocidad, sum(unidad) as unidades " +
                //                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                //                " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                //                " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                //                " and velocidad > 0  group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                Int32 velocidad = Convert.ToInt32(sdr["velocidad"]);
                                //Int32 unidades_escala_altas = Convert.ToInt32(sdr["unidades"]);
                                //liq_valores_megabytes _liq_valores_mega_ = _context.liq_valores_megabytes.Where(x => x.valor_mega == velocidad).FirstOrDefault();
                                if (_liq_valores_megas_ != null && _liq_escalas_altas != null)
                                {
                                    //Int32 codigo_valor = _liq_valores_mega_.codigo_valor;
                                    Int32 codigo_valor_mega = 0;
                                    if (velocidad > 0)
                                    {
                                        var val_valor_mega = _liq_valores_megas_.Find(x => x.valor_mega == velocidad);
                                        if (val_valor_mega != null)
                                        {
                                            codigo_valor_mega = val_valor_mega.codigo_valor;
                                        }
                                        else
                                        {
                                            Int32 max_velocidad = _liq_valores_megas_.Select(x => x.valor_mega).Max();
                                            if (velocidad > max_velocidad)
                                            {
                                                codigo_valor_mega = _liq_valores_megas_.Select(x => x.codigo_valor).Max();
                                            }

                                        }
                                    }

                                    Int32 rango_altas = 0;
                                    if (cantidadFtth > 0)
                                    {
                                        var val_rango_altas = _liq_escalas_altas.Find(x => x.numero_escala_alta == cantidadFtth);
                                        if (val_rango_altas != null)
                                        {
                                            rango_altas = val_rango_altas.rango_altas;
                                        }
                                        else
                                        {
                                            Int32 max_rango_altas = _liq_escalas_altas.Select(x => x.numero_escala_alta).Max();
                                            if (cantidadFtth > max_rango_altas)
                                            {
                                                rango_altas = _liq_escalas_altas.Select(x => x.rango_altas).Max();
                                                rango_altas = _liq_escalas_altas.Select(x => x.rango_altas).Max();
                                            }

                                        }
                                    }
                                    //codigo valor mega y el rango altas se procede a buscar el valor que corresponda por linq 
                                    liq_pap _liq_pap_ = await _context.liq_pap.Where(x => x.nivel_cumplimiento == nivel_cumplimiento
                                                                               && x.valor_nivel == rango_altas
                                                                               && x.valor_mega == codigo_valor_mega
                                                                               && x.codigo_liq_esq == tipo_esquema
                                                                               && x.estado == 1).FirstOrDefaultAsync();
                                    if (_liq_pap_ != null)
                                    {
                                        if (velocidad == valor_velocidad1)
                                        {
                                            valor_mega_1 = _liq_pap_.valor;
                                            arr_valores_mega[0] = valor_mega_1;
                                        }
                                        else if (velocidad == valor_velocidad2)
                                        {
                                            valor_mega_2 = _liq_pap_.valor;
                                            arr_valores_mega[1] = valor_mega_2;
                                        }
                                        else if (velocidad == valor_velocidad3)
                                        {
                                            valor_mega_3 = _liq_pap_.valor;
                                            arr_valores_mega[2] = valor_mega_3;
                                        }
                                        else if (velocidad == valor_velocidad4)
                                        {
                                            valor_mega_4 = _liq_pap_.valor;
                                            arr_valores_mega[3] = valor_mega_4;
                                        }else if(velocidad >= valor_velocidad5)
                                        {
                                            valor_mega_5 = _liq_pap_.valor;
                                            arr_valores_mega[4] = valor_mega_4;
                                        }
                                    }
                                }


                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "valores mega Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return arr_valores_mega;
        }

        public async Task<Int32> sumarTotalTrios(string cedula_asesor, string cedula_supervisor, string periodo, Int32 cod_tipo_esquema, Int32 tipoProceso)
        {

            Int32 total_duos = 0;
            try
            {
                string cuentaUnidad = "";
                if (tipoProceso == 1)
                {
                    cuentaUnidad = " and unidad > 0";
                }
                string query = "select isnull(sum(unidad),0) as cantidad from liq_tmp_base_cierre " +
                              "  where cedula_asesor = '" + cedula_asesor + "' " +
                              "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                              "  and empaqhomo = 'TRIO' and producto = 'Fixed Line Voice'" +
                              "  and estado = 1 " + cuentaUnidad;

                //string query = "select sum(unidad) as cantidad from liq_tmp_base_cierre " +
                //              "  where cedula_asesor = '" + cedula_asesor + "' and cedula_supervisor = '" + cedula_supervisor + "'" +
                //              "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                //              "  and empaqhomo = 'TRIO' and producto = 'Fixed Line Voice'" +
                //              "  and estado = 1 " + cuentaUnidad;
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                total_duos = Convert.ToInt32(sdr["cantidad"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Sumar cantidad duos", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return total_duos;
        }
        #endregion

        #region metodos call
        public async Task<string> procesar_base_cierre_carta_meta_call_v2(string periodo, int TipoProceso)
        {

            string mensaje = "";
            General.crearImprimeMensajeLog("Entro a la funcion ", "procesar_base_cierre_carta_meta_call", _config.GetConnectionString("conexionDbPruebas"));
            try
            {

                var distictSuperPeriodoEsquema = _context.liq_tmp_metas.Select(x => new
                {
                    x.cedula_supervisor,
                    x.periodo_importe,
                    x.cod_tipo_escala,
                    x.estado
                }).Distinct()
                  .Where(y => y.periodo_importe == periodo
                                    && y.cod_tipo_escala == 3
                                    && y.estado == 1).ToList();

                foreach (var i in distictSuperPeriodoEsquema)
                {
                    List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
                    _carta_metas = _context.liq_tmp_metas.Where(x => x.periodo_importe == periodo
                                                                && x.cod_tipo_escala == 3
                                                                && x.estado == 1).ToList();
                    int tam_carta_metas = _carta_metas.Count();
                    General.crearImprimeMensajeLog("El tamaño de la lista es : " + tam_carta_metas, "procesar_base_cierre_carta_meta_call", _config.GetConnectionString("conexionDbPruebas"));

                    List<listar_pap_resumida> _listar_pap_resumidas_ = listar_call_resumidas_v2();

                    List<liq_pap> _liq_pap = _context.liq_pap.Where(x => x.codigo_liq_esq == 3
                                                                    && x.estado == 1).ToList();

                    List<liq_empaqhome> _liq_empaqhome_ = _context.liq_empaqhome.Where(x => x.cod_tipo_esquema == 3).ToList();

                    Int32 valor_mega1 = ObtnerVelocidadMega(10);
                    Int32 valor_mega2 = ObtnerVelocidadMega(11);
                    Int32 valor_mega3 = ObtnerVelocidadMega(12);
                    Int32 valor_mega4 = ObtnerVelocidadMega(13);
                    Int32 valor_mega5 = 0;
                    List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == 3).ToList();
                    if (_carta_metas.Count > 0)
                    {
                        foreach (liq_tmp_metas item in _carta_metas)
                        {
                            Int32 sumasUnidades = sumarTotalUnidadesAsesorCall(item.cedula_asesor, item.cedula_supervisor, item.periodo_importe, TipoProceso, item.cod_tipo_escala);
                            if (sumasUnidades > 0 )
                            {
                                liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                                                              && x.cedula_supervisor == item.cedula_supervisor
                                                                                                              && x.periodo == periodo
                                                                                                              && x.codigo_tipo_escala == item.cod_tipo_escala
                                                                                                              && x.estado == 1).FirstOrDefault();
                                if (_liq_comision_asesor_e != null)
                                {
                                    string data_mensaje_es = _liq_comision_asesor_e.cedula_asesor + " periodo es : " + periodo + " y el tipo proceso : " + TipoProceso + " ";
                                    General.crearImprimeMensajeLog(data_mensaje_es, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));

                                    double porcentaje_asesor_e = 0;
                                    double porcentaje_aux_e = 0;
                                    int meta_asesor_e = _liq_comision_asesor_e.meta_asesor;
                                    _liq_comision_asesor_e.total_venta_alta_velocidad = sumasUnidades;
                                    porcentaje_aux_e = (double)sumasUnidades / meta_asesor_e;
                                    porcentaje_asesor_e = (porcentaje_aux_e * 100);
                                    Int32 portecentaje_asesor_e_i = Convert.ToInt32(porcentaje_asesor_e);
                                    _liq_comision_asesor_e.cumplimiento_asesor = portecentaje_asesor_e_i;


                                    int tabla_cumplimiento = ObtnerNivelCumplimientoPap(_listar_pap_resumidas_, portecentaje_asesor_e_i);
                                    if (sumasUnidades > _liq_comision_asesor_e.meta_asesor)
                                    {
                                        _liq_comision_asesor_e.asesor_cumple = 1;
                                    }
                                    else
                                    {
                                        _liq_comision_asesor_e.asesor_cumple = 0;
                                    }
                                    _liq_comision_asesor_e.nivel = tabla_cumplimiento;
                                    Int32 cantidad_velocidad_1 = 0;
                                    Int32 cantidad_velocidad_2 = 0;
                                    Int32 cantidad_velocidad_3 = 0;
                                    Int32 cantidad_velocidad_4 = 0;

                                    int[] arr_cantidades_velocidad = new int[4];
                                    arr_cantidades_velocidad = await calcularCantidadesMegasCallV2(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                               valor_mega1, valor_mega2, valor_mega3, valor_mega4);

                                    cantidad_velocidad_1 = arr_cantidades_velocidad[0];
                                    cantidad_velocidad_2 = arr_cantidades_velocidad[1];
                                    cantidad_velocidad_3 = arr_cantidades_velocidad[2];
                                    cantidad_velocidad_4 = arr_cantidades_velocidad[3];

                                    _liq_comision_asesor_e.numero_cant_megas_1 = cantidad_velocidad_1;
                                    _liq_comision_asesor_e.numero_cant_megas_2 = cantidad_velocidad_2;
                                    _liq_comision_asesor_e.numero_cant_megas_3 = cantidad_velocidad_3;
                                    _liq_comision_asesor_e.numero_cant_megas_4 = cantidad_velocidad_4;


                                    _liq_comision_asesor_e.nombre_mega_1 = valor_mega1 + "";
                                    _liq_comision_asesor_e.nombre_mega_2 = valor_mega2 + "";
                                    _liq_comision_asesor_e.nombre_mega_3 = valor_mega3 + "";
                                    _liq_comision_asesor_e.nombre_mega_4 = valor_mega4 + "";


                                    double valor_mega_1 = 0;
                                    double valor_mega_2 = 0;
                                    double valor_mega_3 = 0;
                                    double valor_mega_4 = 0;
                                    //aqui lo bueno
                                    double[] arr_valores_megas = new double[5];

                                    arr_valores_megas = await calcularValorMegasCallV2(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                          tabla_cumplimiento, sumasUnidades, valor_mega1, valor_mega2, valor_mega3, valor_mega4);
                                    valor_mega_1 = arr_valores_megas[0];
                                    valor_mega_2 = arr_valores_megas[1];
                                    valor_mega_3 = arr_valores_megas[2];
                                    valor_mega_4 = arr_valores_megas[3];

                                    if (valor_mega1 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_1 = valor_mega_1;
                                        _liq_comision_asesor_e.total_valor_mega_1 = valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1;

                                    }
                                    if (valor_mega_2 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_2 = valor_mega_2;
                                        _liq_comision_asesor_e.total_valor_mega_2 = valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2;
                                    }
                                    if (valor_mega3 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_3 = valor_mega_3;
                                        _liq_comision_asesor_e.total_valor_mega_3 = valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3;
                                    }
                                    if (valor_mega4 > 0)
                                    {
                                        _liq_comision_asesor_e.valor_mega_4 = valor_mega_4;
                                        _liq_comision_asesor_e.total_valor_mega_4 = valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4;
                                    }

                                    double subTotalValorMegas = ((valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1) +
                                                                 (valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2) +
                                                                 (valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3) +
                                                                 (valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4)
                                                                );

                                    Int32 numero_ventas_c2c = 0;

                                    numero_ventas_c2c = await numeroVentasAltaVelocidad(item.cedula_asesor, item.cedula_supervisor, periodo, "VENTA EFECTIVA C2C", item.cod_tipo_escala, TipoProceso);
                                    _liq_comision_asesor_e.numero_venta_c2c = numero_ventas_c2c;
                                    double valor_venta_c2c = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 4, _liq_empaqhome_);
                                    double subTotalC2c = 0;
                                    if (valor_venta_c2c > 0)
                                    {
                                        _liq_comision_asesor_e.valor_venta_c2c = valor_venta_c2c;
                                        subTotalC2c = (numero_ventas_c2c * valor_venta_c2c);
                                        _liq_comision_asesor_e.total_venta_c2c = subTotalC2c;
                                    }
                                    Int32 numero_ventas_venta_base = 0;
                                    numero_ventas_venta_base = await numeroVentasAltaVelocidad(item.cedula_asesor, item.cedula_supervisor, periodo, "VENTA EFECTIVA MOVISTAR BASE", item.cod_tipo_escala, TipoProceso);
                                    _liq_comision_asesor_e.numero_venta_base = numero_ventas_venta_base;
                                    double valor_venta_base = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 5, _liq_empaqhome_);
                                    double subTotalVentaBase = 0;
                                    if (valor_venta_base > 0)
                                    {
                                        _liq_comision_asesor_e.valor_venta_base = valor_venta_base;
                                        subTotalVentaBase = (numero_ventas_venta_base * valor_venta_base);
                                        _liq_comision_asesor_e.total_venta_base = subTotalVentaBase;
                                    }
                                    Int32 total_naked = 0;
                                    total_naked = await sumarTotalNaked(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                                    _liq_comision_asesor_e.numero_naked = total_naked;
                                    double valor_naked = 0;
                                    valor_naked = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 3, _liq_empaqhome_);
                                    double subTotalNaked = 0;
                                    if (valor_naked > 0)
                                    {
                                        _liq_comision_asesor_e.valor_naked = valor_naked;
                                        subTotalNaked = (total_naked * valor_naked);
                                        _liq_comision_asesor_e.total_valor_naked = subTotalNaked;
                                    }

                                    int total_duos = 0;
                                    total_duos = await sumarTotalDuosCall(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                                    _liq_comision_asesor_e.numero_duos = total_duos;
                                    double valor_duos = 0;
                                    valor_duos = await calcularValorEmpaqHomo(item.cod_tipo_escala, portecentaje_asesor_e_i, 1, _liq_empaqhome_);
                                    double subTotalDuos = 0;
                                    if (valor_duos > 0)
                                    {
                                        _liq_comision_asesor_e.valor_duos = valor_duos;
                                        subTotalDuos = (total_duos * valor_duos);
                                        _liq_comision_asesor_e.total_valor_duos = subTotalDuos;
                                    }
                                    _liq_comision_asesor_e.sub_total_comision = subTotalValorMegas + subTotalC2c + subTotalVentaBase + subTotalNaked + subTotalDuos;
                                    _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.sub_total_comision;
                                    _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                                    int sa = await _context.SaveChangesAsync();
                                    if (sa > 0)
                                    {
                                        (from lq_ in _context.liq_tmp_base_cierre
                                         where lq_.velocidad > 0 && lq_.cedula_asesor == item.cedula_asesor
                                         && lq_.cedula_supervisor == item.cedula_supervisor
                                         && lq_.periodo == periodo && lq_.estado == 1
                                         && lq_.EsProcesado == 0
                                         select lq_).ToList()
                                        .ForEach(x => x.EsProcesado = 1);
                                        _context.SaveChanges();

                                    }
                                    General.recalcular_subtotales(item.cedula_asesor, periodo, _config.GetConnectionString("conexionDbPruebas"));
                                }
                            }
                        }
                    }
                }

                
            }
            catch (Exception e)
            {
                //log de errores s
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "procesar_base_cierre_carta_meta_call_v2", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            

            return mensaje;
        }
        public async Task<string> procesar_base_cierre_carta_meta_call(string periodo, int TipoProceso)
        {
            string mensaje = "";
            List<liq_tmp_metas> _carta_metas = new List<liq_tmp_metas>();
            _carta_metas = _context.liq_tmp_metas.Where(x => x.periodo_importe == periodo
                                                        && x.cod_tipo_escala == 3
                                                        && x.estado == 1).ToList();

            int tam_carta_metas = _carta_metas.Count();
            General.crearImprimeMensajeLog("El tamaño de la lista es : " + tam_carta_metas, "procesar_base_cierre_carta_meta_call", _config.GetConnectionString("conexionDbPruebas"));

            List<listar_pap_resumida> _listar_pap_resumidas_ = listar_call_resumidas();
            //List<liq_pap> _liq_pap_l = _context.liq_pap.Where(x => x.codigo_liq_esq == 1).ToList();
            List<liq_esquema_call> _liq_esquema_call = _context.liq_esquema_call.Where(x => x.estado == 1).ToList();
            Int32 valor_mega1 = ObtnerVelocidadMega(10);
            Int32 valor_mega2 = ObtnerVelocidadMega(11);
            Int32 valor_mega3 = ObtnerVelocidadMega(12);
            Int32 valor_mega4 = ObtnerVelocidadMega(13);
            Int32 valor_mega5 = 0;
            //Int32 valor_mega5 = ObtnerVelocidadMega(16);
            //List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_escala_altas == 3).ToList();
            List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == 3).ToList();
            //List<liq_empaqhome> _liq_empaqhome_ = _context.liq_empaqhome.Where(x => x.cod_tipo_esquema == 3).ToList();

            if(_carta_metas.Count > 0)
            {
                foreach (liq_tmp_metas item in _carta_metas)
                {
                    Int32 sumasUnidades = sumarTotalUnidadesAsesor(item.cedula_asesor, item.cedula_supervisor, item.periodo_importe, TipoProceso, item.cod_tipo_escala);
                    if (sumasUnidades > 0)
                    {
                        liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == item.cedula_asesor
                                                                                                      && x.cedula_supervisor == item.cedula_supervisor
                                                                                                      && x.periodo == periodo
                                                                                                      && x.codigo_tipo_escala == item.cod_tipo_escala
                                                                                                      && x.estado == 1).FirstOrDefault();
                        if (_liq_comision_asesor_e != null)
                        {
                            string data_mensaje_es = _liq_comision_asesor_e.cedula_asesor + " periodo es : " + periodo + " y el tipo proceso : " + TipoProceso + " ";
                            General.crearImprimeMensajeLog(data_mensaje_es, "procesar_base_cierre_carta_meta_pap", _config.GetConnectionString("conexionDbPruebas"));
                            double porcentaje_asesor_e = 0;
                            double porcentaje_aux_e = 0;
                            int meta_asesor_e = _liq_comision_asesor_e.meta_asesor;
                            porcentaje_aux_e = (double)sumasUnidades / meta_asesor_e;
                            porcentaje_asesor_e = (porcentaje_aux_e * 100);
                            Int32 portecentaje_asesor_e_i = Convert.ToInt32(porcentaje_asesor_e);
                            _liq_comision_asesor_e.cumplimiento_asesor = portecentaje_asesor_e_i;
                            //string auxmensaje = "";
                            int tabla_cumplimiento = ObtnerNivelCumplimientoPap(_listar_pap_resumidas_, portecentaje_asesor_e_i);
                            if (sumasUnidades > _liq_comision_asesor_e.meta_asesor)
                            {
                                _liq_comision_asesor_e.asesor_cumple = 1;
                            }
                            else
                            {
                                _liq_comision_asesor_e.asesor_cumple = 0;
                            }
                            _liq_comision_asesor_e.nivel = tabla_cumplimiento;
                            Int32 cantidad_velocidad_1 = 0;
                            Int32 cantidad_velocidad_2 = 0;
                            Int32 cantidad_velocidad_3 = 0;
                            Int32 cantidad_velocidad_4 = 0;
                            Int32 cantidad_velocidad_5 = 0;

                            int[] arr_cantidades_velocidad = new int[5];
                            arr_cantidades_velocidad = await calcularCantidadesMegasCall(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                       valor_mega1, valor_mega2, valor_mega3, valor_mega4, valor_mega5);

                            cantidad_velocidad_1 = arr_cantidades_velocidad[0];
                            cantidad_velocidad_2 = arr_cantidades_velocidad[1];
                            cantidad_velocidad_3 = arr_cantidades_velocidad[2];
                            cantidad_velocidad_4 = arr_cantidades_velocidad[3];
                            cantidad_velocidad_5 = arr_cantidades_velocidad[4];
                            _liq_comision_asesor_e.numero_cant_megas_1 = cantidad_velocidad_1;
                            _liq_comision_asesor_e.numero_cant_megas_2 = cantidad_velocidad_2;
                            _liq_comision_asesor_e.numero_cant_megas_3 = cantidad_velocidad_3;
                            _liq_comision_asesor_e.numero_cant_megas_4 = cantidad_velocidad_4;
                            _liq_comision_asesor_e.numero_cant_megas_5 = cantidad_velocidad_5;

                            _liq_comision_asesor_e.nombre_mega_1 = valor_mega1 + "";
                            _liq_comision_asesor_e.nombre_mega_2 = valor_mega2 + "";
                            _liq_comision_asesor_e.nombre_mega_3 = valor_mega3 + "";
                            _liq_comision_asesor_e.nombre_mega_4 = valor_mega4 + "";
                            //_liq_comision_asesor_e.nombre_mega_5 = valor_mega5 + "";

                            double valor_mega_1 = 0;
                            double valor_mega_2 = 0;
                            double valor_mega_3 = 0;
                            double valor_mega_4 = 0;
                            double valor_mega_5 = 0;

                            double[] arr_valores_megas = new double[5];

                            arr_valores_megas = await calcularValorMegasCall(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala,
                                                  tabla_cumplimiento, sumasUnidades, valor_mega1, valor_mega2, valor_mega3, valor_mega4, valor_mega5);
                            valor_mega_1 = arr_valores_megas[0];
                            valor_mega_2 = arr_valores_megas[1];
                            valor_mega_3 = arr_valores_megas[2];
                            valor_mega_4 = arr_valores_megas[3];
                            valor_mega_5 = arr_valores_megas[4];

                            if (valor_mega1 > 0)
                            {
                                _liq_comision_asesor_e.valor_mega_1 = valor_mega_1;
                                _liq_comision_asesor_e.total_valor_mega_1 = valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1;

                            }
                            if (valor_mega_2 > 0)
                            {
                                _liq_comision_asesor_e.valor_mega_2 = valor_mega_2;
                                _liq_comision_asesor_e.total_valor_mega_2 = valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2;
                            }
                            if (valor_mega3 > 0)
                            {
                                _liq_comision_asesor_e.valor_mega_3 = valor_mega_3;
                                _liq_comision_asesor_e.total_valor_mega_3 = valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3;
                            }
                            if (valor_mega4 > 0)
                            {
                                _liq_comision_asesor_e.valor_mega_4 = valor_mega_4;
                                _liq_comision_asesor_e.total_valor_mega_4 = valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4;
                            }
                            if (valor_mega_5 > 0)
                            {
                                _liq_comision_asesor_e.valor_mega_5 = valor_mega_5;
                                _liq_comision_asesor_e.total_valor_mega_5 = valor_mega_5 * _liq_comision_asesor_e.numero_cant_megas_5;
                            }
                            double subTotalValorMegas = ((valor_mega_1 * _liq_comision_asesor_e.numero_cant_megas_1) +
                                                         (valor_mega_2 * _liq_comision_asesor_e.numero_cant_megas_2) +
                                                         (valor_mega_3 * _liq_comision_asesor_e.numero_cant_megas_3) +
                                                         (valor_mega_4 * _liq_comision_asesor_e.numero_cant_megas_4) +
                                                         (valor_mega_5 * _liq_comision_asesor_e.numero_cant_megas_5));

                            Int32 total_linea_basica = 0;
                            total_linea_basica = await sumarTotalLB(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                            _liq_comision_asesor_e.numero_venta_fibra_lb = total_linea_basica;

                            double valor_linea_basica = 0;
                            //validar esto atravez de un parametro
                            valor_linea_basica = await calculaValorLbTv(17, tabla_cumplimiento);
                            double subTotalLineaBasica = 0;
                            if (valor_linea_basica > 0)
                            {
                                _liq_comision_asesor_e.valor_venta_fibra_lb = valor_linea_basica;
                                subTotalLineaBasica = (total_linea_basica * valor_linea_basica);
                                _liq_comision_asesor_e.total_venta_fibra_lb = subTotalLineaBasica;
                            }
                            Int32 total_tv_fibra = 0;
                            total_tv_fibra = await sumarTotalTV(item.cedula_asesor, item.cedula_supervisor, periodo, item.cod_tipo_escala, TipoProceso);
                            _liq_comision_asesor_e.numero_venta_fibra_tv = total_tv_fibra;
                            double valor_tv_fibra = 0;
                            //validar esto atravez de un parametro
                            valor_tv_fibra = await calculaValorLbTv(18, tabla_cumplimiento);
                            double subTotalTvFibra = 0;
                            if (valor_tv_fibra > 0)
                            {
                                _liq_comision_asesor_e.valor_venta_fibra_tv = valor_tv_fibra;
                                subTotalTvFibra = (total_tv_fibra * valor_tv_fibra);
                                _liq_comision_asesor_e.total_venta_fibra_tv = subTotalTvFibra;
                            }
                            _liq_comision_asesor_e.sub_total_comision = subTotalValorMegas + subTotalLineaBasica + subTotalTvFibra;
                            _liq_comision_asesor_e.total_comision = _liq_comision_asesor_e.sub_total_comision;
                            _context.liq_comision_asesor.Update(_liq_comision_asesor_e);
                            int sa = await _context.SaveChangesAsync();
                            if (sa > 0)
                            {
                                (from lq_ in _context.liq_tmp_base_cierre
                                 where lq_.velocidad > 0 && lq_.cedula_asesor == item.cedula_asesor
                                 && lq_.cedula_supervisor == item.cedula_supervisor
                                 && lq_.periodo == periodo && lq_.estado == 1
                                 && lq_.EsProcesado == 0
                                 select lq_).ToList()
                                .ForEach(x => x.EsProcesado = 1);
                                _context.SaveChanges();

                            }
                        }
                    }
                }
            }

            return mensaje;

        }
        public List<listar_pap_resumida> listar_call_resumidas_v2()
        {
            List<listar_pap_resumida> _listar_pap_resumida = new List<listar_pap_resumida>();
            try
            {
                string query = "select cumplimiento,nivel_cumplimiento ,homologa_cumplimiento " +
                               "from liq_pap where codigo_liq_esq in (3) " +
                               "group by cumplimiento, nivel_cumplimiento, homologa_cumplimiento " +
                               "order by nivel_cumplimiento";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _listar_pap_resumida.Add(new listar_pap_resumida
                                {
                                    cumplimiento = sdr["cumplimiento"] + "",
                                    homologa_cumplimiento = sdr["homologa_cumplimiento"] + "",
                                    nivel_cumplimiento = Convert.ToInt32(sdr["nivel_cumplimiento"])
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar pap resumida", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return _listar_pap_resumida;
        }
        public List<listar_pap_resumida> listar_call_resumidas()
        {
            List<listar_pap_resumida> _listar_pap_resumida = new List<listar_pap_resumida>();
            try
            {
                string query = "select cumplimiento,nivel ,homologa_cumplimiento " +
                               " from liq_esquema_call group by cumplimiento, nivel, homologa_cumplimiento " +
                               " order by nivel";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _listar_pap_resumida.Add(new listar_pap_resumida
                                {
                                    cumplimiento = sdr["cumplimiento"] + "",
                                    homologa_cumplimiento = sdr["homologa_cumplimiento"] + "",
                                    nivel_cumplimiento = Convert.ToInt32(sdr["nivel"])
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar pap resumida", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return _listar_pap_resumida;
        }

        public async Task<Int32> sumarTotalDuosCall(string cedula_asesor, string cedula_supervisor, string periodo, Int32 cod_tipo_esquema, Int32 tipoProceso)
        {

            Int32 total_duos = 0;
            try
            {
                string cuentaUnidad = "";
                if (tipoProceso == 1)
                {
                    cuentaUnidad = " and unidad > 0";
                }
                string query = "select isnull(sum(unidad),0) as cantidad from liq_tmp_base_cierre " +
                              "  where cedula_asesor = '" + cedula_asesor + "' and cedula_supervisor = '" + cedula_supervisor + "'" +
                              "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                              "  and empaqhomo = 'DUO' and producto in ('Fixed Line Voice','Broadband Service')" +
                              "  and estado = 1 " + cuentaUnidad;
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                total_duos = Convert.ToInt32(sdr["cantidad"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Sumar cantidad duos", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return total_duos;
        }
        public async Task<int[]> calcularCantidadesMegasCall(string cedula_asesor,
                                                      string cedula_supervisor,
                                                      string periodo,
                                                      Int32 tipo_esquema,
                                                      Int32 valor_velocidad1,
                                                      Int32 valor_velocidad2,
                                                      Int32 valor_velocidad3,
                                                      Int32 valor_velocidad4,
                                                      Int32 valor_velocidad5
                                                      )
        {

            int[] array_cantidades = new int[5];
            try
            {
                Int32 cantidad_velocidad_1 = 0;
                Int32 cantidad_velocidad_2 = 0;
                Int32 cantidad_velocidad_3 = 0;
                Int32 cantidad_velocidad_4 = 0;
                Int32 cantidad_velocidad_5 = 0;


                string query = "select velocidad, isnull(sum(unidad),0) as unidades " +
                                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                                " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                                " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                                " and velocidad > 0 group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                //aqui validar
                                Int32 velocidad = Convert.ToInt32(sdr["velocidad"]);
                                if (velocidad == valor_velocidad1)
                                {
                                    cantidad_velocidad_1 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[0] = cantidad_velocidad_1;
                                }
                                else if (velocidad == valor_velocidad2)
                                {
                                    cantidad_velocidad_2 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[1] = cantidad_velocidad_2;
                                }
                                else if (velocidad == valor_velocidad3)
                                {
                                    cantidad_velocidad_3 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[2] = cantidad_velocidad_3;
                                }
                                else if (velocidad == valor_velocidad4)
                                {
                                    cantidad_velocidad_4 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[3] = cantidad_velocidad_4;
                                }
                                else if (velocidad >= valor_velocidad5)
                                {
                                    cantidad_velocidad_5 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[4] = cantidad_velocidad_5;
                                }
                            }
                        }
                        con.Close();
                    }
                }

            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Cantidad Unidades Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return array_cantidades;
        }

        public async Task<int[]> calcularCantidadesMegasCallV2(string cedula_asesor,
                                                     string cedula_supervisor,
                                                     string periodo,
                                                     Int32 tipo_esquema,
                                                     Int32 valor_velocidad1,
                                                     Int32 valor_velocidad2,
                                                     Int32 valor_velocidad3,
                                                     Int32 valor_velocidad4
                                                     
                                                     )
        {

            int[] array_cantidades = new int[5];
            try
            {
                Int32 cantidad_velocidad_1 = 0;
                Int32 cantidad_velocidad_2 = 0;
                Int32 cantidad_velocidad_3 = 0;
                Int32 cantidad_velocidad_4 = 0;
                


                string query = "select velocidad, isnull(sum(unidad),0) as unidades " +
                                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                                " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                                " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                                " and velocidad > 0 group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                //aqui validar
                                Int32 velocidad = Convert.ToInt32(sdr["velocidad"]);
                                if (velocidad == valor_velocidad1)
                                {
                                    cantidad_velocidad_1 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[0] = cantidad_velocidad_1;
                                }
                                else if (velocidad == valor_velocidad2)
                                {
                                    cantidad_velocidad_2 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[1] = cantidad_velocidad_2;
                                }
                                else if (velocidad == valor_velocidad3)
                                {
                                    cantidad_velocidad_3 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[2] = cantidad_velocidad_3;
                                }
                                else if (velocidad >= valor_velocidad4)
                                {
                                    cantidad_velocidad_4 = Convert.ToInt32(sdr["unidades"]);
                                    array_cantidades[3] = cantidad_velocidad_4;
                                }
                                
                            }
                        }
                        con.Close();
                    }
                }

            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Cantidad Unidades Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return array_cantidades;
        }

        public async Task<Int32> numeroVentasAltaVelocidad(string cedula_asesor,
                                                           string cedula_supervisor,
                                                           string periodo,
                                                           string tipo_campana,
                                                           Int32 tipo_esquema,
                                                           Int32 TipoProceso)
        {
            Int32 suma = 0;
            try
            {
                string cuentaUnidad = "";
                if (TipoProceso == 1)
                {
                    cuentaUnidad = " and unidad > 0";
                }
                string query = "select isnull(sum(unidad),0) as suma from liq_tmp_base_cierre where cedula_asesor = '"+ cedula_asesor + "' " +
                               "and cedula_supervisor = '"+ cedula_supervisor + "' and periodo = '"+ periodo + "' and cod_tipo_esquema = "+ tipo_esquema + " " +
                               "and tipo_campana = '"+ tipo_campana + "' and empaqhomo in ('BA','DUO','TRIO') " +
                               "and velocidad > 0 "+cuentaUnidad;

                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                suma = Convert.ToInt32(sdr["suma"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "valores mega Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return suma;
        }     

        public async Task<double[]> calcularValorMegasCallV2(string cedula_asesor,
                                          string cedula_supervisor,
                                          string periodo,
                                          Int32 tipo_esquema,
                                          Int32 nivel_cumplimiento,
                                          Int32 cantidadFtth,
                                          Int32 valor_velocidad1,
                                          Int32 valor_velocidad2,
                                          Int32 valor_velocidad3,
                                          Int32 valor_velocidad4
                                          )
        {
            double[] arr_valores_mega = new double[4];
            double valor_mega_1 = 0;
            double valor_mega_2 = 0;
            double valor_mega_3 = 0;
            double valor_mega_4 = 0;
            try
            {
                List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                string query = "select velocidad, isnull(sum(unidad),0) as unidades " +
                                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                                " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                                " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                                " and velocidad > 0  group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                Int32 velocidad = Convert.ToInt32(sdr["velocidad"]);
                                if (_liq_valores_megas_ != null)
                                {
                                    Int32 codigo_valor_mega = 0;
                                    if (velocidad > 0)
                                    {
                                        var val_valor_mega = _liq_valores_megas_.Find(x => x.valor_mega == velocidad);
                                        if (val_valor_mega != null)
                                        {
                                            codigo_valor_mega = val_valor_mega.codigo_valor;
                                        }
                                        else
                                        {
                                            Int32 max_velocidad = _liq_valores_megas_.Select(x => x.valor_mega).Max();
                                            if (velocidad > max_velocidad)
                                            {
                                                codigo_valor_mega = _liq_valores_megas_.Select(x => x.codigo_valor).Max();
                                            }

                                        }
                                    }

                                    //LIQ PAP   
                                    liq_pap _liq_pap_ = await _context.liq_pap.Where(x => x.codigo_liq_esq == tipo_esquema
                                                                               && x.nivel_cumplimiento == nivel_cumplimiento
                                                                               && x.valor_mega == codigo_valor_mega).FirstOrDefaultAsync();


                                    if (_liq_pap_ != null)
                                    {
                                        if (velocidad == valor_velocidad1)
                                        {
                                            valor_mega_1 = _liq_pap_.valor;
                                            arr_valores_mega[0] = valor_mega_1;
                                        }else if(velocidad == valor_velocidad2)
                                        {
                                            valor_mega_2 = _liq_pap_.valor;
                                            arr_valores_mega[1] = valor_mega_2;
                                        }else if(velocidad == valor_velocidad3)
                                        {
                                            valor_mega_3 = _liq_pap_.valor;
                                            arr_valores_mega[2] = valor_mega_3;

                                        }else if(velocidad >= valor_velocidad4)
                                        {
                                            valor_mega_4 = _liq_pap_.valor;
                                            arr_valores_mega[3] = valor_mega_4;
                                        }
                                        
                                    }
                                }


                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "valores mega Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return arr_valores_mega;
        }

        public async Task<double[]> calcularValorMegasCall(string cedula_asesor,
                                          string cedula_supervisor,
                                          string periodo,
                                          Int32 tipo_esquema,
                                          Int32 nivel_cumplimiento,
                                          Int32 cantidadFtth,
                                          Int32 valor_velocidad1,
                                          Int32 valor_velocidad2,
                                          Int32 valor_velocidad3,
                                          Int32 valor_velocidad4,
                                          Int32 valor_velocidad5
                                         )

        {

            double[] arr_valores_mega = new double[5];
            double valor_mega_1 = 0;
            double valor_mega_2 = 0;
            double valor_mega_3 = 0;
            double valor_mega_4 = 0;
            double valor_mega_5 = 0;
            try
            {
                List<liq_escala_altas> _liq_escalas_altas = _context.liq_escala_altas.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                List<liq_valores_megabytes> _liq_valores_megas_ = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == tipo_esquema).ToList();
                string query = "select velocidad, isnull(sum(unidad),0) as unidades " +
                                " from liq_tmp_base_cierre where cedula_asesor = '" + cedula_asesor + "'" +
                                " and cedula_supervisor = '" + cedula_supervisor + "' and periodo = '" + periodo + "' " +
                                " and cod_tipo_esquema = " + tipo_esquema + " and estado = 1 " +
                                " and velocidad > 0  group by velocidad order by velocidad ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                Int32 velocidad = Convert.ToInt32(sdr["velocidad"]);
                               
                                if (_liq_valores_megas_ != null && _liq_escalas_altas != null)
                                {
                                    //Int32 codigo_valor = _liq_valores_mega_.codigo_valor;
                                    Int32 codigo_valor_mega = 0;
                                    if (velocidad > 0)
                                    {
                                        var val_valor_mega = _liq_valores_megas_.Find(x => x.valor_mega == velocidad);
                                        if (val_valor_mega != null)
                                        {
                                            codigo_valor_mega = val_valor_mega.codigo_valor;
                                        }
                                        else
                                        {
                                            Int32 max_velocidad = _liq_valores_megas_.Select(x => x.valor_mega).Max();
                                            if (velocidad > max_velocidad)
                                            {
                                                codigo_valor_mega = _liq_valores_megas_.Select(x => x.codigo_valor).Max();
                                            }

                                        }
                                    }

                                    liq_esquema_call _liq_esquema_call_e = _context.liq_esquema_call.Where(x => x.nivel == nivel_cumplimiento
                                                                                                           && x.codigo_tipo_internet == codigo_valor_mega
                                                                                                           && x.estado == 1).FirstOrDefault();

                                    if (_liq_esquema_call_e != null)
                                    {
                                        if (velocidad == valor_velocidad1)
                                        {
                                            valor_mega_1 = _liq_esquema_call_e.valor;
                                            arr_valores_mega[0] = valor_mega_1;
                                        }
                                        else if (velocidad == valor_velocidad2)
                                        {
                                            valor_mega_2 = _liq_esquema_call_e.valor;
                                            arr_valores_mega[1] = valor_mega_2;
                                        }
                                        else if (velocidad == valor_velocidad3)
                                        {
                                            valor_mega_3 = _liq_esquema_call_e.valor;
                                            arr_valores_mega[2] = valor_mega_3;
                                        }
                                        else if (velocidad == valor_velocidad4)
                                        {
                                            valor_mega_4 = _liq_esquema_call_e.valor;
                                            arr_valores_mega[3] = valor_mega_4;
                                        }
                                        else if (velocidad >= valor_velocidad5)
                                        {
                                            valor_mega_5 = _liq_esquema_call_e.valor;
                                            arr_valores_mega[4] = valor_mega_5;
                                        }
                                    }
                                }


                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "valores mega Asesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return arr_valores_mega;
        }

        public async Task<Int32> sumarTotalLB(string cedula_asesor, string cedula_supervisor, string periodo, Int32 cod_tipo_esquema, Int32 tipoProceso)
        {

            Int32 total_linea_basica = 0;
            try
            {
                string cuentaUnidad = "";
                if (tipoProceso == 1)
                {
                    cuentaUnidad = " and unidad > 0";
                }
                string query = "select isnull(sum(unidad),0) as cantidad from liq_tmp_base_cierre " +
                               "  where cedula_asesor = '" + cedula_asesor + "' and cedula_supervisor = '" + cedula_supervisor + "'" +
                               "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                               "  and producto = 'Fixed Line Voice' and estado = 1 " + cuentaUnidad;
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                total_linea_basica = Convert.ToInt32(sdr["cantidad"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Sumar cantidad naked", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return total_linea_basica;
        }

        public async Task<Int32> sumarTotalTV(string cedula_asesor, string cedula_supervisor, string periodo, Int32 cod_tipo_esquema, Int32 tipoProceso)
        {

            Int32 total_tv = 0;
            try
            {
                string cuentaUnidad = "";
                if (tipoProceso == 1)
                {
                    cuentaUnidad = " and unidad > 0";
                }
                string query = "select isnull(sum(unidad),0) as cantidad from liq_tmp_base_cierre " +
                               "  where cedula_asesor = '" + cedula_asesor + "' and cedula_supervisor = '" + cedula_supervisor + "'" +
                               "  and periodo = '" + periodo + "' and cod_tipo_esquema = " + cod_tipo_esquema + "" +
                               "  and producto = 'IPTV' and estado = 1 " + cuentaUnidad;
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                        {
                            while (sdr.Read())
                            {
                                total_tv = Convert.ToInt32(sdr["cantidad"]);
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Sumar cantidad naked", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));

            }
            return total_tv;
        }

        public async Task<double> calculaValorLbTv(Int32 tipoInternet, Int32 nivel) 
        {
            double valorLbTv = 0;
            try
            {
                liq_esquema_call _liq_esquema_call = await _context.liq_esquema_call.Where(x => x.codigo_tipo_internet == tipoInternet
                                                                                     && x.nivel == nivel
                                                                                     && x.estado == 1).FirstOrDefaultAsync();
                if(_liq_esquema_call != null)
                {
                    valorLbTv = _liq_esquema_call.valor;
                }
            }   
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "calcular valor tv/lb call", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return valorLbTv;
        }

        #endregion


        #region metodos supervisores -  recuperadores
        public string validarSaldosRecuperadores(string periodo, string usuario_)
        {
            string msgValid = "";
            List<liq_comision_recuperador> _liq_comision_recuperador = getTotalesRecuperadores(periodo);
            if(_liq_comision_recuperador.Count() > 0)
            {
                foreach(liq_comision_recuperador item in _liq_comision_recuperador)
                {
                    liq_comision_recuperador _liq_comision_recuperador_e = new liq_comision_recuperador();
                    _liq_comision_recuperador_e.cedula_recuperdor = item.cedula_recuperdor;
                    _liq_comision_recuperador_e.codigo_tipo_esquema = item.codigo_tipo_esquema;
                    _liq_comision_recuperador_e.periodo = periodo;
                    _liq_comision_recuperador_e.total_meta_fija = item.total_meta_fija;
                    _liq_comision_recuperador_e.total_altas_fija = item.total_altas_fija;
                    double total_ejecucion_fija = (item.total_altas_fija / item.total_meta_fija) * 100;
                    _liq_comision_recuperador_e.total_ejecucion_fija = total_ejecucion_fija;
                    _liq_comision_recuperador_e.porcentaje_ejecucion_fija = total_ejecucion_fija + " % ";
                    
                    double valor_pagar_fija = 0;

                    _liq_comision_recuperador_e.total_vendedores = item.total_vendedores;
                    _liq_comision_recuperador_e.sum_vendedores_cumplen = item.sum_vendedores_cumplen;
                    double vendedores_cumples = (item.sum_vendedores_cumplen / item.total_vendedores) * 100;
                    _liq_comision_recuperador_e.vendedores_cumplen = vendedores_cumples;
                    _liq_comision_recuperador_e.porcentaje_vendores_cumplen = vendedores_cumples + " % ";

                    double valor_pagar_vendedores = 0;

                    _liq_comision_recuperador_e.total_vendedores = item.total_meta_movil;
                    _liq_comision_recuperador_e.total_altas_movil = item.total_altas_movil;
                    double total_ejecucion_movil = (item.total_altas_movil / item.total_meta_movil) * 100;
                    _liq_comision_recuperador_e.total_ejecucion_movil = total_ejecucion_movil;
                    _liq_comision_recuperador_e.porcentaje_ejecucion_movil = total_ejecucion_movil + " % ";


                    //sumo el total de la ejecucion fija y los vendedores
                    double total_cumpliento_recuperadores = total_ejecucion_fija + vendedores_cumples;
                    List<liq_esquema_recuperador> _liq_esquema_recuperador = _context.liq_esquema_recuperador.Where(x => x.estado == 1).ToList();
                    //aqui tomamos el sub total de la comision
                    double sub_total_comision = procesa_comision_recurperador(_liq_esquema_recuperador, total_cumpliento_recuperadores);

                    List<liq_super_esquema_acelerador> _liq_super_esquema_acelerador = new List<liq_super_esquema_acelerador>();
                    _liq_super_esquema_acelerador = _context.liq_super_esquema_acelerador.Where(x => x.codigo_tipo_esquema == 6
                                                                                                && x.estado == 1).ToList();

                    string valor_factor_mult = "";
                    string aceleracion_desalerelacion = "";
                    double valor_num_factor_mult = 0;
                    if (_liq_super_esquema_acelerador.Count() > 0)
                    {
                        string[] recibe_esquema_acelerador = proceso_liq_esquema_acelerador(_liq_super_esquema_acelerador, total_cumpliento_recuperadores);
                        valor_factor_mult = recibe_esquema_acelerador[0];
                        aceleracion_desalerelacion = recibe_esquema_acelerador[1];
                    }
                    if (!string.IsNullOrEmpty(valor_factor_mult))
                    {
                        if (valor_factor_mult.Contains("-"))
                        {
                            string aux_valor_factor_mult = valor_factor_mult.Substring(1, 4);
                            valor_num_factor_mult = Double.Parse(aux_valor_factor_mult);
                        }
                        valor_num_factor_mult = Double.Parse(valor_factor_mult);
                    }
                    _liq_comision_recuperador_e.sub_total_comision = sub_total_comision;
                    
                    double descuento_comision = sub_total_comision * (valor_num_factor_mult / 100);
                    double total_comision = sub_total_comision + (sub_total_comision);
                    _liq_comision_recuperador_e.aceleracion_desaceleracion = aceleracion_desalerelacion;
                    _liq_comision_recuperador_e.total_comision = total_comision;
                    _liq_comision_recuperador_e.estado = 1;
                    _liq_comision_recuperador_e.usuario = usuario_;
                    _liq_comision_recuperador_e.fecha_creacion = DateTime.Now;
                    _liq_comision_recuperador_e.fecha_modificacion = DateTime.Now;
                    _context.liq_comision_recuperador.Add(_liq_comision_recuperador_e);
                    _context.SaveChanges();

                }
            }
            return msgValid;
        }

        public List<liq_comision_recuperador> getTotalesRecuperadores(string periodo)
        {
            List<liq_comision_recuperador> _liq_comision_recuperador = new List<liq_comision_recuperador>();
            string query = "select cedula_recuperador, " +
                           " codigo_tipo_esquema, " +
                           " SUM(numero_meta_fija) as total_metas_fijas, " +
                           " SUM(numero_altas_fija) as total_altas_fijas, " +
                           " sum(numero_vendedores) as total_vendedores, " +
                           " SUM(vendendores_cumplen) as total_vendedores_cumplen, " +
                           " sum(numero_meta_movil) as total_movil, " +
                           " SUM(numero_altas_movil) as total_altas_movil " +
                           " from liq_tmp_recuperadores where periodo = '" + periodo + "' " +
                           " group by cedula_recuperador,codigo_tipo_esquema";
            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _liq_comision_recuperador.Add(new liq_comision_recuperador
                            {
                                cedula_recuperdor      = reader["cedula_recuperador"]+"",
                                codigo_tipo_esquema    = Convert.ToInt32(reader["codigo_tipo_esquema"]),
                                total_meta_fija        = Convert.ToDouble(reader["total_metas_fijas"]),
                                total_altas_fija       = Convert.ToDouble(reader["total_altas_fijas"]),
                                total_vendedores       = Convert.ToDouble(reader["total_vendedores"]),
                                sum_vendedores_cumplen = Convert.ToDouble(reader["total_vendedores_cumplen"]),
                                total_meta_movil       = Convert.ToDouble(reader["total_movil"]),
                                total_altas_movil      = Convert.ToDouble(reader["total_altas_movil"])
                            });
                        }
                    }
                    con.Close();
                }
            }
            return _liq_comision_recuperador;
        }

        public double procesa_comision_recurperador(List<liq_esquema_recuperador> _liq_esquema_recuperador, double total_homologa_cumplimiento)
        {
            double valor_comision = 0;
            foreach(liq_esquema_recuperador item_sub in _liq_esquema_recuperador)
            {
                string homologa_meta_cum = "";
                homologa_meta_cum = item_sub.homologa_cumplimiento;
                string[] metas_cumpl = homologa_meta_cum.Split(',');
                double cumplimiento_1 = Convert.ToDouble(metas_cumpl[0]);
                double cumplimiento_2 = Convert.ToDouble(metas_cumpl[1]);

                double aux_total_homologa_cump = Math.Round(total_homologa_cumplimiento, 0);
                if(aux_total_homologa_cump >= cumplimiento_1 && aux_total_homologa_cump <= cumplimiento_2)
                {
                    valor_comision = item_sub.valor;
                }
            }
            return valor_comision;
        }

        #endregion

        public double validarSaldosPendientesNuncaPagos(string cedula_asesor, string tipo_operacion)
        {
            double valor_pendiente = 0;
            List<liq_pendientes_nunca_pagos> _liq_pendintes_nunca_pagos = _context.liq_pendientes_nunca_pagos.Where(x => x.cedula_asesor == cedula_asesor
                                                                                                                    && x.tipo_operacion == tipo_operacion
                                                                                                                    && x.pendiente == 1).ToList();
            if(_liq_pendintes_nunca_pagos != null)
            {
                valor_pendiente = _liq_pendintes_nunca_pagos.Sum(x => x.valor_pendiente);
            }
            return valor_pendiente;
        }
        public void saldarNuncaPagosPendientes(string  cedula_asesor, string periodo ,string zona, double valor, double residual, string tipo_operacion, string usuario)
        {
            List<liq_pendientes_nunca_pagos> _liq_pendientes_nunca_pagos = _context.liq_pendientes_nunca_pagos.Where(x => x.cedula_asesor == cedula_asesor
                                                                                                                     && x.pendiente == 1
                                                                                                                     && x.tipo_operacion == tipo_operacion
                                                                                                                     )
                                                                                                                     .OrderByDescending(y => y.id)
                                                                                                                     .ToList();
          
            int pendiente = 1;
            if(residual == 0)
            {
                pendiente = 0;
            }
            _context.liq_pendientes_nunca_pagos.RemoveRange(_liq_pendientes_nunca_pagos);
            liq_pendientes_nunca_pagos _liq_pendientes_nunca_pagos_e = new liq_pendientes_nunca_pagos();
            _liq_pendientes_nunca_pagos_e.cedula_asesor = cedula_asesor;
            _liq_pendientes_nunca_pagos_e.zona_asesor = zona;
            _liq_pendientes_nunca_pagos_e.periodo_np = periodo;
            _liq_pendientes_nunca_pagos_e.valor_pendiente = residual;
            _liq_pendientes_nunca_pagos_e.pendiente = pendiente;
            _liq_pendientes_nunca_pagos_e.estado = 1;
            _liq_pendientes_nunca_pagos_e.usuario = usuario;
            _liq_pendientes_nunca_pagos_e.fecha_creacion = DateTime.Now;
            _liq_pendientes_nunca_pagos_e.fecha_modificacion = DateTime.Now;
            _liq_pendientes_nunca_pagos_e.tipo_operacion = tipo_operacion;
            _context.liq_pendientes_nunca_pagos.Add(_liq_pendientes_nunca_pagos_e);
            //_context.SaveChanges();
          
        }
        public System.Boolean validarExisteEmpleado(string cedula)
        {
            System.Boolean Exite = false;
            int contar_empleado = _context.empleado.Where(x => x.cedula_emp == cedula).Count();
            if( contar_empleado > 0)
            {
                Exite = true;
            }
            return Exite;
        }
        public System.Boolean EsSoloNumero(string cadenaTexto)
        {
            System.Boolean TieneNumero = true;
            if (!string.IsNullOrEmpty(cadenaTexto))
            {
                foreach (char s in cadenaTexto)
                {
                    if (s < '0' || s > '9')
                    {
                        TieneNumero = false;
                    }
                }
            }
            else
            {
                TieneNumero = false;
            }
            
            return TieneNumero;
        }
        public Int64 consecutivo_lote_importe()
        {
            Int64 consecutivo = 0;
            if(_context.lote_importe.Count() > 0)
            {
                consecutivo = _context.lote_importe.Where(x => x.estado == 1).Select(x => x.consecutivo_lote).Max();
                consecutivo = consecutivo + 1;
            }
            else
            {
                consecutivo = 100;
            }
            return consecutivo;
        }

        public async Task<System.Boolean> validarExisteBaseCierre(liq_tmp_base_cierre _liq_base_tmp_cierre)
        {
            System.Boolean Existe = false;
            liq_tmp_base_cierre _liq_base_cierre_aux = new liq_tmp_base_cierre();
            _liq_base_cierre_aux = await _context.liq_tmp_base_cierre.Where(x => x.producto == _liq_base_tmp_cierre.producto
                                                                      && x.cedula_asesor == _liq_base_tmp_cierre.cedula_asesor
                                                                      && x.mes_seg == _liq_base_tmp_cierre.mes_seg
                                                                      && x.unidad == _liq_base_tmp_cierre.unidad
                                                                      && x.cod_peticion == _liq_base_tmp_cierre.cod_peticion
                                                                      && x.velocidad == _liq_base_tmp_cierre.velocidad
                                                                      && x.velocidad_ftth_rango == _liq_base_tmp_cierre.velocidad_ftth_rango
                                                                      && x.velocidad_pymes_rango == _liq_base_tmp_cierre.velocidad_pymes_rango
                                                                      && x.empaqhomo == _liq_base_tmp_cierre.empaqhomo
                                                                      && x.num_doc_cliente == _liq_base_tmp_cierre.num_doc_cliente
                                                                      && x.cedula_supervisor == _liq_base_tmp_cierre.cedula_supervisor
                                                                      && x.observacion == _liq_base_tmp_cierre.observacion
                                                                      && x.cod_tipo_esquema == _liq_base_tmp_cierre.cod_tipo_esquema
                                                                      && x.migracion_otro == _liq_base_tmp_cierre.migracion_otro
                                                                      && x.periodo == _liq_base_tmp_cierre.periodo
                                                                      && x.lote_importe == _liq_base_tmp_cierre.lote_importe
                                                                      && x.estado == _liq_base_tmp_cierre.estado
                                                                      && x.EsProcesado == _liq_base_tmp_cierre.EsProcesado
                                                                      && x.usuario == _liq_base_tmp_cierre.usuario
                                                                      && x.EsIngresado == _liq_base_tmp_cierre.EsIngresado).FirstOrDefaultAsync();
            if(_liq_base_cierre_aux != null)
            {
                string mensaje = "Existe un duplicado : "+_liq_base_cierre_aux.producto+" "+_liq_base_cierre_aux.cedula_asesor+" "+_liq_base_cierre_aux.cod_peticion;
                General.crearImprimeMensajeLog(mensaje, "validarExisteBaseCierre", _config.GetConnectionString("conexionDbPruebas"));
                Existe = true;
            }
            return Existe;
        }

        public double proceso_comision_supervisor(List<liq_esquema_supervisores> _Comision_Supervisors, 
                                                  double total_homologa_cumplimiento)
        {
            double valor_comision = 0;
            foreach (liq_esquema_supervisores item_sub_1 in _Comision_Supervisors)
            {
                string homologa_meta_cump = "";
                homologa_meta_cump = item_sub_1.homologa_meta_cumplimiento;
                string[] metas_cump = homologa_meta_cump.Split(',');
                double cumplimiento_1 = Convert.ToDouble(metas_cump[0]);
                double cumplimiento_2 = Convert.ToDouble(metas_cump[1]);

                double aux_total_homologa_cumplimiento = Math.Round(total_homologa_cumplimiento, 0);
                if (aux_total_homologa_cumplimiento >= cumplimiento_1 && aux_total_homologa_cumplimiento <= cumplimiento_2)
                {
                    valor_comision = item_sub_1.valor;
                }
            }
            return valor_comision;
        }

        public string[] proceso_liq_esquema_acelerador(List<liq_super_esquema_acelerador> _liq_super_esquema_acelerador, 
                                                       double total_homologa_cumplimiento)
        {
            string[] proceso_arr = new string[2];
            foreach (liq_super_esquema_acelerador item_sub_2 in _liq_super_esquema_acelerador)
            {
                string homologa_escala_fact_asesor = "";
                homologa_escala_fact_asesor = item_sub_2.homologa_escala_factor_asesor;
                string[] metas_escala_asesor = homologa_escala_fact_asesor.Split(',');
                double escala_1 = Convert.ToDouble(metas_escala_asesor[0]);
                double escala_2 = Convert.ToDouble(metas_escala_asesor[1]);
                double aux_porcentaje_cumpl_ftth = Math.Round(total_homologa_cumplimiento, 0);
                if (aux_porcentaje_cumpl_ftth >= escala_1 && aux_porcentaje_cumpl_ftth <= escala_2)
                {
                    proceso_arr[0] = item_sub_2.valor;
                    proceso_arr[1] = item_sub_2.aceleracion_desaceleracion;
                }
            }
            return proceso_arr;
        }

        public System.Boolean validoCartaMetaProceso(string cedula_asesor, string cedula_supervisor, string periodo, Int32 Codigo_Tipo_Esquema)
        {
            System.Boolean EsValido = true;
            liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == cedula_asesor
                                                                                            && x.cedula_supervisor == cedula_supervisor
                                                                                            && x.periodo == periodo
                                                                                            && x.codigo_tipo_escala == Codigo_Tipo_Esquema
                                                                                            && x.estado == 1).FirstOrDefault();
            if(_liq_comision_asesor_e == null)
            {
                EsValido = false;
            }
            return EsValido;
        }

        public System.Boolean validoAsesorEnCartaMeta(string cedula_asesor, string periodo, Int32 codigo_Tipo_Esquema)
        {
            System.Boolean EsValido = true;
            liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == cedula_asesor
                                                                                            && x.periodo == periodo
                                                                                            && x.codigo_tipo_escala == codigo_Tipo_Esquema
                                                                                            && x.estado == 1).FirstOrDefault();
            if (_liq_comision_asesor_e == null)
            {
                EsValido = false;
            }
            return EsValido;
        }


        public System.Boolean validoAsesorNoSupervisor(string cedula_asesor, string periodo, Int32 codigo_Tipo_Esquema)
        {
            System.Boolean EsValido = true;
            liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_supervisor == cedula_asesor
                                                                                            && x.periodo == periodo
                                                                                            && x.codigo_tipo_escala == codigo_Tipo_Esquema
                                                                                            && x.estado == 1).FirstOrDefault();
            if (_liq_comision_asesor_e == null)
            {
                EsValido = false;
            }
            return EsValido;
        }

        public  List<listar_no_precesados_mega_resumida> validoNoProcesadosMega(string perido)
        {
            List<listar_no_precesados_mega_resumida> _listar_no_precesados_mega_resumida = new List<listar_no_precesados_mega_resumida>();
            string query = "select cod_tipo_esquema, unidad, count(*) as totales " +
                           " from liq_tmp_base_cierre where EsValido = 0  and periodo = '"+ perido + "' " +
                           " group by cod_tipo_esquema, unidad";

            using(SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using(SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while(sdr.Read())
                        {
                            _listar_no_precesados_mega_resumida.Add(new listar_no_precesados_mega_resumida
                            {
                                cod_tipo_esquema = Convert.ToInt32(sdr["cod_tipo_esquema"]),
                                unidad = Convert.ToInt32(sdr["unidad"]),
                                totales = Convert.ToInt32(sdr["totales"])
                            });
                        }
                    }
                    con.Close();
                }
            }
            return _listar_no_precesados_mega_resumida;
        }

        public List<listar_no_precesados_mega_resumida> validoNoProcesadosMovil(string periodo)
        {
            List<listar_no_precesados_mega_resumida> _listar_no_precesados_mega_resumida = new List<listar_no_precesados_mega_resumida>();
            string query = "select codigo_tipo_escala, unidad, count(*) as totales " +
                           " from liq_tmp_altas_movil where EsValido = 0  and periodo = '"+ periodo + "' " +
                           " group by codigo_tipo_escala, unidad";
            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            _listar_no_precesados_mega_resumida.Add(new listar_no_precesados_mega_resumida
                            {
                                cod_tipo_esquema = Convert.ToInt32(sdr["codigo_tipo_escala"]),
                                unidad = Convert.ToInt32(sdr["unidad"]),
                                totales = Convert.ToInt32(sdr["totales"])
                            });
                        }
                    }
                    con.Close();
                    con.Close();
                }
            }
            return _listar_no_precesados_mega_resumida;
        }
    }
}
