using System;
using System.Collections.Generic;
using _Project.Code.Models;
using _Project.Code.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.UI.EmployeeList
{
    public sealed class EmployeeListView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private EmployeeCardView employeeCardPrefab;
        [SerializeField] private TMP_Text emptyText;
        [SerializeField, Min(1)] private int batchSize = 20;
        [SerializeField, Range(0.01f, 0.5f)] private float bottomThreshold = 0.1f;

        private readonly List<EmployeeCardView> _cards = new List<EmployeeCardView>();
        private readonly List<EmployeeData> _displayedEmployees = new List<EmployeeData>();

        private FavoritesService _favorites;
        private AvatarService _avatars;
        private List<EmployeeData> _employees;
        private ContactListMode _mode;
        private int _nextEmployeeIndex;
        private bool _isLoading;
        private bool _isInitialized;

        public event Action<EmployeeData> OnProfileRequested;

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(HandleScrollChanged);
        }

        private void OnDestroy()
        {
            scrollRect.onValueChanged.RemoveListener(HandleScrollChanged);
            Dispose();
        }

        public void Initialize(
            List<EmployeeData> employees,
            FavoritesService favorites,
            AvatarService avatars,
            ContactListMode mode)
        {
            Dispose();

            _employees = employees;
            _favorites = favorites;
            _avatars = avatars;
            _mode = mode;
            _favorites.OnFavoritesChanged += HandleFavoritesChanged;
            _isInitialized = true;
        }

        public void Show()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (_mode == ContactListMode.Favorites || _cards.Count == 0)
            {
                Rebuild();
            }

            gameObject.SetActive(true);
        }

        public void Dispose()
        {
            if (_favorites != null)
            {
                _favorites.OnFavoritesChanged -= HandleFavoritesChanged;
            }

            _favorites = null;
            _avatars = null;
            _employees = null;
            _isInitialized = false;
            ClearCards();
        }

        private void HandleScrollChanged(Vector2 position)
        {
            if (_isLoading || _nextEmployeeIndex >= _displayedEmployees.Count || position.y > bottomThreshold)
            {
                return;
            }

            LoadNextBatch();
        }

        private void HandleFavoritesChanged()
        {
            if (_mode == ContactListMode.Favorites)
            {
                Rebuild();
                return;
            }

            foreach (var card in _cards)
            {
                card.SetFavoriteState(_favorites.IsFavorite(card.EmployeeId));
            }
        }

        private void Rebuild()
        {
            ClearCards();
            CollectEmployees();

            var isEmpty = _displayedEmployees.Count == 0;
            emptyText.text = _mode == ContactListMode.Favorites
                ? "No favorite employees yet"
                : "No employee data available";
            emptyText.gameObject.SetActive(isEmpty);
            scrollRect.gameObject.SetActive(!isEmpty);

            if (isEmpty)
            {
                return;
            }

            LoadNextBatch();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private void CollectEmployees()
        {
            _displayedEmployees.Clear();

            if (_employees == null)
            {
                return;
            }

            foreach (var employee in _employees)
            {
                if (_mode == ContactListMode.All || _favorites.IsFavorite(employee.id))
                {
                    _displayedEmployees.Add(employee);
                }
            }
        }

        private void LoadNextBatch()
        {
            _isLoading = true;
            var endIndex = Mathf.Min(_nextEmployeeIndex + batchSize, _displayedEmployees.Count);

            while (_nextEmployeeIndex < endIndex)
            {
                var employee = _displayedEmployees[_nextEmployeeIndex];
                var card = Instantiate(employeeCardPrefab, cardsRoot);
                card.Bind(
                    employee,
                    _avatars.GetSprite(employee.id),
                    _favorites.IsFavorite(employee.id),
                    HandleFavoriteRequested,
                    HandleProfileRequested);
                _cards.Add(card);
                _nextEmployeeIndex++;
            }

            _isLoading = false;
        }

        private void ClearCards()
        {
            foreach (var card in _cards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }

            _cards.Clear();
            _displayedEmployees.Clear();
            _nextEmployeeIndex = 0;
        }

        private void HandleFavoriteRequested(EmployeeData employee)
        {
            _favorites.Toggle(employee.id);
        }

        private void HandleProfileRequested(EmployeeData employee)
        {
            OnProfileRequested?.Invoke(employee);
        }
    }
}
