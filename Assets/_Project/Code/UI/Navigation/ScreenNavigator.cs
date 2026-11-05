using System;
using EmployeeContacts.Models;
using UnityEngine;
using UnityEngine.UI;

namespace EmployeeContacts.UI.Navigation
{
    public sealed class ScreenNavigator : MonoBehaviour
    {
        [SerializeField] private GameObject employeeDataPanel;
        [SerializeField] private GameObject favoritesPanel;
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private GameObject bottomNavigationPanel;
        [SerializeField] private Button employeeDataButton;
        [SerializeField] private Button favoritesButton;
        private GameObject previousPanel;
        public event Action<EmployeeData> ProfileOpened;
        public event Action<bool> ListScreenChanged;
        public void ShowEmployees() => ShowList(employeeDataPanel, false);
        public void ShowFavorites() => ShowList(favoritesPanel, true);

        public void OpenProfile(EmployeeData employee)
        {
            previousPanel = employeeDataPanel.activeSelf ? employeeDataPanel : favoritesPanel;
            employeeDataPanel.SetActive(false);
            favoritesPanel.SetActive(false);
            profilePanel.SetActive(true);
            bottomNavigationPanel.SetActive(false);
            ProfileOpened?.Invoke(employee);
        }

        public void CloseProfile()
        {
            profilePanel.SetActive(false);
            previousPanel.SetActive(true);
            bottomNavigationPanel.SetActive(true);
            ListScreenChanged?.Invoke(previousPanel == favoritesPanel);
        }

        private void ShowList(GameObject active, bool favorites)
        {
            employeeDataPanel.SetActive(active == employeeDataPanel);
            favoritesPanel.SetActive(active == favoritesPanel);
            profilePanel.SetActive(false);
            bottomNavigationPanel.SetActive(true);
            employeeDataButton.interactable = favorites;
            favoritesButton.interactable = !favorites;
            ListScreenChanged?.Invoke(favorites);
        }
    }
}