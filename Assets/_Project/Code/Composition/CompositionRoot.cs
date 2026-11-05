using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Code.Models;
using _Project.Code.Services;
using _Project.Code.Storage;
using UnityEngine;

namespace _Project.Code.Composition
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
        public List<EmployeeData> Employees => EmployeeRepository.Employees;
        public bool IsInitialized { get; private set; }
        public bool IsInitializationCompleted { get; private set; }
        public string InitializationMessage { get; private set; }

        public event Action OnInitializationCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CreateDependencies();
        }

        private async void Start()
        {
            await InitializeAsync();
        }

        private Task InitializeAsync()
        {
            if (_initializationTask == null)
            {
                _initializationTask = InitializeInternalAsync();
            }

            return _initializationTask;
        }

        private void OnDestroy()
        {
            AvatarService?.Dispose();

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
                IsInitializationCompleted = true;
                var message = IsInitialized
                    ? null
                    : EmployeeRepository.LastError ?? "No employee data is available.";
                InitializationMessage = message;
                OnInitializationCompleted?.Invoke();
            }
            catch (Exception exception)
            {
                IsInitialized = false;
                IsInitializationCompleted = true;
                var message = $"Application initialization failed: {exception.Message}";
                InitializationMessage = message;
                Debug.LogError(message);
                OnInitializationCompleted?.Invoke();
            }
        }
    }
}
