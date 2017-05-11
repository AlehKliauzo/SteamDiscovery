using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Steam.Common;

namespace Steam.Analyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var games = GamesSerializer.Load();

            games = games.Where(x => x.ReleaseDate.Year >= 2010 && x.AllPositiveReviews + x.AllNegativeReviews >= 100).ToList();
            games = RemoveGamesWithTags(games, "Visual Novel", "Horror", "Early Access", "Pixel Graphics", "3D Platformer", "Puzzle", 
                                        "Hidden Object", "Strategy", "Isometric", "Zombies", "VR", "Point &amp; Click", "Fighting",
                                        "JRPG", "Metroidvania", "Casual", "Sports", "Anime", "Text-Based", "Bullet Hell", "Shoot 'Em Up",
                                        "Top-Down", "Top-Down Shooter", "Platformer", "Puzzle-Platformer", "RPGMaker", "Turn-Based Combat",
                                        "FMV", "Flight", "Rogue-lite", "Rogue-like");
            var confidence = WilsonScore.pnormaldist(0.95);

            foreach (var game in games)
            {
                game.AllTotalReviews = game.AllPositiveReviews + game.AllNegativeReviews;
                game.WilsonScore = WilsonScore.Score(game.AllPositiveReviews, game.AllTotalReviews, confidence);
            }

            var mostReviewed = games.OrderByDescending(x => x.AllTotalReviews).ToList();
            var topRated = games.OrderByDescending(x => x.WilsonScore).ToList();

            //for (var i = 0; i < 10; i++)
            //{
            //    var game = mostReviewed[i];
            //    var output = $"{game.Name} - {game.WilsonScore:F3} ({game.AllTotalReviews} reviews, {game.AllPositiveReviews} positive)";
            //    Console.WriteLine(output);
            //    Console.WriteLine(string.Join(", ", game.Tags));
            //    Console.WriteLine();
            //}

            //Console.WriteLine("10 top rated games by wilson score:");
            for (var i = 0; i < 15; i++)
            {
                var game = topRated[i];
                var output = $"{game.Name} - {game.WilsonScore:F3} ({game.AllTotalReviews} reviews, {game.AllPositiveReviews} positive)";
                Console.WriteLine(output);
                Console.WriteLine("Release date - " + game.ReleaseDate.ToString("yyyy.MM.dd"));
                Console.WriteLine(string.Join(", ", game.Tags));
                Console.WriteLine();
            }

            Console.WriteLine("Done");
            Console.Read();
        }

        private static List<Game> RemoveGamesWithTags(List<Game> games, params string[] tags)
        {
            var filteredList = new List<Game>(games);

            foreach (var tag in tags)
            {
                filteredList = RemoveGamesWithTag(filteredList, tag);
            }

            return filteredList;
        }

        private static List<Game> RemoveGamesWithTag(List<Game> games, string tag)
        {
            var filteredList = new List<Game>();

            foreach (var game in games)
            {
                if (game.Tags.Any(x => x.Equals(tag, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                filteredList.Add(game);
            }

            return filteredList;
        }
    }
}
