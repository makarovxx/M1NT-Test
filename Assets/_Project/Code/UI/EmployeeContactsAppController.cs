using EmployeeContacts.Composition;
using EmployeeContacts.Models;
using EmployeeContacts.UI.Common;
using EmployeeContacts.UI.EmployeeList;
using EmployeeContacts.UI.Navigation;
using EmployeeContacts.UI.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace EmployeeContacts.UI
{
    public sealed class EmployeeContactsAppController : MonoBehaviour
    {
        [SerializeField] private EmployeeListView employeeDataView;
        [SerializeField] private EmployeeListView favoritesView;
        [SerializeField] private ProfileView profileView;
        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private Button employeesButton;
        [SerializeField] private Button favoritesButton;
        private CompositionRoot root;
        private AvatarSpriteProvider avatars;
        private EmployeeListController employees;
        private EmployeeListController favorites;
        private EmployeeData profileEmployee;
        private bool initialized;

        private void OnEnable()
        {
            root = CompositionRoot.Instance;
            if (root != null) root.InitializationCompleted += Initialize;
        }

        private void OnDisable()
        {
            if (root != null)
            {
                root.InitializationCompleted -= Initialize;
                if (initialized)
                {
                    root.FavoritesService.FavoritesChanged -= RefreshProfile;
                    navigator.ProfileOpened -= ShowProfile;
                    navigator.ListScreenChanged -= OnListChanged;
                    profileView.FavoriteRequested -= ToggleFavorite;
                    profileView.BackRequested -= navigator.CloseProfile;
                    employeesButton.onClick.RemoveListener(navigator.ShowEmployees);
                    favoritesButton.onClick.RemoveListener(navigator.ShowFavorites);
                }
            }

            employees?.Dispose();
            favorites?.Dispose();
            avatars?.Dispose();
            employees = null;
            favorites = null;
            avatars = null;
            initialized = false;
        }

        private void Start()
        {
            if (root != null && root.IsInitialized) Initialize(true, null);
        }

        private void Initialize(bool success, string message)
        {
            if (!success || initialized) return;
            avatars = new AvatarSpriteProvider(root.AvatarService);
            employees = new EmployeeListController(root, employeeDataView, avatars, false, navigator.OpenProfile);
            favorites = new EmployeeListController(root, favoritesView, avatars, true, navigator.OpenProfile);
            navigator.ProfileOpened += ShowProfile;
            navigator.ListScreenChanged += OnListChanged;
            profileView.FavoriteRequested += ToggleFavorite;
            profileView.BackRequested += navigator.CloseProfile;
            employeesButton.onClick.AddListener(navigator.ShowEmployees);
            favoritesButton.onClick.AddListener(navigator.ShowFavorites);
            root.FavoritesService.FavoritesChanged += RefreshProfile;
            initialized = true;
            navigator.ShowEmployees();
        }

        private void OnListChanged(bool showFavorites)
        {
            if (showFavorites) favorites.Show();
            else employees.Show();
        }

        private void ShowProfile(EmployeeData employee)
        {
            profileEmployee = employee;
            profileView.Show(employee, avatars.GetSprite(employee.id), root.FavoritesService.IsFavorite(employee.id));
        }

        private void ToggleFavorite(EmployeeData employee)
        {
            root.FavoritesService.Toggle(employee.id);
        }

        private void RefreshProfile()
        {
            if (profileEmployee != null) ShowProfile(profileEmployee);
        }
    }
}