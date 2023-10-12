namespace ScSoMe.ApiDtos
{
    public class ScGroup
    {
        public int GroupId { get; set; }

        public string? GroupName { get; set; }

        public string? Url { get; set; }

        public DateTimeOffset CreatedDt { get; set; }

    }
}