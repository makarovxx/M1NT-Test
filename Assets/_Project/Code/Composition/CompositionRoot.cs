using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeContacts.Models;
using EmployeeContacts.Services;
using EmployeeContacts.Storage;
using UnityEngine;

namespace EmployeeContacts.Composition
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CompositionRoot : MonoBehaviour
    {
        [SerializeField] private Texture2D avatarPlaceholder;

        private Task _initializationTask;

        public static CompositionRoot Instance { get; private set; }
        public EmployeeRepository EmployeeRepository { get; private set; }
        public AvatarService AvatarService { get; private set; }
        public FavoritesService FavoritesService { get; private set; }
        public List<EmployeeData> Employees => EmployeeRepository?.Employees;
        public bool IsInitialized { get; private set; }
        public string InitializationMessage { get; private set; }

        public event Action<bool, string> InitializationCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateDependencies();
        }

        private async void Start()
        {
            await InitializeAsync();
        }

        public Task InitializeAsync()
        {
            if (_initializationTask == null)
            {
                _initializationTask = InitializeInternalAsync();
            }

            return _initializationTask;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void CreateDependencies()
        {
            var storage = new LocalStorageService();
            EmployeeRepository = new EmployeeRepository(storage, new EmployeeDataDownloader());
            AvatarService = new AvatarService(
                storage,
                new ImageDownloader(),
                new AvatarAssignmentRepository(storage),
                avatarPlaceholder);
            FavoritesService = new FavoritesService(new FavoritesRepository(storage));
        }

        private async Task InitializeInternalAsync()
        {
            try
            {
                var employees = await EmployeeRepository.LoadAsync();
                await AvatarService.InitializeAsync(employees);
                FavoritesService.Initialize(employees);

                IsInitialized = employees.Count > 0;
                var message = IsInitialized
                    ? null
                    : EmployeeRepository.LastError ?? "No employee data is available.";
                InitializationMessage = message;
                InitializationCompleted?.Invoke(IsInitialized, message);
            }
            catch (Exception exception)
            {
                IsInitialized = false;
                var message = $"Application initialization failed: {exception.Message}";
                InitializationMessage = message;
                Debug.LogError(message);
                InitializationCompleted?.Invoke(false, message);
            }
        }
    }
}
