using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UDPInterface.Models;

public class Track
{
    public int TrackId { get; set; }
    public string? TrackName { get; set; }
    public string? CourseName { get; set; }
    public int TrackLength { get; set; }
    public float LongestStraight { get; set; }
    public float ElevationDiff { get; set; }
    public int IsReverse { get; set; }
    public float PitLaneDelta { get; set; }
    public int IsOval { get; set; }
    public int NumCorners { get; set; }
    public float P1X { get; set; }
    public float P1Y { get; set; }
    public float P2X { get; set; }
    public float P2Y { get; set; }
    public string? DIR { get; set; }
    public float MINX { get; set; }
    public float MINY { get; set; }
    public float MAXX { get; set; }
    public float MAXY { get; set; }

    public Track()
    {
    }

    public static List<Track> ObtemDadosLocal()
    {
        var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\tracks.dat");

        return JsonSerializer.Deserialize<List<Track>>(json) ?? new List<Track>();
    }
}