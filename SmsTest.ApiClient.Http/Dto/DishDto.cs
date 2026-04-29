using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTest.ApiClient.Http.Dto;

/// <summary>
/// Модель блюда
/// </summary>
public class DishDto
{
    public string Id { get; set; }
    public string Article { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsWeighted { get; set; }
    public string FullPath { get; set; }
    public List<string> Barcodes { get; set; }
}
