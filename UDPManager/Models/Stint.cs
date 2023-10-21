using System.Text.Json.Serialization;

namespace UDPManager.Models;

public class Stint
{
    public Guid StintId { get; set; }
    public string TeamId { get; set; }
    public string DriverId { get; set; }
    public int TrackId { get; set; }
    public int CarId { get; set; }
    public int LapsRun { get; set; }
    public int LapsGPS { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<Lap>? Laps { get; set; }
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
