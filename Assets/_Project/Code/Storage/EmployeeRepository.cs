using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Code.Models;
using _Project.Code.Services;
using UnityEngine;

namespace _Project.Code.Storage
{
    public sealed class EmployeeRepository
    {
        private readonly LocalStorageService _storage;
        private readonly EmployeeDataDownloader _downloader;
        private readonly List<EmployeeData> _employees = new();

        public List<EmployeeData> Employees => _employees;

        public string LastError { get; private set; }

        public EmployeeRepository(LocalStorageService storage, EmployeeDataDownloader downloader)
        {
            _storage = storage;
            _downloader = downloader;
        }

        public async Task<List<EmployeeData>> LoadAsync()
        {
            LastError = null;
            if (_storage.TryReadText(StorageFile.Employees, out var localJson))
            {
                if (TryDeserialize(localJson, out var localEmployees, out var validationError))
                {
                    SetEmployees(localEmployees);
                    return Employees;
                }

                Debug.LogWarning($"Local employee data is invalid and will be refreshed: {validationError}");
            }

            try
            {
                var downloadedJson = await _downloader.DownloadJsonAsync();
                if (!TryDeserialize(downloadedJson, out var downloadedEmployees, out var validationError))
                {
                    LastError = $"Downloaded employee data is invalid: {validationError}";
                    Debug.LogError(LastError);
                    return Employees;
                }

                SetEmployees(downloadedEmployees);
                if (!_storage.TryWriteText(StorageFile.Employees, downloadedJson))
                {
                    Debug.LogWarning("Employee data is available for this session but could not be saved locally.");
                }

                return Employees;
            }
            catch (Exception exception)
            {
                LastError = $"Could not load employee data: {exception.Message}";
                Debug.LogError(LastError);
                return Employees;
            }
        }

        private bool TryDeserialize(string json, out EmployeeData[] employees, out string error)
        {
            employees = null;
            error = null;

            try
            {
                var collection = JsonUtility.FromJson<EmployeeCollection>(json);
                if (collection == null || collection.data == null || collection.data.Length == 0)
                {
                    error = "The data array is missing or empty.";
                    return false;
                }

                var ids = new HashSet<int>();
                for (var index = 0; index < collection.data.Length; index++)
                {
                    var employee = collection.data[index];
                    if (employee == null || !employee.IsValid())
                    {
                        error = $"Employee at index {index} has missing or invalid required fields.";
                        return false;
                    }

                    if (!ids.Add(employee.id))
                    {
                        error = $"Duplicate employee ID {employee.id}.";
                        return false;
                    }
                }

                employees = collection.data;
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private void SetEmployees(IEnumerable<EmployeeData> employees)
        {
            _employees.Clear();
            _employees.AddRange(employees);
        }
    }
}
