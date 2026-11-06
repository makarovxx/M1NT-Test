using System;
using System.Collections.Generic;
using _Project.Code.Models;
using _Project.Code.Services;
using UnityEngine;

namespace _Project.Code.Storage
{
    public sealed class FavoritesStorage
    {
        private readonly LocalStorage _storage;

        public FavoritesStorage(LocalStorage storage)
        {
            _storage = storage;
        }

        public HashSet<int> Load()
        {
            if (!_storage.TryReadText(StorageFile.Favorites, out var json))
                return new HashSet<int>();

            var data = JsonUtility.FromJson<FavoritesData>(json);
            var favorites = new HashSet<int>();

            if (data == null || data.employeeIds == null)
                return favorites;

            foreach (int employeeId in data.employeeIds)
            {
                if (employeeId > 0)
                    favorites.Add(employeeId);
            }

            return favorites;
        }

        public bool Save(HashSet<int> favorites)
        {
            var data = new FavoritesData
            {
                employeeIds = new List<int>(favorites).ToArray()
            };

            string json = JsonUtility.ToJson(data, true);

            return _storage.TryWriteText(StorageFile.Favorites, json);
        }
    }
}