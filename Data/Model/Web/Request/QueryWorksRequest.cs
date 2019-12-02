﻿using Refit;

namespace Pzxlane.Data.Model.Web.Request
{
    public class QueryWorksRequest
    {
        [AliasAs("page")]
        public int Offset { get; set; }

        [AliasAs("q")]
        public string Tag { get; set; }

        [AliasAs("image_sizes")]
        public string ImageSizes { get; set; } = "px_128x128,px_480mw,large";

        [AliasAs("per_page")]
        public int PerPage { get; set; } = 60;

        [AliasAs("period")]
        public string Period { get; set; } = "all";

        [AliasAs("order")]
        public string Order { get; set; } = "desc";

        [AliasAs("sort")]
        public string Sort { get; set; } = "date";

        [AliasAs("mode")]
        public string Mode { get; set; } = "exact_tag";
    }
}