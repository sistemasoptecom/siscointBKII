using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportEmpleadoController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;

        public ImportEmpleadoController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("enviaTmpDataEmpleado")]
        [Authorize]
        public IActionResult enviaTmpDataEmpleado(dynamic data_recibe)
        {
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            List<tmp_data_empleados> _tmp_data_empleados = new List<tmp_data_empleados>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["data"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                _tmp_data_empleados = JsonConvert.DeserializeObject<List<tmp_data_empleados>>(base_);
                if(_tmp_data_empleados.Count > 0)
                {
                    //se validan las funciones y/o comportamiento del importe
                    //valido si la tabla tiene registros o es el primer importe
                    List<tmp_data_empleados> _data_empleados_tbl = _context.tmp_data_empleados.ToList();
                    if(_data_empleados_tbl.Count > 0)
                    {
                       

                        //aqui los inserto los nuevos
                       
                        var empleados_a = _tmp_data_empleados.Select(x => x.cedula).Except(_data_empleados_tbl.Select(x => x.cedula)).ToList();
                        if(empleados_a.Count > 0)
                        {
                           foreach(var i in empleados_a)
                           {
                                tmp_data_empleados tmp_data_empleados_e = new tmp_data_empleados();
                                tmp_data_empleados_e = _tmp_data_empleados.Where(x => x.cedula == i).FirstOrDefault();
                                if(tmp_data_empleados_e != null)
                                {
                                    //
                                    tmp_data_empleados_e.usuario = usuario_;
                                    tmp_data_empleados_e.estado = 1;
                                    tmp_data_empleados_e.fechaCreacion = DateTime.Now;
                                    tmp_data_empleados_e.fechaModificacion = DateTime.Now;
                                    //_context.tmp_data_empleados.Add(_tmp_data_empleado_e);
                                    _context.tmp_data_empleados.Add(tmp_data_empleados_e);
                                    //_context.SaveChanges();
                                }
                           }
                        }
                      
                        //aqui actualizo
                        //_tmp_data_empleados_b = _data_empleados_tbl.Except(_tmp_data_empleados).ToList();
                        var empleados_b = _data_empleados_tbl.Select(x => x.cedula).Except(_tmp_data_empleados.Select(x => x.cedula)).ToList();
                        if(empleados_b.Count > 0) 
                        { 
                            foreach(var k in empleados_b)
                            {
                                tmp_data_empleados tmp_data_empleados_b = new tmp_data_empleados();
                                tmp_data_empleados_b = _data_empleados_tbl.Where(x => x.cedula == k).FirstOrDefault();
                                if(tmp_data_empleados_b != null)
                                {
                                    tmp_data_empleados_b.estado = 0;
                                    tmp_data_empleados_b.usuario = usuario_;
                                    tmp_data_empleados_b.fechaModificacion = DateTime.Now;
                                    _context.tmp_data_empleados.Update(tmp_data_empleados_b);
                                    //_context.SaveChanges();
                                }
                            }
                        }

                       
                        //actualizo toda la data
                        foreach (tmp_data_empleados itemC in _tmp_data_empleados)
                        {
                            tmp_data_empleados tmp_data_empleados_c = new tmp_data_empleados();
                            tmp_data_empleados_c = _context.tmp_data_empleados.Where(x => x.cedula ==itemC.cedula).FirstOrDefault();
                            if(tmp_data_empleados_c != null)
                            {
                                //guardo la fecha de la creacion

                                DateTime fecha_creacion = tmp_data_empleados_c.fechaCreacion;
                                //actualizar uno a uno 
                                //tmp_data_empleados_c = itemC;
                                tmp_data_empleados_c.salario = itemC.salario;
                                tmp_data_empleados_c.usuario = usuario_;
                                tmp_data_empleados_c.fechaCreacion = fecha_creacion;  
                                tmp_data_empleados_c.fechaModificacion = DateTime.Now;
                                _context.tmp_data_empleados.Update(tmp_data_empleados_c);
                                //_context.SaveChanges();
                            }
                            //se actualiza la data real de los empleados
                            empleado empleado_e_1 = _context.empleado.Where(y => y.cedula_emp == itemC.cedula
                                                                          && y.estado == 1).FirstOrDefault();
                            if(empleado_e_1 != null)
                            {
                                empleado_e_1.correo = itemC.correoelectronico;
                                _context.empleado.Update(empleado_e_1);   
                            }
                           
                        }
                    }
                    else
                    {
                        //aqui se insertan si no hay nada
                        foreach (tmp_data_empleados item in _tmp_data_empleados)
                        {
                            tmp_data_empleados _tmp_data_empleado_e = new tmp_data_empleados();
                            
                            _tmp_data_empleado_e = item;
                            _tmp_data_empleado_e.usuario = usuario_;
                            _tmp_data_empleado_e.estado = 1;
                            _tmp_data_empleado_e.fechaCreacion = DateTime.Now;
                            _tmp_data_empleado_e.fechaModificacion = DateTime.Now;
                            _context.tmp_data_empleados.Add(_tmp_data_empleado_e);
                            //_context.SaveChanges();
                            //valido si exite en empleados
                            empleado empleado_e_2 = _context.empleado.Where(x => x.cedula_emp == item.cedula).FirstOrDefault();
                            if(empleado_e_2 == null) 
                            {
                                string[] nombres_empleados = item.nombrecompleto.Split(" ");
                                if(nombres_empleados.Count() == 3 ) 
                                {

                                }
                                else if(nombres_empleados.Count() >= 4)
                                {

                                }
                                else if(nombres_empleados.Count() == 0)
                                {

                                }
                              
                                empleado_e_2.cedula_emp = item.cedula;
                                //empleado_e_2.
                            }
                        }
                    }
                    _context.SaveChanges();
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "enviaTmpDataEmpleado", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok();
        }


        [HttpPost("getDataTmpEmpleado")]
        public IActionResult getDataTmpEmpleado(dynamic data_recibe)
        {
            string mensaje = "";
            string cedula = "";
            tmp_data_empleados tmp_data_empleados_e = new tmp_data_empleados();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                cedula = Convert.ToString(datObject["cedula"]);
                tmp_data_empleados_e = _context.tmp_data_empleados.Where(x => x.cedula == cedula
                                                                                            && x.estado == 1).FirstOrDefault();
                
            }
            catch(Exception e)
            {
                mensaje = e.Message;
            }
            return Ok(tmp_data_empleados_e);
        }
        //private readonly 
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
