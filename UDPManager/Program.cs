using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Timers;
using UDPManager.Models;

namespace UDPManager
{
    internal static class Program
    {

        static Mutex _mutex = new Mutex(true, name: "b9c3a4a8-0784-4d14-b38d-57c3de237904");
        static NotifyIcon? _trayIcon = null;

        static Config config = Config.ObtemDadosLocal();
        static Dispositivo dispositivo = Dispositivo.ObtemDadosLocal();

        static string _udpIP = string.Empty;
        static bool _udpStatus = false;

        [STAThread]
        public static void Main(string[] args)
        {
            if (!_mutex.WaitOne(0))
            {
                LogService.GravaLog($"Existe outra instância do sistema em execução!", LogService.LogType.Debug);
                Environment.Exit(1);
            }

            try
            {
                config.UDPActive = false;
                LogService.GravaLog($"Sistema iniciado.", LogService.LogType.Debug);
                MainAsync(args).GetAwaiter().GetResult();
            }
            finally
            {
                LogService.GravaLog($"Sistema finalizado.", LogService.LogType.Debug);
                _mutex.ReleaseMutex();
            }
        }

        public static async Task MainAsync(string[] args)
        {
            try
            {
                AutoUpdaterDotNET.AutoUpdater.Mandatory = true;
                AutoUpdaterDotNET.AutoUpdater.RunUpdateAsAdmin = true;
                AutoUpdaterDotNET.AutoUpdater.AppTitle = "UDPManager";
                AutoUpdaterDotNET.AutoUpdater.Synchronous = true;
                AutoUpdaterDotNET.AutoUpdater.UpdateMode = AutoUpdaterDotNET.Mode.ForcedDownload;
                AutoUpdaterDotNET.AutoUpdater.Start("https://install.lyli.com.br/AgenteGT/ChangeVersion.xml");

                CultureInfo culture = new CultureInfo("pt-BR");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

                if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes"))
                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes");

                await Dispositivo.ObtemConfiguracoesAsync(dispositivo);

                int tentativas = 0;
                while (dispositivo.Id == 0)
                {
                    dispositivo = Dispositivo.ObtemDadosLocal();
                    Thread.Sleep(1000);
                    tentativas++;

                    if (tentativas >= 30)
                        throw new Exception("Não foi possível localizar o servidor! Tentativas esgotadas.");
                }
                config.ID = dispositivo.Id;

                LyliService.EnviaDadosAPIAsync("GT_DRIVER", JsonSerializer.Serialize(config));
                LyliService.EnviaDadosAPIAsync("GT_DISPOSITIVO", JsonSerializer.Serialize(config));
                LogService.GravaLog($"Autorizando o dispositivo para {config.DriverId}, {config.TeamId}!", LogService.LogType.Debug);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Splash splash = new Splash();
                splash.ShowDialog();

                Application.Run(new AgentAppContext());
            }
            catch (Exception ex)
            {
                LogService.GravaLog(ex);
                throw;
            }
        }

