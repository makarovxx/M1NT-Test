using System;
using System.Collections.Generic;
using EmployeeContacts.Models;
using EmployeeContacts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmployeeContacts.UI.EmployeeList
{
    public sealed class EmployeeListView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private EmployeeCardView employeeCardPrefab;
        [SerializeField] private TMP_Text emptyText;
        [SerializeField, Min(1)] private int batchSize = 20;
        [SerializeField, Range(0.01f, .5f)] private float bottomThreshold = .1f;
        private readonly List<EmployeeCardView> cards = new List<EmployeeCardView>();
        private List<EmployeeData> employees;
        private int nextIndex;
        private bool loading;
        private Func<int, bool> isFavorite;
        private AvatarSpriteProvider avatars;
        public event Action<EmployeeData> FavoriteRequested;
        public event Action<EmployeeData> ProfileRequested;

        private void OnEnable()
        {
            scrollRect.onValueChanged.AddListener(OnScroll);
        }

        private void OnDisable()
        {
            scrollRect.onValueChanged.RemoveListener(OnScroll);
        }

        public void Show(List<EmployeeData> source, Func<int, bool> favoriteState, AvatarSpriteProvider spriteProvider,
            string message, bool reset)
        {
            if (reset) Clear();
            employees = source;
            isFavorite = favoriteState;
            avatars = spriteProvider;
            emptyText.text = message;
            emptyText.gameObject.SetActive(source.Count == 0);
            scrollRect.gameObject.SetActive(source.Count > 0);
            if (reset) LoadNextBatch();
        }

        public void RefreshFavoriteIcons()
        {
            foreach (var card in cards) card.SetFavorite(isFavorite(card.EmployeeId));
        }

        private void OnScroll(Vector2 position)
        {
            if (employees != null && !loading && nextIndex < employees.Count && position.y <= bottomThreshold)
                LoadNextBatch();
        }

        private void LoadNextBatch()
        {
            loading = true;
            var factory = new EmployeeCardFactory(employeeCardPrefab, cardsRoot);
            var end = Mathf.Min(nextIndex + batchSize, employees.Count);
            for (; nextIndex < end; nextIndex++)
            {
                var employee = employees[nextIndex];
                var card = factory.Create();
                card.Bind(employee, avatars.GetSprite(employee.id), isFavorite(employee.id),
                    e => FavoriteRequested?.Invoke(e), e => ProfileRequested?.Invoke(e));
                cards.Add(card);
            }

            loading = false;
        }

        private void Clear()
        {
            foreach (var card in cards) Destroy(card.gameObject);
            cards.Clear();
            nextIndex = 0;
        }
    }
}