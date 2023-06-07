using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsDistributionStudentsPages
{
    public class IndexDistributionStudents
    {
        public List<GroupsModel> Groups_list { get; set; }
        public List<ListDistribution> list_groupsPractice { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_DistributionStudents FilterViewModel { get; set; }
    }
    public class ListDistribution:GroupsModel
    {
        public int count_students { get; set; }
        public Specialization specialization { get; set; }
        public List<Practice> list_practices { get; set; }
        public List<Student> list_students { get; set; }
    }
}
