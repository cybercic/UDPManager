using Refit;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using UDPManager.Models;

namespace UDPManager;

public class LyliService
{
    public static string BaseUrl => "https://lylifunctions.azurewebsites.net/api";
    //public static string BaseUrl => "http://localhost:7071/api";
    public static string BaseUrlIDS => "https://id.lyli.com.br/auth/realms/Lyli/protocol/openid-connect/token";

    public static async Task<string> GetToken()
    {
        List<KeyValuePair<string, string>> authenticationCredentials = new List<KeyValuePair<string, string>>();
        authenticationCredentials.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
        authenticationCredentials.Add(new KeyValuePair<string, string>("scope", "audience"));
        authenticationCredentials.Add(new KeyValuePair<string, string>("client_id", ""));
        authenticationCredentials.Add(new KeyValuePair<string, string>("client_secret", ""));

        FormUrlEncodedContent content = new FormUrlEncodedContent(authenticationCredentials);

        using (var client = new HttpClient())
        {
            HttpResponseMessage response = await client.PostAsync(BaseUrlIDS, content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string message = String.Format("POST failed. Received HTTP {0}", response.StatusCode);
                throw new ApplicationException(message);
            }

            string responseString = await response.Content.ReadAsStringAsync();

            Token token = JsonSerializer.Deserialize<Token>(responseString);

            return token.AccessToken ?? string.Empty;
        }
    }

    public static async Task EnviaDadosAPIAsync(string chave, string texto)
    {
        try
        {
            if (!texto.StartsWith("[")) 
                texto = "[" + texto + "]";

            DadosEntradaAPI dados = new DadosEntradaAPI(chave, texto);

            var apiClient = RestService.For<IDadosEntradaAPI>(BaseUrl);
            var retorno = await apiClient.PostDados(JsonSerializer.Serialize(dados));
        }
        catch (Exception ex)
        {
            LogService.GravaLog($"Erro no EnviaDadosAPI: {chave}", LogService.LogType.Error);
            LogService.GravaLog(ex);
        }
    }

    public static void EnviaErroAPI(string chave, string erro, string funcao)
    {
        List<string> erros = new List<string> { erro };

        EnviaErroAPI(chave, erros, funcao);
    }

    public static void EnviaErroAPI(string chave, List<string> erros, string funcao)
    {
        //List<DadosErroAPI> log = new List<DadosErroAPI>();
        //foreach (var erro in erros)
        //    LogService.Add(new DadosErroAPI(chave, erro, funcao));
        //
        //EnviaDadosAPIAsync(JsonSerializer.Serialize(log), "LITE_ERRO");
    }
}

public class Token
{
    public Token()
    {
        Issued = DateTime.Now;
    }

    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("as:client_id")]
    public string? ClientId { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("as:region")]
    public string? Region { get; set; }

    [JsonPropertyName(".issued")]
    public DateTime Issued { get; set; }

    [JsonPropertyName(".expires")]
    public DateTime Expires
    {
        get { return Issued.AddMilliseconds(ExpiresIn); }
    }

    [JsonPropertyName("bearer")]
    public string? Bearer { get; set; }
}

public interface IComandos
{
    [Get("/GetComandos/{chave}")]
    //[Headers("Authorization: Bearer")]
    Task<List<string>> GetComandos(string chave);
}

public interface IDadosEntradaAPI
{
    //https://lylifunctions.azurewebsites.net/api/PostDados?code=QzG_wylDfYNy8iJGLpNffESNTKv65JXkFkl4OTmSJf4uAzFuSdiQlg==
    [Post("/PostDados?code=QzG_wylDfYNy8iJGLpNffESNTKv65JXkFkl4OTmSJf4uAzFuSdiQlg==")]
    [Headers("Content-Type: application/json")]
    //[Headers("Authorization: Bearer")]
    Task<string> PostDados([Body] string dados);
}

public interface IDispositivo
{
    [Get("/AutenticaDispositivo/{id}")]
    Task<string> AutenticaDispositivo(string id);

    //https://lylifunctions.azurewebsites.net/api/AtualizaDispositivo?code=Cefl3dyURQEfXU9ePdW-4NG2BKYdD8X4nYyDDCPB7U1tAzFujquzlw==
    [Post("/AtualizaDispositivo?code=Cefl3dyURQEfXU9ePdW-4NG2BKYdD8X4nYyDDCPB7U1tAzFujquzlw==")]
    [Headers("Content-Type: application/json")]
    Task<string> AtualizaDispositivo([Body] string dados);
}

public class DadosErroAPI
{
    public DadosErroAPI(string chave, string erro, string processo)
    {
        CNS = chave;
        Erro = erro;
        Processo = processo;
    }

    public string CNS { get; set; }
    public string Erro { get; set; }
    public string Processo { get; set; }
}

public class DadosEntradaAPI
{
    public DadosEntradaAPI(string chave, string json)
    {
        Chave = chave;
        Json = json;
    }

    public string Chave { get; set; }
    public string Json { get; set; }
}