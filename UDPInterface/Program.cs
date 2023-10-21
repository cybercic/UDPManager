using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

using PDTools.SimulatorInterface;
using System.Collections.Generic;
using System.Timers;
using Refit;
using System.Net.Http;
using UDPInterface.Models;
using System.Drawing;

namespace UDPInterface;

public class Program
{

    private static Config _config = Config.ObtemDadosLocal();

    private static List<Car> _cars = Car.ObtemDadosLocal();
    private static string _carName = string.Empty;

    private static List<Track> _tracks = Track.ObtemDadosLocal();
    private static string _trackName = string.Empty;

    private static Stint? _stint = null;

    #region Lap
    private static int _currentLap = 0;
    private static int _currentSec = 1;

    private static double _lapFuelAtStart = 0;

    private static double _lapMeters_Dist = 0;
    private static double _lapMillis_Time = 0;

    private static int _lapMaxThrottle = 0;
    private static int _lapMaxBrake = 0;
    private static int _lapCarCoast = 0;
    private static int _lapTireSpin = 0;
    private static int _lapTireHeat = 0;
    private static int _lapTrialBrake = 0;
    private static int _lapEarlyGas = 0;

    private static string _tempFileNameLaps = string.Empty;
  //private static string _tempFileNameZ1 = string.Empty;
    #endregion

    private static SimulatorInterface? simInterface = null;
    private static SimulatorPacket? _previousPacket = null;

    private static bool _tracePackets = false;

    private static bool _firstPacketReceived = false;
    private static DateTime _lastPacketReceived;

    private static System.Timers.Timer _timer = new System.Timers.Timer();

    static async Task Main(string[] args)
    {

        AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

        Console.WriteLine();
        Console.WriteLine("GT7 UDP Packet Data - Baseado no Nenkai#9075");
        Console.WriteLine();

        if (args.Length == 0)
        {
            Console.WriteLine("Comando: UDPInterface.exe <Endereço IP do PS4/PS5> ('--gtsport' para o GT Sport)");
            Console.WriteLine();
            Console.WriteLine("Opcional: '--trace' para salvar todos os Pacotes no modo DEBUG)");
            Console.WriteLine();
            return;
        }

        _tracePackets = args.Contains("--trace");

        bool gtsport = args.Contains("--gtsport");

        Console.WriteLine("Iniciando a interface ...");
        Console.WriteLine();
        LogService.GravaLog("Iniciando a interface ...", LogService.LogType.Debug);

        if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes"))
            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes");

#if DEBUG

        Console.WriteLine("Logando as informações quando o carro está no paddock!");
        Console.WriteLine();

        if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Stints.csv"))
            File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Stints.csv", new string[] { new SimulatorPacket().HeaderToDebugStints() });

        if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Laps.csv"))
            File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Laps.csv", new string[] { new SimulatorPacket().HeaderToDebugLaps() });

        if (_tracePackets)
        {

            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\UDP_Trace_Packets.csv"))
                File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\UDP_Trace_Packets.csv", new string[] { new SimulatorPacket().HeaderToTraceMode() });

            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Details.csv"))
                File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Details.csv", new string[] { new SimulatorPacket().HeaderToDebugDetails() });

            Console.WriteLine("Logando as informações mesmo quando o carro não está inscrito!");
            Console.WriteLine();

        }

#endif

        // Validar o dispositivo ...
        try
        {
            var apiClient = RestService.For<IDispositivo>(LyliService.BaseUrl);
            var resultAPI = await apiClient.AutenticaDispositivo(_config.ID.ToString());

            if (resultAPI.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Dispositivo não autorizado!");
                LogService.GravaLog("Dispositivo não autorizado!", LogService.LogType.Error);
                return;
            }
        }
        catch (Exception ex)
        {
            LogService.GravaLog(ex);
            LogService.GravaLog("Dispositivo não autorizado!", LogService.LogType.Debug);
            return;
        }

        SimulatorInterfaceGameType type = SimulatorInterfaceGameType.GT7;
        if (gtsport)
            type = SimulatorInterfaceGameType.GTSport;

        simInterface = new SimulatorInterface(args[0], type);
        simInterface.OnReceive += SimInterface_OnReceive;

        var cts = new CancellationTokenSource(); // Como funciona esse CTS? Não serve para o Console!

        new Thread(() =>
        {
            _timer.Interval = 30000;
            _timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            _timer.Start();
        }).Start();

        var task = simInterface.Start(cts.Token);

        // Recebendo os pacotes ...
        try
        {
            await task;

            // Para testar as threads ...
            // for (int i = 0; i < 100000; i++)
            // {
            //     _recevingPackages = true;
            //     LogService.GravaLog($"Processando {i}", LogService.LogType.Debug);
            //     Thread.Sleep(500);
            //     _recevingPackages = false;
            // }
        }
        catch (OperationCanceledException e)
        {
            LogService.GravaLog(e);
            Console.WriteLine($"Encerrando a interface ...");
            Console.WriteLine();

            throw;
        }
        catch (Exception e)
        {
            LogService.GravaLog(e);
            Console.WriteLine($"Erro durante a execução: {e.Message}");
            Console.WriteLine();

            throw;
        }
        finally
        {
            // Important to clear up underlaying socket
            LogService.GravaLog("Cancelando a interface ...", LogService.LogType.Debug);
            Console.WriteLine($"Cancelando a interface ...");
            Console.WriteLine();

            simInterface.Dispose();
        }
    }

