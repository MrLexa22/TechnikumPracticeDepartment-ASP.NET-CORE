using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsStudentsPages
{
    public class IndexStudentsModel
    {
        public List<Students> list_students { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_Students FilterViewModel { get; set; }
    }
    public class Students : GroupsModel 
    {
        public Student student { get; set; }
        public int age { get; set; }
}
}
