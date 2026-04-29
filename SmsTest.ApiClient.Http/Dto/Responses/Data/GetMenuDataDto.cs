using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTest.ApiClient.Http.Dto.Responses.Data;

/// <summary>
/// Тип данных возвращаемый командой "GetMenu"
/// </summary>
public class GetMenuDataDto
{
    public List<DishDto> MenuItems { get; set; }
}
