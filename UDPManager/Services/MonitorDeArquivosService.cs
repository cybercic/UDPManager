using Azure.Storage.Blobs;
using System.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using UDPManager.Models;

namespace UDPManager;

internal class MonitorDeArquivosService
{
    //private static List<string> _arquivosAlterados = new List<string>();
    //private static FileSystemWatcher? _monitorDeArquivos = null;
    private static Config _config = Config.ObtemDadosLocal();
    private static DateTime _ultimaVerificao = DateTime.Now;
    static string _caminho = string.Empty;
    static string _filtro = string.Empty;
    private static bool isRunning = false;

    public static void MonitorarArquivos(string caminho, string filtro)
    {
        _caminho = caminho;
        _filtro = filtro;

        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Interval = 15000;
        timer.AutoReset = false;
        timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        timer.Start();
        LogService.GravaLog("O Timer do MonitorarArquivos foi executado ...", LogService.LogType.Debug);

        //_monitorDeArquivos = new FileSystemWatcher(path, filtro)
        //{
        //    IncludeSubdirectories = false,
        //    InternalBufferSize = 64 * 1024,
        //    EnableRaisingEvents = true,
        //    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess
        //};

        //_monitorDeArquivos.Changed += OnFileChanged;
        //_monitorDeArquivos.Error += OnError;
    }

    //private static void OnFileChanged(object sender, FileSystemEventArgs e)
    //{
    //    if (!_arquivosAlterados.Contains(e.FullPath))
    //        _arquivosAlterados.Add(e.FullPath);
    //}

    //private static void OnError(object sender, ErrorEventArgs e)
    //{
    //    MessageBox.Show($"Erro: {e.ToString()}");
    //}

    private static async void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (isRunning)
            return;

        isRunning = true;

        string[] files = Directory.GetFiles(_caminho, _filtro, SearchOption.TopDirectoryOnly);
        var minutoVerificacao = DateTime.Now;

