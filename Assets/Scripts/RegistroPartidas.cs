using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public struct Partida
{
    public string fecha;
    public int puntos;
    public int segundos;
    public bool meta;
}

public static class RegistroPartidas
{
    const string ClaveRegistro = "registro";
    const string ClaveTotal = "partidas";
    public const int Maximo = 10;

    public static int Total
    {
        get { return PlayerPrefs.GetInt(ClaveTotal, 0); }
    }

    public static void Guardar(int puntos, float duracion, bool meta)
    {
        List<Partida> lista = Obtener();
        Partida nueva = new Partida();
        nueva.fecha = DateTime.Now.ToString("dd/MM HH:mm", CultureInfo.InvariantCulture);
        nueva.puntos = puntos;
        nueva.segundos = Mathf.RoundToInt(duracion);
        nueva.meta = meta;
        lista.Insert(0, nueva);
        if (lista.Count > Maximo) lista.RemoveRange(Maximo, lista.Count - Maximo);

        List<string> filas = new List<string>();
        foreach (Partida p in lista)
            filas.Add(p.fecha + "|" + p.puntos + "|" + p.segundos + "|" + (p.meta ? 1 : 0));

        PlayerPrefs.SetString(ClaveRegistro, string.Join(";", filas.ToArray()));
        PlayerPrefs.SetInt(ClaveTotal, Total + 1);
        PlayerPrefs.Save();
    }

    public static List<Partida> Obtener()
    {
        List<Partida> lista = new List<Partida>();
        string datos = PlayerPrefs.GetString(ClaveRegistro, "");
        if (string.IsNullOrEmpty(datos)) return lista;

        foreach (string fila in datos.Split(';'))
        {
            string[] c = fila.Split('|');
            if (c.Length != 4) continue;
            Partida p = new Partida();
            p.fecha = c[0];
            int.TryParse(c[1], out p.puntos);
            int.TryParse(c[2], out p.segundos);
            p.meta = c[3] == "1";
            lista.Add(p);
        }
        return lista;
    }

    public static void Borrar()
    {
        PlayerPrefs.DeleteKey(ClaveRegistro);
        PlayerPrefs.DeleteKey(ClaveTotal);
        PlayerPrefs.Save();
    }

    public static string FormatoTiempo(int segundos)
    {
        return (segundos / 60) + ":" + (segundos % 60).ToString("00");
    }
}
