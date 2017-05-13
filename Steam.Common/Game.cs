using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Steam.Common
{
    public class Game
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Tags { get; set; }

        public int SteamReviews { get; set; }
        public int SteamPositiveReviewPercent { get; set; }

        public int AllPositiveReviews { get; set; }
        public int AllNegativeReviews { get; set; }
        public int AllTotalReviews { get; set; }
        public double WilsonScore { get; set; }

        public DateTime InfoDownloaded { get; set; }

        [JsonIgnore]
        public double PreferenceScore { get; set; }

        [JsonIgnore]
        public double TotalScore { get; set; }

        public Game()
        {
            Tags = new List<string>();
        }

        public override string ToString()
        {
            var tags = string.Join(", ", Tags);
            var gameInfo = $"Id: {Id}\r\nName: {Name}\r\nRelease date: {ReleaseDate}\r\nTags: {tags}\r\n";
            var steamReviews = $"Steam reviews: {SteamReviews} ({SteamPositiveReviewPercent}% positive)\r\n";
            var allreviews = $"All reviews: {AllPositiveReviews} positive, {AllNegativeReviews} negative";
            return gameInfo + steamReviews + allreviews;
        }
    }
}
