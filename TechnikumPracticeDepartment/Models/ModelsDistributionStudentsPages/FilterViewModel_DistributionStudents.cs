namespace TechnikumPracticeDepartment.Models.ModelsDistributionStudentsPages
{
    public class FilterViewModel_DistributionStudents
    {
        public FilterViewModel_DistributionStudents(int? typeSortList, int? filterListBySpecializaion, int? filterListByCourse, int? filterListByAvaliable, string search)
        {
            TypeSortList = typeSortList;
            FilterListBySpecializaion = filterListBySpecializaion;
            FilterListByCourse = filterListByCourse;
            FilterListByAvaliable = filterListByAvaliable;
            Search = search;
        }
        public int? TypeSortList { get; private set; }
        public int? FilterListBySpecializaion { get; private set; }
        public int? FilterListByCourse { get; private set; }
        public int? FilterListByAvaliable { get; private set; }
        public string Search { get; private set; }
    }
}
