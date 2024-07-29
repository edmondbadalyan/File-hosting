using HostingLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.IO;

namespace HostingLib.Data.Context 
{
    public class HostingDbContext : DbContext
    {
        private string connection_string;

        public DbSet<User> Users { get; set; }
        public DbSet<HostingLib.Data.Entities.File> Files { get; set; }
        public DbSet<User_Files> User_Files { get; set; }

        public HostingDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public HostingDbContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();

            Console.WriteLine(Directory.GetCurrentDirectory());
            connection_string = config.GetConnectionString("Test");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (connection_string == null)
            {
                
                throw new InvalidOperationException("Connection string 'Test' is null.");
            }
            optionsBuilder.UseSqlServer(connection_string);
        }
    }
}
