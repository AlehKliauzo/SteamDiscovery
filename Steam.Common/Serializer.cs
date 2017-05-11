using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Common
{
    public class Serializer
    {
        const string Games = "games.json";
        const string Settings = "settings.json";

        public static List<Game> LoadGames()
        {
            if (File.Exists(Games))
            {
                var json = File.ReadAllText(Games);
                return JsonConvert.DeserializeObject<List<Game>>(json);
            }

            return new List<Game>();
        }

        public static void SaveGames(List<Game> games)
        {
            var json = JsonConvert.SerializeObject(games, Formatting.Indented);
            File.WriteAllText(Games, json);
        }

        public static Settings LoadSettings()
        {
            if (File.Exists(Settings))
            {
                var json = File.ReadAllText(Settings);
                return JsonConvert.DeserializeObject<Settings>(json);
            }

            return new Settings();
        }

        public static void SaveSettings(Settings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(Settings, json);
        }
    }
}
