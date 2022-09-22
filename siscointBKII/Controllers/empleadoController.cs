using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message, _config.GetConnectionString("conexion"));
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

                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message, _config.GetConnectionString("conexion"));
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

                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message, _config.GetConnectionString("conexion"));
            }

            string json = JsonConvert.SerializeObject(new { Result = result, Mensaje = mensaje });
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

                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message, _config.GetConnectionString("conexion"));
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
