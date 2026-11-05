using System;
using _Project.Code.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.UI.EmployeeList
{
    public sealed class EmployeeCardView : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image favoriteIcon;
        [SerializeField] private Button favoriteButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text detailsText;
        [SerializeField] private Sprite favoriteOnSprite;
        [SerializeField] private Sprite favoriteOffSprite;

        private EmployeeData _employee;
        private Action<EmployeeData> _favoriteRequested;
        private Action<EmployeeData> _profileRequested;

        public int EmployeeId => _employee == null ? 0 : _employee.id;

        private void Awake()
        {
            favoriteButton.onClick.AddListener(HandleFavoriteButtonClicked);
            profileButton.onClick.AddListener(HandleProfileButtonClicked);
        }

        private void OnDestroy()
        {
            favoriteButton.onClick.RemoveListener(HandleFavoriteButtonClicked);
            profileButton.onClick.RemoveListener(HandleProfileButtonClicked);
        }

        public void Bind(
            EmployeeData employee,
            Sprite avatar,
            bool isFavorite,
            Action<EmployeeData> favoriteRequested,
            Action<EmployeeData> profileRequested)
        {
            _employee = employee;
            _favoriteRequested = favoriteRequested;
            _profileRequested = profileRequested;

            avatarImage.sprite = avatar;
            favoriteIcon.sprite = isFavorite ? favoriteOnSprite : favoriteOffSprite;
            nameText.text = employee.FullName;
            detailsText.text = $"{employee.email}   |   {employee.ip_address}";
        }

        public void SetFavoriteState(bool isFavorite)
        {
            favoriteIcon.sprite = isFavorite ? favoriteOnSprite : favoriteOffSprite;
        }

        private void HandleFavoriteButtonClicked()
        {
            _favoriteRequested?.Invoke(_employee);
        }

        private void HandleProfileButtonClicked()
        {
            _profileRequested?.Invoke(_employee);
        }
    }
}
