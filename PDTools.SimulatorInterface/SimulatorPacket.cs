using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using System.Numerics;
using Syroot.BinaryData.Memory;
using System.Reflection;
using System.Net.Sockets;

namespace PDTools.SimulatorInterface
{
    /// <summary>
    /// Packet from the GT Engine Simulation.
    /// </summary>
    public class SimulatorPacket
    {
        #region PacketDefinitions
        /// <summary>
        /// Peer address.
        /// </summary>
        public IPEndPoint RemoteIP { get; private set; }

        /// <summary>
        /// Date when this packet was received.
        /// </summary>
        public DateTimeOffset DateReceived { get; private set; }

        /// <summary>
        /// Game Type linked to this packet.
        /// </summary>
        public SimulatorInterfaceGameType GameType { get; set; }

        /// <summary>
        /// Position on the track. Track units are in meters.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Velocity in track units (which are meters) for each axis.
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Rotation (Pitch/Yaw/Roll) from -1 to 1.
        /// </summary>
        public Vector3 Rotation { get; set; }

        /// <summary>
        /// Orientation to North. 1.0 is north, 0.0 is south.
        /// </summary>
        public float ROtNorth { get; set; }

        /// <summary>
        /// How fast the car turns around axes. (In radians/second, -1 to 1).
        /// </summary>
        public Vector3 AngularVelocity { get; set; }

        /// <summary>
        /// Body height.
        /// </summary>
        public float RideHeight { get; set; }

        /// <summary>
        /// Engine revolutions per minute
        /// </summary>
        public float EngineRPM { get; set; }

        /// <summary>
        /// Fuel level for the current car (in liters, from 0 to <see cref="FuelCapacity"/>).
        /// <para> Note: This may change from 0 when regenerative braking with electric cars, check accordingly with <see cref="FuelCapacity"/>. </para>
        /// </summary>
        public float FuelLevel { get; set; }

        /// <summary>
        /// Max fuel capacity for the current car.
        /// Will be 100 for most cars, 5 for karts, 0 for electric cars
        /// </summary>
        public float FuelCapacity { get; set; }

        /// <summary>
        /// Current speed in meters per second. <see cref="MetersPerSecond * 3.6"/> to get it in KPH.
        /// </summary>
        public float MetersPerSecond { get; set; }

        /// <summary>
        /// Value below 1.0 is below 0 ingame, so 2.0 = 1 x 100kPa
        /// </summary>
        public float TurboBoost { get; set; }

        /// <summary>
        /// Oil Pressure (in Bars)
        /// </summary>
        public float OilPressure { get; set; }

        /// <summary>
        /// Games will always send 85.
        /// </summary>
        public float WaterTemp { get; set; }

        /// <summary>
        /// Games will always send 110.
        /// </summary>
        public float OilTemp { get; set; }

        /// <summary>
        /// Front Left Tire - Surface Temperature (in °C)
        /// </summary>
        public float TireTemp_FL { get; set; }

        /// <summary>
        /// Front Right - Surface Temperature (in °C)
        /// </summary>
        public float TireTemp_FR { get; set; }

        /// <summary>
        /// Rear Left - Surface Temperature (in °C)
        /// </summary>
        public float TireTemp_RL { get; set; }

        /// <summary>
        /// Rear Right - Surface Temperature (in °C)
        /// </summary>
        public float TireTemp_RR { get; set; }

        /// <summary>
        /// Id of the packet for proper ordering.
        /// </summary>
        public int PacketId { get; set; }

        /// <summary>
        /// Current lap count.
        /// </summary>
        public short CurrentLap { get; set; }

        /// <summary>
        /// Laps to finish.
        /// </summary>
        public short TotalRaceLaps { get; set; }

        /// <summary>
        /// Best Lap Time. 
        /// <para>Defaults to -1 millisecond when not set.</para>
        /// </summary>
        public TimeSpan BestLapTime { get; set; }

        /// <summary>
        /// Last Lap Time.
        /// <para>Defaults to -1 millisecond when not set.</para>
        /// </summary>
        public TimeSpan LastLapTime { get; set; }

        /// <summary>
        /// Current time of day on the track.
        /// </summary>
        public TimeSpan TimeOfDayProgression { get; set; }

        /// <summary>
        /// Position of the car before the race has started. Qualifying position.
        /// <para>Will be -1 once the race is started.</para>
        /// </summary>
        public short PreRaceStartPosition { get; set; }

        /// <summary>
        /// Number of cars in the race before the race has started.
        /// <para>Will be -1 once the race is started.</para>
        /// </summary>
        public short NumCarsAtPreRace { get; set; }

        /// <summary>
        /// Minimum RPM to which the rev limiter shows an alert.
        /// </summary>
        public short MinAlertRPM { get; set; }

        /// <summary>
        /// Maximum RPM to the rev limiter alert.
        /// </summary>
        public short MaxAlertRPM { get; set; }

        /// <summary>
        /// Possible max speed achievable using the current transmission settings.
        /// <para> Will change depending on transmission settings.</para>
        /// </summary>
        public short CarMaximumSpeed { get; set; }

        /// <summary>
        /// Packet flags.
        /// </summary>
        public SimulatorFlags Flags { get; set; }

        /// <summary>
        /// Current Gear for the car.
        /// <para> This value will never be more than 15 (4 bits).</para>
        /// </summary>
        public byte GearCurrent { get; set; }

        /// <summary>
        /// (Assist) Suggested Gear to downshift to. 
        /// <para> This value will never be more than 15 (4 bits), All bits on (aka 15) implies there is no current suggested gear.</para>
        /// </summary>
        public byte GearSuggested { get; set; }

        /// <summary>
        /// Throttle (0-255)
        /// </summary>
        public byte ThrottleRAW { get; set; }

        /// <summary>
        /// Brake Pedal (0-255)
        /// </summary>
        public byte BrakeRAW { get; set; }

        public byte Empty_0x93 { get; set; }

        public Vector3 RoadPlane { get; set; }

        public float RoadPlaneDistance { get; set; }

        /// <summary>
        /// Front Left Wheel - Revolutions Per Second (in Radians)
        /// </summary>
        public float WheelRevsPerSec_FL { get; set; }

        /// <summary>
        /// Front Right Wheel - Revolutions Per Second (in Radians)
        /// </summary>
        public float WheelRevsPerSec_FR { get; set; }

        /// <summary>
        /// Rear Left Wheel - Revolutions Per Second (in Radians)
        /// </summary>
        public float WheelRevsPerSec_RL { get; set; }

