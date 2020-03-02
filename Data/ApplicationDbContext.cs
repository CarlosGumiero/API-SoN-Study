using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using API.Models;

namespace API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Produto> Produtos {get; set;}
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
              
        }
    }
}