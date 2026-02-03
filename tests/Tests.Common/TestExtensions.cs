using Newtonsoft.Json;

namespace Tests.Common;

public static class TestExtensions
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters = { new JsonObjectToStringConverter() }
    };
    
    public static async Task<T> ToResponseModel<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(content, Settings)
               ?? throw new ArgumentException("Response content cannot be null");
    }
}