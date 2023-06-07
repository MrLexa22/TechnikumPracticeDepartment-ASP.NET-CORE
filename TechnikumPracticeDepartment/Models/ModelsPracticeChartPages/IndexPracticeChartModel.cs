using TechnikumPracticeDepartment.Models.ModelsPracticePages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsPracticeChartPages
{
    public class IndexPracticeChartModel
    {
        public List<PracticeChart> list_practiceChart { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_PracticeChart FilterViewModel { get; set; }
    }
}
