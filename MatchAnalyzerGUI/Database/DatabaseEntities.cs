using MatchAnalyzer.Database.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Database
{
    class DatabaseEntities : DbContext
    {
        public DatabaseEntities() : base("myConnectionString") { }

        public DbSet<User> Users { get; set; }
    }
}