        /// <summary>
        /// Rear Right Wheel - Revolutions Per Second (in Radians)
        /// </summary>
        public float WheelRevsPerSec_RR { get; set; }

        /// <summary>
        /// Front Left Tire - Tire Radius (in Meters)
        /// </summary>
        public float TireRadius_FL { get; set; }

        /// <summary>
        /// Front Right Tire - Tire Radius (in Meters)
        /// </summary>
        public float TireRadius_FR { get; set; }

        /// <summary>
        /// Rear Left Tire - Tire Radius (in Meters)
        /// </summary>
        public float TireRadius_RL { get; set; }

        /// <summary>
        /// Rear Right Tire - Tire Radius (in Meters)
        /// </summary>
        public float TireRadius_RR { get; set; }

        /// <summary>
        /// Front Left Tire - Suspension Height
        /// </summary>
        public float SuspensionHeight_FL { get; set; }

        /// <summary>
        /// Front Right Tire - Suspension Height
        /// </summary>
        public float SuspensionHeight_FR { get; set; }

        /// <summary>
        /// Rear Left Tire - Suspension Height
        /// </summary>
        public float SuspensionHeight_RL { get; set; }

        /// <summary>
        /// Rear Right Tire - Suspension Height
        /// </summary>
        public float SuspensionHeight_RR { get; set; }

        /// <summary>
        /// 0.0 to 1.0
        /// </summary>
        public float Clutch { get; set; }

        /// <summary>
        /// 0.0 to 1.0
        /// </summary>
        public float ClutchEngagement { get; set; }

        /// <summary>
        /// Basically same as engine rpm when in gear and the clutch pedal is not depressed.
        /// </summary>
        public float ClutchRPMAfter { get; set; }

        /// <summary>
        /// Top Speed (as a Gear Ratio value)
        /// </summary>
        public float GearsTopSpeed { get; set; }

        /// <summary>
        /// Gear ratios for the car. Up to 7.
        /// </summary>
        public float[] GearsRatios { get; set; } = new float[7];

        /// <summary>
        /// Internal code that identifies the car.
        /// <para>This value may be overriden if using a car that uses 9 or more gears (oversight).</para>
        /// </summary>
        public int CarID { get; set; }

        public void Read(Span<byte> data)
        {
            SpanReader sr = new SpanReader(data);
            int magic = sr.ReadInt32();
            if (magic == 0x30533647) // G6S0 - GT6
                sr.Endian = Syroot.BinaryData.Core.Endian.Big; // GT6 is on PS3, it'll be sending a big endian packet
            else if (magic == 0x47375330) // 0S7G - GTSport/GT7
                sr.Endian = Syroot.BinaryData.Core.Endian.Little;
            else
                throw new InvalidDataException($"Unexpected packet magic '{magic}'.");

            Position = new Vector3(sr.ReadSingle(), sr.ReadSingle(), sr.ReadSingle());
            Velocity = new Vector3(sr.ReadSingle(), sr.ReadSingle(), sr.ReadSingle());
            Rotation = new Vector3(sr.ReadSingle(), sr.ReadSingle(), sr.ReadSingle());
            ROtNorth = sr.ReadSingle();
            AngularVelocity = new Vector3(sr.ReadSingle(), sr.ReadSingle(), sr.ReadSingle());
            RideHeight = sr.ReadSingle();
            EngineRPM = sr.ReadSingle();
            sr.Position += sizeof(int); // Skip IV
            FuelLevel = sr.ReadSingle();
            FuelCapacity = sr.ReadSingle();
            MetersPerSecond = sr.ReadSingle();
            TurboBoost = sr.ReadSingle();
            OilPressure = sr.ReadSingle();
            WaterTemp = sr.ReadSingle();
            OilTemp = sr.ReadSingle();
            TireTemp_FL = sr.ReadSingle();
            TireTemp_FR = sr.ReadSingle();
            TireTemp_RL = sr.ReadSingle();
            TireTemp_RR = sr.ReadSingle();
            PacketId = sr.ReadInt32();
            CurrentLap = sr.ReadInt16();
            TotalRaceLaps = sr.ReadInt16();
            BestLapTime = TimeSpan.FromMilliseconds(sr.ReadInt32());
            LastLapTime = TimeSpan.FromMilliseconds(sr.ReadInt32());
            TimeOfDayProgression = TimeSpan.FromMilliseconds(sr.ReadInt32());
            PreRaceStartPosition = sr.ReadInt16();
            NumCarsAtPreRace = sr.ReadInt16();
            MinAlertRPM = sr.ReadInt16();
            MaxAlertRPM = sr.ReadInt16();
            CarMaximumSpeed = sr.ReadInt16();
            Flags = (SimulatorFlags)sr.ReadInt16();

            int bits = sr.ReadByte();
            GearCurrent = (byte)(bits & 0b1111); // 4 bits
            GearSuggested = (byte)(bits >> 4); // Also 4 bits

            ThrottleRAW = sr.ReadByte();
            BrakeRAW = sr.ReadByte();
            Empty_0x93 = sr.ReadByte();

            RoadPlane = new Vector3(sr.ReadSingle(), sr.ReadSingle(), sr.ReadSingle());
            RoadPlaneDistance = sr.ReadSingle();

            WheelRevsPerSec_FL = sr.ReadSingle();
            WheelRevsPerSec_FR = sr.ReadSingle();
            WheelRevsPerSec_RL = sr.ReadSingle();
            WheelRevsPerSec_RR = sr.ReadSingle();
            TireRadius_FL = sr.ReadSingle();
            TireRadius_FR = sr.ReadSingle();
            TireRadius_RL = sr.ReadSingle();
            TireRadius_RR = sr.ReadSingle();
            SuspensionHeight_FL = sr.ReadSingle();
            SuspensionHeight_FR = sr.ReadSingle();
            SuspensionHeight_RL = sr.ReadSingle();
            SuspensionHeight_RR = sr.ReadSingle();

            sr.Position += sizeof(int) * 8; // Seems to be reserved - server does not set that

            Clutch = sr.ReadSingle();
            ClutchEngagement = sr.ReadSingle();
            ClutchRPMAfter = sr.ReadSingle();

            GearsTopSpeed = sr.ReadSingle();

            // Always read as a fixed 7 gears
            for (var i = 0; i < 7; i++)
                GearsRatios[i] = sr.ReadSingle();

            // Normally this one is not set at all. The game memcpy's the gear ratios without bound checking
            // The LC500 which has 10 gears even overrides the car code 😂
            float empty_or_gearRatio8 = sr.ReadSingle();

            CarID = sr.ReadInt32();
        }
        #endregion

