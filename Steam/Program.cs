using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Steam.Common;
using Formatting = Newtonsoft.Json.Formatting;

namespace Steam
{
    class Program
    {
        private static WebClientWithDelay webClient = new WebClientWithDelay();

        static void Main(string[] args)
        {
            var games = GamesSerializer.Load();

            //for (var i = 1; i <= 1; i++)
            //{
            //    GetInfoForAllGamesOnTheSearchPage(i, games);
            //    GamesSerializer.Save(games);
            //}

            //ConsistencyCheck(games);

            //CalculateScores(games);
            //FixHtmlEncodedCharacters(games);
            RemoveSpecialSymbolsInNames(games);
            GamesSerializer.Save(games);

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void RemoveSpecialSymbolsInNames(List<Game> games)
        {
            foreach (var game in games)
            {
                var name = game.Name;

                name = name.Replace("\u2122", "");
                name = name.Replace("\u0099", "");
                name = name.Replace("\u00ae", "");

                game.Name = name;
            }
        }

        private static void FixHtmlEncodedCharacters(List<Game> games)
        {
            var htmlEncodedAmpersand = "&amp;";
            var usualAmpersand = "&";

            foreach (var game in games)
            {
                if (game.Name.Contains(htmlEncodedAmpersand))
                {
                    game.Name = game.Name.Replace(htmlEncodedAmpersand, usualAmpersand);
                }

                var tagsWithAmpersands = game.Tags.Where(x => x.Contains(htmlEncodedAmpersand)).ToList();

                foreach (var tag in tagsWithAmpersands)
                {
                    game.Tags.Remove(tag);
                    var fixedTag = tag.Replace(htmlEncodedAmpersand, usualAmpersand);
                    game.Tags.Add(fixedTag);
                }
            }
        }

        private static void CalculateScores(List<Game> games)
        {
            var confidence = WilsonScore.pnormaldist(0.95);

            foreach (var game in games)
            {
                game.AllTotalReviews = game.AllPositiveReviews + game.AllNegativeReviews;
                game.WilsonScore = WilsonScore.Score(game.AllPositiveReviews, game.AllTotalReviews, confidence) * 100;
            }
        }

        private static void ConsistencyCheck(List<Game> games)
        {
            var defaultReleaseDate = default(DateTime);
            Dictionary<string, int> duplicatesById = new Dictionary<string, int>();
            List<string> gamesWithNoReleaseDate = new List<string>();

            Console.WriteLine();
            Console.WriteLine("Consistency check:");

            foreach (var game in games)
            {
                var idCount = games.Count(x => x.Id == game.Id);

                if (game.ReleaseDate == defaultReleaseDate)
                {
                    gamesWithNoReleaseDate.Add(game.Id);
                }

                if (idCount != 1 && !duplicatesById.ContainsKey(game.Id))
                {
                    duplicatesById.Add(game.Id, idCount);
                }
            }

            if (duplicatesById.Count == 0 && gamesWithNoReleaseDate.Count == 0)
            {
                Console.WriteLine("Data seems to be ok");
                Console.WriteLine();
                return;
            }

            foreach (var error in duplicatesById)
            {
                var output = $"Game with id {error.Key} occurs {error.Value} times";
                Console.WriteLine(output);
            }

            foreach (var game in gamesWithNoReleaseDate)
            {
                var output = $"Game with id {game} has no release date";
                Console.WriteLine(output);
            }

            Console.WriteLine();
        }

        private static void GetInfoForAllGamesOnTheSearchPage(int page, List<Game> gamesInfo)
        {
            Console.WriteLine("Requesting search page " + page);
            Console.WriteLine();
            var discoveryUrl = GetDiscoveryUrl(page);
            var discoveryHtml = webClient.DownloadString(discoveryUrl);

            var xml = ConvertSearchPageToXmlWithResults(discoveryHtml);
            var gamesIds = GetGamesIds(xml);

            foreach (var id in gamesIds)
            {
                //ignore packages and bundles
                if (id.Contains(","))
                {
                    continue;
                }

                var idExists = gamesInfo.Any(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));

                if (idExists)
                {
                    Console.WriteLine("Info for game with id " + id + " already downloaded. skipping");
                    continue;
                }

                var appUrl = GetAppUrl(id);

                Console.WriteLine("Requesting game page " + id);
                var appHtml = webClient.DownloadString(appUrl);
                Game game = null;

                try
                {
                    game = GetGameInfo(appHtml);
                    game.Id = id;
                    gamesInfo.Add(game);
                    Console.WriteLine(game);
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error when retrieving info for appId " + id);
                    Console.WriteLine(ex);

                    var html = "error" + id + ".html";
                    File.WriteAllText(html, appHtml);

                    var stacktrace = "error" + id + ".txt";
                    File.WriteAllText(stacktrace, ex.Message + "\r\n" + ex.StackTrace);

                    Console.WriteLine("html saved to " + html);
                    Console.WriteLine("exception saved to " + stacktrace);
                    Console.WriteLine();
                }
            }
        }

