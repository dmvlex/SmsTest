using Microsoft.EntityFrameworkCore;
using SmsTest.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmsTest.Data;

public class AppDbContext : DbContext
{
    public DbSet<Dish> Dishes { get; set; }

    public AppDbContext() {}
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //a.kh: в тз не указаны ограничения полей, потому ограничения не точные.
        //В реальном проекте я бы проанализировал артикулы,пути и наменования, что бы выставить более
        //точный размер колонок.

        modelBuilder.Entity<Dish>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Article).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.FullPath).HasMaxLength(500);

            //a.kh: для тестового храню штрихкоды как json.
            //В реальном проекте завел бы отдельную таблицу
            entity.Property(e => e.Barcodes)
                  .HasColumnType("jsonb")
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()
                  );
        });
    }
}
