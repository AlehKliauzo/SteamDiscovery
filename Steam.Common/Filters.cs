using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Common
{
    public class Filters
    {
        public bool IsNameContainsFilterEnabled { get; set; }
        public string NameContains { get; set; }

        public bool IsNameDoesntContainFilterEnabled { get; set; }
        public string NameDoesntContain { get; set; }

        public bool IsReleasedAfterFilterEnabled { get; set; }
        public string ReleasedAfter { get; set; }

        public bool IsMoreThanXReviewsFilterEnabled { get; set; }
        public string MoreThanXReviews { get; set; }

        public bool IsMoreThanXTagsFilterEnabled { get; set; }
        public string MoreThanXTags { get; set; }

        public bool IsGameScoreHigherThanXFilterEnabled { get; set; }
        public string GameScoreHigherThanX { get; set; }

        public bool IsHasTagsFilterEnabled { get; set; }
        public string HasTags { get; set; }

        public bool IsDoesntHaveTagsFilterEnabled { get; set; }
        public string DoesntHaveTags { get; set; }

        public string TagsPriority { get; set; }
    }
}
