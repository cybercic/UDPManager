using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UDPInterface.Models;

public class Stint
{

    private static List<Track> _tracks = Track.ObtemDadosLocal();
    private static List<Car> _cars = Car.ObtemDadosLocal();

    public Guid StintId { get; set; }
    public string TeamId { get; set; }
    public string DriverId { get; set; }
    private int trackId;

    public int TrackId
    {
        get => trackId;
        set
        {
            trackId = value;
            foreach (Track track in _tracks)
            {
                if (track.TrackId == value)
                {
                    MensagemManager($"Capturando voltas na pista de {track.TrackName}.");
                    break;
                }
            }

        }
    }

    private int carId;
    public int CarId
    {
        get => carId;
        set
        {
            carId = value;
            foreach (Car car in _cars)
            {
                if (car.CarId==value)
                {
                    MensagemManager($"Capturando voltas com o carro {car.MakerAndCar}.");
                    break;
                }
            }

        }
    }

    public int LapsRun { get; set; }
    public int LapsGPS { get; set; }
    public DateTime DateStart { get; set; }
    public TimeSpan? BestLap { get; set; }
    public TimeSpan? LastLap { get; set; }
    public double? FuelLeft { get; set; }
    public double? FuelCapacity { get; set; }
    public short CarsAtPreRace { get; set; }
    public short PositionAtPreRace { get; set; }
    public short TotalLaps { get; set; }
    public bool HasTurbo { get; set; }
    public short CarMaxSpeed { get; set; }
    public short RPMFlashing { get; set; }
    public short RPMLimiter { get; set; }
    public float TireRadius_FL { get; set; }
    public float TireRadius_FR { get; set; }
    public float TireRadius_RL { get; set; }
    public float TireRadius_RR { get; set; }
    public float GearMaxSpeed { get; set; }
    public string? GearRatios { get; set; }

    private DateTime? dateEnd;
    public DateTime? DateEnd
    {
        get => dateEnd;
        set
        {
            dateEnd = value;
            GravaDadosLocal(); //Salvando a DateEnd!

#if DEBUG
            File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Stints.csv", new string[] { LineToDebugStints() });
#endif

        }
    }

    public List<Lap> Laps { get; set; }

    public Stint()
    {
        var config = Config.ObtemDadosLocal();
        TeamId = config.TeamId;
        DriverId = config.DriverId;

        StintId = Guid.NewGuid();
        DateStart = DateTime.Now;
        Laps = new List<Lap>();

        GravaDadosLocal();
    }

    public void AddLap(Lap lap)
    {
        Laps.Add(lap);
        GravaDadosLocal();

#if DEBUG
        File.AppendAllLines($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\DEBUG_Laps.csv", new string[] { LineToDebugLaps(lap.LapNumber, lap.LapTime, lap.LapDistance, lap.FuelUsed, lap.MaxThrottle, lap.MaxBrake, lap.CarCoast, lap.TireSpin, lap.TireHeat, lap.TrialBrake, lap.EarlyGas) });
#endif

    }

    public Stint ObtemDadosLocal()
    {
        var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\{StintId}_000.config");

        return JsonSerializer.Deserialize<Stint>(json) ?? new Stint();
    }

    public void GravaDadosLocal()
    {
        if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes"))
             Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes");

        File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Pacotes\\{StintId}_000.config", ToString());
    }

    public string LineToDebugStints()
    {

        // return "StintId;TeamId;DriverId;TrackId;CarId;LapsRun;LapsGPS;DateStart;DateEnd;BestLap;LastLap;FuelLeft;FuelCapacity;CarsAtPreRace;PositionAtPreRace;TotalLaps;HasTurbo;CarMaxSpeed;RPMFlashing;RPMLimiter;TireRadius_FL;TireRadius_FR;TireRadius_RL;TireRadius_RR;GearMaxSpeed;GearRatios";

        var config = Config.ObtemDadosLocal();

        StringBuilder linha = new StringBuilder();

        linha.Append($"{StintId};");
        linha.Append($"{config.TeamId};");
        linha.Append($"{config.DriverId};");
        linha.Append($"{TrackId};");
        linha.Append($"{CarId};");
        linha.Append($"{LapsRun};");
        linha.Append($"{LapsGPS};");
        linha.Append($"{DateStart};");
        linha.Append($"{DateEnd};");
        linha.Append($"{BestLap};");
        linha.Append($"{LastLap};");
        linha.Append($"{FuelLeft:F3};");
        linha.Append($"{FuelCapacity:F3};");
        linha.Append($"{CarsAtPreRace};");
        linha.Append($"{PositionAtPreRace};");
        linha.Append($"{TotalLaps};");
        linha.Append($"{HasTurbo};");
        linha.Append($"{CarMaxSpeed};");
        linha.Append($"{RPMFlashing};");
        linha.Append($"{RPMLimiter};");
        linha.Append($"{TireRadius_FL};");
        linha.Append($"{TireRadius_FR};");
        linha.Append($"{TireRadius_RL};");
        linha.Append($"{TireRadius_RR};");
        linha.Append($"{GearMaxSpeed};");
        linha.Append($"{string.Join(", ", GearRatios)}");


        return linha.ToString();
    }

    public string LineToDebugLaps(int lapNumber, TimeSpan lapTime, double lapDistance, double fuelUsed, int maxThrottle, int maxBrake, int carCoast, int tireSpin, int tireHeat, int trialBrake, int earlyGas)
    {
        // return "StintId;LapNumber;LapTime;LapDistance;FuelUsed;MaxThrottle;MaxBrake;CarCoast;TireSpin;TireHeat;TrialBrake;EarlyGas";

        var config = Config.ObtemDadosLocal();

        StringBuilder linha = new StringBuilder();

        linha.Append($"{StintId};");
        linha.Append($"{lapNumber};");
        linha.Append($"{lapTime:mm\\:ss\\.fff};");
        linha.Append($"{lapDistance:F3};");
        linha.Append($"{fuelUsed:F3};");
        linha.Append($"{maxThrottle};");
        linha.Append($"{maxBrake};");
        linha.Append($"{carCoast};");
        linha.Append($"{tireSpin};");
        linha.Append($"{tireHeat};");
        linha.Append($"{trialBrake};");
        linha.Append($"{earlyGas}");

        return linha.ToString();
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    private void MensagemManager(string textoMensagem)
    {
        var config = Config.ObtemDadosLocal();
        config.MensagemManager = $"{textoMensagem}";
    }
}

public class Lap
{
    public Guid StintId { get; set; }
    public int LapNumber { get; set; }
    public TimeSpan LapTime { get; set; }
    public double LapDistance { get; set; }

    public double FuelUsed { get; set; } 
    public int MaxThrottle { get; set; } 
    public int MaxBrake { get; set; }    
    public int CarCoast { get; set; }    
    public int TireSpin { get; set; }    
    public int TireHeat { get; set; }    
    public int TrialBrake { get; set; }
    public int EarlyGas { get; set; }
}
