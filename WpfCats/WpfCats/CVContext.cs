using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCats
{
    class CVContext : DbContext
    {
        //static CVContext()
        //{
        //    Database.SetInitializer<CVContext>(new DropCreateDatabaseAlways<CVContext>());
        //}

        public DbSet<Cat> Cats {get;set;}
        public DbSet<PlateNumber> Plates { get; set; }
    }
}
