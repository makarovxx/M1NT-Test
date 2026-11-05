using _Project.Code.Composition;
using _Project.Code.Models;
using _Project.Code.UI.EmployeeList;
using _Project.Code.UI.Navigation;
using _Project.Code.UI.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.UI
{
    public sealed class EmployeeContactsAppController : MonoBehaviour
    {
        [SerializeField] private EmployeeListView employeeDataView;
        [SerializeField] private EmployeeListView favoritesView;
        [SerializeField] private ProfileView profileView;
        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private Button employeesButton;
        [SerializeField] private Button favoritesButton;

        private CompositionRoot _appServices;
        private bool _isInitialized;

        private void OnEnable()
        {
            _appServices = CompositionRoot.Instance;
            if (_appServices != null)
            {
                _appServices.OnInitializationCompleted += Initialize;

                if (_appServices.IsInitializationCompleted)
                {
                    Initialize();
                }
            }
        }

        private void OnDisable()
        {
            if (_appServices != null)
            {
                _appServices.OnInitializationCompleted -= Initialize;
            }

            DisposeViews();
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            employeeDataView.Initialize(
                _appServices.Employees,
                _appServices.FavoritesService,
                _appServices.AvatarService,
                ContactListMode.All);
            favoritesView.Initialize(
                _appServices.Employees,
                _appServices.FavoritesService,
                _appServices.AvatarService,
                ContactListMode.Favorites);
            profileView.Initialize(_appServices.FavoritesService);

            employeeDataView.OnProfileRequested += OpenProfile;
            favoritesView.OnProfileRequested += OpenProfile;
            navigator.ProfileOpened += ShowProfile;
            navigator.ListScreenChanged += ShowList;
            profileView.OnBackRequested += navigator.CloseProfile;
            employeesButton.onClick.AddListener(navigator.ShowEmployees);
            favoritesButton.onClick.AddListener(navigator.ShowFavorites);

            _isInitialized = true;
            navigator.ShowEmployees();
        }

        private void DisposeViews()
        {
            if (!_isInitialized)
            {
                return;
            }

            employeeDataView.OnProfileRequested -= OpenProfile;
            favoritesView.OnProfileRequested -= OpenProfile;
            navigator.ProfileOpened -= ShowProfile;
            navigator.ListScreenChanged -= ShowList;
            profileView.OnBackRequested -= navigator.CloseProfile;
            employeesButton.onClick.RemoveListener(navigator.ShowEmployees);
            favoritesButton.onClick.RemoveListener(navigator.ShowFavorites);

            employeeDataView.Dispose();
            favoritesView.Dispose();
            profileView.Dispose();
            _isInitialized = false;
        }

        private void OpenProfile(EmployeeData employee)
        {
            navigator.OpenProfile(employee);
        }

        private void ShowProfile(EmployeeData employee)
        {
            profileView.Show(employee, _appServices.AvatarService.GetSprite(employee.id));
        }

        private void ShowList(bool showFavorites)
        {
            if (showFavorites)
            {
                favoritesView.Show();
                return;
            }

            employeeDataView.Show();
        }
    }
}
