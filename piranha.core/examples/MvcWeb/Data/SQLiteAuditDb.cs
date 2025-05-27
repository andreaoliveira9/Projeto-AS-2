using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.Data.EF.Audit;

namespace MvcWeb.Data;

public class SQLiteAuditDb : Db<SQLiteAuditDb>, IAuditDb
{
    public DbSet<StateChangeRecord> StateChangeRecord { get; set; }

    public SQLiteAuditDb(DbContextOptions<SQLiteAuditDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Use the official Audit configuration
        modelBuilder.ConfigureAudit();
    }
}
