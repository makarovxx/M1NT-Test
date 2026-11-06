using System;
using System.Collections.Generic;
using _Project.Code.Models;
using _Project.Code.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.UI.EmployeeList
{
    public abstract class EmployeeListViewBase : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private EmployeeCardView employeeCardPrefab;
        [SerializeField] private TMP_Text emptyText;
        [SerializeField, Min(1)] private int batchSize = 20;
        [SerializeField, Range(0.01f, 0.5f)] private float bottomThreshold = 0.1f;

        private readonly List<EmployeeCardView> _cards = new List<EmployeeCardView>();
        private readonly List<EmployeeData> _displayedEmployees = new List<EmployeeData>();
        private List<EmployeeData> _employees;
        private FavoritesService _favorites;
        private AvatarService _avatars;
        private int _nextEmployeeIndex;
        private bool _isInitialized;
        private bool _isLoading;

        public event Action<EmployeeData> OnProfileRequested;

        protected FavoritesService Favorites => _favorites;

        protected abstract bool ShouldDisplay(EmployeeData employee);
        protected virtual bool RebuildWhenFavoritesChanged => false;
        protected virtual bool RebuildWhenShow => false;

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
            AvatarService avatars)
        {
            Dispose();

            _employees = employees;
            _favorites = favorites;
            _avatars = avatars;
            _favorites.OnFavoritesChanged += HandleFavoritesChanged;
            _isInitialized = true;
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

        public void Show()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (_cards.Count == 0 || RebuildWhenShow)
            {
                Rebuild();
            }

            gameObject.SetActive(true);
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
            if (RebuildWhenFavoritesChanged)
            {
                if (gameObject.activeInHierarchy)
                {
                    Rebuild();
                }

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
                if (ShouldDisplay(employee))
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
                card.Bind(employee,
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