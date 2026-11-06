using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Code.Models;
using _Project.Code.Storage;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace _Project.Code.Services
{
    public sealed class AvatarService
    {
        private const string ImageUrl = "https://picsum.photos/200";
        private const string PlaceholderPath = "Images/progile-avatar-bg";
        private const int AvatarCount = 5;

        private readonly LocalStorage _storage;
        private readonly ApiClient _apiClient;
        private readonly AvatarAssignmentStorage _assignmentStorage;

        private readonly Dictionary<int, int> _assignments = new();
        private readonly Dictionary<int, Sprite> _sprites = new();

        private readonly Random _random = new();
        private readonly Sprite _placeholder;

        public AvatarService(
            LocalStorage storage,
            ApiClient apiClient,
            AvatarAssignmentStorage assignmentStorage)
        {
            _storage = storage;
            _apiClient = apiClient;
            _assignmentStorage = assignmentStorage;

            _placeholder = Resources.Load<Sprite>(PlaceholderPath);
        }

        public async Task InitializeAsync(List<EmployeeData> employees)
        {
            await LoadAvatarsAsync();

            LoadAssignments();
            AssignAvatars(employees);
        }

        public Sprite GetSprite(int employeeId)
        {
            if (!_assignments.TryGetValue(employeeId, out var imageId))
                return _placeholder;

            return _sprites.GetValueOrDefault(imageId, _placeholder);
        }

        public void Dispose()
        {
            foreach (var sprite in _sprites.Values)
            {
                Object.Destroy(sprite);
                Object.Destroy(sprite.texture);
            }

            _sprites.Clear();
        }

        private async Task LoadAvatarsAsync()
        {
            for (var imageId = 0; imageId < AvatarCount; imageId++)
            {
                if (TryLoadLocalAvatar(imageId))
                    continue;

                await DownloadAvatarAsync(imageId);
            }
        }

        private bool TryLoadLocalAvatar(int imageId)
        {
            if (!_storage.TryReadImage(imageId, out var bytes))
                return false;

            var sprite = CreateSprite(bytes);

            // if (sprite == null)
            // {
            //     Debug.LogWarning($"Avatar {imageId} is corrupted.");
            //     return false;
            // }

            _sprites[imageId] = sprite;
            return true;
        }

        private async Task DownloadAvatarAsync(int imageId)
        {
            byte[] bytes;

            try
            {
                bytes = await _apiClient.GetBytesAsync(ImageUrl);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed download avatar {imageId}: {exception.Message}");
                return;
            }

            var sprite = CreateSprite(bytes);

            if (sprite == null)
            {
                Debug.LogWarning($"Downloaded avatar {imageId} is invalid.");
                return;
            }

            _sprites[imageId] = sprite;

            if (!_storage.TryWriteImage(imageId, bytes))
                Debug.LogWarning($"Failed to save avatar {imageId}.");
        }

        private Sprite CreateSprite(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            var texture = new Texture2D(2, 2);

            if (!texture.LoadImage(bytes, true))
            {
                Object.Destroy(texture);
                return null;
            }

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        private void LoadAssignments()
        {
            _assignments.Clear();

            var savedAssignments = _assignmentStorage.Load();

            if (savedAssignments == null)
                return;

            foreach (var pair in savedAssignments)
            {
                if (pair.Value >= 0 && pair.Value < AvatarCount)
                    _assignments[pair.Key] = pair.Value;
            }
        }

        private void AssignAvatars(List<EmployeeData> employees)
        {
            if (employees == null)
                return;

            var changed = false;
            var previousImageId = -1;

            foreach (var employee in employees)
            {
                if (!_assignments.TryGetValue(employee.id, out var imageId) ||
                    imageId == previousImageId)
                {
                    imageId = GetRandomImageId(previousImageId);

                    _assignments[employee.id] = imageId;
                    changed = true;
                }

                previousImageId = imageId;
            }

            if (changed && !_assignmentStorage.Save(_assignments))
                Debug.LogWarning("Failed to save avatar assignments.");
        }

        private int GetRandomImageId(int previousImageId)
        {
            if (previousImageId < 0)
                return _random.Next(AvatarCount);

            var imageId = _random.Next(AvatarCount - 1);

            return imageId >= previousImageId
                ? imageId + 1
                : imageId;
        }
    }
}