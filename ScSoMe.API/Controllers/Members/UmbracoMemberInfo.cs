namespace ScSoMe.API.Controllers.Members
{
    [System.Runtime.Serialization.DataContract(Name = "MemberInfo")]
    public class UmbracoMemberInfo
    {
        [System.Runtime.Serialization.DataMember]
        public int Id { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Name { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Url { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Login { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Email { get; set; }
        [System.Runtime.Serialization.DataMember]
        public DateTime CreateDate { get; set; }
        [System.Runtime.Serialization.DataMember]
        public DateTime UpdateDate { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string RawPasswordValue { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string ContentType { get; set; }
        [System.Runtime.Serialization.DataMember]
        public bool IsApproved { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Avatar { get; set; }
        [System.Runtime.Serialization.DataMember]
        public bool IsAdmin { get; set; }
    }
}
