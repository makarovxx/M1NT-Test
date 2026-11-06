using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.UI.Navigation
{
    public sealed class ScreenNavigator : MonoBehaviour
    {
        [SerializeField] private GameObject employeeDataPanel;
        [SerializeField] private GameObject favoritesPanel;
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private GameObject bottomNavigationPanel;
        [SerializeField] private Button employeeDataButton;
        [SerializeField] private Button favoritesButton;

        private EmployeeListScreen _currentListScreen = EmployeeListScreen.AllEmployees;
        
        public event Action<EmployeeListScreen> OnListScreenChanged;

        public void Initialize(EmployeeListScreen currentListScreen)
        {
            ShowList(currentListScreen);
            _currentListScreen = currentListScreen;
        }

        private void OnEnable()
        {
            employeeDataButton.onClick.AddListener(ShowEmployees);
            favoritesButton.onClick.AddListener(ShowFavorites);
        }

        private void OnDisable()
        {
            employeeDataButton.onClick.RemoveListener(ShowEmployees);
            favoritesButton.onClick.RemoveListener(ShowFavorites);
        }

        public void ShowProfile()
        {
            if (profilePanel.activeSelf)
            {
                return;
            }

            employeeDataPanel.SetActive(false);
            favoritesPanel.SetActive(false);
            profilePanel.SetActive(true);
            bottomNavigationPanel.SetActive(false);
        }

        public void CloseProfile()
        {
            if (!profilePanel.activeSelf)
            {
                return;
            }

            profilePanel.SetActive(false);
            bottomNavigationPanel.SetActive(true);
            ShowList(_currentListScreen);
        }

        private void ShowEmployees()
        {
            ShowList(EmployeeListScreen.AllEmployees);
        }

        private void ShowFavorites()
        {
            ShowList(EmployeeListScreen.Favorites);
        }

        public void ShowList(EmployeeListScreen screen)
        {
            _currentListScreen = screen;
            var showFavorites = screen == EmployeeListScreen.Favorites;

            employeeDataPanel.SetActive(!showFavorites);
            favoritesPanel.SetActive(showFavorites);
            profilePanel.SetActive(false);
            bottomNavigationPanel.SetActive(true);

            employeeDataButton.interactable = showFavorites;
            favoritesButton.interactable = !showFavorites;

            OnListScreenChanged?.Invoke(screen);
        }
    }
}