        static void IniciaProcessoUDP()
        {
            using (Process shell = new Process())
            {
                shell.StartInfo.FileName = "UDPInterface.exe";
                shell.StartInfo.Arguments = _udpIP;
                shell.StartInfo.UseShellExecute = false;
                shell.StartInfo.Verb = "runas";
                shell.StartInfo.CreateNoWindow = true;
                shell.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                try
                {
                    shell.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            dispositivo.BitLocker = "UDP.ONLINE";
            Dispositivo.AtualizaStatusAsync(dispositivo);
            LogService.GravaLog("IniciaProcessoUDP alterou o status para UDP.ONLINE.", LogService.LogType.Debug);

        }

        static void EncerraProcessoUDP()
        {
            var processes = Process.GetProcessesByName("UDPInterface");
            foreach (var p in processes)
            {
                p.Kill();
                p.WaitForExit(30000);
            }

            dispositivo.BitLocker = "UDP.OFFLINE";
            Dispositivo.AtualizaStatusAsync(dispositivo);
            LogService.GravaLog("EncerraProcessoUDP alterou o status para UDP.OFFLINE.", LogService.LogType.Debug);

        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Exit(sender, e);
        }

        static void Exit(object sender, EventArgs e)
        {
            EncerraProcessoUDP();
            _trayIcon.Visible = false;
            Application.Exit();
        }

        public class AgentAppContext : ApplicationContext
        {
            private System.Timers.Timer timManager = null;

            void Abrir(object sender, EventArgs e)
            {
                frmMain form = new frmMain();
                form.Show();
            }

            public AgentAppContext()
            {
                try
                {
                    timManager = new System.Timers.Timer();
                    timManager.Interval = 60000;
                    timManager.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                    timManager.AutoReset = false;

                    _trayIcon = new NotifyIcon()
                    {
                        Icon = Properties.Resources.Lyli_LETTERS_Red,
                        ContextMenuStrip = new ContextMenuStrip(),
                        Visible = true,
                        Text = $"LaudaGT Manager"
                    };

                    _trayIcon.ContextMenuStrip.Items.AddRange(new ToolStripMenuItem[] {
                        new ToolStripMenuItem($"Encerrar a sessão de {config.DriverId}", null, Exit)
                    });

                    new Thread(() => MonitorDeArquivosService.MonitorarArquivos($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes", "*.config")).Start();

                    _udpIP = config.PlayStationIP;
                    new Thread(IniciaProcessoUDP).Start();

                    timManager.Start();
                }
                catch (Exception ex)
                {
                    LogService.GravaLog(ex);
                  //LyliService.EnviaErroAPI(config.ID.ToString(), ex.Message, "UDPManager");
                    return;
                }
                finally
                {
                    Mensagem("Bem-vindo!", ToolTipIcon.Info);
                }
            }

            void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                int numlogs = 0;
                try
                {
                    config = Config.ObtemDadosLocal();
                    if (_udpIP != config.PlayStationIP || !config.UDPActive)
                    {
                        if (_udpIP != config.PlayStationIP)
                        {
                            _udpIP = config.PlayStationIP;
                            LogService.GravaLog($"O endereço IP foi alterado para {_udpIP}.", LogService.LogType.Debug);
                            numlogs = numlogs + 1;
                        }

                        EncerraProcessoUDP();
                        IniciaProcessoUDP();
                    }

                    if (config.UDPActive && !_udpStatus)
                    {
                        _udpStatus = true;
                        dispositivo.BitLocker = "GT7.ONLINE";
                        Dispositivo.AtualizaStatusAsync(dispositivo);
                        LogService.GravaLog($"O status foi alterado para GT7.ONLINE pelo Timer.", LogService.LogType.Debug);
                        numlogs = numlogs + 1;
                    }

                    if (!config.UDPActive && _udpStatus)
                    {
                        _udpStatus = false;
                        dispositivo.BitLocker = "GT7.OFFLINE";
                        Dispositivo.AtualizaStatusAsync(dispositivo);
                        LogService.GravaLog($"O status foi alterado para GT7.OFFLINE pelo Timer.", LogService.LogType.Debug);
                        numlogs = numlogs + 1;
                    }

                    if (config.UDPActive && !string.IsNullOrEmpty(config.MensagemManager))
                    {
                        Mensagem(config.MensagemManager, ToolTipIcon.Info);
                        config.MensagemManager = null;
                    }

                    ((System.Timers.Timer)sender).Start();
                    if (numlogs == 0)
                        LogService.GravaLog("O Timer do sistema foi executado ...", LogService.LogType.Debug);
                }
                catch (Exception ex)
                {
                    LogService.GravaLog($"Erro no Timer: {ex.Message}", LogService.LogType.Error);
                }
            }

            private void Mensagem(string texto, ToolTipIcon icon, int tempo = 5000)
            {
                _trayIcon.BalloonTipIcon = icon;
                _trayIcon.BalloonTipText = texto;
                _trayIcon.BalloonTipTitle = "LaudaGT Manager";
                _trayIcon.ShowBalloonTip(tempo);
                LogService.GravaLog($"Mensagem exibida: {texto}", LogService.LogType.Debug);
            }

        }
    }
}