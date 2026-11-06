using System;
using System.Collections.Generic;
using _Project.Code.Models;
using _Project.Code.Storage;

namespace _Project.Code.Services
{
    public sealed class FavoritesService
    {
        private readonly FavoritesStorage _favoritesStorage;
        private readonly HashSet<int> _favorites = new();

        public event Action OnFavoritesChanged;

        public FavoritesService(FavoritesStorage favoritesStorage)
        {
            _favoritesStorage = favoritesStorage;
        }

        public void Initialize(List<EmployeeData> employees)
        {
            _favorites.Clear();
            _favorites.UnionWith(_favoritesStorage.Load());

            var validIds = new HashSet<int>();
            foreach (var employee in employees)
            {
                validIds.Add(employee.id);
            }

            if (_favorites.RemoveWhere(id => !validIds.Contains(id)) > 0)
            {
                Save();
            }
        }

        public bool IsFavorite(int employeeId)
        {
            return _favorites.Contains(employeeId);
        }

        public void Toggle(int employeeId)
        {
            if (!_favorites.Add(employeeId))
            {
                _favorites.Remove(employeeId);
            }

            Save();
            OnFavoritesChanged?.Invoke();
        }

        private void Save()
        {
            _favoritesStorage.Save(_favorites);
        }
    }
}