        public void SetPacketInfo(SimulatorInterfaceGameType gameType, IPEndPoint remoteEndPoint, DateTimeOffset dateReceived)
        {
            GameType = gameType;
            RemoteIP = remoteEndPoint;
            DateReceived = dateReceived;
        }

        public void PrintPacket(bool debug = false)
        {

            //Console.SetCursorPosition(0, 0);

            //Console.WriteLine($"[{DateReceived} - ID {PacketId}] Simulator Interface Packet");

            //Console.WriteLine("[Car Data]");
            //Console.WriteLine($"- Car Code:      {CarCode}");
            //Console.WriteLine($"- Throttle:      {Throttle}");
            //Console.WriteLine($"- Brake:         {Brake}");
            //Console.WriteLine($"- RPM:           {EngineRPM}");
            //Console.WriteLine($"- KPH:           {Math.Round(MetersPerSecond * 3.6, 2)}"); // *** UTILIZAR ***
            //Console.WriteLine($"- Turbo Boost:   {((TurboBoost - 1.0) * 100.0):F2}kPa");   // *** UTILIZAR ***
            //Console.WriteLine($"- Fuel Level:    {GasLevel:F2}");
            //Console.WriteLine($"- Fuel Capacity: {GasCapacity:F2}");

            //Console.WriteLine($"- Oil Pressure:  {OilPressure:F2}");
            //Console.WriteLine($"- Body Height:   {BodyHeight:F2}");
            //Console.WriteLine($"- Clutch Pedal:  {ClutchPedal:F2}");
            //Console.WriteLine($"- Clutch Engag:  {ClutchEngagement:F2}");
            //Console.WriteLine($"- Clutch RPM:    {RPMFromClutchToGearbox:F2}");


            //if (SuggestedGear == 15)
            //    Console.WriteLine($"- Gear: {CurrentGear}");
            //else
            //    Console.WriteLine($"- Gear: {CurrentGear} (Suggested: {SuggestedGear})");

            //Console.WriteLine($"- Calculated Max Speed: {CalculatedMaxSpeed}kph");
            //Console.WriteLine($"- Min & Max RPM Alerts: {MinAlertRPM} - {MaxAlertRPM}");

            //Console.WriteLine($"- Flags: {Flags,-100}");
            //Console.WriteLine($"- Gear Ratios: {string.Join(", ", GearRatios)}");          // *** UTILIZAR ***
            //Console.WriteLine($"- Tire Heights > FL:{TireFL_SusHeight:F2} FR:{TireFR_SusHeight:F2} RL:{TireRL_SusHeight:F2} RR:{TireRR_SusHeight:F2}");
            //Console.WriteLine($"- Tire RPS     > FL:{WheelFL_RevPerSecond:F2} FR:{WheelFR_RevPerSecond:F2} RL:{WheelRL_RevPerSecond:F2} RR:{WheelRR_RevPerSecond:F2}");
            //Console.WriteLine($"- Tire Radius  > FL:{TireFL_TireRadius:F2} FR:{TireFR_TireRadius:F2} RL:{TireRL_TireRadius:F2} RR:{TireRR_TireRadius:F2}");

            //Console.WriteLine($"- Tire Temperatures");
            //Console.WriteLine($"    FL: {TireFL_SurfaceTemperature:F2}°C | FR: {TireFR_SurfaceTemperature:F2}°C   ");
            //Console.WriteLine($"    RL: {TireRL_SurfaceTemperature:F2}°C | RR: {TireRR_SurfaceTemperature:F2}°C   ");

            //Console.WriteLine();
            //Console.WriteLine("[Race Data]");
            //Console.WriteLine($"- Current Lap: {CurrentLap}");

            //if (BestLapTime.TotalMilliseconds == -1)
            //    Console.WriteLine($"- Best: N/A");
            //else
            //    Console.WriteLine($"- Best: {BestLapTime:mm\\:ss\\.fff}");

            //if (LastLapTime.TotalMilliseconds == -1)
            //    Console.WriteLine($"- Last: N/A");
            //else
            //    Console.WriteLine($"- Last: {LastLapTime:mm\\:ss\\.fff}");

            //Console.WriteLine($"- Time of Day: {TimeOfDayProgression:hh\\:mm\\:ss}");

            //Console.WriteLine($"- PreRaceStartPositionOrQualiPos: {PreRaceStartPositionOrQualiPos}");
            //Console.WriteLine($"- NumCarsAtPreRace: {NumCarsAtPreRace}");

            //Console.WriteLine();
            //Console.WriteLine("[Positional Information]");
            //Console.WriteLine($"- Position: {Position:F6}     ");
            //Console.WriteLine($"- Velocity: {Velocity:F6}    ");
            //Console.WriteLine($"- Rotation: {Rotation:F6}     ");
            //Console.WriteLine($"- Angular Velocity: {AngularVelocity:F6}   ");
            //Console.WriteLine($"- Road Plane: {RoadPlane:F6}   ");

            //if (debug)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("[Unknowns]");
            //    Console.WriteLine($"0x48 (Float): {FuelCapacity:F2}   ");
            //    Console.WriteLine($"0x93 (byte): {Empty_0x93:F2}   ");
            //}

        } // Método original do Nenkai!

        public string HeaderToTraceMode()
        {
            return "Route;PacketId;DateReceived;LoadOrProc;Paused;CarId;CarOnPaddock;CurrentLap;GT7_ToDs;GT7_ToDm;CarInGear;CarHandBraked;CarPosition;CarSpeed;CarsAtPreRace;PositionAtPreRace;TotalLaps;BestLap;LastLap";
        }

        public string LineToTraceMode(string sourceRoute)
        {

            // return "Route;PacketId;DateReceived;LoadOrProc;Paused;CarId;CarOnPaddock;CurrentLap;GT7_ToDs;GT7_ToDm;CarInGear;CarHandBraked;CarPosition;CarSpeed;CarsAtPreRace;PositionAtPreRace;TotalLaps;BestLap;LastLap";

            StringBuilder linha = new StringBuilder();

            linha.Append($"{sourceRoute};");
            linha.Append($"{PacketId};");
            linha.Append($"{DateReceived:dd/MM/yyyy HH:mm:ss.fff};");

            linha.Append($"{Flags.HasFlag(SimulatorFlags.Processing)};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.Paused)};");

            linha.Append($"{CarID};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.CarOnPaddock)};");
            linha.Append($"{CurrentLap};");
            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss};");
            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss\\.fff};");

