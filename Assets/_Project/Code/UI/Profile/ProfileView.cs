using System;
using _Project.Code.Models;
using _Project.Code.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.UI.Profile
{
    public sealed class ProfileView : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image favoriteIcon;
        [SerializeField] private Button favoriteButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text genderText;
        [SerializeField] private TMP_Text emailText;
        [SerializeField] private TMP_Text ipText;
        [SerializeField] private Sprite favoriteOn;
        [SerializeField] private Sprite favoriteOff;

        private FavoritesService _favorites;
        private EmployeeData _employee;

        public event Action OnBackRequested;

        private void Awake()
        {
            favoriteButton.onClick.AddListener(HandleFavoriteButtonClicked);
            backButton.onClick.AddListener(HandleBackButtonClicked);
        }

        private void OnDestroy()
        {
            favoriteButton.onClick.RemoveListener(HandleFavoriteButtonClicked);
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
            Dispose();
        }

        public void Initialize(FavoritesService favorites)
        {
            Dispose();

            _favorites = favorites;
            _favorites.OnFavoritesChanged += RefreshFavoriteState;
        }

        public void Show(EmployeeData employee, Sprite avatar)
        {
            _employee = employee;
            avatarImage.sprite = avatar;
            nameText.text = employee.FullName;
            genderText.text = $"Gender: {employee.gender}";
            emailText.text = employee.email;
            ipText.text = employee.ip_address;
            RefreshFavoriteState();
        }

        public void Dispose()
        {
            if (_favorites != null)
            {
                _favorites.OnFavoritesChanged -= RefreshFavoriteState;
            }

            _favorites = null;
            _employee = null;
        }

        private void HandleFavoriteButtonClicked()
        {
            if (_employee == null)
            {
                return;
            }

            _favorites.Toggle(_employee.id);
        }

        private void HandleBackButtonClicked()
        {
            OnBackRequested?.Invoke();
        }

        private void RefreshFavoriteState()
        {
            if (_employee == null)
            {
                return;
            }

            favoriteIcon.sprite = _favorites.IsFavorite(_employee.id)
                ? favoriteOn
                : favoriteOff;
        }
    }
}
