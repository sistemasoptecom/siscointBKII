using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        public List<UsuariosModel> usuarios = new List<UsuariosModel>();
        private AplicationDbContext _context;
        private IConfiguration _config;
        public LoginController(IConfiguration config, AplicationDbContext context)
        {
            _config = config;
            _context = context;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var user = Authenticate(userLogin);
            if(user == null)
                return NotFound("User not found");

            var token = Generate(user);
            return Ok(token);
        }

        private string Generate(usuario user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var crediales = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                
                new Claim(ClaimTypes.SerialNumber, user.codigo),
                new Claim(ClaimTypes.Name, user.nombre_usuario),
                new Claim(ClaimTypes.NameIdentifier, user.username)
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"], 
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: crediales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private usuario Authenticate(UserLogin userLogin)
        {
            var user = new usuario();
            Boolean EsUsuarioValido = false;
            try
            {
                user = _context.usuario.FirstOrDefault(o => o.username == userLogin.Username);
                string passwordHash = "";
                if (user != null)
                    passwordHash = user.pssword;

                passwordHash = General.DesencriptarPassword(passwordHash, userLogin.Password);
                if (passwordHash != "")
                {
                    if(passwordHash.Contains(userLogin.Password))
                    {
                        EsUsuarioValido = true;
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }

            //var currenUser = _context.usuario.FirstOrDefault(o => o.username.ToLower() == userLogin.Username.ToLower() && o.pssword == userLogin.Password);
            //if(currenUser == null)
            //{
            //    return null;
            //}
            if (!EsUsuarioValido)
                return null;
            return user;

        }

       

    }
}
