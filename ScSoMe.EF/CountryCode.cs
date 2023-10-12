using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class CountryCode
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? DialCode { get; set; }
        public string? ShortName { get; set; }
        public string Flag { get; set; } = null!;
    }
}
