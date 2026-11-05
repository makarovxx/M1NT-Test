using System.Collections.Generic;
using EmployeeContacts.Models;
using UnityEngine;

namespace EmployeeContacts.Storage
{
    public sealed class FavoritesRepository
    {
        private readonly LocalStorageService _storage;

        public FavoritesRepository(LocalStorageService storage)
        {
            _storage = storage;
        }

        public HashSet<int> Load()
        {
            if (!_storage.TryReadText(StorageFile.Favorites, out var json))
            {
                return new HashSet<int>();
            }

            try
            {
                var data = JsonUtility.FromJson<FavoritesData>(json);
                var favorites = new HashSet<int>();
                if (data?.employeeIds == null)
                {
                    return favorites;
                }

                foreach (var employeeId in data.employeeIds)
                {
                    if (employeeId > 0)
                    {
                        favorites.Add(employeeId);
                    }
                }

                return favorites;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Could not deserialize local favorites: {exception.Message}");
                return new HashSet<int>();
            }
        }

        public bool Save(IEnumerable<int> employeeIds)
        {
            var ids = new List<int>(employeeIds);
            ids.Sort();
            var json = JsonUtility.ToJson(new FavoritesData { employeeIds = ids.ToArray() }, true);
            return _storage.TryWriteText(StorageFile.Favorites, json);
        }
    }
}
