using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Common;

public class BaseHttpService : IBaseHttpService
{
    public async Task<T> GetRequestBody<T>(HttpRequest req)
    {
        string requestBody;
        using (var streamReader = new StreamReader(req.Body))
        {
            requestBody = await streamReader.ReadToEndAsync();
        }

        return JsonConvert.DeserializeObject<T>(requestBody);
    }

    public string GetRequiredQueryParam(HttpRequest req, string key)
    {
        if (!req.Query.TryGetValue(key, out StringValues side))
        {
            throw new ArgumentException($"\"{key}\" query param is missing");
        }

        return side;
    }
}

