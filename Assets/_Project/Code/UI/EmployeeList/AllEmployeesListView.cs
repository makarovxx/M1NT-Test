using _Project.Code.Models;

namespace _Project.Code.UI.EmployeeList
{
    public sealed class AllEmployeesListView : EmployeeListViewBase
    {
        protected override bool ShouldDisplay(EmployeeData employee)
        {
            return true;
        }
    }
}
