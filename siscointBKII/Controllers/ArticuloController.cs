using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticuloController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;

        public ArticuloController(AplicationDbContext contex, IConfiguration config)
        {
            _context = contex;
            _config = config;
        }

        [HttpGet("tipoArticulo")]
        [Authorize]
        public IActionResult tipoArticulo()
        {
            var data = _context.tipo_articulo.ToList();
            if (data == null)
                return null;

            return Ok(data);
        }

        [HttpGet("validarImei/{imei}")]
        [Authorize]
        public IActionResult validarImei(string imei)
        {
            Boolean ExisteObjeto = false;
            string mensaje = "";
            try
            {
                var data = _context.objeto.FirstOrDefault(x => x.imei == imei);
                if(data != null)
                {
                    ExisteObjeto = true;
                    mensaje = "Imei Existente";
                }

            }
            catch (Exception e)
            {
                //log de eventos
            }
            string json = JsonConvert.SerializeObject(new { Result = ExisteObjeto, Mensaje = mensaje });
            return Ok(json);
        }
    }
}
