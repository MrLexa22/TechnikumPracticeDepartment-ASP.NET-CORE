using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsOrganizationPages
{
    public class IndexResumeStudents
    {
        public List<Students> list_resume { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_ResumeStudents FilterViewModel { get; set; }
    }
    public class FilterViewModel_ResumeStudents
    {
        public FilterViewModel_ResumeStudents(int? typeSortList, int? course, int? specializationId, string? filterTags, string search)
        {
            TypeSortList = typeSortList;
            FilterTags = filterTags;
            Search = search;
            Course = course;
            SpecializationId = specializationId;
        }
        public int? SpecializationId { get; private set; }
        public int? Course { get; private set; }
        public int? TypeSortList { get; private set; }
        public string? FilterTags { get; private set; }
        public string Search { get; private set; }
    }
}