            linha.Append($"{Flags.HasFlag(SimulatorFlags.InGear)};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.HandBrakeActive)};");
            linha.Append($"{Position:F6};");
            linha.Append($"{Math.Round(MetersPerSecond * 3.6, 3)};");
            linha.Append($"{NumCarsAtPreRace};");
            linha.Append($"{PreRaceStartPosition};");
            linha.Append($"{TotalRaceLaps};");
            linha.Append($"{BestLapTime:mm\\:ss\\.fff};");
            linha.Append($"{LastLapTime:mm\\:ss\\.fff}");

            return linha.ToString();
        }

        public string HeaderToDebugStints()
        {
            return "StintId;TeamId;DriverId;TrackId;CarId;LapsRun;LapsGPS;DateStart;DateEnd;BestLap;LastLap;FuelLeft;FuelCapacity;CarsAtPreRace;PositionAtPreRace;TotalLaps;HasTurbo;CarMaxSpeed;RPMFlashing;RPMLimiter;TireRadius_FL;TireRadius_FR;TireRadius_RL;TireRadius_RR;GearMaxSpeed;GearRatios";
        }

     // public string LineToDebugStints está na classe Stint (arquivo Stint.cs)

        public string HeaderToDebugLaps()
        {
            return "StintId;LapNumber;LapTime;LapDistance;FuelUsed;MaxThrottle;MaxBrake;CarCoast;TireSpin;TireHeat;TrialBrake;EarlyGas";
        }

     // public string LineToDebugLaps está na classe Lap (arquivo Stint.cs) 

        public string HeaderToDebugDetails()
        {
            return "StintId;GT7_ToDs;GT7_ToDm;PacketId;PacketCm;PacketMs;LapNumber;SectorId;LapDistance;LapTime;BrakeRaw;ThrottleRaw;Brake;Throttle;Clutch;ClutchEng;InGear;Gear;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;Position;posX;posY;posZ;ROtNorth;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ;RideHeight;BodyHeight;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR;OilTemp;WaterTemp;RPM;RPMClutch;TurboBoost;FuelLeft;OilPressure;HandBrake;TCS;ASM";
        }

        public string LineToDebugDetails(string stintId, string sectorId, double lapDistance, double lapTime)
        {

            // return 80 "StintId;GT7_ToDs;GT7_ToDm;PacketId;PacketCm;PacketMs;LapNumber;SectorId;LapDistance;LapTime;BrakeRaw;ThrottleRaw;Brake;Throttle;Clutch;ClutchEng;InGear;Gear;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;Position;posX;posY;posZ;ROtNorth;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ;RideHeight;BodyHeight;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR;OilTemp;WaterTemp;RPM;RPMClutch;TurboBoost;FuelLeft;OilPressure;HandBrake;TCS;ASM"

            // params 04 "StintId;SectorId;LapDistance;LapTime"
            // domath 17 "PacketCm;PacketMs;Brake;Throttle;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw"
            // holder 04 "TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR"
            // format 22 "GT7_ToDs;GT7_ToDm;Position;posX;posY;posZ;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ"
            // direct 33 "PacketId;LapNumber;BrakeRaw;ThrottleRaw;Clutch;ClutchEng;InGear;Gear;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;ROtNorth;RideHeight;BodyHeight;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;OilTemp;WaterTemp;RPM;RPMClutch;TurboBoost;FuelLeft;OilPressure;HandBrake;TCS;ASM"


            StringBuilder linha = new StringBuilder();

            linha.Append($"{stintId};");
            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss};");
            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss\\.fff};");
            linha.Append($"{PacketId};");
            linha.Append($"{Math.Round((MetersPerSecond / 60) * 100, 3)};"); //Cálculo do PacketCm!
            linha.Append($"{Math.Round(16.666667, 3)};");                    //Cálculo do PacketMs!
            linha.Append($"{CurrentLap};");
            linha.Append($"{sectorId};");
            linha.Append($"{lapDistance};");
            linha.Append($"{lapTime};");
            linha.Append($"{BrakeRAW};");
            linha.Append($"{ThrottleRAW};");

            linha.Append($"{Math.Round((BrakeRAW / 255.0) * 100.0, 3)};");      //Cálculo do Brake!
            linha.Append($"{Math.Round((ThrottleRAW / 255.0) * 100.0, 3)};");   //Cálculo do Throttle!

            linha.Append($"{Clutch};");
            linha.Append($"{ClutchEngagement};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.InGear)};");
            linha.Append($"{GearCurrent};");
            linha.Append($"{Math.Round(MetersPerSecond * 3.6, 3)};");        //Cálculo do KPH_Car!

            // Calcular as velocidades das rodas!
            linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3)};");

            linha.Append($"{WheelRevsPerSec_FL * -1};");
            linha.Append($"{WheelRevsPerSec_FR * -1};");
            linha.Append($"{WheelRevsPerSec_RL * -1};");
            linha.Append($"{WheelRevsPerSec_RR * -1};");

            // Calcular o Spin de cada roda!
            if (Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            // Calcular o Lock de cada roda!
            if (Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            linha.Append($"{Position:F6};");
            linha.Append($"{Position.X:F6};");
            linha.Append($"{Position.Y:F6};");
            linha.Append($"{Position.Z:F6};");
            linha.Append($"{ROtNorth};");

            linha.Append($"{RoadPlane:F6};");
            linha.Append($"{RoadPlane.X:F6};");
            linha.Append($"{RoadPlane.Y:F6};");
            linha.Append($"{RoadPlane.Z:F6};");

            linha.Append($"{Velocity:F6};");
            linha.Append($"{Velocity.X:F6};");
            linha.Append($"{Velocity.Y:F6};");
            linha.Append($"{Velocity.Z:F6};");

            linha.Append($"{AngularVelocity:F6};");
            linha.Append($"{AngularVelocity.X:F6};");
            linha.Append($"{AngularVelocity.Y:F6};");
            linha.Append($"{AngularVelocity.Z:F6};");

            linha.Append($"{Rotation:F6};");
            linha.Append($"{Rotation.X:F6};");
            linha.Append($"{Rotation.Y:F6};");
            linha.Append($"{Rotation.Z:F6};");

            linha.Append($"{RoadPlaneDistance:F6};");
            linha.Append($"{RideHeight};");
            linha.Append($"{SuspensionHeight_FL};");
            linha.Append($"{SuspensionHeight_FR};");
            linha.Append($"{SuspensionHeight_RL};");
            linha.Append($"{SuspensionHeight_RR};");

            linha.Append($"{TireTemp_FL};");
            linha.Append($"{TireTemp_FR};");
            linha.Append($"{TireTemp_RL};");
            linha.Append($"{TireTemp_RR};");

            linha.Append("100;"); // Calcular TireWear_FL
            linha.Append("100;"); // Calcular TireWear_FR
            linha.Append("100;"); // Calcular TireWear_RL
            linha.Append("100;"); // Calcular TireWear_RR

            linha.Append($"{OilTemp};");
            linha.Append($"{WaterTemp};");
            linha.Append($"{EngineRPM};");
            linha.Append($"{ClutchRPMAfter};");
            linha.Append($"{((TurboBoost - 1.0) * 100.0):F3};");
            linha.Append($"{FuelLevel:F3};");
            linha.Append($"{OilPressure:F3};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.HandBrakeActive)};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.TCSActive)};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.ASMActive)}");

            return linha.ToString();
        }

        public string HeaderToGTDBFileV0()
        {
            return "GT7_ToDs;GT7_ToDm;PacketId;PacketCm;PacketMs;LapNumber;SectorId;LapDistance;LapTime;BrakeRaw;ThrottleRaw;Brake;Throttle;Clutch;ClutchEng;InGear;Gear;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;Position;posX;posY;posZ;ROtNorth;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ;RideHeight;BodyHeight;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR;OilTemp;WaterTemp;RPM;RPMClutch;TurboBoost;FuelLeft;OilPressure;HandBrake;TCS;ASM";
        }

        public string HeaderToGTDBFile()
        {
            return "PktId;Pkt_DoRs;Pkt_DoRm;GT7_ToDs;GT7_ToDm;PktCmters;PktMillis;LapNumber;SecNumber;LapMeters;LapMillis;ThrottleRaw;BrakeRaw;Throttle;Brake;Clutch;ClutchEng;InGear;Gear;HandBrake;OilTemp;WaterTemp;RPM;RPMClutch;TurboBoost;FuelLeft;OilPressure;TCS;ASM;KPH_Car;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;Position;posX;posY;posZ;RoadPlane;rplX;rplY;rplZ;rplD;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;RideHeight;Rotation;rotX;rotY;rotZ;ROtNorth;TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;mGLat;mGLon;mGVer;mVelX;mVelY;mVelZ;mGPSx;mGPSz";
        }

        public string LineToGTDBFileV0(string sectorId, double lapDistance, double lapTime)
        {

            // return 79 "GT7_ToDs;GT7_ToDm;PacketId;PacketCm;PacketMs;LapNumber;SectorId;LapDistance;LapTime;BrakeRaw;ThrottleRaw;Brake;Throttle;Clutch;ClutchEng;InGear;Gear;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;Position;posX;posY;posZ;ROtNorth;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ;RideHeight;BodyHeight;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR;OilTemp;WaterTemp;RPM;RPMClutch;TurboBoost;FuelLeft;OilPressure;HandBrake;TCS;ASM"

            // params 03 "SectorId;LapDistance;LapTime"
            // format 22 "GT7_ToDs;GT7_ToDm;Position;posX;posY;posZ;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ"
            // direct 33 "PacketId;LapNumber;BrakeRaw;ThrottleRaw;Clutch;ClutchEng;InGear;Gear;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;ROtNorth;RideHeight;BodyHeight;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;OilTemp;WaterTemp;RPM;RPMClutch;TurboBoost;FuelLeft;OilPressure;HandBrake;TCS;ASM"
            // domath 17 "PacketCm;PacketMs;Brake;Throttle;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw"
            // holder 04 "TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR"

            StringBuilder linha = new StringBuilder();

            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss};");
            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss\\.fff};");
            linha.Append($"{PacketId};");
            linha.Append($"{Math.Round((MetersPerSecond / 60) * 100, 3)};");    //Cálculo do PacketCm!
            linha.Append($"{Math.Round(16.666667, 3)};");                       //Cálculo do PacketMs!
            linha.Append($"{CurrentLap};");
            linha.Append($"{sectorId};");
            linha.Append($"{lapDistance};");
            linha.Append($"{lapTime};");
            linha.Append($"{BrakeRAW};");
            linha.Append($"{ThrottleRAW};");

            linha.Append($"{Math.Round((BrakeRAW / 255.0) * 100.0, 3)};");      //Cálculo do Brake!
            linha.Append($"{Math.Round((ThrottleRAW / 255.0) * 100.0, 3)};");   //Cálculo do Throttle!

            linha.Append($"{Clutch};");
            linha.Append($"{ClutchEngagement};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.InGear)};");
            linha.Append($"{GearCurrent};");
            linha.Append($"{Math.Round(MetersPerSecond * 3.6, 3)};");           //Cálculo do KPH_Car!

            // Calcular as velocidades das rodas!
            linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3)};");

            linha.Append($"{WheelRevsPerSec_FL * -1};");
            linha.Append($"{WheelRevsPerSec_FR * -1};");
            linha.Append($"{WheelRevsPerSec_RL * -1};");
            linha.Append($"{WheelRevsPerSec_RR * -1};");

            // Calcular o Spin de cada roda!
            if (Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            // Calcular o Lock de cada roda!
            if (Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            linha.Append($"{Position:F6};");
            linha.Append($"{Position.X:F6};");
            linha.Append($"{Position.Y:F6};");
            linha.Append($"{Position.Z:F6};");
            linha.Append($"{ROtNorth};");

            linha.Append($"{RoadPlane:F6};");
            linha.Append($"{RoadPlane.X:F6};");
            linha.Append($"{RoadPlane.Y:F6};");
            linha.Append($"{RoadPlane.Z:F6};");

            linha.Append($"{Velocity:F6};");
            linha.Append($"{Velocity.X:F6};");
            linha.Append($"{Velocity.Y:F6};");
            linha.Append($"{Velocity.Z:F6};");

            linha.Append($"{AngularVelocity:F6};");
            linha.Append($"{AngularVelocity.X:F6};");
            linha.Append($"{AngularVelocity.Y:F6};");
            linha.Append($"{AngularVelocity.Z:F6};");

            linha.Append($"{Rotation:F6};");
            linha.Append($"{Rotation.X:F6};");
            linha.Append($"{Rotation.Y:F6};");
            linha.Append($"{Rotation.Z:F6};");

            linha.Append($"{RoadPlaneDistance:F6};");
            linha.Append($"{RideHeight};");
            linha.Append($"{SuspensionHeight_FL};");
            linha.Append($"{SuspensionHeight_FR};");
            linha.Append($"{SuspensionHeight_RL};");
            linha.Append($"{SuspensionHeight_RR};");

            linha.Append($"{TireTemp_FL};");
            linha.Append($"{TireTemp_FR};");
            linha.Append($"{TireTemp_RL};");
            linha.Append($"{TireTemp_RR};");

            linha.Append("100;"); // Calcular TireWear_FL
            linha.Append("100;"); // Calcular TireWear_FR
            linha.Append("100;"); // Calcular TireWear_RL
            linha.Append("100;"); // Calcular TireWear_RR

            linha.Append($"{OilTemp};");
            linha.Append($"{WaterTemp};");
            linha.Append($"{EngineRPM};");
            linha.Append($"{ClutchRPMAfter};");
            linha.Append($"{((TurboBoost - 1.0) * 100.0):F3};");
            linha.Append($"{FuelLevel:F3};");
            linha.Append($"{OilPressure:F3};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.HandBrakeActive)};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.TCSActive)};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.ASMActive)}");

            return linha.ToString();
        }

        public string LineToGTDBFile(int secNumber, double lapMeters, double lapMillis, SimulatorPacket previousPacket)
        {

            // return 89 "PktId;Pkt_DoRs;Pkt_DoRm;GT7_ToDs;GT7_ToDm;PktCmters;PktMillis;LapNumber;SecNumber;LapMeters;LapMillis;ThrottleRaw;BrakeRaw;Throttle;Brake;Clutch;ClutchEng;InGear;Gear;HandBrake;OilTemp;WaterTemp;RPM;RPMClutch;TurboBoost;FuelLeft;OilPressure;TCS;ASM;KPH_Car;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;Position;posX;posY;posZ;RoadPlane;rplX;rplY;rplZ;rplD;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;RideHeight;Rotation;rotX;rotY;rotZ;ROtNorth;TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;mGLat;mGLon;mGVer;mVelX;mVelY;mVelZ;mGPSx;mGPSz";

            // params 03 "SecNumber;LapMeters;LapMillis"
            // holder 04 "TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR"

            // direct 20 "PktId;LapNumber;ThrottleRAW;BrakeRAW;Clutch;ClutchEng;InGear;Gear;HandBrake;OilTemp;WaterTemp;RPM;RPMClutch;HandBrake;OilTemp;WaterTemp;RPM;RPMClutch;TCS;ASM;"
            // format 37 "Pkt_DoRs;Pkt_DoRm;GT7_ToDs;GT7_ToDm;TurboBoost;FuelLeft;OilPressure;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;Position;posX;posY;posZ;RoadPlane;rplX;rplY;rplZ;rplD;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ;ROtNorth;"
            // domath 25 "PktCmters;PktMillis;Brake;Throttle;KPH_Car;SuspHeight_FL;SuspHeight_FR;SuspHeight_RL;SuspHeight_RR;RideHeight;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;mGLat;mGLon;mGVer;mVelX;mVelY;mVelZ;mGPSx;mGPSz"

            StringBuilder linha = new StringBuilder();

            linha.Append($"{PacketId};");

            linha.Append($"{DateReceived:yyyy-MM-dd HH:mm:ss};");
            linha.Append($"{DateReceived:yyyy-MM-dd HH:mm:ss.fff};");

            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss};");
            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss\\.fff};");

            linha.Append($"{Math.Round((MetersPerSecond / 60) * 100, 3)};");    //Cálculo do PktCmters!
            // ToDo: Calcular os Millis a partir do GPS! 
            linha.Append($"{Math.Round(16.6666667, 3)};");                      //Cálculo do PktMillis! 

            linha.Append($"{CurrentLap};");
            linha.Append($"{secNumber};");
            linha.Append($"{lapMeters:F3};");
            linha.Append($"{lapMillis:F3};");

            linha.Append($"{ThrottleRAW};");
            linha.Append($"{BrakeRAW};");

            linha.Append($"{Math.Round((ThrottleRAW / 255.0) * 100.0, 3)};");   //Cálculo do Throttle!
            linha.Append($"{Math.Round((BrakeRAW / 255.0) * 100.0, 3)};");      //Cálculo do Brake!

            linha.Append($"{Clutch};");
            linha.Append($"{ClutchEngagement};");

            linha.Append($"{Flags.HasFlag(SimulatorFlags.InGear)};");
            linha.Append($"{GearCurrent};");

            linha.Append($"{Flags.HasFlag(SimulatorFlags.HandBrakeActive)};");
            linha.Append($"{OilTemp};");
            linha.Append($"{WaterTemp};");
            linha.Append($"{EngineRPM};");
            linha.Append($"{ClutchRPMAfter};");
            linha.Append($"{((TurboBoost - 1.0) * 100.0):F3};");
            linha.Append($"{FuelLevel:F3};");
            linha.Append($"{OilPressure:F3};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.TCSActive)};");
            linha.Append($"{Flags.HasFlag(SimulatorFlags.ASMActive)};");

            linha.Append($"{Math.Round(MetersPerSecond * 3.6, 3)};");           //Cálculo do KPH_Car!

            linha.Append($"{WheelRevsPerSec_FL * -1:F7};");
            linha.Append($"{WheelRevsPerSec_FR * -1:F7};");
            linha.Append($"{WheelRevsPerSec_RL * -1:F7};");
            linha.Append($"{WheelRevsPerSec_RR * -1:F7};");

            linha.Append($"{SuspensionHeight_FL * 100:F7};");
            linha.Append($"{SuspensionHeight_FR * 100:F7};");
            linha.Append($"{SuspensionHeight_RL * 100:F7};");
            linha.Append($"{SuspensionHeight_RR * 100:F7};");

            linha.Append($"{TireTemp_FL:F3};");
            linha.Append($"{TireTemp_FR:F3};");
            linha.Append($"{TireTemp_RL:F3};");
            linha.Append($"{TireTemp_RR:F3};");

            string tmpX = null;
            string tmpY = null;
            string tmpZ = null;

            tmpX = $"{Position.X:F7}".Replace(",", ".");
            tmpY = $"{Position.Y:F7}".Replace(",", ".");
            tmpZ = $"{Position.Z:F7}".Replace(",", ".");

            string tmpPosition = $"<{tmpX}, {tmpY}, {tmpZ}>";
            linha.Append($"{tmpPosition};");
            linha.Append($"{Position.X:F7};");
            linha.Append($"{Position.Y:F7};");
            linha.Append($"{Position.Z:F7};");

            tmpX = $"{RoadPlane.X:F7}".Replace(",", ".");
            tmpY = $"{RoadPlane.Y:F7}".Replace(",", ".");
            tmpZ = $"{RoadPlane.Z:F7}".Replace(",", ".");

            string tmpRoadPlane = $"<{tmpX}, {tmpY}, {tmpZ}>";
            linha.Append($"{tmpRoadPlane};");
            linha.Append($"{RoadPlane.X:F7};");
            linha.Append($"{RoadPlane.Y:F7};");
            linha.Append($"{RoadPlane.Z:F7};");

            linha.Append($"{RoadPlaneDistance:F7};");

            tmpX = $"{Velocity.X:F7}".Replace(",", ".");
            tmpY = $"{Velocity.Y:F7}".Replace(",", ".");
            tmpZ = $"{Velocity.Z:F7}".Replace(",", ".");

            string tmpVelocity = $"<{tmpX}, {tmpY}, {tmpZ}>";
            linha.Append($"{tmpVelocity};");
            linha.Append($"{Velocity.X:F7};");
            linha.Append($"{Velocity.Y:F7};");
            linha.Append($"{Velocity.Z:F7};");

            tmpX = $"{AngularVelocity.X:F7}".Replace(",", ".");
            tmpY = $"{AngularVelocity.Y:F7}".Replace(",", ".");
            tmpZ = $"{AngularVelocity.Z:F7}".Replace(",", ".");

            string tmpAngularVelocity = $"<{tmpX}, {tmpY}, {tmpZ}>";
            linha.Append($"{tmpAngularVelocity};");
            linha.Append($"{AngularVelocity.X:F7};");
            linha.Append($"{AngularVelocity.Y:F7};");
            linha.Append($"{AngularVelocity.Z:F7};");

            linha.Append($"{RideHeight * 100:F7};");

            tmpX = $"{Rotation.X:F7}".Replace(",", ".");
            tmpY = $"{Rotation.Y:F7}".Replace(",", ".");
            tmpZ = $"{Rotation.Z:F7}".Replace(",", ".");

            string tmpRotation = $"<{tmpX}, {tmpY}, {tmpZ}>";
            linha.Append($"{tmpRotation};");
            linha.Append($"{Rotation.X:F7};");
            linha.Append($"{Rotation.Y:F7};");
            linha.Append($"{Rotation.Z:F7};");

            linha.Append($"{ROtNorth};");

            linha.Append("100;"); // Calcular TireWear_FL
            linha.Append("100;"); // Calcular TireWear_FR
            linha.Append("100;"); // Calcular TireWear_RL
            linha.Append("100;"); // Calcular TireWear_RR

            // Calcular as velocidades das rodas!
            linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3)};");

            // Calcular o Spin de cada roda!
            if (Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            // Calcular o Lock de cada roda!
            if (Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (previousPacket == null)
            {
                // O _previousPacket ainda não foi carregado
                linha.Append($"0;0;0;0;0;0;0;0");
            }
            else
            {
                // Calcular as métricas para o MoTeC i2

                Vector3 deltaV = (Velocity - previousPacket.Velocity) * Rotation;

                double gLat = deltaV.X * 60.0 / 9.8;  // X freq = 60.0
                double gLon = deltaV.Z * 60.0 / 9.8;  // Z freq = 60.0
                double gVer = deltaV.Y * 60.0 / 9.8;  // Y freq = 60.0

                linha.Append($"{gLat:F7};");
                linha.Append($"{-gLon:F7};");
                linha.Append($"{gVer:F7};");

                linha.Append($"{deltaV.X:F7};");
                linha.Append($"{deltaV.Y:F7};");
                linha.Append($"{-deltaV.Z:F7};");

                var convertedCoords = GPSConvert(x: Position.X, z: -Position.Z);

                linha.Append($"{convertedCoords.Item1:F7};");
                linha.Append($"{convertedCoords.Item2:F7}");

            }

            return linha.ToString();
        }

        public string HeaderToZ1File()
        {
            return "GT7_ToDs;GT7_ToDm;PacketId;PacketCm;PacketMs;DriverId;DateStart;TrackName;TimeStart;CarName;BestLap;LastLap;ThisLap;LapNumber;SectorId;LapDistance;LapTime;BrakeRaw;ThrottleRaw;Brake;Throttle;Clutch;Gear;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;Position;posX;posY;posZ;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ;RideHeight;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR;OilTemp;WaterTemp;RPM;FuelLeft;OilPressure";
        }

        public string LineToZ1File(string driverId, DateTime dateStart, string trackName, string carName, string sectorId, double lapDistance, double lapTime) // ***FINALIZAR*** //
        {

            // return 74 "GT7_ToDs;GT7_ToDm;PacketId;PacketCm;PacketMs;DriverId;DateStart;TrackName;TimeStart;CarName;BestLap;LastLap;ThisLap;LapNumber;SectorId;LapDistance;LapTime;BrakeRaw;ThrottleRaw;Brake;Throttle;Clutch;Gear;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw;Position;posX;posY;posZ;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ;RideHeight;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR;OilTemp;WaterTemp;RPM;FuelLeft;OilPressure"

            // params 09 "DriverId;DateStart;TrackName;TimeStart;CarName;ThisLap;SectorId;LapDistance;LapTime"
            // format 24 "GT7_ToDs;GT7_ToDm;BestLap;LastLap;Position;posX;posY;posZ;RoadPlane;rplX;rplY;rplZ;Velocity;velX;velY;velZ;AngVelocity;angX;angY;angZ;Rotation;rotX;rotY;rotZ"
            // direct 20 "PacketId;LapNumber;BrakeRaw;ThrottleRaw;Clutch;Gear;RPS_FLw;RPS_FRw;RPS_RLw;RPS_RRw;RideHeight;TireTemp_FL;TireTemp_FR;TireTemp_RL;TireTemp_RR;OilTemp;WaterTemp;RPM;FuelLeft;OilPressure"
            // domath 17 "PacketCm;PacketMs;Brake;Throttle;KPH_Car;KPH_FLw;KPH_FRw;KPH_RLw;KPH_RRw;Spin_FLw;Spin_FRw;Spin_RLw;Spin_RRw;Lock_FLw;Lock_FRw;Lock_RLw;Lock_RRw"
            // holder 04 "TireWear_FL;TireWear_FR;TireWear_RL;TireWear_RR"

            StringBuilder linha = new StringBuilder();

            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss};");
            linha.Append($"{TimeOfDayProgression:hh\\:mm\\:ss\\.fff};");

            linha.Append($"{PacketId};");
            linha.Append($"{Math.Round((MetersPerSecond / 60) * 100, 3)};"); //Cálculo do PacketCm!
            linha.Append($"{Math.Round(16.666667, 3)};");                    //Cálculo do PacketMs!

            linha.Append($"{driverId};");
            // DateStart
            linha.Append($"{dateStart:dd/MM/yyyy};");
            linha.Append($"{trackName};");
            linha.Append($"{dateStart:hh\\:mm\\:ss};");
            linha.Append($"{carName};");

            linha.Append($"{BestLapTime:mm\\:ss\\.fff};");
            linha.Append($"{LastLapTime:mm\\:ss\\.fff};");
            linha.Append($"#ThisLapPH#;");                                   // ***SUBSTITUIR*** //

            linha.Append($"{CurrentLap};");
            linha.Append($"{sectorId};");
            linha.Append($"{lapDistance};");
            linha.Append($"{lapTime};");

            linha.Append($"{BrakeRAW};");
            linha.Append($"{ThrottleRAW};");
            linha.Append($"{Math.Round((BrakeRAW / 255.0) * 100.0, 3)};");      //Cálculo do Brake!
            linha.Append($"{Math.Round((ThrottleRAW / 255.0) * 100.0, 3)};");   //Cálculo do Throttle!
            linha.Append($"{Clutch};");
            linha.Append($"{GearCurrent};");

            linha.Append($"{Math.Round(MetersPerSecond * 3.6, 3)};");        //Cálculo do KPH_Car!

            // Calcular as velocidades das rodas!
            linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3)};");
            linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3)};");

            linha.Append($"{WheelRevsPerSec_FL * -1};");
            linha.Append($"{WheelRevsPerSec_FR * -1};");
            linha.Append($"{WheelRevsPerSec_RL * -1};");
            linha.Append($"{WheelRevsPerSec_RR * -1};");

            // Calcular o Spin de cada roda!
            if (Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) > Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            // Calcular o Lock de cada roda!
            if (Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FL * WheelRevsPerSec_FL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_FR * WheelRevsPerSec_FR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RL * WheelRevsPerSec_RL * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            if (Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) < Math.Round(MetersPerSecond * 3.6, 3))
                linha.Append($"{Math.Round((TireRadius_RR * WheelRevsPerSec_RR * -1) * 3.6, 3) - Math.Round(MetersPerSecond * 3.6, 3)};");
            else
                linha.Append($"{0};");

            linha.Append($"{Position:F6};");
            linha.Append($"{Position.X:F6};");
            linha.Append($"{Position.Y:F6};");
            linha.Append($"{Position.Z:F6};");

            linha.Append($"{RoadPlane:F6};");
            linha.Append($"{RoadPlane.X:F6};");
            linha.Append($"{RoadPlane.Y:F6};");
            linha.Append($"{RoadPlane.Z:F6};");

            linha.Append($"{Velocity:F6};");
            linha.Append($"{Velocity.X:F6};");
            linha.Append($"{Velocity.Y:F6};");
            linha.Append($"{Velocity.Z:F6};");

            linha.Append($"{AngularVelocity:F6};");
            linha.Append($"{AngularVelocity.X:F6};");
            linha.Append($"{AngularVelocity.Y:F6};");
            linha.Append($"{AngularVelocity.Z:F6};");

            linha.Append($"{Rotation:F6};");
            linha.Append($"{Rotation.X:F6};");
            linha.Append($"{Rotation.Y:F6};");
            linha.Append($"{Rotation.Z:F6};");

            linha.Append($"{RoadPlaneDistance:F6};");

            linha.Append($"{TireTemp_FL};");
            linha.Append($"{TireTemp_FR};");
            linha.Append($"{TireTemp_RL};");
            linha.Append($"{TireTemp_RR};");

            linha.Append("100;"); // Calcular TireWear_FL
            linha.Append("100;"); // Calcular TireWear_FR
            linha.Append("100;"); // Calcular TireWear_RL
            linha.Append("100;"); // Calcular TireWear_RR

            linha.Append($"{OilTemp};");
            linha.Append($"{WaterTemp};");
            linha.Append($"{EngineRPM};");
            linha.Append($"{FuelLevel:F3};");
            linha.Append($"{OilPressure:F3}");

            return linha.ToString();
        }

        public static (double, double) GPSConvert(double? x = null, double? z = null, double latmid = 52.83067304956695, double longmid = -1.3740268265085214)
        {

            double latmid_rad = Math.PI * latmid / 180.0;
            double longmid_rad = Math.PI * longmid / 180.0;

            double m_per_deg_lat = 111132.954 - (559.822 * Math.Cos(2 * latmid_rad)) + (1.175 * Math.Cos(4 * latmid_rad)) - (0.0023 * Math.Cos(6 * latmid_rad));
            double m_per_deg_lon = (111412.84 * Math.Cos(latmid_rad)) - (93.5 * Math.Cos(3 * latmid_rad)) + (0.118 * Math.Cos(5 * latmid_rad));

            // z is lat, x is long
            double dlat = z.GetValueOrDefault() / m_per_deg_lat;
            double dlong = x.GetValueOrDefault() / m_per_deg_lon;

            double lat = latmid + dlat;
            double lon = longmid + dlong;

            return (lat, lon);
        }

    }

}

