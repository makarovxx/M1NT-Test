using System;
using System.Collections.Generic;
using _Project.Code.Models;
using _Project.Code.Storage;
using UnityEngine;

namespace _Project.Code.Services
{
    public sealed class FavoritesService
    {
        private readonly FavoritesRepository _repository;
        private readonly HashSet<int> _favorites = new HashSet<int>();

        public FavoritesService(FavoritesRepository repository)
        {
            _repository = repository;
        }

        public event Action OnFavoritesChanged;
        public void Initialize(List<EmployeeData> employees)
        {
            _favorites.Clear();
            _favorites.UnionWith(_repository.Load());

            // Do not remove saved IDs while employee data is unavailable offline.
            if (employees == null || employees.Count == 0)
            {
                return;
            }

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
            if (employeeId <= 0)
            {
                Debug.LogWarning("Cannot change favorite state for an invalid employee ID.");
                return;
            }

            if (!_favorites.Add(employeeId))
            {
                _favorites.Remove(employeeId);
            }

            Save();
            OnFavoritesChanged?.Invoke();
        }

        private void Save()
        {
            if (!_repository.Save(_favorites))
            {
                Debug.LogWarning("Favorites changed in memory but could not be saved locally.");
            }
        }
    }
}
