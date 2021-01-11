using BattleCampus.Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampus.ManageServer.Persistence
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GameServerModel> Servers { get; set; }
        public DbSet<User> GameUsers { get; set; }

        //We don't persist match datas as it can be retrieved from the Match server as well as can be modified.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GameServerModel>()
                .OwnsOne(x => x.IpPortInfo);
        }
    }
}
