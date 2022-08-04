using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewsController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        public ViewsController(AplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("views")]
        [Authorize]
        public IActionResult views()
        {
            var vistas = getListViews();
            return Ok(vistas);
        }

        //[HttpGet("views/{username}")]
        //[Authorize]
        //public IActionResult views(string username)
        //{

        //}

        public Object getListViews()
        {
            var vistas = _context.views.ToList();
            return vistas;
        }
        //public Object getListViewsUser(string username)
        //{
        //    //var viewsUser = _context.views.Where(o => o)
        //}

    }
}
