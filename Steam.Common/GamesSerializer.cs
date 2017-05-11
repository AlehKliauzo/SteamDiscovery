using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Common
{
    public class GamesSerializer
    {
        const string Path = "gameInfo.json";

        public static List<Game> Load()
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path);
                return JsonConvert.DeserializeObject<List<Game>>(json);
            }

            return new List<Game>();
        }

        public static void Save(List<Game> games)
        {
            var json = JsonConvert.SerializeObject(games, Formatting.Indented);
            File.WriteAllText(Path, json);
        }
    }
}