    private static void SimInterface_OnReceive(SimulatorPacket packet)
    {

        _lastPacketReceived = DateTime.Now;

        if (!packet.Flags.HasFlag(SimulatorFlags.Processing))
        {
            if (packet.Flags.HasFlag(SimulatorFlags.CarOnPaddock))
            {
                if (_stint != null && _currentLap > packet.CurrentLap)
                {

#if DEBUG

                    if (_tracePackets)
                    {
                        File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\UDP_Trace_Packets.csv", new string[] { packet.LineToTraceMode("Route5: if=true") });
                    }

#endif

                    // Voltou para os boxes!
                    if (_stint.LapsRun > 0)
                    {
                        // Voltou para os boxes após completar pelo menos uma volta!
                        Console.WriteLine();
                        Console.WriteLine($"Abortando a volta {_currentLap.ToString()} após {(_lapMeters_Dist / 1000).ToString("F2")} km ...");
                        LogService.GravaLog($"Abortando a volta {_currentLap.ToString()} após {(_lapMeters_Dist / 1000).ToString("F2")} km ...", LogService.LogType.Debug);

                        File.Move(_tempFileNameLaps, _tempFileNameLaps.Replace("running", "aborted"));
                        //File.Move(_tempFileNameZ1, _tempFileNameZ1.Replace("capturing", "incompleted"));

                        _lapMeters_Dist = 0;
                        _lapMillis_Time = 0;
                        _lapMaxThrottle = 0;
                        _lapMaxBrake = 0;
                        _lapCarCoast = 0;
                        _lapTireSpin = 0;
                        _lapTireHeat = 0;
                        _lapTrialBrake = 0;
                        _lapEarlyGas = 0;

                        _stint.DateEnd = DateTime.Now;
                        _stint = null;
                    }
                    else
                    {
                        // Voltou para os boxes sem completar nenhuma volta!
                        Console.WriteLine();
                        Console.WriteLine($"Abortando a volta inicial {_currentLap.ToString()} após {(_lapMeters_Dist / 1000).ToString("F2")} km ...");
                        LogService.GravaLog($"Abortando a volta inicial {_currentLap.ToString()} após {(_lapMeters_Dist / 1000).ToString("F2")} km ...", LogService.LogType.Debug);

                        _lapMeters_Dist = 0;
                        _lapMillis_Time = 0;
                        _lapMaxThrottle = 0;
                        _lapMaxBrake = 0;
                        _lapCarCoast = 0;
                        _lapTireSpin = 0;
                        _lapTireHeat = 0;
                        _lapTrialBrake = 0;
                        _lapEarlyGas = 0;

                        _stint.DateEnd = DateTime.Now;

                        // Apagar os arquivos!
                        if (File.Exists(_tempFileNameLaps))
                            File.Delete(_tempFileNameLaps);
                        //if (File.Exists(_tempFileNameZ1))
                        //    File.Delete(_tempFileNameZ1);
                        if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\{_stint.StintId}_000.config"))
                            File.Delete($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\{_stint.StintId}_000.config");

#if DEBUG

                        if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\\\DEBUG_Details_{_stint.StintId}.csv"))
                            File.Delete($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\\\DEBUG_Details_{_stint.StintId}.csv");

#endif

                        _stint = null;
                    }
                }
                else if (packet.CurrentLap > 0)
                {

#if DEBUG

                    if (_tracePackets)
                    {
                        File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\UDP_Trace_Packets.csv", new string[] { packet.LineToTraceMode("Route4: elseif ") });
                    }

#endif

                    if (_stint == null)
                    {
                        // Foi para a pista agora! Criar o stint!
                        _stint = new Stint();
                        _stint.CarId = packet.CarID;

                        _stint.FuelCapacity = packet.FuelCapacity;
                        _stint.CarsAtPreRace = packet.NumCarsAtPreRace;
                        _stint.PositionAtPreRace = packet.PreRaceStartPosition;
                        _stint.TotalLaps = packet.TotalRaceLaps;
                        _stint.HasTurbo = packet.Flags.HasFlag(SimulatorFlags.HasTurbo);
                        _stint.CarMaxSpeed = packet.CarMaximumSpeed;
                        _stint.RPMFlashing = packet.MinAlertRPM;
                        _stint.RPMLimiter = packet.MaxAlertRPM;
                        _stint.TireRadius_FL = packet.TireRadius_FL;
                        _stint.TireRadius_FR = packet.TireRadius_FR;
                        _stint.TireRadius_RL = packet.TireRadius_RL;
                        _stint.TireRadius_RR = packet.TireRadius_RR;
                        _stint.GearMaxSpeed = packet.GearsTopSpeed;
                        _stint.GearRatios = string.Join(". ", packet.GearsRatios);

                        _lapFuelAtStart = packet.FuelLevel;
                        _currentSec = 1;

                        _tempFileNameLaps = $"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\{_stint.StintId}_{packet.CurrentLap.ToString().PadLeft(3, '0')}.running";
                        File.AppendAllLines(_tempFileNameLaps, new string[] { packet.HeaderToGTDBFile() });
                        //_tempFileNameZ1 = $"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\{_stint.StintId}_{packet.CurrentLap.ToString().PadLeft(3, '0')}.capturing";
                        //File.AppendAllLines(_tempFileNameZ1, new string[] { packet.HeaderToZ1File() });

#if DEBUG

                        File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Details_{_stint.StintId}.csv", new string[] { new SimulatorPacket().HeaderToDebugDetails() });

#endif

                        Console.WriteLine();
                        Console.WriteLine($"Criado o Stint {_stint.StintId.ToString()} ...");
                        LogService.GravaLog($"Criado o Stint {_stint.StintId.ToString()} ...", LogService.LogType.Debug);

                        foreach (Car car in _cars)
                        {
                            if (car.CarId == packet.CarID)
                            {
                                Console.WriteLine($"Capturando voltas com o carro {car.MakerAndCar} ...");
                                LogService.GravaLog($"Capturando voltas com o carro {car.MakerAndCar} ...", LogService.LogType.Debug);
                                _carName = car.MakerAndCar;
                                break;
                            }
                        }

                    }

                    if (packet.CurrentLap > _currentLap && _currentLap > 0)
                    {
                        // Completou a volta! 
                        Console.WriteLine();
                        Console.WriteLine($"Completou a volta {_currentLap:D3} com {(_lapMeters_Dist / 1000):F2} km ...");
                        LogService.GravaLog($"Completou a volta {_currentLap:D3} com {(_lapMeters_Dist / 1000):F2} km ...", LogService.LogType.Debug);

                        _stint.LapsRun++;
                        _currentSec = 1;

                        if (_stint.LapsRun == 1)
                        {
                            _firstX = packet.Position.X;
                            _firstZ = packet.Position.Z; // Z = Y para identificar a pista!
                            _lastX = _previousPacket.Position.X;
                            _lastZ = _previousPacket.Position.Z; // Z = Y para identificar a pista!

                            Console.WriteLine();
                            Console.WriteLine($"Identificando a pista: lastX:{_lastX}, lastZ:{_lastZ} > firstX:{_firstX}, firstZ:{_firstZ} ...");
                            LogService.GravaLog($"Identificando a pista: lastX:{_lastX}, lastZ:{_lastZ} > firstX:{_firstX}, firstZ:{_firstZ} ...", LogService.LogType.Debug);

                            foreach (Track track in _tracks)
                            {
                                if (DoLinesIntersect(track.P1X, track.P1Y, track.P2X, track.P2Y, _firstX, _firstZ, _lastX, _lastZ))
                                {
                                    Console.WriteLine($"Capturando voltas na pista de  {track.TrackName} ...");
                                    LogService.GravaLog($"Capturando voltas na pista de  {track.TrackName} ...", LogService.LogType.Debug);
                                    _stint.TrackId = track.TrackId;
                                    _trackName = track.TrackName;
                                    break;
                                }
                            }

                        }

                        if (_stint.LapsRun == 2)
                        {
                            _stint.BestLap = packet.LastLapTime;
                        }

                        if (_stint.LapsRun > 2)
                        {
                            if (_stint.BestLap == null)
                                _stint.BestLap = packet.LastLapTime;
                            else if (_stint.BestLap > packet.LastLapTime)
                                _stint.BestLap = packet.LastLapTime;
                        }

                        _stint.LastLap = packet.LastLapTime;
                        _stint.FuelLeft = packet.FuelLevel;

                        //var texto = File.ReadAllText(_tempFileNameZ1);
                        //texto = texto.Replace("#ThisLapPH#", $"{packet.LastLapTime:mm\\:ss\\.fff}");
                        //File.WriteAllText(_tempFileNameZ1, texto);

                        File.Move(_tempFileNameLaps, _tempFileNameLaps.Replace("running", "finished"));
                        //File.Move(_tempFileNameZ1, _tempFileNameZ1.Replace("capturing", "LOG"));

                        _stint.AddLap(new Lap() { StintId = _stint.StintId, LapNumber = _currentLap, LapTime = packet.LastLapTime, LapDistance = _lapMeters_Dist, FuelUsed = _lapFuelAtStart - packet.FuelLevel, MaxThrottle = _lapMaxThrottle, MaxBrake = _lapMaxBrake, CarCoast = _lapCarCoast, TireSpin = _lapTireSpin, TireHeat = _lapTireHeat, TrialBrake = _lapTrialBrake, EarlyGas = _lapEarlyGas });

                        _lapMeters_Dist = 0;
                        _lapMillis_Time = 0;
                        _lapMaxThrottle = 0;
                        _lapMaxBrake = 0;
                        _lapCarCoast = 0;
                        _lapTireSpin = 0;
                        _lapTireHeat = 0;
                        _lapTrialBrake = 0;
                        _lapEarlyGas = 0;
                        _lapFuelAtStart = packet.FuelLevel;

                        _tempFileNameLaps = $"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\{_stint.StintId}_{packet.CurrentLap.ToString().PadLeft(3, '0')}.running";
                        File.AppendAllLines(_tempFileNameLaps, new string[] { packet.HeaderToGTDBFile() });
                        //_tempFileNameZ1 = $"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\{_stint.StintId}_{packet.CurrentLap.ToString().PadLeft(3, '0')}.capturing";
                        //File.AppendAllLines(_tempFileNameZ1, new string[] { packet.HeaderToZ1File() });
                    }

                    // Está na pista! Processar o pacote ...
                    _lapMeters_Dist += Math.Round(packet.MetersPerSecond / 60, 7);
                    _lapMillis_Time += Math.Round(16.6666667, 7);

                    // ToDo: Incluir o código para verificar se mudou de setor ...

                    File.AppendAllLines(_tempFileNameLaps, new string[] { packet.LineToGTDBFile(1, _lapMeters_Dist, _lapMillis_Time, _previousPacket) });
                    //File.AppendAllLines(_tempFileNameZ1, new string[] { packet.LineToZ1File(_stint.DriverId, _stint.DateStart, _trackName, _carName, "SecId", _lapDistanceMeters, _lapTimeMilliseconds) });

#if DEBUG

                    File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Details_{_stint.StintId}.csv", new string[] { packet.LineToDebugDetails(_stint.StintId.ToString(), "SecId", _lapDistanceMeters, _lapTimeMilliseconds) });

#endif

                    if (packet.Position.X > _maxX)
                        _maxX = packet.Position.X;

                    if (packet.Position.X < _minX)
                        _minX = packet.Position.X;

                    if (packet.Position.Z > _maxZ) // Z = Y para identificar a pista!
                        _maxZ = packet.Position.Z;

                    if (packet.Position.Z < _minZ) // Z = Y para identificar a pista!
                        _minZ = packet.Position.Z;

                    if (packet.ThrottleRAW == 255)
                        _lapMaxThrottle = _lapMaxThrottle + 1;

                    if (packet.BrakeRAW == 255)
                        _lapMaxBrake = _lapMaxBrake + 1;

                    if (packet.ThrottleRAW == 0 &&
                        packet.BrakeRAW == 0)
                        _lapCarCoast = _lapCarCoast + 1;

                    // Calcular TireSpin (rodas acima de 1.1x a velocidade do chassis)
                    if (packet.WheelRevsPerSec_FL * -1 * packet.TireRadius_FL >= packet.MetersPerSecond * 1.1 ||
                        packet.WheelRevsPerSec_FR * -1 * packet.TireRadius_FR >= packet.MetersPerSecond * 1.1 ||
                        packet.WheelRevsPerSec_RL * -1 * packet.TireRadius_RL >= packet.MetersPerSecond * 1.1 ||
                        packet.WheelRevsPerSec_RR * -1 * packet.TireRadius_RR >= packet.MetersPerSecond * 1.1)
                        _lapTireSpin = _lapTireSpin + 1;

                    // Calcular TireHeat (acima de 91ºC)
                    if (packet.TireTemp_FL >= 92 ||
                        packet.TireTemp_FR >= 92 ||
                        packet.TireTemp_RL >= 92 ||
                        packet.TireTemp_RR >= 92)
                        _lapTireHeat = _lapTireHeat + 1;

                    // Calcular TrialBrake (14% a 16%)
                    if (packet.BrakeRAW >= 35 && packet.BrakeRAW <= 40)
                        _lapTrialBrake = _lapTrialBrake + 1;

                    // Calcular EarlyGas (14% a 16%)
                    if (packet.ThrottleRAW >= 35 && packet.ThrottleRAW <= 40)
                        _lapEarlyGas = _lapEarlyGas + 1;

                    //_lastX = packet.Position.X;
                    //_lastZ = packet.Position.Z; // Z = Y para identificar a pista!
                    _currentLap = packet.CurrentLap;
                }
                else
                {

#if DEBUG

                    if (_tracePackets)
                    {
                        File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\UDP_Trace_Packets.csv", new string[] { packet.LineToTraceMode("Route3: else depois do elseif ") });
                        File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Details.csv", new string[] { packet.LineToDebugDetails("StintId N/A", "SecId", _lapDistanceMeters, _lapTimeMilliseconds) });
                    }

#endif

                }
            }
            else
            {

#if DEBUG

                if (_tracePackets)
                {
                    File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\UDP_Trace_Packets.csv", new string[] { packet.LineToTraceMode("Route0: CarOnPaddock=FALSE") });
                    File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Details.csv", new string[] { packet.LineToDebugDetails("StintId N/A", "SecId", _lapDistanceMeters, _lapTimeMilliseconds) });
                }

#endif

                if (_stint != null)
                {

#if DEBUG

                    if (_tracePackets)
                    {
                        File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\UDP_Trace_Packets.csv", new string[] { packet.LineToTraceMode("Route2B: CarOnPaddock=FALSE - Vai encerrar o Stint") });
                        File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Details.csv", new string[] { packet.LineToDebugDetails(_stint.StintId.ToString(), "SecId", _lapDistanceMeters, _lapTimeMilliseconds) });
                    }

#endif

                    Console.WriteLine();
                    Console.WriteLine($"Pacote CarOnPaddock = FALSE após abrir um stint ...");
                    LogService.GravaLog($"Pacote CarOnPaddock = FALSE após abrir um stint ...", LogService.LogType.Debug);

                    // Se desinscreveu! Encerrar o stint!
                    _stint.DateEnd = DateTime.Now;
                    _stint = null;
                }
            }
        }
        else
        {

#if DEBUG

            // Ignorar ou Debugar o pacote!

            // Criar uma lógica para capturar replays!

            if (_tracePackets)
            {
                File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\UDP_Trace_Packets.csv", new string[] { packet.LineToTraceMode("Route1: LoadingOrProcessing=TRUE") });
                File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Details.csv", new string[] { packet.LineToDebugDetails("StintId N/A", "SecId", _lapDistanceMeters, _lapTimeMilliseconds) });
            }

#endif

        }
        _previousPacket = packet;
    }

    private static void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        int numlogs = 0;
        if (_lastPacketReceived == DateTime.MinValue)
        {
            LogService.GravaLog("A interface ainda não recebeu nenhum pacote ...", LogService.LogType.Debug);
            numlogs = numlogs + 1;
        }
        else
        {
            if (_firstPacketReceived == false)
            {
                _firstPacketReceived = true;
                LogService.GravaLog($"Primeiro pacote recebido em {_lastPacketReceived} ...", LogService.LogType.Debug);
                numlogs = numlogs + 1;
            }
            var intervalo = DateTime.Now - _lastPacketReceived;
            if (intervalo.TotalMilliseconds > 15000)
            {
                _config.UDPActive = false;
                LogService.GravaLog("A interface não está mais recebendo pacotes ...", LogService.LogType.Debug);
                numlogs = numlogs + 1;
            }
            else
            {
                if (_config.UDPActive == false)
                    _config.UDPActive = true;

                if (numlogs == 0)
                    LogService.GravaLog($"Pacote recebido em {_lastPacketReceived} ...", LogService.LogType.Debug);
                numlogs = numlogs + 1;
            }
        }
        if (numlogs == 0)
            LogService.GravaLog("O Timer do UDPInterface foi executado ...", LogService.LogType.Debug);
    }

    #region TrackDetect

    private static float _lastX = 0;
    private static float _lastZ = 0;
    private static float _firstX = 0;
    private static float _firstZ = 0;

    private static double _minX = 0;
    private static double _minZ = 0;
    private static double _maxX = 0;
    private static double _maxZ = 0;

    private struct Point
    {
        public float x, y;

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private struct Line
    {
        public Point start, end;

        public Line(Point start, Point end)
        {
            this.start = start;
            this.end = end;
        }
    }

    private static bool DoLinesIntersect(float carPosLastX, float carPosLastY, float carPosNowX, float carPosNowY, float startPos1X, float startPos1Y, float startPos2X, float startPos2Y)
    {
        // Definição dos pontos
        Point startPointCarPath = new Point(carPosLastX, carPosLastY);
        Point endPointCarPath = new Point(carPosNowX, carPosNowY);
        Point startPointFinishLine = new Point(startPos1X, startPos1Y);
        Point endPointFinishLine = new Point(startPos2X, startPos2Y);

        // Criação das linhas
        Line carPath = new Line(startPointCarPath, endPointCarPath);
        Line finishLine = new Line(startPointFinishLine, endPointFinishLine);

        float q = (carPath.start.y - finishLine.start.y) * (finishLine.end.x - finishLine.start.x) - (carPath.start.x - finishLine.start.x) * (finishLine.end.y - finishLine.start.y);
        float d = (carPath.end.x - carPath.start.x) * (finishLine.end.y - finishLine.start.y) - (carPath.end.y - carPath.start.y) * (finishLine.end.x - finishLine.start.x);

        if (d == 0)
        {
            return false;
        }

        float r = q / d;

        q = (carPath.start.y - finishLine.start.y) * (carPath.end.x - carPath.start.x) - (carPath.start.x - finishLine.start.x) * (carPath.end.y - carPath.start.y);
        float s = q / d;

        if (r < 0 || r > 1 || s < 0 || s > 1)
        {
            return false;
        }

        return true;
    }

    static void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        _config.UDPActive = false;
        LogService.GravaLog($"Atualizando a propriedade UDPActive para FALSE ao sair ...", LogService.LogType.Debug);
        if (simInterface != null) simInterface.Dispose();
    }
    #endregion
}