        private static string RemoveAllOccurencesOfTextBetween(string xml, string start, string end)
        {
            while (xml.Contains(start))
            {
                var startIndex = xml.IndexOf(start, StringComparison.InvariantCultureIgnoreCase);
                var endIndex = xml.IndexOf(end, startIndex + start.Length, StringComparison.InvariantCultureIgnoreCase);
                xml = xml.Remove(startIndex, endIndex - startIndex + end.Length);
            }

            return xml;
        }

        private static string RemoveTextBetween(string xml, string start, string end)
        {
            var startIndex = xml.IndexOf(start, StringComparison.InvariantCultureIgnoreCase);
            var endIndex = xml.IndexOf(end, startIndex + start.Length, StringComparison.InvariantCultureIgnoreCase);
            xml = xml.Remove(startIndex, endIndex - startIndex + end.Length);

            return xml;
        }

        private static string Remove(string xml, string substring)
        {
            while (xml.Contains(substring))
            {
                xml = xml.Replace(substring, "");
            }

            return xml;
        }

        private static string GetDiscoveryUrl(int page)
        {
            const string discoveryUrl = "http://store.steampowered.com/search/?sort_by=Released_DESC&category1=998&page=";
            return discoveryUrl + page.ToString();
        }

        private static string GetAppUrl(string id)
        {
            const string appUrl = "http://store.steampowered.com/app/";
            return appUrl + id;
        }

        private static XmlDocument ConvertSearchPageToXmlWithResults(string discoveryHtml)
        {
            //turn part of html page with search results into valid xml by removing everything that causes issues with parsing
            //we only care about presence of review icon and appId
            var xmlStartIndex = discoveryHtml.IndexOf("<!-- End Extra empty div -->", StringComparison.InvariantCultureIgnoreCase);
            var xmlEndIndex = discoveryHtml.IndexOf("<!-- End List Items -->", StringComparison.InvariantCultureIgnoreCase);
            var xml = discoveryHtml.Substring(xmlStartIndex, xmlEndIndex - xmlStartIndex);
            xml = xml.Insert(0, "<div>");
            xml = xml.Insert(xml.Length, "</div>");

            xml = RemoveAllOccurencesOfTextBetween(xml, "onmouseover=\"", "\"");
            xml = RemoveAllOccurencesOfTextBetween(xml, "onmouseout=\"", "\"");
            xml = RemoveAllOccurencesOfTextBetween(xml, "<div class=\"col search_capsule\">", "</div>");
            xml = Remove(xml, "<br>");
            xml = Remove(xml, "<!-- End Extra empty div -->");
            xml = xml.Replace(" & ", " &amp; ");
            xml = xml.Replace("&trade;", "");
            xml = xml.Replace("&reg;", "");

            var xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = false;
            xmlDocument.LoadXml(xml);
            return xmlDocument;
        }

        private static List<string> GetGamesIds(XmlDocument xmlDocument)
        {
            var gamesIds = new List<string>();

            var rootNode = xmlDocument.FirstChild;
            var children = rootNode.ChildNodes;

            foreach (XmlNode child in children)
            {
                //var name = child.SelectSingleNode(".//span[@class='title']");
                var review = child.SelectSingleNode(".//div[@class='col search_reviewscore responsive_secondrow']");

                if (string.IsNullOrWhiteSpace(review.InnerXml))
                {
                    continue;
                }

                var appId = child.Attributes["data-ds-appid"].InnerText;

                //several games have wrong ids in data-ds-appid attribute
                if (appId == "33220")
                {
                    appId = "33229";
                }

                if (appId == "40960")
                {
                    appId = "40950";
                }

                gamesIds.Add(appId);
            }

            return gamesIds;
        }

