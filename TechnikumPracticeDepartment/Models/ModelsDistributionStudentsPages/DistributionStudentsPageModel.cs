using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsDistributionStudentsPages
{
    public class DistributionStudentsPageModel
    {
        public int ID_Group { get; set; }
        public string GroupName { get; set; }
        public List<Student> list_students { get; set; }
        public List<Organization> list_organization { get; set; }
        public List<Practice_DistributionStudent> list_practice { get; set; }
    }
    public class Practice_DistributionStudent
    {
        public int ID_Practice { get; set; }
        public string NamePractice { get; set; }
        public bool IsEnded { get; set; }
        public string NameProfModule { get; set; }
        public List<PracticesChartDate> list_dates { get; set; }
        public List<PracticeChartDistibution> list_distributesStudents { get; set; }
    }
}
