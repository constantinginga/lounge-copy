using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScSoMe.ApiDtos
{
    public class MemberInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset UpdateDate { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Avatar { get; set; }
        public string Alias { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsApproved { get; set; }

        // public string RawPasswordValue { get; set; } = string.Empty;
    }

    public class MinimalMemberInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool isFreeUser { get; set; }
    }
}