        private static Game GetGameInfo(string gameHtml)
        {
            var nameStartTag = "<div class=\"apphub_AppName\">";
            var nameEndTag = "</div>";
            var releaseStartTag = "Release Date: <span class=\"date\">";
            var releaseEndTag = "</span>";
            var tagsStartTag = "<div class=\"glance_tags_label\">Popular user-defined tags for this product:</div>";
            var tagsEndTag = "</div>";
            //var idStartTag = "data-appid=\"";
            //var idEndTag = "\">";
            var tagStartTag = "class=\"app_tag\" style=\"display: none;\">";
            var tagEndTag = "</a>";
            var steamReviewsStartAreaTag = "<div class=\"user_reviews_summary_row\" data-store-tooltip=\"";
            var steamReviewsEndAreaTag = "are positive";
            //var steamReviewsStartTag = "<span class=\"game_review_summary positive\" data-store-tooltip=\"";
            //var steamReviewsEndTag = "for this game";
            var positiveReviewsStartTag = "<label for=\"review_type_positive\">Positive&nbsp;<span class=\"user_reviews_count\">(";
            var positiveReviewsEndTag = ")</span>";
            var negativeReviewsStartTag = "<label for=\"review_type_negative\">Negative&nbsp;<span class=\"user_reviews_count\">(";
            var negativeReviewsEndTag = ")</span>";

            var gameInfo = new Game();

            var name = GetTextBetween(gameHtml, nameStartTag, nameEndTag);

            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Unexpected html for game");
            }

            gameInfo.Name = name;

            var releaseDate = GetTextBetween(gameHtml, releaseStartTag, releaseEndTag);

            if (DateTime.TryParse(releaseDate, out DateTime releasedOn))
            {
                gameInfo.ReleaseDate = releasedOn;
            }
            else
            {
                //several games don't have release date
                gameInfo.ReleaseDate = GetGameReleaseDate(name);
            }

            var tagsHtml = GetTextBetween(gameHtml, tagsStartTag, tagsEndTag);
            //gameInfo.Id = GetTextBetween(tagsHtml, idStartTag, idEndTag);
            var tags = new List<string>();
            while (tagsHtml.Contains("</a>"))
            {
                var tag = GetTextBetween(tagsHtml, tagStartTag, tagEndTag);
                tagsHtml = RemoveTextBetween(tagsHtml, tagStartTag, tagEndTag);
                tag = tag.Trim();
                tags.Add(tag);
            }
            gameInfo.Tags = tags;

            var steamReviews = GetTextBetween(gameHtml, steamReviewsStartAreaTag, steamReviewsEndAreaTag);
            //var steamReviews = GetTextBetween(steamReviewsArea, steamReviewsStartTag, steamReviewsEndTag);

            if (!string.IsNullOrEmpty(steamReviews))
            {
                var percentIndex = steamReviews.IndexOf("%", StringComparison.InvariantCultureIgnoreCase);
                var positivePercent = steamReviews.Substring(0, percentIndex);
                var reviewsCount = GetTextBetween(steamReviews, "the", "user");

                gameInfo.SteamPositiveReviewPercent = int.Parse(positivePercent, NumberStyles.Any);
                gameInfo.SteamReviews = int.Parse(reviewsCount, NumberStyles.Any);
            }

            var positiveReviews = GetTextBetween(gameHtml, positiveReviewsStartTag, positiveReviewsEndTag);
            var negativeReviews = GetTextBetween(gameHtml, negativeReviewsStartTag, negativeReviewsEndTag);
            gameInfo.AllPositiveReviews = int.Parse(positiveReviews, NumberStyles.Any);
            gameInfo.AllNegativeReviews = int.Parse(negativeReviews, NumberStyles.Any);
            gameInfo.InfoDownloaded = DateTime.Now;

            return gameInfo;
        }

        private static string GetTextBetween(string text, string start, string end)
        {
            var startIndex = text.IndexOf(start, StringComparison.InvariantCultureIgnoreCase);

            if (startIndex == -1)
            {
                return string.Empty;
            }

            var endIndex = text.IndexOf(end, startIndex + start.Length, StringComparison.InvariantCultureIgnoreCase);

            if (endIndex == -1 || startIndex > endIndex)
            {
                return string.Empty;
            }

            return text.Substring(startIndex + start.Length, endIndex - startIndex - start.Length);
        }

        private static DateTime GetGameReleaseDate(string name)
        {
            var releaseDates = new Dictionary<string, DateTime>
            {
                { "RAGE", new DateTime(2011, 10, 04)},
                { "Sam &amp; Max 301: The Penal Zone", new DateTime(2010, 04, 03) },
                { "SpellForce 2 - Anniversary Edition", new DateTime(2017, 04, 13) },
            };

            if (releaseDates.ContainsKey(name))
            {
                return releaseDates[name];
            }

            return default(DateTime);
        }
    }
}
