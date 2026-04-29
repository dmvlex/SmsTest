using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTest.ApiClient.Http.Dto.Responses;

//a.kh: Формат ответа очень похожа на 1С8.

/// <summary>
/// Стандартная модель ответа внешнего api
/// </summary>
public class ApiResposeDto
{
    public bool Success { get; set; }
    public string Command { get; set; }
    public string ErrorMessage { get; set; }
}

/// <summary>
/// Моедель ответа внешнего api с полезными данными
/// </summary>
/// <typeparam name="TData">Модель получаемых данных</typeparam>
public class ApiResposeDto<TData> : ApiResposeDto
{
    public TData Data { get; set; }
}