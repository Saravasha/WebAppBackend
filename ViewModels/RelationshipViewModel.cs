namespace WebAppBackend.ViewModels
{
    public class RelationshipViewModel
    {

        public string? ParentLabel { get; set; }
        public string? ParentTitle { get; set; }


        public string? ChildLabel { get; set; }

        public List<string>? Children { get; set; } = new();

        public string? GrandchildLabel { get; set; }
        public List<string?> Grandchildren { get; set; } = new();
    }
}
