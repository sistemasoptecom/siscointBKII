using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.ModelosQ;
using siscointBKII.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class liqConsultasController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;

        public liqConsultasController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpGet("listarLiqConsultas")]
        [Authorize]
        public IActionResult listarLiqConsultas()
        {
            List<liq_consultas> _liq_consultas = _context.liq_consultas.Where(x => x.estado == 1).ToList();
            return Ok(_liq_consultas);
        }

        [HttpPost("descargoArchivoConsula")]
        [Authorize]
        public IActionResult descargoArchivoConsula(dynamic data_recibe)
        {
            int tipo_proceso = 0;
            string periodo = "";
            var data_envia = new Object();
            string data_imprimo_mensaje = "";
            int validoPeriodo = 0;
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                tipo_proceso = Convert.ToInt32(datObject["codigoTipoComisionConsulta"]);
                periodo = Convert.ToString(datObject["periodoConsulta"]);
                //validamos que periodo exista
                liq_periodo_comision_v2 _liq_periodo_comision_v2_e = _context.liq_periodo_comision_v2.Where(x => x.periodo == periodo
                                                                                                            && x.estado == 1).FirstOrDefault();
                if (_liq_periodo_comision_v2_e != null) 
                {
                    validoPeriodo = 1;
                    switch (tipo_proceso)
                    {
                        case 1:
                            List<listar_liq_comisiones_asesor_supervisor> _listar_liq_comison_ase_super = _listar_comision_asesor_supervisor(periodo);
                            //data_imprimo_mensaje = "El tamaño de la lista es : "+_listar_liq_comison_ase_super.Count();
                            data_envia = _listar_liq_comison_ase_super;
                            break;
                        case 2:
                            break;
                        case 3:
                            List<listar_pendientes_nunca_pagos> _listar_pendientes_nunca_pagos = Listar_pendientes_nunca_pagos(periodo);
                            data_imprimo_mensaje = "El tamaño de la lista es : " + _listar_pendientes_nunca_pagos.Count();
                            General.crearImprimeMensajeLog(data_imprimo_mensaje, "Reporte Pendiente Nunca Pagos", _config.GetConnectionString("conexionDbPruebas"));
                            data_envia = _listar_pendientes_nunca_pagos;
                            break;
                        case 5:
                            List<listar_liq_comisiones_asesor_supervisor> _listar_liq_comison_ase_super_res = _listar_comision_asesor_supervisor_resumen(periodo);
                            data_envia = _listar_liq_comison_ase_super_res;
                            break;
                    }
                }
               

            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "descargoArchivoConsula", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }

            string json = JsonConvert.SerializeObject(new { Resultado = data_envia, Valor = validoPeriodo });
            return Ok(json);
        }

        [HttpPost("procesarSolicitudNp")]
        [Authorize]
        public IActionResult procesarSolicitudNp(dynamic data_recibe)
        {
            int tipo_proceso = 0;
            string periodo = "";
            var data_envia = new Object();
            int validoPeriodo = 0;
            string data_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            string resultado = "";
            List<listar_tmp_solicitud_np> _listar_tmp_solicitud_np = new List<listar_tmp_solicitud_np>();
            List<listar_liq_tmp_nunca_pagos_megas> _listar_liq_tmp_nunca_pagos = new List<listar_liq_tmp_nunca_pagos_megas>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                data_ = Convert.ToString(datObject["data"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                _listar_tmp_solicitud_np = JsonConvert.DeserializeObject<List<listar_tmp_solicitud_np>>(data_);


                //var validarTipo
                //validar los tipos de nunca pagos
                // que sea atravez del peticion megas y por movil atravez del imei
                //el otro tipo peticion es que sea promediada

                Boolean EsArchivoValido = false;
                var validar_perido_np = _listar_tmp_solicitud_np.Select(x => new {x.PERIODO_CM}).Distinct().ToList();
                var validar_tipo_operacion_unica = _listar_tmp_solicitud_np.Select(x => new {x.CALCULO}).Distinct().ToList();   
                var validar_tipo_comision = _listar_tmp_solicitud_np.Select(x => new {x.TIPO_OPERACION}).Distinct().ToList();

                if (validar_perido_np.Count() ==1 
                    && validar_tipo_operacion_unica.Count() == 1 
                    && validar_tipo_comision.Count() == 1)
                {
                    EsArchivoValido = true;
                    string tipo_calculo = validar_tipo_operacion_unica.Select(X => X.CALCULO).ToString();
                    string tipo_operacion = validar_tipo_comision.Select(x => x.TIPO_OPERACION).ToString(); 
                    string periodo_cm = validar_perido_np.Select(x => x.PERIODO_CM).ToString(); 
                    switch (tipo_calculo)
                    {
                        case "PROMEDIO":
                            _listar_liq_tmp_nunca_pagos = listarPendientesNuncaPagosPromedio(_listar_tmp_solicitud_np, tipo_operacion, periodo_cm);
                            break;
                        case "PETICION":
                            _listar_liq_tmp_nunca_pagos = listarPendientesNuncaPagosPeticion(_listar_tmp_solicitud_np, tipo_operacion, periodo_cm);
                            break;
                    }
                }
                else
                {
                    resultado = "EL TIPO DE ARCHIVO DEBE TENER UN PERIDO PARA SU LIQUIDACION Y/O DEBE TENER UN TIPO DE CALCULO UNICO";
                }

            }
            catch (Exception e) 
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "procesarSolicitudNp", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok();
        }


        [HttpPost("consultoCantidadLiq")]
        [Authorize]
        public IActionResult consultoCantidadLiq(dynamic data_recibe)
        {
            var data_envia = new Object();
            //int[] array_cantidades = new int[15];
            List<int> array_int = new List<int>();
            int codigoTipoEsquema = 0;
            string periodo = "";
            string parametroBusqueda = "";
            int altasMovil = 0;
            int altasMegas = 0;
            int procesadosNp = 0;
            int procesadosOc = 0;
            int PenalizaMegas = 0;
            int penalizaMovil = 0;
            int noProcesadoNp = 0;
            int noProcesadoOc = 0;
            int migracionMegas = 0;
            int migracionMovil = 0;
            int noProcesaMegas = 0;
            int noProcesaMovil = 0;
            int TotalMegas = 0;
            int TotalMovil = 0;
            int TotalNp = 0;
            int TotalOc = 0;
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                codigoTipoEsquema = Convert.ToInt32(datObject["codigoTipoEsquema"]);
                periodo = Convert.ToString(datObject["periodo"]);
                parametroBusqueda = Convert.ToString(datObject["parametroBusqueda"]);
                //altas de todas las megas
                altasMegas = _context.liq_tmp_base_cierre.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                && x.periodo == periodo
                                                                && x.estado == 1
                                                                && x.EsValido == 1
                                                                && x.unidad > 0).Count();
                array_int.Add(altasMegas);
                //altas de todas las moviles
                altasMovil = _context.liq_tmp_altas_movil.Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                && x.periodo == periodo
                                                                && x.estado == 1
                                                                && x.EsValido == 1
                                                                && x.unidad > 0).Count();
                array_int.Add(altasMovil);
                //procesados nunca pagos
                procesadosNp = _context.liq_tmp_nunca_pagos_megas.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                        && x.periodo == periodo
                                                                        && x.estado == 1
                                                                        && x.EsValido == 1).Count();
                array_int.Add(procesadosNp);
                //proceso otros conceptos
                procesadosOc = _context.liq_tmp_otros_conceptos.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                      && x.periodo == periodo
                                                                      && x.estado == 1
                                                                      && x.EsValido == 1).Count();
                array_int.Add(procesadosOc);
                //penalizaciones megas
                PenalizaMegas = _context.liq_tmp_base_cierre.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                   && x.periodo == periodo
                                                                   && x.estado == 1
                                                                   && x.EsValido == 1
                                                                   && x.unidad < 0).Count();
                array_int.Add(PenalizaMegas);
                //penalizaciones movil
                penalizaMovil = _context.liq_tmp_altas_movil.Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                   && x.periodo == periodo
                                                                   && x.estado == 1
                                                                   && x.EsValido == 1
                                                                   && x.unidad < 0).Count();
                array_int.Add(penalizaMovil);
                //nunca pagos no procesados
                noProcesadoNp = _context.liq_tmp_nunca_pagos_megas.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                        && x.periodo == periodo
                                                                        && x.estado == 1
                                                                        && x.EsValido == 0).Count();
                array_int.Add(noProcesadoNp);
                //no procesado otros conceptos
                noProcesadoOc = _context.liq_tmp_otros_conceptos.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                      && x.periodo == periodo
                                                                      && x.estado == 1
                                                                      && x.EsValido == 0).Count();
                array_int.Add(noProcesadoOc);
                //migraciones megas 
                migracionMegas  = _context.liq_tmp_base_cierre.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                   && x.periodo == periodo
                                                                   && x.estado == 1
                                                                   && x.EsValido == 1
                                                                   && x.unidad  == 0).Count();
                array_int.Add(migracionMegas);
                //migraciones movil
                migracionMovil = _context.liq_tmp_altas_movil.Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                    && x.periodo == periodo
                                                                    && x.estado == 1
                                                                    && x.EsValido == 1
                                                                    && x.unidad == 0).Count();
                array_int.Add(migracionMovil);
                //no procesados megas
                List<liq_tmp_base_cierre> _base_cierre = _context.liq_tmp_base_cierre.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                            && x.periodo == periodo
                                                                                            && x.estado == 1
                                                                                            && x.EsValido == 0
                                                                                            )
                                                                                      .ToList();
                List<liq_tmp_metas> _metas = _context.liq_tmp_metas.Where(x=> x.cod_tipo_escala == codigoTipoEsquema
                                                                          && x.periodo_importe == periodo
                                                                          && x.estado == 1)
                                                                   .ToList();
              
                noProcesaMegas = _base_cierre.Where(x => !_metas.Any(y => x.cedula_asesor == y.cedula_asesor)).Count();
                array_int.Add(noProcesaMegas);
                //no procesados movil
                noProcesaMovil = _context.liq_tmp_altas_movil.Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                    && x.periodo == periodo
                                                                    && x.estado == 1
                                                                    && x.EsValido == 0).Count();
                array_int.Add(noProcesaMovil);
                //total megas
                TotalMegas = _context.liq_tmp_base_cierre.Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                && x.periodo == periodo
                                                                && x.estado == 1).Count();
                array_int.Add(TotalMegas);
                //total movil
                TotalMovil = _context.liq_tmp_altas_movil.Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                && x.periodo == periodo
                                                                && x.estado == 1).Count();
                array_int.Add(TotalMovil);
                //total nunca pago
                TotalNp = _context.liq_tmp_nunca_pagos_megas.Where(x => x.cod_tipo_esquema == codigoTipoEsquema  
                                                                && x.periodo == periodo
                                                                && x.estado == 1).Count();
                array_int.Add(TotalNp);
                //total otros conceptos
                TotalOc = _context.liq_tmp_otros_conceptos.Where(x => x.cod_tipo_esquema == codigoTipoEsquema 
                                                                && x.periodo == periodo
                                                                && x.estado == 1).Count();
                array_int.Add(TotalOc);
                data_envia = array_int;
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "consultoCantidadLiq", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            //string json = JsonConvert.SerializeObject(new {Resultado = array_int });
            return Ok(array_int);
        }

        [HttpPost("descargarRptEsquemas")]
        [Authorize]
        public IActionResult descargarRptEsquemas(dynamic data_recibe)
        {
            var data_envia = new Object();
            string tipo_rpt = "";
            string periodo = "";
            string data_imprimo_mensaje = "";
            int codigoTipoEsquema = 0;
            string nombre_tipo_esquema = "";
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                tipo_rpt = Convert.ToString(datObject["tipo_rpt"]);
                periodo = Convert.ToString(datObject["periodo"]);
                codigoTipoEsquema = Convert.ToInt32(datObject["codigoTipoEsquema"]);
                switch (tipo_rpt)
                {
                    case "altasMegas":
                        List<liq_tmp_base_cierre> _liq_tmp_base_cierre_a = _context.liq_tmp_base_cierre
                                                                           .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                  && x.periodo == periodo
                                                                                  && x.estado == 1
                                                                                  && x.EsValido == 1
                                                                                  && x.unidad > 0).ToList();

                        nombre_tipo_esquema = get_nombre_tipo_esquema(codigoTipoEsquema);
                       
                        List<listar_tmp_base_cierre_v2> _listar_tmp_base_cierre = new List<listar_tmp_base_cierre_v2>();
                        foreach(liq_tmp_base_cierre item in _liq_tmp_base_cierre_a)
                        {
                            listar_tmp_base_cierre_v2 _lista_tmp_base_cierre_v2_e = new listar_tmp_base_cierre_v2();
                            _lista_tmp_base_cierre_v2_e.producto = item.producto;
                            _lista_tmp_base_cierre_v2_e.cedula_asesor = item.cedula_asesor;
                            string nombre_empleado = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _lista_tmp_base_cierre_v2_e.nombre_asesor = nombre_empleado;
                            _lista_tmp_base_cierre_v2_e.mes_seg = item.mes_seg;
                            _lista_tmp_base_cierre_v2_e.unidad = item.unidad;
                            _lista_tmp_base_cierre_v2_e.cod_peticion = item.cod_peticion;
                            _lista_tmp_base_cierre_v2_e.velocidad = item.velocidad;
                            _lista_tmp_base_cierre_v2_e.velocidad_ftth_rango = item.velocidad_ftth_rango;
                            _lista_tmp_base_cierre_v2_e.velocidad_pymes_rango = item.velocidad_pymes_rango;
                            _lista_tmp_base_cierre_v2_e.empaqhomo = item.empaqhomo;
                            _lista_tmp_base_cierre_v2_e.num_doc_cliente = item.num_doc_cliente;
                            _lista_tmp_base_cierre_v2_e.cedula_supervisor = item.cedula_supervisor;
                            string nombre_supervisor = General.getNombreCompletoEmpleado(item.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _lista_tmp_base_cierre_v2_e.nombre_supervisor = nombre_supervisor;
                            _lista_tmp_base_cierre_v2_e.observacion = item.observacion;
                            _lista_tmp_base_cierre_v2_e.cod_tipo_esquema = item.cod_tipo_esquema;
                            _lista_tmp_base_cierre_v2_e.migracion_otro = item.migracion_otro;
                            _lista_tmp_base_cierre_v2_e.periodo = item.periodo;
                            _lista_tmp_base_cierre_v2_e.lote_importe = item.lote_importe;
                            _lista_tmp_base_cierre_v2_e.estado = item.estado;
                            _lista_tmp_base_cierre_v2_e.EsProcesado = item.EsProcesado;
                            _lista_tmp_base_cierre_v2_e.EsIngresado = item.EsIngresado;
                            _lista_tmp_base_cierre_v2_e.usuario = item.usuario;
                            _lista_tmp_base_cierre_v2_e.tipo_campana = item.tipo_campana;
                            _lista_tmp_base_cierre_v2_e.EsValido = item.EsValido;
                            _lista_tmp_base_cierre_v2_e.nombre_tipo_esquema = nombre_tipo_esquema;
                            _listar_tmp_base_cierre.Add(_lista_tmp_base_cierre_v2_e);
                        }
                        data_envia = _listar_tmp_base_cierre;
                        break;
                    case "altasMovil":
                        List<liq_tmp_altas_movil> _liq_tmp_altas_movil_a = _context.liq_tmp_altas_movil
                                                                           .Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                                  && x.periodo == periodo
                                                                                  && x.estado == 1
                                                                                  && x.EsValido == 1
                                                                                  && x.unidad > 0).ToList();
                        nombre_tipo_esquema = get_nombre_tipo_esquema(codigoTipoEsquema);
                        List<listar_tmp_altas_movil_v2> _listar_tmp_altas_movil_v2_a = new List<listar_tmp_altas_movil_v2>();
                        foreach(liq_tmp_altas_movil item in _liq_tmp_altas_movil_a)
                        {
                            listar_tmp_altas_movil_v2 _listar_tmp_altas_movil_v2_e = new listar_tmp_altas_movil_v2();
                            _listar_tmp_altas_movil_v2_e.cedula_asesor = item.cedula_asesor;
                            string nombre_empleado = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_altas_movil_v2_e.nombre_asesor = nombre_empleado;
                            _listar_tmp_altas_movil_v2_e.cedula_supervisor = item.cedula_supervisor;
                            string nombre_supervisor = General.getNombreCompletoEmpleado(item.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_altas_movil_v2_e.nombre_supervisor = nombre_supervisor;
                            _listar_tmp_altas_movil_v2_e.unidad = item.unidad;
                            _listar_tmp_altas_movil_v2_e.valor = item.valor;
                            _listar_tmp_altas_movil_v2_e.periodo = item.periodo;
                            _listar_tmp_altas_movil_v2_e.observacion = item.observacion;
                            _listar_tmp_altas_movil_v2_e.estado = item.estado;
                            _listar_tmp_altas_movil_v2_e.EsProcesado = item.EsProcesado;
                            _listar_tmp_altas_movil_v2_e.lote_importe = item.lote_importe;
                            _listar_tmp_altas_movil_v2_e.usuario = item.usuario;
                            _listar_tmp_altas_movil_v2_e.cedula_cliente = item.cedula_cliente;
                            _listar_tmp_altas_movil_v2_e.imei = item.imei;
                            _listar_tmp_altas_movil_v2_e.celular = item.celular;
                            _listar_tmp_altas_movil_v2_e.codigo_tipo_escala = item.codigo_tipo_escala;
                            _listar_tmp_altas_movil_v2_e.EsValido = item.EsValido;
                            _listar_tmp_altas_movil_v2_e.fecha_creacion = item.fecha_creacion;
                            _listar_tmp_altas_movil_v2_e.fecha_modificacion = item.fecha_modificacion;
                            _listar_tmp_altas_movil_v2_e.nombre_tipo_esquema = nombre_tipo_esquema;
                            _listar_tmp_altas_movil_v2_a.Add(_listar_tmp_altas_movil_v2_e);
                        }
                        data_envia = _listar_tmp_altas_movil_v2_a;
                        break;
                    case "procesadosNp":
                        List<liq_tmp_nunca_pagos_megas> _liq_tmp_nunca_pagos_megas_ = _context.liq_tmp_nunca_pagos_megas
                                                                                      .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                             && x.periodo == periodo
                                                                                             && x.estado == 1
                                                                                             && x.EsValido == 1).ToList();
                        data_envia = _liq_tmp_nunca_pagos_megas_;
                        break;
                    case "procesadosOc":
                        List<liq_tmp_otros_conceptos> _liq_tmp_otros_conceptos_ = _context.liq_tmp_otros_conceptos
                                                                                    .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                           && x.periodo == periodo
                                                                                           && x.estado == 1
                                                                                           && x.EsValido == 1).ToList();
                        data_envia = _liq_tmp_otros_conceptos_;
                        break;
                    case "PenalizaMegas":
                        List<liq_tmp_base_cierre> _liq_tmp_base_cierre_p = _context.liq_tmp_base_cierre
                                                                            .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                   && x.periodo == periodo
                                                                                   && x.estado == 1
                                                                                   && x.EsValido == 1
                                                                                   && x.unidad < 0).ToList();
                        nombre_tipo_esquema = get_nombre_tipo_esquema(codigoTipoEsquema);
                        List<listar_tmp_base_cierre_v2> _listar_tmp_base_cierre_v2 = new List<listar_tmp_base_cierre_v2>();
                        foreach (liq_tmp_base_cierre item in _liq_tmp_base_cierre_p)
                        {
                            listar_tmp_base_cierre_v2 _lista_tmp_base_cierre_v2_e = new listar_tmp_base_cierre_v2();
                            _lista_tmp_base_cierre_v2_e.producto = item.producto;
                            _lista_tmp_base_cierre_v2_e.cedula_asesor = item.cedula_asesor;
                            string nombre_empleado = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _lista_tmp_base_cierre_v2_e.nombre_asesor = nombre_empleado;
                            _lista_tmp_base_cierre_v2_e.mes_seg = item.mes_seg;
                            _lista_tmp_base_cierre_v2_e.unidad = item.unidad;
                            _lista_tmp_base_cierre_v2_e.cod_peticion = item.cod_peticion;
                            _lista_tmp_base_cierre_v2_e.velocidad = item.velocidad;
                            _lista_tmp_base_cierre_v2_e.velocidad_ftth_rango = item.velocidad_ftth_rango;
                            _lista_tmp_base_cierre_v2_e.velocidad_pymes_rango = item.velocidad_pymes_rango;
                            _lista_tmp_base_cierre_v2_e.empaqhomo = item.empaqhomo;
                            _lista_tmp_base_cierre_v2_e.num_doc_cliente = item.num_doc_cliente;
                            _lista_tmp_base_cierre_v2_e.cedula_supervisor = item.cedula_supervisor;
                            string nombre_supervisor = General.getNombreCompletoEmpleado(item.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _lista_tmp_base_cierre_v2_e.nombre_supervisor = nombre_supervisor;
                            _lista_tmp_base_cierre_v2_e.observacion = item.observacion;
                            _lista_tmp_base_cierre_v2_e.cod_tipo_esquema = item.cod_tipo_esquema;
                            _lista_tmp_base_cierre_v2_e.migracion_otro = item.migracion_otro;
                            _lista_tmp_base_cierre_v2_e.periodo = item.periodo;
                            _lista_tmp_base_cierre_v2_e.lote_importe = item.lote_importe;
                            _lista_tmp_base_cierre_v2_e.estado = item.estado;
                            _lista_tmp_base_cierre_v2_e.EsProcesado = item.EsProcesado;
                            _lista_tmp_base_cierre_v2_e.EsIngresado = item.EsIngresado;
                            _lista_tmp_base_cierre_v2_e.usuario = item.usuario;
                            _lista_tmp_base_cierre_v2_e.tipo_campana = item.tipo_campana;
                            _lista_tmp_base_cierre_v2_e.EsValido = item.EsValido;
                            _lista_tmp_base_cierre_v2_e.nombre_tipo_esquema = nombre_tipo_esquema;
                            _listar_tmp_base_cierre_v2.Add(_lista_tmp_base_cierre_v2_e);
                        }
                        data_envia = _liq_tmp_base_cierre_p;
                        break;
                    case "penalizaMovil":
                        List<liq_tmp_altas_movil> _liq_tmp_altas_movil_p = _context.liq_tmp_altas_movil
                                                                            .Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                                   && x.periodo == periodo
                                                                                   && x.estado == 1
                                                                                   && x.EsValido == 1
                                                                                   && x.unidad < 0).ToList();

                        nombre_tipo_esquema = get_nombre_tipo_esquema(codigoTipoEsquema);
                        List<listar_tmp_altas_movil_v2> _listar_tmp_altas_movil_v2_p = new List<listar_tmp_altas_movil_v2>();
                        foreach (liq_tmp_altas_movil item in _liq_tmp_altas_movil_p)
                        {
                            listar_tmp_altas_movil_v2 _listar_tmp_altas_movil_v2_e = new listar_tmp_altas_movil_v2();
                            _listar_tmp_altas_movil_v2_e.cedula_asesor = item.cedula_asesor;
                            string nombre_empleado = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_altas_movil_v2_e.nombre_asesor = nombre_empleado;
                            _listar_tmp_altas_movil_v2_e.cedula_supervisor = item.cedula_supervisor;
                            string nombre_supervisor = General.getNombreCompletoEmpleado(item.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_altas_movil_v2_e.nombre_supervisor = nombre_supervisor;
                            _listar_tmp_altas_movil_v2_e.unidad = item.unidad;
                            _listar_tmp_altas_movil_v2_e.valor = item.valor;
                            _listar_tmp_altas_movil_v2_e.periodo = item.periodo;
                            _listar_tmp_altas_movil_v2_e.observacion = item.observacion;
                            _listar_tmp_altas_movil_v2_e.estado = item.estado;
                            _listar_tmp_altas_movil_v2_e.EsProcesado = item.EsProcesado;
                            _listar_tmp_altas_movil_v2_e.lote_importe = item.lote_importe;
                            _listar_tmp_altas_movil_v2_e.usuario = item.usuario;
                            _listar_tmp_altas_movil_v2_e.cedula_cliente = item.cedula_cliente;
                            _listar_tmp_altas_movil_v2_e.imei = item.imei;
                            _listar_tmp_altas_movil_v2_e.celular = item.celular;
                            _listar_tmp_altas_movil_v2_e.codigo_tipo_escala = item.codigo_tipo_escala;
                            _listar_tmp_altas_movil_v2_e.EsValido = item.EsValido;
                            _listar_tmp_altas_movil_v2_e.fecha_creacion = item.fecha_creacion;
                            _listar_tmp_altas_movil_v2_e.fecha_modificacion = item.fecha_modificacion;
                            _listar_tmp_altas_movil_v2_e.nombre_tipo_esquema = nombre_tipo_esquema;
                            _listar_tmp_altas_movil_v2_p.Add(_listar_tmp_altas_movil_v2_e);
                        }
                        data_envia = _listar_tmp_altas_movil_v2_p;
                        break;
                    case "noProcesadoNp":
                        List<liq_tmp_nunca_pagos_megas> _liq_tmp_nunca_pagos_megas_n = _context.liq_tmp_nunca_pagos_megas
                                                                                        .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                               && x.periodo == periodo
                                                                                               && x.estado == 1
                                                                                               && x.EsValido == 0).ToList();
                        List<listar_tmp_nunca_pagos_megas_v2> _listar_tmp_nunca_pagos_megas_v2 = new List<listar_tmp_nunca_pagos_megas_v2>();
                        foreach(liq_tmp_nunca_pagos_megas it in _liq_tmp_nunca_pagos_megas_n)
                        {
                            listar_tmp_nunca_pagos_megas_v2 _listar_tmp_nunca_pagos_megas_v2_e = new listar_tmp_nunca_pagos_megas_v2();
                            _listar_tmp_nunca_pagos_megas_v2_e.cedula_asesor = it.cedula_asesor;
                            _listar_tmp_nunca_pagos_megas_v2_e.nombre_asesor = General.getNombreCompletoEmpleado(it.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_nunca_pagos_megas_v2_e.cedula_supervisor = it.cedula_supervisor;    
                            _listar_tmp_nunca_pagos_megas_v2_e.nombre_supervisor = General.getNombreCompletoEmpleado(it.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_nunca_pagos_megas_v2_e.zona = it.zona;
                            _listar_tmp_nunca_pagos_megas_v2_e.periodo = it.periodo;
                            _listar_tmp_nunca_pagos_megas_v2_e.cod_tipo_esquema = it.cod_tipo_esquema;
                            _listar_tmp_nunca_pagos_megas_v2_e.observacion = it.observacion;    
                            _listar_tmp_nunca_pagos_megas_v2_e.total = it.total;
                            _listar_tmp_nunca_pagos_megas_v2_e.estado = it.estado;
                            _listar_tmp_nunca_pagos_megas_v2_e.EsProcesado = it.EsProcesado;    
                            _listar_tmp_nunca_pagos_megas_v2_e.lote_importe = it.lote_importe;  
                            _listar_tmp_nunca_pagos_megas_v2_e.usuario = it.usuario;
                            _listar_tmp_nunca_pagos_megas_v2_e.periodo_ant = it.periodo_ant;
                            _listar_tmp_nunca_pagos_megas_v2_e.tipo_operacion = it.tipo_operacion;
                            _listar_tmp_nunca_pagos_megas_v2_e.EsValido = it.EsValido;  
                            _listar_tmp_nunca_pagos_megas_v2_e.fecha_creacion = it.fecha_creacion;
                            _listar_tmp_nunca_pagos_megas_v2_e.fecha_modificacion = it.fecha_modificacion;
                            _listar_tmp_nunca_pagos_megas_v2.Add(_listar_tmp_nunca_pagos_megas_v2_e);
                        }
                        data_envia = _listar_tmp_nunca_pagos_megas_v2;
                        break;
                    case "noProcesadoOc":
                        List<liq_tmp_otros_conceptos> _liq_tmp_otros_conceptos_n = _context.liq_tmp_otros_conceptos
                                                                                  .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                         && x.periodo == periodo
                                                                                         && x.estado == 1
                                                                                         && x.EsValido == 0).ToList();
                        data_envia = _liq_tmp_otros_conceptos_n;
                        break;
                    case "migracionMegas":
                        List<liq_tmp_base_cierre> _liq_tmp_base_cierre_m = _context.liq_tmp_base_cierre
                                                                            .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                   && x.periodo == periodo
                                                                                   && x.estado == 1
                                                                                   && x.EsValido == 1
                                                                                   && x.unidad == 0).ToList();
                        nombre_tipo_esquema = get_nombre_tipo_esquema(codigoTipoEsquema);
                        List<listar_tmp_base_cierre_v2> _listar_tmp_base_cierre_v2_m = new List<listar_tmp_base_cierre_v2>();
                        foreach (liq_tmp_base_cierre item in _liq_tmp_base_cierre_m)
                        {
                            listar_tmp_base_cierre_v2 _lista_tmp_base_cierre_v2_e = new listar_tmp_base_cierre_v2();
                            _lista_tmp_base_cierre_v2_e.producto = item.producto;
                            _lista_tmp_base_cierre_v2_e.cedula_asesor = item.cedula_asesor;
                            string nombre_empleado = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _lista_tmp_base_cierre_v2_e.nombre_asesor = nombre_empleado;
                            _lista_tmp_base_cierre_v2_e.mes_seg = item.mes_seg;
                            _lista_tmp_base_cierre_v2_e.unidad = item.unidad;
                            _lista_tmp_base_cierre_v2_e.cod_peticion = item.cod_peticion;
                            _lista_tmp_base_cierre_v2_e.velocidad = item.velocidad;
                            _lista_tmp_base_cierre_v2_e.velocidad_ftth_rango = item.velocidad_ftth_rango;
                            _lista_tmp_base_cierre_v2_e.velocidad_pymes_rango = item.velocidad_pymes_rango;
                            _lista_tmp_base_cierre_v2_e.empaqhomo = item.empaqhomo;
                            _lista_tmp_base_cierre_v2_e.num_doc_cliente = item.num_doc_cliente;
                            _lista_tmp_base_cierre_v2_e.cedula_supervisor = item.cedula_supervisor;
                            string nombre_supervisor = General.getNombreCompletoEmpleado(item.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _lista_tmp_base_cierre_v2_e.nombre_supervisor = nombre_supervisor;
                            _lista_tmp_base_cierre_v2_e.observacion = item.observacion;
                            _lista_tmp_base_cierre_v2_e.cod_tipo_esquema = item.cod_tipo_esquema;
                            _lista_tmp_base_cierre_v2_e.migracion_otro = item.migracion_otro;
                            _lista_tmp_base_cierre_v2_e.periodo = item.periodo;
                            _lista_tmp_base_cierre_v2_e.lote_importe = item.lote_importe;
                            _lista_tmp_base_cierre_v2_e.estado = item.estado;
                            _lista_tmp_base_cierre_v2_e.EsProcesado = item.EsProcesado;
                            _lista_tmp_base_cierre_v2_e.EsIngresado = item.EsIngresado;
                            _lista_tmp_base_cierre_v2_e.usuario = item.usuario;
                            _lista_tmp_base_cierre_v2_e.tipo_campana = item.tipo_campana;
                            _lista_tmp_base_cierre_v2_e.EsValido = item.EsValido;
                            _lista_tmp_base_cierre_v2_e.nombre_tipo_esquema = nombre_tipo_esquema;
                            _listar_tmp_base_cierre_v2_m.Add(_lista_tmp_base_cierre_v2_e);
                        }
                            data_envia = _listar_tmp_base_cierre_v2_m;
                        break;
                    case "migracionMovil":
                        List<liq_tmp_altas_movil> _liq_tmp_altas_movil_m = _context.liq_tmp_altas_movil
                                                                            .Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                                   && x.periodo == periodo
                                                                                   && x.estado == 1
                                                                                   && x.EsValido == 1
                                                                                   && x.unidad == 0).ToList();

                        nombre_tipo_esquema = get_nombre_tipo_esquema(codigoTipoEsquema);
                        List<listar_tmp_altas_movil_v2> _listar_tmp_altas_movil_v2_m = new List<listar_tmp_altas_movil_v2>();
                        foreach (liq_tmp_altas_movil item in _liq_tmp_altas_movil_m)
                        {
                            listar_tmp_altas_movil_v2 _listar_tmp_altas_movil_v2_e = new listar_tmp_altas_movil_v2();
                            _listar_tmp_altas_movil_v2_e.cedula_asesor = item.cedula_asesor;
                            string nombre_empleado = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_altas_movil_v2_e.nombre_asesor = nombre_empleado;
                            _listar_tmp_altas_movil_v2_e.cedula_supervisor = item.cedula_supervisor;
                            string nombre_supervisor = General.getNombreCompletoEmpleado(item.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_altas_movil_v2_e.nombre_supervisor = nombre_supervisor;
                            _listar_tmp_altas_movil_v2_e.unidad = item.unidad;
                            _listar_tmp_altas_movil_v2_e.valor = item.valor;
                            _listar_tmp_altas_movil_v2_e.periodo = item.periodo;
                            _listar_tmp_altas_movil_v2_e.observacion = item.observacion;
                            _listar_tmp_altas_movil_v2_e.estado = item.estado;
                            _listar_tmp_altas_movil_v2_e.EsProcesado = item.EsProcesado;
                            _listar_tmp_altas_movil_v2_e.lote_importe = item.lote_importe;
                            _listar_tmp_altas_movil_v2_e.usuario = item.usuario;
                            _listar_tmp_altas_movil_v2_e.cedula_cliente = item.cedula_cliente;
                            _listar_tmp_altas_movil_v2_e.imei = item.imei;
                            _listar_tmp_altas_movil_v2_e.celular = item.celular;
                            _listar_tmp_altas_movil_v2_e.codigo_tipo_escala = item.codigo_tipo_escala;
                            _listar_tmp_altas_movil_v2_e.EsValido = item.EsValido;
                            _listar_tmp_altas_movil_v2_e.fecha_creacion = item.fecha_creacion;
                            _listar_tmp_altas_movil_v2_e.fecha_modificacion = item.fecha_modificacion;
                            _listar_tmp_altas_movil_v2_e.nombre_tipo_esquema = nombre_tipo_esquema;
                            _listar_tmp_altas_movil_v2_m.Add(_listar_tmp_altas_movil_v2_e);
                        }
                        data_envia = _listar_tmp_altas_movil_v2_m;
                        break;
                    case "noProcesaMegas":
                        List<listar_tmp_base_cierre_v2> _listar_tmp_base_cierre_v2_npm = new List<listar_tmp_base_cierre_v2>();
                        _listar_tmp_base_cierre_v2_npm = Listar_no_procesados_megas(periodo, codigoTipoEsquema);
                        data_imprimo_mensaje = "Tamaño de la lista es : "+ _listar_tmp_base_cierre_v2_npm.Count();
                        General.crearImprimeMensajeLog(data_imprimo_mensaje, "Reporte Megas", _config.GetConnectionString("conexionDbPruebas"));

                        data_envia = _listar_tmp_base_cierre_v2_npm;
                        break;
                    case "noProcesaMovil":
                        List<listar_tmp_altas_movil_v2> _listar_tmp_altas_movil_v2 = new List<listar_tmp_altas_movil_v2>();
                        _listar_tmp_altas_movil_v2 = Listar_no_procesados_movil(periodo, codigoTipoEsquema);
                        data_imprimo_mensaje = "Tamaño de la lista es : " + _listar_tmp_altas_movil_v2.Count();
                        General.crearImprimeMensajeLog(data_imprimo_mensaje, "Reporte Movil", _config.GetConnectionString("conexionDbPruebas"));
                        data_envia = _listar_tmp_altas_movil_v2;
                        break;
                    case "TotalMegas":
                        List<liq_tmp_base_cierre> _liq_tmp_base_cierre_t = _context.liq_tmp_base_cierre
                                                                          .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                 && x.periodo == periodo
                                                                                 && x.estado == 1).ToList();
                        nombre_tipo_esquema = get_nombre_tipo_esquema(codigoTipoEsquema);
                        List<listar_tmp_base_cierre_v2> _listar_tmp_base_cierre_v2_t = new List<listar_tmp_base_cierre_v2>();
                        foreach (liq_tmp_base_cierre item in _liq_tmp_base_cierre_t)
                        {
                            listar_tmp_base_cierre_v2 _lista_tmp_base_cierre_v2_e = new listar_tmp_base_cierre_v2();
                            _lista_tmp_base_cierre_v2_e.producto = item.producto;
                            _lista_tmp_base_cierre_v2_e.cedula_asesor = item.cedula_asesor;
                            string nombre_empleado = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _lista_tmp_base_cierre_v2_e.nombre_asesor = nombre_empleado;
                            _lista_tmp_base_cierre_v2_e.mes_seg = item.mes_seg;
                            _lista_tmp_base_cierre_v2_e.unidad = item.unidad;
                            _lista_tmp_base_cierre_v2_e.cod_peticion = item.cod_peticion;
                            _lista_tmp_base_cierre_v2_e.velocidad = item.velocidad;
                            _lista_tmp_base_cierre_v2_e.velocidad_ftth_rango = item.velocidad_ftth_rango;
                            _lista_tmp_base_cierre_v2_e.velocidad_pymes_rango = item.velocidad_pymes_rango;
                            _lista_tmp_base_cierre_v2_e.empaqhomo = item.empaqhomo;
                            _lista_tmp_base_cierre_v2_e.num_doc_cliente = item.num_doc_cliente;
                            _lista_tmp_base_cierre_v2_e.cedula_supervisor = item.cedula_supervisor;
                            string nombre_supervisor = General.getNombreCompletoEmpleado(item.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _lista_tmp_base_cierre_v2_e.nombre_supervisor = nombre_supervisor;
                            _lista_tmp_base_cierre_v2_e.observacion = item.observacion;
                            _lista_tmp_base_cierre_v2_e.cod_tipo_esquema = item.cod_tipo_esquema;
                            _lista_tmp_base_cierre_v2_e.migracion_otro = item.migracion_otro;
                            _lista_tmp_base_cierre_v2_e.periodo = item.periodo;
                            _lista_tmp_base_cierre_v2_e.lote_importe = item.lote_importe;
                            _lista_tmp_base_cierre_v2_e.estado = item.estado;
                            _lista_tmp_base_cierre_v2_e.EsProcesado = item.EsProcesado;
                            _lista_tmp_base_cierre_v2_e.EsIngresado = item.EsIngresado;
                            _lista_tmp_base_cierre_v2_e.usuario = item.usuario;
                            _lista_tmp_base_cierre_v2_e.tipo_campana = item.tipo_campana;
                            _lista_tmp_base_cierre_v2_e.EsValido = item.EsValido;
                            _lista_tmp_base_cierre_v2_e.nombre_tipo_esquema = nombre_tipo_esquema;
                            _listar_tmp_base_cierre_v2_t.Add(_lista_tmp_base_cierre_v2_e);
                        }
                        data_envia = _liq_tmp_base_cierre_t;
                        break;
                    case "TotalMovil":
                        List<liq_tmp_altas_movil> _liq_tmp_altas_movil_t = _context.liq_tmp_altas_movil
                                                                          .Where(x => x.codigo_tipo_escala == codigoTipoEsquema
                                                                                 && x.periodo == periodo
                                                                                 && x.estado == 1).ToList();

                        nombre_tipo_esquema = get_nombre_tipo_esquema(codigoTipoEsquema);
                        List<listar_tmp_altas_movil_v2> _listar_tmp_altas_movil_v2_t = new List<listar_tmp_altas_movil_v2>();
                        foreach (liq_tmp_altas_movil item in _liq_tmp_altas_movil_t)
                        {
                            listar_tmp_altas_movil_v2 _listar_tmp_altas_movil_v2_e = new listar_tmp_altas_movil_v2();
                            _listar_tmp_altas_movil_v2_e.cedula_asesor = item.cedula_asesor;
                            string nombre_empleado = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_altas_movil_v2_e.nombre_asesor = nombre_empleado;
                            _listar_tmp_altas_movil_v2_e.cedula_supervisor = item.cedula_supervisor;
                            string nombre_supervisor = General.getNombreCompletoEmpleado(item.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                            _listar_tmp_altas_movil_v2_e.nombre_supervisor = nombre_supervisor;
                            _listar_tmp_altas_movil_v2_e.unidad = item.unidad;
                            _listar_tmp_altas_movil_v2_e.valor = item.valor;
                            _listar_tmp_altas_movil_v2_e.periodo = item.periodo;
                            _listar_tmp_altas_movil_v2_e.observacion = item.observacion;
                            _listar_tmp_altas_movil_v2_e.estado = item.estado;
                            _listar_tmp_altas_movil_v2_e.EsProcesado = item.EsProcesado;
                            _listar_tmp_altas_movil_v2_e.lote_importe = item.lote_importe;
                            _listar_tmp_altas_movil_v2_e.usuario = item.usuario;
                            _listar_tmp_altas_movil_v2_e.cedula_cliente = item.cedula_cliente;
                            _listar_tmp_altas_movil_v2_e.imei = item.imei;
                            _listar_tmp_altas_movil_v2_e.celular = item.celular;
                            _listar_tmp_altas_movil_v2_e.codigo_tipo_escala = item.codigo_tipo_escala;
                            _listar_tmp_altas_movil_v2_e.EsValido = item.EsValido;
                            _listar_tmp_altas_movil_v2_e.fecha_creacion = item.fecha_creacion;
                            _listar_tmp_altas_movil_v2_e.fecha_modificacion = item.fecha_modificacion;
                            _listar_tmp_altas_movil_v2_e.nombre_tipo_esquema = nombre_tipo_esquema;
                            _listar_tmp_altas_movil_v2_t.Add(_listar_tmp_altas_movil_v2_e);
                        }
                        data_envia = _listar_tmp_altas_movil_v2_t;
                        break;
                    case "TotalNp":
                        List<liq_tmp_nunca_pagos_megas> _liq_tmp_nunca_pagos_megas_t = _context.liq_tmp_nunca_pagos_megas
                                                                                        .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                               && x.periodo == periodo
                                                                                               && x.estado == 1).ToList();
                        data_envia = _liq_tmp_nunca_pagos_megas_t;
                        break;
                    case "TotalOc":
                        List<liq_tmp_otros_conceptos> _liq_tmp_otros_conceptos_t = _context.liq_tmp_otros_conceptos
                                                                                    .Where(x => x.cod_tipo_esquema == codigoTipoEsquema
                                                                                           && x.periodo == periodo
                                                                                           && x.estado == 1).ToList();
                        data_envia = _liq_tmp_otros_conceptos_t;
                        break;
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "descargarRptEsquemas", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            //string json = JsonConvert.SerializeObject(new { Resultado = data_envia });
            return Ok(data_envia);
        }

        public List<listar_liq_comisiones_asesor_supervisor> _listar_comision_asesor_supervisor_resumen(string periodo)
        {
            List<listar_liq_comisiones_asesor_supervisor> _listar_comision_asesor_supervisor = new List<listar_liq_comisiones_asesor_supervisor>();
            string query = "select lqa.cedula_asesor,(select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido)  from empleado where cedula_emp = lqa.cedula_asesor) " +
                           " as nombre_completo, (select 'ASESOR') as tipo, isnull((select ciudad from  liq_tmp_metas where cedula_asesor = lqa.cedula_asesor and periodo_importe = '" + periodo + "'),'#N/A') as ciudad, " +
                           " lqa.zona, (select lqa.numero_cant_megas_1+ lqa.numero_cant_megas_2+lqa.numero_cant_megas_3+lqa.numero_cant_megas_4+lqa.numero_cant_megas_5+lqa.numero_cant_mega_6) as cantidad_ftth, " +
                           " (select lqa.total_valor_mega_1+lqa.total_valor_mega_2+lqa.total_valor_mega_3+lqa.total_valor_mega_4+lqa.total_valor_mega_5+lqa.total_valor_mega_6) as total_valor_ftth,lqa.total_migracion, " +
                           " lqa.total_valor_duos,lqa.total_valor_trios,lqa.total_valor_naked,lqa.numero_plan_movil, lqa.total_plan_movil, lqa.total_valor_preferencial,lqa.total_valor_dedicado,lqa.total_venta_base, " +
                           " lqa.total_venta_c2c,lqa.total_venta_alta_velocidad, lqa.sub_total_comision, lqa.total_nunca_pago_movil,lqa.total_otros_conceptos,lqa.total_nunca_pago, lqa.total_comision, " +
                           " (select esquema from liq_tipo_esquema where codigo_valor = lqa.codigo_tipo_escala) as esquema,(select emp.nombre from empresa emp inner join empleado empl on emp.id = empl.empresa " +
                           " where empl.cedula_emp = lqa.cedula_asesor) as empresa, lqa.periodo  from liq_comision_asesor lqa where lqa.periodo = '" + periodo + "' " +
                           " union all " +
                           " select lqs.cedula_supervisor, (select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido)  from empleado where cedula_emp = lqs.cedula_supervisor) as nombre_completo, " +
                           " (select 'SUPERVISOR') as tipo, isnull((select top(1)ciudad from  liq_tmp_metas where cedula_supervisor = lqs.cedula_supervisor and periodo_importe = '" + periodo + "'),'#N/A') as ciudad, " +
                           " (select top(1)zona from liq_comision_asesor where cedula_supervisor = lqs.cedula_supervisor) as zona, (select 0 ) as cantidad_ftth,(select 0 ) as total_valor_ftth, (select 0 ) as total_migracion, " +
                           " (select 0 ) as total_valor_duos,(select 0 ) as total_valor_trios,(select 0 ) as total_valor_naked,(select 0 ) as numero_plan_movil, (select 0 ) as total_plan_movil, (select 0 ) as total_valor_preferencial," +
                           " (select 0 ) as total_valor_dedicado,(select 0 ) as total_venta_base,(select 0 ) as total_venta_c2c,(select 0 ) as total_venta_alta_velocidad,lqs.comision, (select 0 ) as total_nunca_pago_movil," +
                           " (select 0 ) as total_otros_conceptos,(select 0) as total_nunca_pago, lqs.total_comision, (select esquema from liq_tipo_esquema where codigo_valor = lqs.codigo_tipo_esquema) as esquema," +
                           " (select emp.nombre from empresa emp inner join empleado empl on emp.id = empl.empresa where empl.cedula_emp = lqs.cedula_supervisor) as empresa, " +
                           " lqs.periodo from liq_comision_supervisor lqs where lqs.periodo = '" + periodo + "'";

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
                            _listar_comision_asesor_supervisor.Add(new listar_liq_comisiones_asesor_supervisor
                            {
                                cedula = sdr["cedula_asesor"] + "",
                                nombres = sdr["nombre_completo"] + "",
                                tipo = sdr["tipo"] + "",
                                ciudad = sdr["ciudad"] + "",
                                zona = sdr["zona"] + "",
                                subtotal_comision = Convert.ToDouble(sdr["sub_total_comision"]),
                                total_comision = Convert.ToDouble(sdr["total_comision"]),
                                tipo_esquema = sdr["esquema"] + "",
                                empresa = sdr["empresa"] + "",
                                periodo = sdr["periodo"] + ""
                            });
                        }
                    }
                    con.Close();
                }
            }

            return _listar_comision_asesor_supervisor;
        }

        public List<listar_liq_comisiones_asesor_supervisor> _listar_comision_asesor_supervisor(string perido)
        {
            List<listar_liq_comisiones_asesor_supervisor> _listar_comision_asesor_supervisor = new List<listar_liq_comisiones_asesor_supervisor>();

            //string query = "select (select 'ASESOR') as tipo, lqa.zona, lqa.cedula_asesor,(select concat(empleado.nombre,' ',empleado.snombre,' '," +
            //               "empleado.ppellido,' ',empleado.spellido)  from empleado where cedula_emp = lqa.cedula_asesor)as nombre_completo, " +
            //               "lqa.sub_total_comision, lqa.total_nunca_pago, lqa.total_comision,lqa.numero_plan_movil, lqa.total_plan_movil, " +
            //               "(select lqa.numero_cant_megas_1+ lqa.numero_cant_megas_2+lqa.numero_cant_megas_3+lqa.numero_cant_megas_4+lqa.numero_cant_megas_5+" +
            //               "lqa.numero_cant_mega_6) as cantidad_ftth,(select lqa.total_valor_mega_1+lqa.total_valor_mega_2+lqa.total_valor_mega_3+" +
            //               "lqa.total_valor_mega_4+lqa.total_valor_mega_5+lqa.total_valor_mega_6) as total_valor_ftth, (select esquema from liq_tipo_esquema where " +
            //               "codigo_valor = lqa.codigo_tipo_escala) as esquema,(select emp.nombre from empresa emp inner join empleado empl on emp.id = " +
            //               "empl.empresa where empl.cedula_emp = lqa.cedula_asesor) as empresa, lqa.periodo from liq_comision_asesor lqa where lqa.periodo = '" + perido + "'" +
            //               "union all " +
            //               "select  (select 'SUPERVISOR') as tipo, (select top(1)zona from liq_comision_asesor where cedula_supervisor = lqs.cedula_supervisor) " +
            //               "as zona, lqs.cedula_supervisor, (select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido)  " +
            //               "from empleado where cedula_emp = lqs.cedula_supervisor) as nombre_completo, lqs.total_comision, (select 0) as total_nunca_pago, " +
            //               "lqs.comision, (select 0 ) as numero_plan_movil,(select 0 ) as total_plan_movil, (select 0 ) as cantidad_ftth,(select 0 ) as total_valor_ftth, " +
            //               "(select esquema from liq_tipo_esquema where codigo_valor = lqs.codigo_tipo_esquema) as esquema, " +
            //               "(select emp.nombre from empresa emp inner join empleado empl on emp.id = empl.empresa where empl.cedula_emp = lqs.cedula_supervisor) as empresa, " +
            //               "lqs.periodo from liq_comision_supervisor lqs where lqs.periodo = '"+perido+"'";
            string query = "select lqa.cedula_asesor,(select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido)  from empleado where cedula_emp = lqa.cedula_asesor) " +
                           " as nombre_completo, (select 'ASESOR') as tipo, isnull((select ciudad from  liq_tmp_metas where cedula_asesor = lqa.cedula_asesor and periodo_importe = '"+ perido + "'),'#N/A') as ciudad, " +
                           " lqa.zona, (select lqa.numero_cant_megas_1+ lqa.numero_cant_megas_2+lqa.numero_cant_megas_3+lqa.numero_cant_megas_4+lqa.numero_cant_megas_5+lqa.numero_cant_mega_6) as cantidad_ftth, " +
                           " (select lqa.total_valor_mega_1+lqa.total_valor_mega_2+lqa.total_valor_mega_3+lqa.total_valor_mega_4+lqa.total_valor_mega_5+lqa.total_valor_mega_6) as total_valor_ftth,lqa.total_migracion, " +
                           " lqa.total_valor_duos,lqa.total_valor_trios,lqa.total_valor_naked,lqa.numero_plan_movil, lqa.total_plan_movil, lqa.total_valor_preferencial,lqa.total_valor_dedicado,lqa.total_venta_base, " +
                           " lqa.total_venta_c2c,lqa.total_venta_alta_velocidad, lqa.sub_total_comision, lqa.total_nunca_pago_movil,lqa.total_otros_conceptos,lqa.total_nunca_pago, lqa.total_comision, " +
                           " (select esquema from liq_tipo_esquema where codigo_valor = lqa.codigo_tipo_escala) as esquema,(select emp.nombre from empresa emp inner join empleado empl on emp.id = empl.empresa " +
                           " where empl.cedula_emp = lqa.cedula_asesor) as empresa, lqa.periodo  from liq_comision_asesor lqa where lqa.periodo = '"+ perido + "' and lqa.meta_asesor <> 100 " +
                           " union all " +
                           " select lqs.cedula_supervisor, (select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido)  from empleado where cedula_emp = lqs.cedula_supervisor) as nombre_completo, " +
                           " (select 'SUPERVISOR') as tipo, isnull((select top(1)ciudad from  liq_tmp_metas where cedula_supervisor = lqs.cedula_supervisor and periodo_importe = '"+perido+"'),'#N/A') as ciudad, " +
                           " (select top(1)zona from liq_comision_asesor where cedula_supervisor = lqs.cedula_supervisor) as zona, (select 0 ) as cantidad_ftth,(select 0 ) as total_valor_ftth, (select 0 ) as total_migracion, " +
                           " (select 0 ) as total_valor_duos,(select 0 ) as total_valor_trios,(select 0 ) as total_valor_naked,(select 0 ) as numero_plan_movil, (select 0 ) as total_plan_movil, (select 0 ) as total_valor_preferencial," +
                           " (select 0 ) as total_valor_dedicado,(select 0 ) as total_venta_base,(select 0 ) as total_venta_c2c,(select 0 ) as total_venta_alta_velocidad,lqs.comision, (select 0 ) as total_nunca_pago_movil," +
                           " (select 0 ) as total_otros_conceptos,(select 0) as total_nunca_pago, lqs.total_comision, (select esquema from liq_tipo_esquema where codigo_valor = lqs.codigo_tipo_esquema) as esquema," +
                           " (select emp.nombre from empresa emp inner join empleado empl on emp.id = empl.empresa where empl.cedula_emp = lqs.cedula_supervisor) as empresa, " +
                           " lqs.periodo from liq_comision_supervisor lqs where lqs.periodo = '"+perido+"'";

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
                            _listar_comision_asesor_supervisor.Add(new listar_liq_comisiones_asesor_supervisor
                            {
                                cedula                     = sdr["cedula_asesor"] + "",
                                nombres                    = sdr["nombre_completo"] + "",
                                tipo                       = sdr["tipo"] + "",
                               
                                ciudad                     = sdr["ciudad"]+"",
                                zona                       = sdr["zona"] + "",
                                total_ftth                 = Convert.ToInt32(sdr["cantidad_ftth"]),
                                valor_total_ftth           = Convert.ToDouble(sdr["total_valor_ftth"]),
                                valor_migracion            = Convert.ToDouble(sdr["total_migracion"]),
                                total_valor_duos           = Convert.ToDouble(sdr["total_valor_duos"]),
                                total_valor_trios          = Convert.ToDouble(sdr["total_valor_trios"]),
                                total_valor_naked          = Convert.ToDouble(sdr["total_valor_naked"]),
                                total_movil                = Convert.ToInt32(sdr["numero_plan_movil"]),
                                valor_total_movil          = Convert.ToDouble(sdr["total_plan_movil"]),
                                total_venta_base           = Convert.ToDouble(sdr["total_venta_base"]),
                                total_venta_c2c            = Convert.ToDouble(sdr["total_venta_c2c"]),
                                total_venta_alta_velocidad = Convert.ToDouble(sdr["total_venta_alta_velocidad"]),

                                subtotal_comision          = Convert.ToDouble(sdr["sub_total_comision"]),
                                total_nunca_pago_movil     = Convert.ToDouble(sdr["total_nunca_pago_movil"]),
                                total_otros_conceptos      = Convert.ToDouble(sdr["total_otros_conceptos"]),
                                nunca_pagos                = Convert.ToDouble(sdr["total_nunca_pago"]),
                                total_comision             = Convert.ToDouble(sdr["total_comision"]),
                               
                               
                                tipo_esquema               = sdr["esquema"]+"",
                                empresa                    = sdr["empresa"]+"",
                                periodo                    = sdr["periodo"]+""
                            });
                        }
                    }
                    con.Close();
                }
            }

            return _listar_comision_asesor_supervisor;
        }

        public List<listar_pendientes_nunca_pagos> Listar_pendientes_nunca_pagos(string periodo)
        {
            List<listar_pendientes_nunca_pagos> _listar_pendientes_nunca_pagos = new List<listar_pendientes_nunca_pagos>();
            List<liq_pendientes_nunca_pagos> _liq_pendientes_nunca_pagos = _context.liq_pendientes_nunca_pagos.Where(x => x.periodo_np == periodo 
                                                                                                                     && x.valor_pendiente > 0).ToList();
            foreach(liq_pendientes_nunca_pagos item in _liq_pendientes_nunca_pagos)
            {
                listar_pendientes_nunca_pagos _listar_pendientes_nunca_pagos_e = new listar_pendientes_nunca_pagos();
                _listar_pendientes_nunca_pagos_e.cedula_asesor = item.cedula_asesor;
                _listar_pendientes_nunca_pagos_e.nombre_asesor = General.getNombreCompletoEmpleado(item.cedula_asesor, _config.GetConnectionString("conexionDbPruebas"));
                _listar_pendientes_nunca_pagos_e.zona_asesor = item.zona_asesor;
                _listar_pendientes_nunca_pagos_e.periodo_np = item.periodo_np;
                _listar_pendientes_nunca_pagos_e.valor_pendiente = item.valor_pendiente;
                _listar_pendientes_nunca_pagos_e.pendiente = item.pendiente;
                _listar_pendientes_nunca_pagos_e.estado = item.estado;
                _listar_pendientes_nunca_pagos_e.usuario = item.usuario;
                _listar_pendientes_nunca_pagos_e.tipo_operacion = item.tipo_operacion;
                _listar_pendientes_nunca_pagos_e.fecha_creacion = item.fecha_creacion;
                _listar_pendientes_nunca_pagos_e.fecha_modificacion =   item.fecha_modificacion;
                _listar_pendientes_nunca_pagos.Add(_listar_pendientes_nunca_pagos_e);

            }
            return _listar_pendientes_nunca_pagos;
        }

        public List<listar_tmp_base_cierre_v2> Listar_no_procesados_megas(string periodo, Int32 CodigoTipoEsquema)
        {
            string nombre_tipo_esquema = get_nombre_tipo_esquema(CodigoTipoEsquema);
            //liq_tipo_esquema _liq_tipo_esquema_e = _context.liq_tipo_esquema.Where(x => x.codigo_valor == CodigoTipoEsquema).FirstOrDefault();
            //if(_liq_tipo_esquema_e != null)
            //{
            //    nombre_tipo_esquema = _liq_tipo_esquema_e.nombre_tipo_esquema;
            //}
            List<listar_tmp_base_cierre_v2> _listar_tmp_base_cierre_v2 = new List<listar_tmp_base_cierre_v2>();
            string query = "select lbc.producto,lbc.cedula_asesor, isnull((select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido) " +
                           " from empleado where cedula_emp = lbc.cedula_asesor),'#N/A') as nombre_asesor, lbc.mes_seg, lbc.unidad, lbc.cod_peticion, lbc.velocidad, " +
                           " lbc.velocidad_ftth_rango, lbc.velocidad_pymes_rango, lbc.empaqhomo, lbc.num_doc_cliente, lbc.cedula_supervisor,isnull((select concat(empleado.nombre,' ', " +
                           " empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido) from empleado where cedula_emp = lbc.cedula_supervisor),'#N/A') as nombre_supervisor, " +
                           " lbc.observacion, lbc.cod_tipo_esquema, lbc.migracion_otro, lbc.periodo, lbc.lote_importe, lbc.usuario, lbc.tipo_campana, lbc.EsValido " +
                           " from liq_tmp_base_cierre lbc where lbc.EsValido = 0 and lbc.periodo = '" + periodo + "' and lbc.cod_tipo_esquema = '" + CodigoTipoEsquema + "' and lbc.cedula_asesor " +
                           " not in (select cedula_asesor from liq_tmp_metas where periodo_importe = '" + periodo + "' and cod_tipo_escala = '" + CodigoTipoEsquema + "')";

            //string query = "select lbc.producto,lbc.cedula_asesor, isnull((select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido)  " +
            //               " from empleado where cedula_emp = lbc.cedula_asesor),'#N/A') as nombre_asesor, lbc.mes_seg, lbc.unidad, lbc.cod_peticion, lbc.velocidad,  lbc.velocidad_ftth_rango, " +
            //               " lbc.velocidad_pymes_rango, lbc.empaqhomo, lbc.num_doc_cliente, lbc.cedula_supervisor,isnull((select concat(empleado.nombre,' ',  empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido) " +
            //               " from empleado where cedula_emp = lbc.cedula_supervisor),'#N/A') as nombre_supervisor,  lbc.observacion, lbc.cod_tipo_esquema, lbc.migracion_otro, lbc.periodo, lbc.lote_importe, lbc.usuario, " +
            //               "lbc.tipo_campana, lbc.EsValido  from liq_tmp_base_cierre lbc where lbc.EsValido = 0 and lbc.periodo = '"+ periodo + "' and lbc.cod_tipo_esquema = '"+ CodigoTipoEsquema + "' and lbc.cedula_asesor  " +
            //               "not in (select cedula_asesor from liq_tmp_metas where periodo_importe = '"+ periodo + "' and cod_tipo_escala = '"+ CodigoTipoEsquema + "') " +
            //               "union all " +
            //               " select lbc.producto,lbc.cedula_asesor, isnull((select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido)  from empleado " +
            //               " where cedula_emp = lbc.cedula_asesor),'#N/A') as nombre_asesor, lbc.mes_seg, lbc.unidad, lbc.cod_peticion, lbc.velocidad,  lbc.velocidad_ftth_rango, lbc.velocidad_pymes_rango, " +
            //               " lbc.empaqhomo, lbc.num_doc_cliente, lbc.cedula_supervisor,isnull((select concat(empleado.nombre,' ',  empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido) from empleado " +
            //               " where cedula_emp = lbc.cedula_supervisor),'#N/A') as nombre_supervisor,  lbc.observacion, lbc.cod_tipo_esquema, lbc.migracion_otro, lbc.periodo, lbc.lote_importe, lbc.usuario, " +
            //               " lbc.tipo_campana, lbc.EsValido  from liq_tmp_base_cierre lbc where lbc.EsValido = 0 and lbc.periodo = '"+ periodo + "' and lbc.cod_tipo_esquema = '"+ CodigoTipoEsquema + "' and lbc.cedula_supervisor  " +
            //               " not in (select cedula_supervisor from liq_tmp_metas where periodo_importe = '"+ periodo + "' and cod_tipo_escala = '"+ CodigoTipoEsquema + "') ";
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
                            _listar_tmp_base_cierre_v2.Add(new listar_tmp_base_cierre_v2 { 
                                producto              = sdr["producto"]+"",
                                cedula_asesor         = sdr["cedula_asesor"]+"",
                                nombre_asesor         = sdr["nombre_asesor"]+"",
                                mes_seg               = Convert.ToInt32(sdr["mes_seg"]),
                                unidad                = Convert.ToInt32(sdr["unidad"]),
                                cod_peticion          = sdr["cod_peticion"]+"",
                                velocidad             = Convert.ToDouble(sdr["velocidad"]),
                                velocidad_ftth_rango  = Convert.ToInt32(sdr["velocidad_ftth_rango"]),
                                velocidad_pymes_rango = Convert.ToInt32(sdr["velocidad_pymes_rango"]),
                                empaqhomo             = sdr["empaqhomo"]+"",
                                num_doc_cliente       = sdr["num_doc_cliente"]+"",
                                cedula_supervisor     = sdr["cedula_supervisor"]+"",
                                nombre_supervisor     = sdr["nombre_supervisor"]+"",
                                observacion           = sdr["observacion"]+"",
                                cod_tipo_esquema      = Convert.ToInt32(sdr["cod_tipo_esquema"]),
                                migracion_otro        = sdr["migracion_otro"]+"",
                                periodo               = sdr["periodo"]+"",
                                lote_importe          = Convert.ToInt32(sdr["lote_importe"]),
                              
                                usuario               = sdr["usuario"]+"",
                                tipo_campana          = sdr["tipo_campana"]+"",
                                EsValido              = Convert.ToInt32(sdr["EsValido"]),
                                nombre_tipo_esquema   = nombre_tipo_esquema
                            });
                        }
                    }
                    con.Close();
                }
            }

            return _listar_tmp_base_cierre_v2;
        }

        public List<listar_tmp_altas_movil_v2> Listar_no_procesados_movil(string periodo , Int32 codigoTipoEsquema)
        {
            List<listar_tmp_altas_movil_v2> _listar_tmp_altas_movil_v2 = new List<listar_tmp_altas_movil_v2>();
            string query = "select lpa.cedula_asesor,isnull((select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ', " +
                           " empleado.spellido)  from empleado where cedula_emp = lpa.cedula_asesor),'#N/A') as nombre_asesor, lpa.cedula_supervisor, " +
                           " isnull((select concat(empleado.nombre,' ',empleado.snombre,' ',empleado.ppellido,' ',empleado.spellido)  from empleado " +
                           " where cedula_emp = lpa.cedula_supervisor),'#N/A') as nombre_supervisor, lpa.unidad, lpa.valor, lpa.periodo, lpa.observacion, " +
                           " lpa.estado, lpa.EsProcesado, lpa.lote_importe, lpa.usuario, lpa.cedula_cliente, lpa.imei, lpa.celular, lpa.codigo_tipo_escala, " +
                           " lpa.EsValido, lpa.fecha_creacion, lpa.fecha_modificacion from liq_tmp_altas_movil lpa where lpa.EsValido = 0 and  " +
                           " lpa.periodo = '"+ periodo + "' and lpa.cedula_asesor not in (select cedula_asesor from liq_tmp_metas where periodo_importe =  " +
                           " '"+ periodo + "' and estado = 1)";
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
                            _listar_tmp_altas_movil_v2.Add(new listar_tmp_altas_movil_v2
                            {
                                cedula_asesor      = sdr["cedula_asesor"]+"",
                                nombre_asesor      = sdr["nombre_asesor"]+"",
                                cedula_supervisor  = sdr["cedula_supervisor"]+"",
                                nombre_supervisor  = sdr["nombre_supervisor"]+"",
                                unidad             = Convert.ToInt32(sdr["unidad"]),
                                valor              = Convert.ToDouble(sdr["valor"]),
                                periodo            = sdr["periodo"]+"",
                                observacion        = sdr["observacion"]+"",
                                estado             = Convert.ToInt32(sdr["estado"]),
                                EsProcesado        = Convert.ToInt32(sdr["EsProcesado"]),
                                lote_importe       = Convert.ToInt32(sdr["lote_importe"]),
                                usuario            = sdr["usuario"]+"",
                                cedula_cliente     = sdr["cedula_cliente"]+"",
                                imei               = sdr["imei"]+"",
                                celular            = sdr["celular"]+"",
                                codigo_tipo_escala = Convert.ToInt32(sdr["codigo_tipo_escala"]),
                                EsValido           = Convert.ToInt32(sdr["EsValido"])
                                
                            });
                        }
                    }
                    con.Close();
                }
            }

            return _listar_tmp_altas_movil_v2;
        }

        public List<listar_liq_tmp_nunca_pagos_megas> _solicitud_nunca_pagos_prom(List<listar_tmp_solicitud_np> _listar_tmp_solicitud_np)
        {
            List<listar_liq_tmp_nunca_pagos_megas> _listar_liq_tmp_nunca_pagos_megas = new List<listar_liq_tmp_nunca_pagos_megas>();

            return _listar_liq_tmp_nunca_pagos_megas;
        }

        public string get_nombre_tipo_esquema(int codigoTipoEsquema)
        {
            string nombreTipoEsquema = "";
            liq_tipo_esquema _liq_tipo_esquema_e = _context.liq_tipo_esquema.Where(x => x.codigo_valor == codigoTipoEsquema).FirstOrDefault();
            if (_liq_tipo_esquema_e != null)
            {
                nombreTipoEsquema = _liq_tipo_esquema_e.esquema;
            }

            return nombreTipoEsquema;
        }
        public List<listar_liq_tmp_nunca_pagos_megas> listarPendientesNuncaPagosPromedio(List<listar_tmp_solicitud_np> listar_tmp_solicitud_np, 
                                                                                         string tipo_operacion, 
                                                                                         string periodo_cm)
        {
            List<listar_liq_tmp_nunca_pagos_megas> _listar_pendientes_nunca_pagos = new List<listar_liq_tmp_nunca_pagos_megas>();

            if(listar_tmp_solicitud_np.Count > 0)
            {
                foreach(listar_tmp_solicitud_np item in listar_tmp_solicitud_np)
                {
                    liq_tmp_solicitud_np _liq_tmp_solicitud_np_e =  new liq_tmp_solicitud_np();
                    _liq_tmp_solicitud_np_e.cedula_asesor           = item.CEDULA_ASESOR;
                    _liq_tmp_solicitud_np_e.nombre_asesor           = General.getNombreCompletoEmpleado(item.CEDULA_ASESOR, _config.GetConnectionString("conexionDbPruebas"));
                    _liq_tmp_solicitud_np_e.id_peticion             = item.ID_PETICION;
                    _liq_tmp_solicitud_np_e.imei                    = item.IMEI;
                    _liq_tmp_solicitud_np_e.tipo_operacion          = item.TIPO_OPERACION;
                    _liq_tmp_solicitud_np_e.tipo_operacion_peticion = item.TIPO_OPERACION_PETICION;
                    _liq_tmp_solicitud_np_e.periodo_np              = item.PERIODO_NP;
                    _liq_tmp_solicitud_np_e.periodo_cm              = item.PERIODO_CM;
                    _liq_tmp_solicitud_np_e.calculo                 = item.CALCULO;
                    _liq_tmp_solicitud_np_e.EsIngresado             = 0;
                    _liq_tmp_solicitud_np_e.Esprocesado             = 0;
                    _liq_tmp_solicitud_np_e.fecha_creacion          = DateTime.Now;
                    _liq_tmp_solicitud_np_e.fecha_modificacion      = DateTime.Now;
                    _context.liq_tmp_solicitud_np.Add(_liq_tmp_solicitud_np_e);
                    _context.SaveChanges();
                }
                //ahora se agrupan
                // CON EL TIPO DE OPERACION VALIDAR COMO SE PROMEDIAN POR MEGAS Y POR MOVIL
                List<listar_tmp_solicitud_np> _listar_tmp_solicitud_np = General.ListarPendientesNpgroup(periodo_cm, tipo_operacion, _config.GetConnectionString("conexionDbPruebas"));
                if(_listar_tmp_solicitud_np.Count > 0)
                {
                    foreach(listar_tmp_solicitud_np item in _listar_tmp_solicitud_np)
                    {
                        //si exite en la liquidacion comision asesor
                        double valor_total_comision = 0;
                        Int32 total_de_cantidad = 0;
                        double valor_nunca_pago = 0;
                        liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.periodo == item.PERIODO_NP 
                                                                                                        && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                                        && x.estado == 1
                                                                                                        && x.EsAsesorValido).FirstOrDefault();
                        if(_liq_comision_asesor_e != null)
                        {
                            //sumo los totales 
                            switch (tipo_operacion)
                            {
                                case "MEGAS":
                                    valor_total_comision = (_liq_comision_asesor_e.total_valor_mega_1 +
                                                   _liq_comision_asesor_e.total_valor_mega_2 +
                                                   _liq_comision_asesor_e.total_valor_mega_3 +
                                                   _liq_comision_asesor_e.total_valor_mega_4 +
                                                   _liq_comision_asesor_e.total_valor_mega_5 +
                                                   _liq_comision_asesor_e.total_valor_mega_6);
                                    total_de_cantidad = (_liq_comision_asesor_e.numero_cant_megas_1 +
                                                      _liq_comision_asesor_e.numero_cant_megas_2 +
                                                      _liq_comision_asesor_e.numero_cant_megas_3 +
                                                      _liq_comision_asesor_e.numero_cant_megas_4 +
                                                      _liq_comision_asesor_e.numero_cant_megas_5 +
                                                      _liq_comision_asesor_e.numero_cant_mega_6);
                                    

                                    break;
                                case "MOVIL":
                                    valor_total_comision = _liq_comision_asesor_e.total_plan_movil;
                                    total_de_cantidad = _liq_comision_asesor_e.numero_plan_movil;
                                    break;

                            }
                            valor_nunca_pago = (valor_total_comision / total_de_cantidad) * item.TOTAL;
                            //se llena el array
                            listar_liq_tmp_nunca_pagos_megas _listar_liq_tmp_nunca_pagos_megas_e = new listar_liq_tmp_nunca_pagos_megas();
                            _listar_liq_tmp_nunca_pagos_megas_e.CEDULA_ASESOR = item.CEDULA_ASESOR;
                            _listar_liq_tmp_nunca_pagos_megas_e.NOMBRE_ASESOR = General.getNombreCompletoEmpleado(item.CEDULA_ASESOR, _config.GetConnectionString("conexionDbPruebas"));
                            string cedula_supervisor = "";
                            string nombre_supervisor = "";
                            string zona_asesor = "";
                            Int32 codigo_tipo_esquema = 0;
                            string nombre_tipo_esquema = "";
                            liq_tmp_metas _liq_tmp_metas_e = _context.liq_tmp_metas.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                          && x.periodo_importe == periodo_cm
                                                                                          && x.estado == 1).FirstOrDefault();
                            if(_liq_tmp_metas_e != null)
                            {
                                cedula_supervisor = _liq_tmp_metas_e.cedula_supervisor;
                                nombre_supervisor = General.getNombreCompletoEmpleado(cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                                zona_asesor = _liq_tmp_metas_e.zona;
                                codigo_tipo_esquema = _liq_tmp_metas_e.cod_tipo_escala;
                                nombre_tipo_esquema = get_nombre_tipo_esquema(codigo_tipo_esquema);
                            }
                            _listar_liq_tmp_nunca_pagos_megas_e.CEDULA_SUPERVISOR = cedula_supervisor;
                            _listar_liq_tmp_nunca_pagos_megas_e.NOMBRE_SUPERVISOR = nombre_supervisor;
                            _listar_liq_tmp_nunca_pagos_megas_e.ZONA = zona_asesor;
                            _listar_liq_tmp_nunca_pagos_megas_e.PERIODO = periodo_cm;
                            _listar_liq_tmp_nunca_pagos_megas_e.ESQUEMA = nombre_tipo_esquema;
                            _listar_liq_tmp_nunca_pagos_megas_e.TOTAL = valor_nunca_pago+"";
                            _listar_liq_tmp_nunca_pagos_megas_e.OBSERVACION = "COMISION PROMEDIADA DEL PERIODO "+ item.PERIODO_NP;
                            _listar_liq_tmp_nunca_pagos_megas_e.TIPO_OPERACION = tipo_operacion;
                            _listar_pendientes_nunca_pagos.Add(_listar_liq_tmp_nunca_pagos_megas_e);
                        }
                    }
                }
            }


            return _listar_pendientes_nunca_pagos;
        }

        public List<listar_liq_tmp_nunca_pagos_megas> listarPendientesNuncaPagosPeticion(List<listar_tmp_solicitud_np> listar_tmp_solicitud_np, 
                                                                                         string tipo_operacion, 
                                                                                         string periodo_cm)
        {
            List<listar_liq_tmp_nunca_pagos_megas> _listar_pendientes_nunca_pagos = new List<listar_liq_tmp_nunca_pagos_megas>();
            if (listar_tmp_solicitud_np.Count > 0)
            {
                string cedula_asesor_aux = "";
                double valor_comision = 0;
                List<listar_tmp_solicitud_np> _listar_tmp_solicitud_np_aux = new List<listar_tmp_solicitud_np>();
                _listar_tmp_solicitud_np_aux = listar_tmp_solicitud_np.OrderBy(x => x.CEDULA_ASESOR).ToList();
                foreach (listar_tmp_solicitud_np item in _listar_tmp_solicitud_np_aux)
                {
                    
                    cedula_asesor_aux = item.CEDULA_ASESOR;
                    double valor_comision_aux = 0;
                    liq_tmp_base_cierre _liq_tmp_base_cierre_e = _context.liq_tmp_base_cierre.Where(x => x.cod_peticion == item.ID_PETICION
                                                                                                    && x.producto == item.TIPO_OPERACION_PETICION
                                                                                                    && x.cedula_asesor == item.CEDULA_ASESOR
                                                                                                    && x.periodo == item.PERIODO_NP
                                                                                                    && x.estado == 1).FirstOrDefault();
                    if(_liq_tmp_base_cierre_e != null)
                    {
                        //validamos que el codigo de la peticion y producto se obtiene el valor de la mega y acuerdo a eso se saca a cuanto se pago en la comision
                        liq_comision_asesor _liq_comision_asesor_e = _context.liq_comision_asesor.Where(x => x.cedula_asesor == _liq_tmp_base_cierre_e.cedula_asesor
                                                                                                        && x.periodo == _liq_tmp_base_cierre_e.periodo
                                                                                                        && x.estado == 1
                                                                                                        && x.EsAsesorValido).FirstOrDefault();
                        if(_liq_comision_asesor_e != null)
                        {
                            if (_liq_tmp_base_cierre_e.velocidad.Equals(_liq_comision_asesor_e.nombre_mega_1))
                            {
                                valor_comision_aux = _liq_comision_asesor_e.valor_mega_1;
                            }else if (_liq_tmp_base_cierre_e.velocidad.Equals(_liq_comision_asesor_e.nombre_mega_2))
                            {
                                valor_comision_aux = _liq_comision_asesor_e.valor_mega_2;
                            }else if (_liq_tmp_base_cierre_e.velocidad.Equals(_liq_comision_asesor_e.nombre_mega_3))
                            {
                                valor_comision_aux = _liq_comision_asesor_e.valor_mega_3;
                            }else if (_liq_tmp_base_cierre_e.velocidad.Equals(_liq_comision_asesor_e.nombre_mega_4))
                            {
                                valor_comision_aux = _liq_comision_asesor_e.valor_mega_4;
                            }else if (_liq_tmp_base_cierre_e.velocidad.Equals(_liq_comision_asesor_e.nombre_mega_5))
                            {
                                valor_comision_aux = _liq_comision_asesor_e.valor_mega_5;
                            }else if (_liq_tmp_base_cierre_e.velocidad.Equals(_liq_comision_asesor_e.nombre_mega_6))
                            {
                                valor_comision_aux = _liq_comision_asesor_e.valor_mega_6;
                            }
                        }
                        if(item.CEDULA_ASESOR.Equals(cedula_asesor_aux))
                        {
                            valor_comision = valor_comision + valor_comision_aux;
                        }
                        else
                        {
                            listar_liq_tmp_nunca_pagos_megas _listar_liq_tmp_nunca_pagos_megas_e = new listar_liq_tmp_nunca_pagos_megas();
                            _listar_liq_tmp_nunca_pagos_megas_e.CEDULA_ASESOR = item.CEDULA_ASESOR;
                            _listar_liq_tmp_nunca_pagos_megas_e.NOMBRE_ASESOR = General.getNombreCompletoEmpleado(item.CEDULA_ASESOR, _config.GetConnectionString("conexionDbPruebas"));
                            string cedula_supervisor = "";
                            string nombre_supervisor = "";
                            string zona_asesor = "";
                            Int32 codigo_tipo_esquema = 0;
                            string nombre_tipo_esquema = "";
                            liq_tmp_metas _liq_tmp_metas_e = _context.liq_tmp_metas.Where(x => x.cedula_asesor == item.CEDULA_ASESOR
                                                                                           && x.periodo_importe == periodo_cm
                                                                                           && x.estado == 1).FirstOrDefault();

                            if (_liq_tmp_metas_e != null)
                            {
                                cedula_supervisor = _liq_tmp_metas_e.cedula_supervisor;
                                nombre_supervisor = General.getNombreCompletoEmpleado(cedula_supervisor, _config.GetConnectionString("conexionDbPruebas"));
                                zona_asesor = _liq_tmp_metas_e.zona;
                                codigo_tipo_esquema = _liq_tmp_metas_e.cod_tipo_escala;
                                nombre_tipo_esquema = get_nombre_tipo_esquema(codigo_tipo_esquema);
                            }
                            _listar_liq_tmp_nunca_pagos_megas_e.CEDULA_SUPERVISOR = cedula_supervisor;
                            _listar_liq_tmp_nunca_pagos_megas_e.NOMBRE_SUPERVISOR = nombre_supervisor;
                            _listar_liq_tmp_nunca_pagos_megas_e.ZONA = zona_asesor;
                            _listar_liq_tmp_nunca_pagos_megas_e.PERIODO = periodo_cm;
                            _listar_liq_tmp_nunca_pagos_megas_e.ESQUEMA = nombre_tipo_esquema;
                            _listar_liq_tmp_nunca_pagos_megas_e.TOTAL = valor_comision + "";
                            _listar_liq_tmp_nunca_pagos_megas_e.OBSERVACION = "COMISION PETICION DEL PERIODO " + item.PERIODO_NP;
                            _listar_liq_tmp_nunca_pagos_megas_e.TIPO_OPERACION = tipo_operacion;
                            _listar_pendientes_nunca_pagos.Add(_listar_liq_tmp_nunca_pagos_megas_e);
                            valor_comision = 0;
                        }
                    }
                }
            }
            return _listar_pendientes_nunca_pagos;
        }

    }
}
