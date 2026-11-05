using System;
using System.Collections.Generic;
using EmployeeContacts.Composition;
using EmployeeContacts.Models;
using EmployeeContacts.UI.Common;

namespace EmployeeContacts.UI.EmployeeList
{
    public sealed class EmployeeListController : IDisposable
    {
        private readonly CompositionRoot root;
        private readonly EmployeeListView view;
        private readonly AvatarSpriteProvider avatars;
        private readonly bool favoritesOnly;
        private readonly Action<EmployeeData> openProfile;

        public EmployeeListController(CompositionRoot value, EmployeeListView target, AvatarSpriteProvider provider,
            bool onlyFavorites, Action<EmployeeData> profile)
        {
            root = value;
            view = target;
            avatars = provider;
            favoritesOnly = onlyFavorites;
            openProfile = profile;
            view.FavoriteRequested += Toggle;
            view.ProfileRequested += openProfile;
            root.FavoritesService.FavoritesChanged += OnFavoritesChanged;
        }

        private bool hasShown;

        public void Show()
        {
            var data = favoritesOnly ? Filter() : root.Employees;
            var reset = favoritesOnly || !hasShown;
            view.Show(data, root.FavoritesService.IsFavorite, avatars,
                favoritesOnly ? "No favorite employees yet" : "No employee data available", reset);
            hasShown = true;
        }

        private List<EmployeeData> Filter()
        {
            var result = new List<EmployeeData>();
            foreach (var item in root.Employees)
                if (root.FavoritesService.IsFavorite(item.id))
                    result.Add(item);
            return result;
        }

        private void Toggle(EmployeeData employee)
        {
            root.FavoritesService.Toggle(employee.id);
        }

        private void OnFavoritesChanged()
        {
            if (favoritesOnly) Show();
            else view.RefreshFavoriteIcons();
        }

        public void Dispose()
        {
            view.FavoriteRequested -= Toggle;
            view.ProfileRequested -= openProfile;
            root.FavoritesService.FavoritesChanged -= OnFavoritesChanged;
        }
    }
}