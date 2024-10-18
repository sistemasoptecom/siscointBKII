using Chilkat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class empleadoController : Controller
    {

        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        public object JavaScriptSerializer { get; private set; }

        public empleadoController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("getEmpleado")]
        public IActionResult getEmpleado(dynamic data_recibe)
        {
            string cedula = "";
            var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
            var datObject = JObject.Parse(dataJson);
            cedula = Convert.ToString(datObject["cedula"]);
            empleado _empleado_e = _context.empleado.Where(x => x.cedula_emp == cedula
                                                           && x.estado == 1).FirstOrDefault();
           
            return Ok(_empleado_e);
        }
        [HttpGet("getEmpresas")]
        [Authorize]
        public IActionResult getEmpresas()
        {
            var data = _context.empresa.ToList();
            if (data == null)
                return null;

            return Ok(data);
        }

        [HttpPost("getAreaCcosto")]
        [Authorize]
        public IActionResult getAreaCcosto(dynamic data)
        {
            //var data = new Object();
            //try
            //{
            //    data = _context.area_ccosto.Where(x => x.area == area_ccostos.area || x.ccosto == area_ccostos.ccosto).ToList();
            //    if (data == null)
            //        return null;
            //}
            //catch (Exception e)
            //{
            //    //log de eventos
            //}
            //return Ok(data);
            return Ok();
        }

        [HttpPost("busquedaEmpleado")]
        [Authorize]
        public IActionResult busquedaEmpleado([FromBody] empleado Empleado)
        {
            //var data = new empleado();
            List<dataBusquedaCompleta> busqueda = new List<dataBusquedaCompleta>();
            try
            {
                var data = _context.empleado.Where(x => x.cedula_emp == Empleado.cedula_emp ||
                                                    x.nombre.Contains(Empleado.nombre) ||
                                                    x.snombre.Contains(Empleado.snombre) ||
                                                    x.ppellido.Contains(Empleado.ppellido) ||
                                                    x.spellido.Contains(Empleado.spellido)).ToList();
                
                if (data == null)
                    return null;

                foreach(empleado item in data)
                {
                    dataBusquedaCompleta busq = new dataBusquedaCompleta();
                    busq.id = item.id;
                    busq.valor1 = item.cedula_emp;
                    busq.valor2 = item.nombre;
                    busq.valor3 = item.snombre;
                    busq.valor4 = item.ppellido;
                    busq.valor5 = item.spellido;

                    busqueda.Add(busq);
                }
            }
            catch (Exception e)
            {
                //log de registros
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(busqueda);
        }

        [HttpPost("busquedaEmpleadoId")]
        [Authorize]
        public IActionResult busquedaEmpleadoId([FromBody] empleado Empleado)
        {
            var data = new Object();
            try
            {
                data = _context.empleado.Where(x => x.id == Empleado.id).ToList();
                if (data == null)
                    return null;
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(data);
        }
        [HttpPost("agregarEmpleado")]
        [Authorize]
        public IActionResult agregarEmpleado([FromBody] empleado Empleado)
        {
            //string mensaje = "";
            Boolean EsEmpleadoValido = false;
            EsEmpleadoValido = validoEmpleado(Empleado.cedula_emp);
            if (EsEmpleadoValido)
            {
                _context.Add(Empleado);
                return Ok(_context.SaveChanges());
            }
            {
                return null;
            }
            
        }

        [HttpPut("EditarEmpleado/{id}")]
        [Authorize]
        public IActionResult Edit(int id, [FromBody] empleado Empleado)
        {
            int result = 0;
            string mensaje = "";
            try
            {
                if (id == Empleado.id)
                {
                    _context.Update(Empleado);
                    result = _context.SaveChanges();
                     mensaje = "";
                    if(result  > 0)
                    {
                        mensaje = "Datos Actualizados !";
                    }
                    else
                    {
                        mensaje = "Error al Actualizar";
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
                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexionDbPruebas"));
            }

            string json = JsonConvert.SerializeObject(new { Result = result, Mensaje = mensaje });
            return Ok(json);
        }

        [HttpPost("agregarEmpleadosMasivos")]
        [Authorize]
        public async Task<IActionResult> agregarEmpleadosMasivos(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string json = "";
            
            List<temp_import_update_empleados_v1> _data_empleados = new List<temp_import_update_empleados_v1>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["base"]);
                
                _data_empleados = JsonConvert.DeserializeObject<List<temp_import_update_empleados_v1>>(base_);
                if (_data_empleados.Count > 0) 
                {
                    foreach (temp_import_update_empleados_v1 item in _data_empleados)
                    {
                        //validamos que la cedula no este
                        empleado empleado_e = _context.empleado.Where(x => x.cedula_emp == item.Cedula).FirstOrDefault();
                        if(empleado_e == null)
                        {
                            await General.crearEmpleadosV3(item.Cedula,
                                                 item.NombreCompleto,
                                                 item.Area,
                                                 item.Cargo,
                                                 item.Contrato,
                                                 Convert.ToInt32(item.Centrodecostos),
                                                 Convert.ToInt32(item.codEmpresa),
                                                 item.CorreoElectronico,
                                                 _config.GetConnectionString("conexionDbPruebas"));
                        }
                        
                        //empleado empleado_e = new empleado();
                        
                    }
                }
                resultado = "PROCESADO DE FORMA CORRECTA";
            }
            catch (Exception e) 
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "agregarEmpleadosMasivos", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            json = JsonConvert.SerializeObject(resultado);
            return Ok(json);
        }

        private Boolean validoEmpleado(string cedulaEmpleado)
        {
            Boolean EsValido = false;
            try
            {
                var dato = _context.empleado.FirstOrDefault(x => x.cedula_emp == cedulaEmpleado);
                if (dato == null)
                    EsValido = true;
            }
            catch(Exception e)
            {
                //Log de Errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexionDbPruebas"));
            }

            return EsValido;
        }
        //// GET: empleadoController
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //// GET: empleadoController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: empleadoController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: empleadoController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: empleadoController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: empleadoController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: empleadoController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: empleadoController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
