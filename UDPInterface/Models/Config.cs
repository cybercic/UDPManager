﻿using System;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace UDPInterface.Models;

public class Config
{
    private string playStationIP;
    private bool uDPActive;
    private string? mensagemManager;

    public string DriverId { get; set; }
    public string TeamId { get; set; }
    public bool SendFiles { get; set; }
    public int ID { get; set; }
    
    public string? MensagemManager
    {
        get => mensagemManager;
        set
        {
            mensagemManager = value;
            GravaDadosLocal();
        }
    }
    public string PlayStationIP
    {
        get => playStationIP;
        set
        {
            playStationIP = value;
            GravaDadosLocal();
        }
    }
    public bool UDPActive
    {
        get => uDPActive;
        set
        {
            uDPActive = value;
            GravaDadosLocal();
        }
    }

    public static Config ObtemDadosLocal()
    {
        var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\config.dat");

        return JsonSerializer.Deserialize<Config>(json);
    }

    public void GravaDadosLocal()
    {
        try
        {
            File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\config.dat", ToString());
        }
        catch (Exception ex)
        {
            Thread.Sleep(1000);
            File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\config.dat", ToString());
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}