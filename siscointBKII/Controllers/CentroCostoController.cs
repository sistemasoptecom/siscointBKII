using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class CentroCostoController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        public CentroCostoController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("getCentroCosto")]
        [Authorize]
        public IActionResult getCentroCosto([FromBody] area_ccosto area_ccostos)
        {
            var dato = new Object();
            try
            {
                dato = _context.area_ccosto.Where(x => x.id == area_ccostos.id || x.ccosto == area_ccostos.ccosto).ToList();
            }
            catch(Exception e)
            {
                //log de errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "area_ccosto", e.Message, _config.GetConnectionString("conexion"));
            }
            return Ok(dato);
        }

        [HttpGet("listarCentroCostos")]
        [Authorize]
        public IActionResult getListAreaCentroCosto()
        {
            var data = new Object();
            try
            {
                data = _context.area_ccosto.ToList();
                if(data == null)
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "area_ccosto", e.Message, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }
    }
}
