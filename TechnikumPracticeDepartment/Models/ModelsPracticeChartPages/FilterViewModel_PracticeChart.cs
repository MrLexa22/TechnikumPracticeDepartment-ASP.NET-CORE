namespace TechnikumPracticeDepartment.Models.ModelsPracticeChartPages
{
    public class FilterViewModel_PracticeChart
    {
        public FilterViewModel_PracticeChart(int? typeSortList, int? filterListByGroup, int? filterListByActivate, string search)
        {
            TypeSortList = typeSortList;
            FilterListByGroup = filterListByGroup;
            FilterListByActivate = filterListByActivate;
            Search = search;
        }
        public int? TypeSortList { get; private set; }
        public int? FilterListByGroup { get; private set; }
        public int? FilterListByActivate { get; private set; }
        public string Search { get; private set; }
    }
}
