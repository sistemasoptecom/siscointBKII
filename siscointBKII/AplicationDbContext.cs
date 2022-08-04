using Microsoft.EntityFrameworkCore;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII
{
    public class AplicationDbContext : DbContext
    {
        

        public AplicationDbContext(DbContextOptions<AplicationDbContext> options) : base(options) { }

      
        public DbSet<usuario> usuario { get; set; }
        public DbSet<views> views { get; set; }
    }
}
