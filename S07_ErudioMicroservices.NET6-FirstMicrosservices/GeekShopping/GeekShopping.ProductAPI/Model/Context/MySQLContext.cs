using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Model.Context;

public class MySQLContext : DbContext
{
    public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) { }

    public MySQLContext() { }

    public DbSet<Product> Products { get; set; }
    
}

