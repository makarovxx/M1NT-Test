using _Project.Code.Models;

namespace _Project.Code.UI.EmployeeList
{
    public sealed class FavoriteEmployeesListView : EmployeeListViewBase
    {
        protected override bool RebuildWhenFavoritesChanged => true;
        protected override bool RebuildWhenShow => true;

        protected override bool ShouldDisplay(EmployeeData employee)
        {
            return Favorites.IsFavorite(employee.id);
        }
    }
}
