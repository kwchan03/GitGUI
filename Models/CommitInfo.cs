namespace GitGUI.Models
{
    public class CommitInfo
    {
        public string Sha { get; set; }
        public string Message { get; set; }
        public string AuthorName { get; set; }
        public DateTime Date { get; set; }
    }
}
