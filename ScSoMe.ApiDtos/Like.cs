using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScSoMe.ApiDtos
{
    public class Like
    {
        [System.Text.Json.Serialization.JsonPropertyName("M")]
        public int MemberId { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("L")]
        public int LikeType { get; set; }
    }
}
