using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UDPInterface.Models;

public class Car
{
    public int MakerId { get; set; }
    public string? MakerName { get; set; }
    public int CarId { get; set; }
    public string? CarName { get; set; }
    public string? MakerAndCar { get; set; }

    public Car()
    {
    }

    public static List<Car> ObtemDadosLocal()
    {
        var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\cars.dat");

        return JsonSerializer.Deserialize<List<Car>>(json) ?? new List<Car>();
    }
}