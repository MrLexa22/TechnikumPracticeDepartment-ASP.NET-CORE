using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsOrganizationPages
{
    public class DistributionStudentWithPractices
    {
        public List<ConcatinationStudentWithPractice> list { get; set; }
    }
    public class ConcatinationStudentWithPractice
    {
        public Student student { get; set; }
        public Vacancy? vacancy { get; set; }
        public Students? students { get; set; }
        public int typeResponse { get; set; }
        public int fromDeletet { get; set; }
        public ResponseFromStudent? response { get; set; }
        public ResponseFromOrganization? response2 { get; set; }
        public List<StudentsInformationPractice> list_practice { get; set; }
    }
    public class StudentsInformationPractice
    {
        public int ID_Practice { get; set; }
        public string NameProfModule { get; set; }
        public string NamePractice { get; set; }
        public string Hours { get; set; }
        public bool IsEnded { get; set; }
        public List<PracticeChart> list_periods { get; set; }
    }
}
