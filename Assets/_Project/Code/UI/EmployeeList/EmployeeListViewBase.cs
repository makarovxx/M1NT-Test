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
        private const float CardHeight = 130f;

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform cardsRoot;
        [SerializeField] private EmployeeCardView employeeCardPrefab;
        [SerializeField] private TMP_Text emptyText;

        [SerializeField, Min(0f)] private float spacingBetweenItems = 20f;
        [SerializeField, Min(0f)] private float topPadding = 250f;
        [SerializeField, Min(0f)] private float bottomPadding = 250f;
        [SerializeField, Min(0)] private int bufferSize = 2;

        private readonly List<EmployeeCardView> _cardsViews = new();
        private readonly List<EmployeeData> _displayedEmployees = new();

        private List<EmployeeData> _employees;
        private FavoritesService _favorites;
        private AvatarService _avatarService;
        private int _lastStartIndex = -1;

        public event Action<EmployeeData> OnProfileRequested;

        protected FavoritesService Favorites => _favorites;

        protected abstract bool ShouldDisplay(EmployeeData employee);
        protected virtual bool RebuildWhenFavoritesChanged => false;
        protected virtual bool RebuildWhenShow => false;

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
        }

        private void OnDestroy()
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);

            Dispose();
        }

        public void Initialize(List<EmployeeData> employees, FavoritesService favorites, AvatarService avatars)
        {
            Dispose();

            _employees = employees;
            _favorites = favorites;
            _avatarService = avatars;
            _favorites.OnFavoritesChanged += HandleFavoritesChanged;
            // _isInitialized = true;
        }

        public void Dispose()
        {
            if (_favorites != null)
                _favorites.OnFavoritesChanged -= HandleFavoritesChanged;
        }

        public void Show()
        {
            // if (!_isInitialized)
            //     return;

            gameObject.SetActive(true);

            if (_cardsViews.Count == 0 || RebuildWhenShow)
                Rebuild();
        }

        private void InitializeVirtualization()
        {
            float viewportHeight = scrollRect.viewport.rect.height;
            float itemSpace = CardHeight + spacingBetweenItems;
            int visibleCardCount = Mathf.Max(1, Mathf.CeilToInt((viewportHeight + spacingBetweenItems) / itemSpace));
            int requiredCardCount = visibleCardCount + bufferSize * 2;

            CreatePool(requiredCardCount);
        }

        private void OnScrollChanged(Vector2 position)
        {
            UpdateVisibleCards();
        }

        private void HandleFavoritesChanged()
        {
            if (RebuildWhenFavoritesChanged)
            {
                if (gameObject.activeInHierarchy)
                    Rebuild();

                return;
            }

            foreach (EmployeeCardView card in _cardsViews)
            {
                if (card.gameObject.activeSelf)
                    card.SetFavoriteState(_favorites.IsFavorite(card.EmployeeId));
            }
        }

        private void Rebuild()
        {
            CollectEmployees();

            bool isEmpty = _displayedEmployees.Count == 0;
            if (emptyText != null)
                emptyText.gameObject.SetActive(isEmpty);

            ClearCards();

            if (isEmpty)
            {
                return;
            }

            InitializeVirtualization();
            if (_cardsViews.Count == 0)
            {
                return;
            }

            UpdateContentHeight();
            scrollRect.verticalNormalizedPosition = 1f;
            _lastStartIndex = -1;
            UpdateVisibleCards(0);
            // _isRebuilding = false;
        }

        private void CollectEmployees()
        {
            _displayedEmployees.Clear();

            foreach (EmployeeData employee in _employees)
            {
                if (ShouldDisplay(employee))
                    _displayedEmployees.Add(employee);
            }
        }

        private void CreatePool(int poolSize)
        {
            for (int i = 0; i < poolSize; i++)
            {
                EmployeeCardView card = Instantiate(employeeCardPrefab, cardsRoot);
                card.gameObject.SetActive(false);
                _cardsViews.Add(card);
            }
        }

        private void UpdateContentHeight()
        {
            RectTransform content = scrollRect.content;
            float totalHeight = topPadding + bottomPadding;

            if (_displayedEmployees.Count > 0)
                totalHeight += _displayedEmployees.Count * CardHeight + (_displayedEmployees.Count - 1) * spacingBetweenItems;

            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }

        private void UpdateVisibleCards(int forcedStartIndex = -1)
        {
            int firstVisibleIndex;
            if (forcedStartIndex >= 0)
            {
                firstVisibleIndex = forcedStartIndex;
            }
            else
            {
                float scrollY = Mathf.Max(0f, scrollRect.content.anchoredPosition.y - topPadding);
                firstVisibleIndex = Mathf.FloorToInt(scrollY / (CardHeight + spacingBetweenItems));
            }

            int startIndex = Mathf.Clamp(firstVisibleIndex - bufferSize, 0,
                Mathf.Max(0, _displayedEmployees.Count - _cardsViews.Count));

            if (startIndex == _lastStartIndex)
                return;

            _lastStartIndex = startIndex;

            for (int i = 0; i < _cardsViews.Count; i++)
            {
                int employeeIndex = startIndex + i;
                EmployeeCardView card = _cardsViews[i];

                if (employeeIndex >= _displayedEmployees.Count)
                {
                    card.gameObject.SetActive(false);
                    continue;
                }

                EmployeeData employee = _displayedEmployees[employeeIndex];
                BindEmployeeToView(card, employee);

                SetItemPosition(card, employeeIndex);
                card.gameObject.SetActive(true);
            }
        }

        private void SetItemPosition(EmployeeCardView card, int employeeIndex)
        {
            RectTransform rect = card.transform as RectTransform;
            rect.anchoredPosition = new Vector2(0f, -(topPadding + employeeIndex * (CardHeight + spacingBetweenItems)));
        }

        private void BindEmployeeToView(EmployeeCardView card, EmployeeData employee)
        {
            card.Bind(
                employee,
                _avatarService?.GetSprite(employee.id),
                _favorites != null && _favorites.IsFavorite(employee.id),
                HandleFavoriteRequested,
                HandleProfileRequested);
        }

        private void ClearCards()
        {
            foreach (EmployeeCardView card in _cardsViews)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }

            _cardsViews.Clear();
            _lastStartIndex = -1;
        }

        private void HandleFavoriteRequested(EmployeeData employee)
        {
            _favorites?.Toggle(employee.id);
        }

        private void HandleProfileRequested(EmployeeData employee)
        {
            OnProfileRequested?.Invoke(employee);
        }
    }
}