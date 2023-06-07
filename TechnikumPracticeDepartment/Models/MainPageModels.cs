namespace TechnikumPracticeDepartment.Models
{
    public class NewsModel
    {
        public string Date { get; set; }
        public string title { get; set; }
        public string hrefs { get; set; }
    }

    public class MainPageModel
    {
        public List<PracticesStudentWithDistribution>? list_practices { get; set; }
        public List<NewsModel> list_newsUniversity { get; set; }
    }
}
