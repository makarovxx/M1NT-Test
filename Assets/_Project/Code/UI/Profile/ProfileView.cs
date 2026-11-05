using System;
using EmployeeContacts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmployeeContacts.UI.Profile
{
    public sealed class ProfileView : MonoBehaviour
    {
        [SerializeField] private Image avatarImage; [SerializeField] private Image favoriteIcon; [SerializeField] private Button favoriteButton; [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text nameText; [SerializeField] private TMP_Text genderText; [SerializeField] private TMP_Text emailText; [SerializeField] private TMP_Text ipText; [SerializeField] private Sprite favoriteOn; [SerializeField] private Sprite favoriteOff;
        private EmployeeData employee; public event Action<EmployeeData> FavoriteRequested; public event Action BackRequested;
        private void Awake(){favoriteButton.onClick.AddListener(()=>FavoriteRequested?.Invoke(employee));backButton.onClick.AddListener(()=>BackRequested?.Invoke());}
        public void Show(EmployeeData value,Sprite avatar,bool favorite){employee=value;avatarImage.sprite=avatar;favoriteIcon.sprite=favorite?favoriteOn:favoriteOff;nameText.text=value.FullName;genderText.text="Gender: "+value.gender;emailText.text=value.email;ipText.text=value.ip_address;}
    }
}
