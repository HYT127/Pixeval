﻿using Refit;

namespace Pzxlane.Data.Model.Web.Request
{
    public class UploadsRequest
    {
        [AliasAs("page")]
        public int Page { get; set; }

        [AliasAs("publicity")]
        public string Publicity { get; set; } = "public";

        [AliasAs("per_page")]
        public int PerPage { get; set; } = 60;
    }
}