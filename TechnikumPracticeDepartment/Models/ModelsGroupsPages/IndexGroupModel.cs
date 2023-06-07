using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsGroupsPages
{
    public class IndexGroupModel
    {
        public List<GroupsModel> list_groups { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_Group FilterViewModel { get; set; }
    }
    public class GroupsModel
    {
        public int ID_Group { get; set; }
        public string NameGroups { get; set; }
        public int Course { get; set; }
        public string SpecializationName { get; set; }
        public int Specialization_ID { get; set; }
        public short YearStartEducation { get; set; }
        public short YearOfGraduation { get; set; }
        public bool? IsEnded { get; set; }
    }
}
