using System;
using EmployeeContacts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmployeeContacts.UI.EmployeeList
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
        private EmployeeData employee;
        private Action<EmployeeData> favoriteRequested;
        private Action<EmployeeData> profileRequested;
        public int EmployeeId => employee == null ? 0 : employee.id;

        private void Awake()
        {
            favoriteButton.onClick.AddListener(() => favoriteRequested?.Invoke(employee));
            profileButton.onClick.AddListener(() => profileRequested?.Invoke(employee));
        }

        public void Bind(EmployeeData value, Sprite avatar, bool favorite, Action<EmployeeData> onFavorite,
            Action<EmployeeData> onProfile)
        {
            employee = value;
            favoriteRequested = onFavorite;
            profileRequested = onProfile;
            avatarImage.sprite = avatar;
            favoriteIcon.sprite = favorite ? favoriteOnSprite : favoriteOffSprite;
            nameText.text = value.FullName;
            detailsText.text = value.email + "   |   " + value.ip_address;
        }

        public void SetFavorite(bool value)
        {
            if (employee != null) favoriteIcon.sprite = value ? favoriteOnSprite : favoriteOffSprite;
        }
    }
}