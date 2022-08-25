using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntradasController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        public EntradasController(AplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("ObtenerJefes")]
        [Authorize]
        public IActionResult getJefes()
        {
            var data = new Object();
            try
            {
                data = _context.jefes.ToList();
                if(data == null)
                {
                    return null;
                }
            }
            catch(Exception e)
            {

            }
            return Ok(data);
        }
        [HttpGet("ObtnerObjeto/{id}")]
        [Authorize]
        public IActionResult getObjeto(int id)
        {
            var data = new Object();
            try
            {
                data = _context.objeto.Where(x => x.id == id).ToList();
                if(data == null)
                {
                    return null;
                }
            }
            catch(Exception e)
            {

            }

            return Ok(data);
        }

        [HttpPost("AgregarEntrada")]
        [Authorize]
        public IActionResult crearEntreda(dynamic json)
        {
            var data = new Object();
            var dataJson = System.Text.Json.JsonSerializer.Serialize(json);
            //string tipoentrega = Convert.ToString(dataJson["tipoEntrega"]);
            var datObject = JObject.Parse(dataJson);

            return Ok(data);
        }

    }
}
