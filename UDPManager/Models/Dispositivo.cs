using System.Net.Http.Headers;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Management;
using Microsoft.Win32;
using System.Net;
using UDPManager.Services;
using Refit;
using System.Text.Json;
using Microsoft.VisualBasic.Logging;
using System.Diagnostics;

namespace UDPManager.Models;

public class Dispositivo
{
    public int Id { get; set; }

    public string   NumeroSerial { get; set; }
    public string   Nome { get; set; }
    public string   Cliente { get; set; }
    public string   Patrimonio { get; set; }
    public string   Usuario { get; set; }
    public string   Departamento { get; set; }
    public string   Observacao { get; set; }
    public string   IDAnydesk { get; set; }
    public string   IDTeamviewer { get; set; }
    public string   StatusAntivirus { get; set; }
    public string   StatusFirewall { get; set; }
    public string   IPPrivado { get; set; }
    public string   IPPublico { get; set; }
    public bool     WindowsAtivado { get; set; }
    public string   BitLocker { get; set; }
    public string   VersaoAgente { get; set; }
    public DateTime UltimoScan { get; set; }
    public bool     Ativo { get; set; }


    public static Dispositivo ObtemDadosLocal()
    {
        var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\data.dat");

        return JsonSerializer.Deserialize<Dispositivo>(json);
    }

    public static void GravaDadosLocal(Dispositivo dispositivo)
    {
        File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\data.dat", JsonSerializer.Serialize(dispositivo));
    }

    public static async Task ObtemConfiguracoesAsync(Dispositivo dispositivo)
    {
        var modelOS = WmicService.ObtemValorUnico("computersystem get model").ToUpper();
        dispositivo.Nome = WmicService.ObtemValorUnico("os get csname");

        if (modelOS.Contains("VMWARE") || modelOS.Contains("VIRTUAL"))
        {
            var networks = NetworkInterface.GetAllNetworkInterfaces();
            var activeNetworks = networks.Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);
            var sortedNetworks = activeNetworks.OrderByDescending(ni => ni.Speed);
            dispositivo.NumeroSerial = dispositivo.Nome + sortedNetworks.First().GetPhysicalAddress().ToString();
        }
        else
            dispositivo.NumeroSerial = WmicService.ObtemValorUnico("bios get serialnumber") + WmicService.ObtemValorUnico("csproduct get uuid");

        // ID do Anydesk
        var anydesk = ShellService.ExecutaShell("GetAnyDeskID.bat");
        dispositivo.IDAnydesk = anydesk.Substring(anydesk.IndexOf(':') + 2).Replace(Environment.NewLine, "").Trim();

        // ID do TeamViewer
        foreach (var path in new[] { "SOFTWARE\\TeamViewer", "SOFTWARE\\Wow6432Node\\TeamViewer" })
        {
            if (Registry.LocalMachine.OpenSubKey(path) != null)
                dispositivo.IDTeamviewer = Registry.LocalMachine.OpenSubKey(path).GetValue("ClientID", "0").ToString();
        }

        // IP Privado
        IPHostEntry Host = default(IPHostEntry);
        Host = Dns.GetHostEntry(System.Environment.MachineName);
        foreach (IPAddress IP in Host.AddressList)
            if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                dispositivo.IPPrivado = Convert.ToString(IP);

        // IP Público
        var retorno = ShellService.ObtemVariosValores("nslookup", "myip.opendns.com. resolver1.opendns.com");
        var linha = retorno[retorno.Count - 1];
        dispositivo.IPPublico = linha.Substring(linha.IndexOf(':') + 1).Trim();

        // BitLocker
        //var BitLocker = ShellService.ObtemVariosValores("manage-bde", "-BitLocker");
        //if (BitLocker.Any(x => x.Contains("Conversion Status")))
        //    config.BitLocker = BitLocker.First(x => x.Contains("Conversion Status")).Split(':')[1].Trim();
        //if (BitLocker.Any(x => x.Contains("Status da Conver")))
        //    config.BitLocker = BitLocker.First(x => x.Contains("Status da Conver")).Split(':')[1].Trim();

        dispositivo.VersaoAgente = $"M.{System.Reflection.Assembly.GetEntryAssembly().GetName().Version}_I.{FileVersionInfo.GetVersionInfo("UDPInterface.exe").FileVersion}".Replace("1.0.", "");
        dispositivo.UltimoScan = DateTime.UtcNow;

        try
        {
            var apiClient = RestService.For<IDispositivo>(LyliService.BaseUrl);
            var resultAPI = await apiClient.AtualizaDispositivo(JsonSerializer.Serialize(dispositivo));
            dispositivo = JsonSerializer.Deserialize<Dispositivo>(resultAPI);
        }
        catch (Exception ex)
        {
            //LogService.GravaLog(ex);
            LogService.GravaLog(ex);
        }

        GravaDadosLocal(dispositivo);
    }

    public static async Task AtualizaStatusAsync(Dispositivo dispositivo)
    {
        dispositivo.UltimoScan = DateTime.UtcNow;

        try
        {
            var apiClient = RestService.For<IDispositivo>(LyliService.BaseUrl);
            var resultAPI = await apiClient.AtualizaDispositivo(JsonSerializer.Serialize(dispositivo));
            dispositivo = JsonSerializer.Deserialize<Dispositivo>(resultAPI);
        }
        catch (Exception ex)
        {
            //LogService.GravaLog(ex);
            LogService.GravaLog(ex);
        }
    }

    //static async Task<Dispositivo> Post(Dispositivo config)
    //{
    //    using (HttpClient httpClient = new HttpClient())
    //    {
    //        using (var content = new StringContent(JsonConvert.SerializeObject(config), System.Text.Encoding.UTF8, "application/json"))
    //        {
    //            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    //            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"https://lylifunctions.azurewebsites.net/api/AtualizaDispositivo", content);

    //            if (!httpResponseMessage.IsSuccessStatusCode)
    //                LogService.GravaLog($"Erro no Post: {httpResponseMessage.ReasonPhrase}", LogService.LogType.Info);

    //            return JsonConvert.DeserializeObject<Dispositivo>(httpResponseMessage.Content.ReadAsStringAsync().Result);
    //        }
    //    }
    //}

    public override string ToString()
    {
        return Id.ToString("000,000,000");
    }
}