using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbv.Models.DataBase;

namespace Tbv.Data.Entity
{
    public class EntityContext : DbContext
    {
        public EntityContext() : base("transactionBankViewerConn")
        {
            Database.SetInitializer<EntityContext>(null);
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 600;
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<BankAccount> BankAccount { get; set; }
        public DbSet<AccountTransaction> AccountTransaction { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Remove a plurarização das tabelas
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //Remove a exclusão em cascata em relacionamentos um para muitos
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //Remove a exclusão em cascata em relacionamentos muitos para muitos
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            //Decimal precision definition
            modelBuilder.Entity<AccountTransaction>().Property(x => x.Amount).HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
