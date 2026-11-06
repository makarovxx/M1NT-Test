using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Code.Models;
using _Project.Code.Storage;
using UnityEngine;

namespace _Project.Code.Services
{
    public sealed class EmployeeDataService
    {
        private const string DownloadUrl = "https://drive.usercontent.google.com/download?id=1YvE6Y5-vxVWXrrYyb83ssqMfVUUkndLw&export=download&confirm=t";

        private readonly LocalStorage _storage;
        private readonly ApiClient _apiClient;
        private readonly List<EmployeeData> _employees = new();

        public EmployeeDataService(LocalStorage storage, ApiClient apiClient)
        {
            _storage = storage;
            _apiClient = apiClient;
        }

        public async Task<List<EmployeeData>> LoadAsync()
        {
            if (_storage.TryReadText(StorageFile.Employees, out var localJson))
            {
                if (TryDeserialize(localJson, out var localEmployees))
                {
                    SetEmployees(localEmployees);
                    return _employees;
                }
            }

            try
            {
                var downloadedJson = await _apiClient.GetTextAsync(DownloadUrl);
                if (!TryDeserialize(downloadedJson, out var downloadedEmployees))
                {
                    return _employees;
                }

                SetEmployees(downloadedEmployees);
                if (!_storage.TryWriteText(StorageFile.Employees, downloadedJson))
                {
                    Debug.LogWarning("Employee data is available for this session but could not be saved locally.");
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return _employees;
        }

        private bool TryDeserialize(string json, out EmployeeData[] employees)
        {
            employees = null;

            try
            {
                var collection = JsonUtility.FromJson<EmployeeCollection>(json);
                if (collection?.data == null || collection.data.Length == 0)
                {
                    return false;
                }

                var ids = new HashSet<int>();
                foreach (var employee in collection.data)
                {
                    if (employee == null || employee.id <= 0)
                    {
                        return false;
                    }

                    if (!ids.Add(employee.id))
                    {
                        return false;
                    }
                }

                employees = collection.data;
                return true;
            }
            catch (Exception)
            {
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
