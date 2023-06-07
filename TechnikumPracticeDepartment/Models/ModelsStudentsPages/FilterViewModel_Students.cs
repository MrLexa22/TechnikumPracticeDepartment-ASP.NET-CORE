using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechnikumPracticeDepartment.Models.ModelsStudentsPages
{
    public class FilterViewModel_Students
    {
        public FilterViewModel_Students(int? typeSortListStudents, int? filterListStudentsByGroup, int? filterListStudentsByCourse, int? filterListStudentsByAvaliable, string search)
        {
            TypeSortListStudents = typeSortListStudents;
            FilterListStudentsByGroup = filterListStudentsByGroup;
            FilterListStudentsByCourse = filterListStudentsByCourse;
            FilterListStudentsByAvaliable = filterListStudentsByAvaliable;
            Search = search;
        }
        public int? TypeSortListStudents { get; private set; }
        public int? FilterListStudentsByGroup { get; private set; }
        public int? FilterListStudentsByCourse { get; private set; }
        public int? FilterListStudentsByAvaliable { get; private set; }
        public string Search { get; private set; }    
    }
}
