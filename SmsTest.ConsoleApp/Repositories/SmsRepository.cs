using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmsTest.ConsoleApp.Abstractions;
using SmsTest.Data;
using SmsTest.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SmsTest.ConsoleApp.Repositories;

public class SmsRepository : ISmsRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger _logger;
    public SmsRepository(IDbContextFactory<AppDbContext> contextFactory,
        ILogger<SmsRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task EnsureMigrateDatabaseAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            _logger.LogInformation("Требуется обновление схемы базы данных");
            await context.Database.MigrateAsync();
            _logger.LogInformation("База данных обновлена успешно");
        }
    }

    /// <summary>
    /// Актуализирует таблицу блюд по переданному списку.
    /// </summary>
    /// <remarks>a.kh: это не самый оптимизированный вариант обновления данных.
    /// Но библиотека с bulk операциями платная, а писать процедуру руками под
    /// маленький объем данных тестового задания нет смысла</remarks>
    /// <param name="dishes">Список блюд</param>
    public async Task UpsertDishesAsync(IEnumerable<Dish> dishes)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var ids = dishes.Select(d => d.Id).ToList();

        var existingDishes = await context.Dishes
            .Where(d => ids.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id);

        var toUpdate = new List<Dish>();
        var toAdd = new List<Dish>();

        foreach (var dish in dishes)
        {
            if (!existingDishes.TryGetValue(dish.Id, out var existingDish)) toAdd.Add(dish);
            else
            {
                existingDish.Name = dish.Name;
                existingDish.Barcodes = dish.Barcodes;
                existingDish.Article = dish.Article;
                existingDish.Price = dish.Price;
                existingDish.IsWeighted = dish.IsWeighted;
                existingDish.FullPath = dish.FullPath;
            }
        }

        context.Dishes.AddRange(toAdd);
        context.Dishes.UpdateRange(toUpdate);
        await context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Проверяет по списку id наличие блюд в базе
    /// </summary>
    /// <param name="id">id блюда</param>
    /// <returns>False если нет блюда с таким id, иначе - true</returns>
    public async Task<bool> IsDishesExistAsync(IEnumerable<string> ids)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Dishes
                            .Where(x => ids.Contains(x.Id))
                            .AnyAsync();
    }
}