        foreach (string file in files)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (fileInfo.LastWriteTime > _ultimaVerificao)
            {
                //LogService.GravaLog($"Pegou arquivo {file}", LogService.LogType.Debug);

                string texto = File.ReadAllText(file);
                Stint? stint = JsonSerializer.Deserialize<Stint>(texto);

                if (stint != null)
                {
                    List<Lap> laps = stint?.Laps;

                    try
                    {
                        stint.Laps = null;
                        LyliService.EnviaDadosAPIAsync("GT_STINT", JsonSerializer.Serialize(stint));
                        LogService.GravaLog($"Stint {stint.StintId} enviado.", LogService.LogType.Debug);

                        //laps.ForEach(x => x.StintId = stint.StintId);
                        foreach (var lap in laps)
                        {
                            LyliService.EnviaDadosAPIAsync("GT_LAP", JsonSerializer.Serialize(lap));

                            BlobServiceClient blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=stogtml;AccountKey=/YhFpeSUmxY0WsLofOrVM7yRukucsXg0ZXk12oNO4I+tX3wH9zM51HEBFn6TQ2JdFEgFyKlIWsYF+AStaY5wPA==;EndpointSuffix=core.windows.net");
                            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("arquivos");

                            if (_config.SendFiles)
                            {
                                var arquivosVoltasGTDB = Directory.EnumerateFiles($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes", $"{stint.StintId}_{lap.LapNumber.ToString().PadLeft(3, '0')}.finished");

                                foreach (var nomeArquivoGTDB in arquivosVoltasGTDB)
                                {
                                    LogService.GravaLog($"Lap {lap.LapNumber} do Stint {stint.StintId} enviada (GTDB).", LogService.LogType.Debug);
                                    await UploadFromBinaryDataAsync(containerClient, nomeArquivoGTDB, "GTDB", stint);
                                    File.Delete(nomeArquivoGTDB);
                                }

                                //var arquivosVoltasLOG = Directory.EnumerateFiles($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes", $"{stint.StintId}_{lap.LapNumber.ToString().PadLeft(3, '0')}.LOG");
                                //
                                //foreach (var nomeArquivoZ1 in arquivosVoltasLOG)
                                //{
                                //    //LogService.GravaLog($"Lap {lap.LapNumber} do Stint {stint.StintId} enviada (LOG).", LogService.LogType.Debug);
                                //    //await UploadFromBinaryDataAsync(containerClient, nomeArquivoZ1, "LOG", stint);
                                //    File.Delete(nomeArquivoZ1);
                                //}
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogService.GravaLog(ex);
                    }
                }
            }
        }

        _ultimaVerificao = minutoVerificacao;
        isRunning = false;
        ((System.Timers.Timer)sender).Start();

        //for (int i = _arquivosAlterados.Count - 1; i >= 0; i--)
        //{
        //    string texto = File.ReadAllText(_arquivosAlterados[i]);
        //    Stint? stint = JsonSerializer.Deserialize<Stint>(texto);

        //    if (stint != null)
        //    {
        //        List<Lap> laps = stint?.Laps;

        //        try
        //        {
        //            stint.Laps = null;
        //            LyliService.EnviaDadosAPIAsync("GT_STINT", JsonSerializer.Serialize(stint));
        //            LogService.GravaLog($"Stint {stint.StintId} enviado", LogService.LogType.Debug);

        //            //laps.ForEach(x => x.StintId = stint.StintId);
        //            foreach (var lap in laps)
        //            {
        //                LyliService.EnviaDadosAPIAsync("GT_LAP", JsonSerializer.Serialize(lap));
        //                LogService.GravaLog($"Volta {lap.LapNumber} do Stint {stint.StintId} enviado", LogService.LogType.Debug);

        //                if (_config.SendFiles)
        //                {
        //                    var arquivosVoltas = Directory.EnumerateFiles($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes", $"{stint.StintId}_{lap.LapNumber.ToString().PadLeft(3, '0')}.finished");

        //                    foreach (var nomeArquivo in arquivosVoltas)
        //                    {
        //                        using (var file = File.OpenRead(nomeArquivo))
        //                        {
        //                            using (MemoryStream stream = new MemoryStream())
        //                            {
        //                                file.CopyTo(stream);
        //                                var fileBytes = stream.ToArray();
        //                                stream.Write(fileBytes, 0, fileBytes.Length);
        //                                BlobServiceClient blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=stogtml;AccountKey=/YhFpeSUmxY0WsLofOrVM7yRukucsXg0ZXk12oNO4I+tX3wH9zM51HEBFn6TQ2JdFEgFyKlIWsYF+AStaY5wPA==;EndpointSuffix=core.windows.net");
        //                                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("arquivos");
        //                                BlobClient blobClient = containerClient.GetBlobClient($"{stint.StintId}/{Path.GetFileName(file.Name)}");
        //                                stream.Position = 0;
        //                                blobClient.Upload(stream, true);
        //                                LogService.GravaLog($"Arquivo {Path.GetFileName(file.Name)} enviado", LogService.LogType.Debug);
        //                            }
        //                        }

        //                        File.Move(nomeArquivo, nomeArquivo.Replace("finished", "sended"));
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogService.GravaLog(ex);
        //        }

        //        _arquivosAlterados.RemoveAt(i);
        //    }
        //}
    }

    public static async Task UploadFromBinaryDataAsync(BlobContainerClient containerClient, string localFilePath, string fileType, Stint stint)
    {
        string fileName = Path.GetFileName(localFilePath);

        if (fileType == "GTDB")
        {
            BlobClient blobClient = containerClient.GetBlobClient($"{stint.StintId}/{Path.GetFileName(localFilePath.Replace("finished", "GTDB"))}");

            FileStream fileStream = File.OpenRead(localFilePath);
            BinaryReader reader = new BinaryReader(fileStream);

            byte[] buffer = new byte[fileStream.Length];
            reader.Read(buffer, 0, buffer.Length);
            BinaryData binaryData = new BinaryData(buffer);

            await blobClient.UploadAsync(binaryData, true);

            fileStream.Close();
        }

        if (fileType == "LOG")
        {
            BlobClient blobClient = containerClient.GetBlobClient($"{stint.StintId}/{Path.GetFileName(localFilePath)}");

            FileStream fileStream = File.OpenRead(localFilePath);
            BinaryReader reader = new BinaryReader(fileStream);

            byte[] buffer = new byte[fileStream.Length];
            reader.Read(buffer, 0, buffer.Length);
            BinaryData binaryData = new BinaryData(buffer);

            await blobClient.UploadAsync(binaryData, true);

            fileStream.Close();
        }

    }
}
