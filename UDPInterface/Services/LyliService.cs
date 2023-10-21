using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace UDPInterface;

public class LyliService
{
    public static string BaseUrl => "https://lylifunctions.azurewebsites.net/api";
    //public static string BaseUrl => "http://localhost:7071/api";
}

public interface IDispositivo
{
    [Get("/AutenticaDispositivo/{id}")]
    Task<HttpResponseMessage> AutenticaDispositivo(string id);
}
