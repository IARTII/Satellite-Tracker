using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using static SatelliteTracker.MainWindow;

namespace SatelliteTracker
{
    internal class TLEFile
    {
        private static readonly Random _random = new Random();

        public static ObservableCollection<Satellite> Read(string destination)
        {
            ObservableCollection<Satellite> list = new ObservableCollection<Satellite>();
            string[] tleList = File.ReadAllLines(destination);

            if (tleList.Length % 4 == 0)
            {
                for (int i = 0; i < tleList.Length; i += 4)
                {
                    string[] arr = tleList[i + 1]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length >= 3 &&
                        byte.TryParse(arr[0], out byte r) &&
                        byte.TryParse(arr[1], out byte g) &&
                        byte.TryParse(arr[2], out byte b))
                    {
                        Color selectedColor = Color.FromRgb(r, g, b);

                        var newSatellite = new Satellite
                        {
                            Name = tleList[i],
                            TrackColor = new SolidColorBrush(selectedColor)
                        };

                        list.Add(newSatellite);
                    }
                }

                return list;
            }

            MessageBox.Show("Ошибка чтения сохраненных TLE пакетов!");
            return null;
        }

        public static (string Line1, string Line2, Color Color)[] ReadTle(string destination)
        {
            string[] tleList = File.ReadAllLines(destination);

            // 1 спутник = 4 строки
            if (tleList.Length % 4 != 0)
            {
                MessageBox.Show("Ошибка чтения TLE!");
                return null;
            }

            int satelliteCount = tleList.Length / 4;

            var result = new (string Line1, string Line2, Color Color)[satelliteCount];

            for (int i = 0, satIndex = 0; i < tleList.Length; i += 4, satIndex++)
            {
                string[] colorArr = tleList[i + 1]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (colorArr.Length >= 3 &&
                    byte.TryParse(colorArr[0], out byte r) &&
                    byte.TryParse(colorArr[1], out byte g) &&
                    byte.TryParse(colorArr[2], out byte b))
                {
                    Color color = Color.FromRgb(r, g, b);

                    result[satIndex] = (
                        tleList[i + 2], // TLE line 1
                        tleList[i + 3], // TLE line 2
                        color
                    );
                }
            }

            return result;
        }

        public static void DeleteTLEfromFile(string destenation, string TLEname)
        {
            List<string> lines = File.ReadAllLines(destenation).ToList();

            for(int i  = 0; i < lines.Count; i++)
            {
                if (lines[i] == TLEname)
                {
                    for(int j = i; j < i + 4; j++)
                    {
                        lines.RemoveAt(i);
                    }
                }
            }
            File.WriteAllLines(destenation, lines.ToArray());
        }

        public static void ChangeTLEinFile(string destenation, string TLEname, TLESett newTLE)
        {
            List<string> lines = File.ReadAllLines(destenation).ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if(lines[i] == TLEname)
                {
                    lines[i] = newTLE.TLEname;
                    lines[i + 1] = newTLE.TLEcolor;
                    lines[i + 2] = newTLE.TLE1;
                    lines[i + 3] = newTLE.TLE2;
                } 
            }
            File.WriteAllText(destenation, String.Empty);
            File.WriteAllLines(destenation, lines);
        }
    }
}
