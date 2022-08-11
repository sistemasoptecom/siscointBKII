﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentroCostoController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        public CentroCostoController(AplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("getCentroCosto")]
        [Authorize]
        public IActionResult getCentroCosto([FromBody] area_ccosto area_ccostos)
        {
            var dato = new Object();
            try
            {
                dato = _context.area_ccosto.Where(x => x.id == area_ccostos.id).ToList();
            }
            catch(Exception e)
            {
                //log de errores
            }
            return Ok(dato);
        }
    }
}