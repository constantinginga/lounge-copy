using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScSoMe.ApiDtos
{
    public class SubscribersJson
    {
        [System.Text.Json.Serialization.JsonPropertyName("M")]
        public int MemberId { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("R")]

        public bool IsRead { get; set; }

    }
}
