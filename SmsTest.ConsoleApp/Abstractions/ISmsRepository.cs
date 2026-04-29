using SmsTest.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTest.ConsoleApp.Abstractions;

public interface ISmsRepository
{
    Task EnsureMigrateDatabaseAsync();
    Task<bool> IsDishesExistAsync(IEnumerable<string> ids);
    Task UpsertDishesAsync(IEnumerable<Dish> dishes);
}