/// <summary>
/// Flags/States of the simulation.
/// </summary>
[Flags]
    public enum SimulatorFlags : short
    {
        None = 0,

        /// <summary>
        /// Track or car is currently being loaded onto the track.
        /// </summary>
        Processing = 1 << 2,

        /// <summary>
        /// The game's simulation is paused. 
        /// Note: The simulation will not be paused while in the pause menu in online modes.
        /// </summary>
        Paused = 1 << 1,

        /// <summary>
        /// The car is on the paddock or track, with data available.
        /// </summary>
        CarOnPaddock = 1 << 0,

        /// <summary>
        /// Hand Brake is active.
        /// </summary>
        HandBrakeActive = 1 << 6,

        /// <summary>
        /// Needs more investigation
        /// </summary>
        InGear = 1 << 3,

        /// <summary>
        /// Current car has a Turbo.
        /// </summary>
        HasTurbo = 1 << 4,

        /// <summary>
        /// Rev Limiting is active.
        /// </summary>
        RevLimiterActive = 1 << 5,

        /// <summary>
        /// Lights are active.
        /// </summary>
        LightsActive = 1 << 7,

        /// <summary>
        /// High Beams are turned on.
        /// </summary>
        HighBeamActive = 1 << 8,

        /// <summary>
        /// Low Beams are turned on.
        /// </summary>
        LowBeamActive = 1 << 9,

        /// <summary>
        /// ASM is active.
        /// </summary>
        ASMActive = 1 << 10,

        /// <summary>
        /// Traction Control is active.
        /// </summary>
        TCSActive = 1 << 11,
    }
