using DatingApp.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DatingApp.Data
{
    public class DataContext : DbContext

    {
        private readonly IConfiguration _configuration;
        public DataContext( DbContextOptions options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.UseSqlServer(_configuration["DbContext:ConnectionString"]);
        }


        public DbSet<AppUser> Users { get; set; }
        
    }
}
