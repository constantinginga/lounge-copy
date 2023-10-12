namespace ScSoMe.API.Controllers.DirectMessaging
{
    public class DcHistory
    {
        public DcHistory(string groupName, string displayName, string newdisplayname)
        {
            this.groupName = groupName;
            this.displayName = displayName;
            this.newDisplayName = newdisplayname;
        }

        public string groupName { get; set; }
        public string displayName { get; set; }
        public string newDisplayName { get; set; }
    }
}
