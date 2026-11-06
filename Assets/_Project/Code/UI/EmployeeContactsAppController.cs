using _Project.Code.Models;
using _Project.Code.Services;
using _Project.Code.UI.EmployeeList;
using _Project.Code.UI.Navigation;
using _Project.Code.UI.Profile;
using UnityEngine;

namespace _Project.Code.UI
{
    public sealed class EmployeeContactsAppController : MonoBehaviour
    {
        private readonly EmployeeListScreen _initialListScreen = EmployeeListScreen.AllEmployees;
        private AllEmployeesListView _employeeDataView;
        private FavoriteEmployeesListView _favoritesView;
        private ProfileView _profileView;
        private ScreenNavigator _navigator;
        private AvatarService _avatars;

        public void Initialize(AllEmployeesListView employeeDataView, FavoriteEmployeesListView favoritesView,
            ProfileView profileView, ScreenNavigator navigator, AvatarService avatars)
        {
            _employeeDataView = employeeDataView;
            _favoritesView = favoritesView;
            _profileView = profileView;
            _navigator = navigator;
            _avatars = avatars;

            _employeeDataView.OnProfileRequested += HandleProfileRequested;
            _favoritesView.OnProfileRequested += HandleProfileRequested;
            _profileView.OnBackButtonClick += HandleBackButtonClick;
            _navigator.OnListScreenChanged += HandleListScreenChanged;

            _navigator.Initialize(_initialListScreen);
        }

        private void OnDestroy()
        {
            _employeeDataView.OnProfileRequested -= HandleProfileRequested;
            _favoritesView.OnProfileRequested -= HandleProfileRequested;
            _profileView.OnBackButtonClick -= HandleBackButtonClick;
            _navigator.OnListScreenChanged -= HandleListScreenChanged;

            _employeeDataView.Dispose();
            _favoritesView.Dispose();
            _profileView.Dispose();

            _employeeDataView = null;
            _favoritesView = null;
            _profileView = null;
            _navigator = null;
            _avatars = null;
        }

        private void HandleProfileRequested(EmployeeData employee)
        {
            _profileView.Show(employee, _avatars.GetSprite(employee.id));
            _navigator.ShowProfile();
        }

        private void HandleBackButtonClick()
        {
            _navigator.CloseProfile();
        }

        private void HandleListScreenChanged(EmployeeListScreen screen)
        {
            if (screen == EmployeeListScreen.Favorites)
            {
                _favoritesView.Show();
                return;
            }

            _employeeDataView.Show();
        }
    }
}