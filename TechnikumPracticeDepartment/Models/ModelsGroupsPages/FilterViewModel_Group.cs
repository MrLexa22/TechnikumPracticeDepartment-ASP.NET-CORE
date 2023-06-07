namespace TechnikumPracticeDepartment.Models.ModelsGroupsPages
{
    public class FilterViewModel_Group
    {
        public FilterViewModel_Group(int? typeSortListGroups, int? filterSpecialization, int? filterCourse, int? filterAvaliable, string search)
        {
            TypeSortListGroups = typeSortListGroups;
            FilterSpecialization = filterSpecialization;
            FilterCourse = filterCourse;
            FilterAvaliable = filterAvaliable;
            Search = search;
        }
        public int? TypeSortListGroups { get; private set; }
        public int? FilterSpecialization { get; private set; }
        public int? FilterCourse { get; private set; }
        public int? FilterAvaliable { get; private set; }
        public string Search { get; private set; }
    }
}
