using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Code.Models;
using _Project.Code.Services;
using _Project.Code.Storage;
using _Project.Code.UI;
using _Project.Code.UI.EmployeeList;
using _Project.Code.UI.Navigation;
using _Project.Code.UI.Profile;
using UnityEngine;

namespace _Project.Code.Composition
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CompositionRoot : MonoBehaviour
    {
        [SerializeField] private EmployeeContactsAppController applicationController;
        [SerializeField] private AllEmployeesListView employeeDataView;
        [SerializeField] private FavoriteEmployeesListView favoritesView;
        [SerializeField] private ProfileView profileView;
        [SerializeField] private ScreenNavigator screenNavigator;

        private AvatarService _avatarService;
        private EmployeeDataService _employeeDataService;
        private FavoritesService _favoritesService;

        private void Awake()
        {
            ComposeApplication();
        }

        private async void Start()
        {
            await InitializeApplicationAsync();
        }

        private void OnDestroy()
        {
            _avatarService?.Dispose();
        }

        private void ComposeApplication()
        {
            var localStorage = new LocalStorage();
            var apiClient = new ApiClient();

            var favoritesStorage = new FavoritesStorage(localStorage);
            var avatarAssignmentStorage = new AvatarAssignmentStorage(localStorage);

            _employeeDataService = new EmployeeDataService(localStorage, apiClient);
            _favoritesService = new FavoritesService(favoritesStorage);
            _avatarService = new AvatarService(localStorage, apiClient, avatarAssignmentStorage);
        }

        private async Task InitializeApplicationAsync()
        {
            var employees = new List<EmployeeData>();

            try
            {
                employees = await _employeeDataService.LoadAsync();
                await _avatarService.InitializeAsync(employees);
                _favoritesService.Initialize(employees);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Application initialization failed: {exception.Message}");
                _favoritesService.Initialize(employees);
            }

            employeeDataView.Initialize(employees, _favoritesService, _avatarService);
            favoritesView.Initialize(employees, _favoritesService, _avatarService);
            profileView.Initialize(_favoritesService);

            applicationController.Initialize(employeeDataView, favoritesView, profileView, screenNavigator,
                _avatarService);
        }
    }
}