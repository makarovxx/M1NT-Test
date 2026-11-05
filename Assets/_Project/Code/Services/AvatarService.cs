using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Code.Models;
using _Project.Code.Storage;
using UnityEngine;

namespace _Project.Code.Services
{
    public sealed class AvatarService
    {
        public const int AvatarCount = 5;

        private readonly LocalStorageService _storage;
        private readonly ImageDownloader _downloader;
        private readonly AvatarAssignmentRepository _assignmentRepository;
        private readonly Texture2D _placeholderTexture;
        private readonly Dictionary<int, int> _assignments = new Dictionary<int, int>();
        private readonly Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>();
        private Sprite _placeholderSprite;

        public AvatarService(
            LocalStorageService storage,
            ImageDownloader downloader,
            AvatarAssignmentRepository assignmentRepository,
            Texture2D placeholderTexture)
        {
            _storage = storage;
            _downloader = downloader;
            _assignmentRepository = assignmentRepository;
            _placeholderTexture = placeholderTexture;
        }

        public string LastError { get; private set; }

        public async Task InitializeAsync(List<EmployeeData> employees)
        {
            LastError = null;
            await EnsureAvatarFilesAsync();

            _assignments.Clear();
            foreach (var pair in _assignmentRepository.Load())
            {
                if (IsValidImageId(pair.Value))
                {
                    _assignments[pair.Key] = pair.Value;
                }
            }

            if (employees == null || employees.Count == 0)
            {
                return;
            }

            var assignmentsChanged = false;
            var previousImageId = -1;
            var random = new System.Random();
            for (var index = 0; index < employees.Count; index++)
            {
                var employee = employees[index];
                if (!_assignments.TryGetValue(employee.id, out var imageId))
                {
                    imageId = GetRandomImageId(random, previousImageId);
                    _assignments[employee.id] = imageId;
                    assignmentsChanged = true;
                }

                previousImageId = imageId;
            }

            if (assignmentsChanged && !_assignmentRepository.Save(_assignments))
            {
                Debug.LogWarning("Avatar assignments are available for this session but could not be saved locally.");
            }
        }

        private bool TryGetImageId(int employeeId, out int imageId)
        {
            return _assignments.TryGetValue(employeeId, out imageId);
        }

        public Sprite GetSprite(int employeeId)
        {
            if (!TryGetImageId(employeeId, out var imageId))
            {
                return GetPlaceholderSprite();
            }

            if (_sprites.TryGetValue(imageId, out var sprite))
            {
                return sprite;
            }

            var texture = LoadAvatarTexture(employeeId);
            sprite = CreateSprite(texture);
            _sprites[imageId] = sprite;
            return sprite;
        }

        public void Dispose()
        {
            foreach (var sprite in _sprites.Values)
            {
                if (sprite == null)
                {
                    continue;
                }

                var texture = sprite.texture;
                UnityEngine.Object.Destroy(sprite);
                if (texture != null && texture != _placeholderTexture)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }

            _sprites.Clear();

            if (_placeholderSprite != null)
            {
                UnityEngine.Object.Destroy(_placeholderSprite);
                _placeholderSprite = null;
            }
        }

        private Texture2D LoadAvatarTexture(int employeeId)
        {
            if (!_assignments.TryGetValue(employeeId, out var imageId)
                || !_storage.TryReadImage(imageId, out var imageBytes)
                || !ImageDownloader.TryCreateTexture(imageBytes, out var texture))
            {
                return _placeholderTexture;
            }

            return texture;
        }

        private async Task EnsureAvatarFilesAsync()
        {
            for (var imageId = 0; imageId < AvatarCount; imageId++)
            {
                if (_storage.TryReadImage(imageId, out var localBytes) && ImageDownloader.IsImageDataValid(localBytes))
                {
                    continue;
                }

                try
                {
                    var downloadedBytes = await _downloader.DownloadImageAsync();
                    if (!_storage.TryWriteImage(imageId, downloadedBytes))
                    {
                        LastError = $"Avatar {imageId} could not be saved locally.";
                        Debug.LogWarning(LastError);
                    }
                }
                catch (Exception exception)
                {
                    LastError = $"Avatar {imageId} could not be downloaded: {exception.Message}";
                    Debug.LogWarning(LastError);
                }
            }
        }

        private bool IsValidImageId(int imageId)
        {
            return imageId >= 0 && imageId < AvatarCount;
        }

        private int GetRandomImageId(System.Random random, int previousImageId)
        {
            var imageId = random.Next(AvatarCount - 1);
            if (previousImageId >= 0 && imageId >= previousImageId)
            {
                imageId++;
            }

            return imageId;
        }

        private static Sprite CreateSprite(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
        }

        private Sprite GetPlaceholderSprite()
        {
            if (_placeholderSprite == null)
            {
                _placeholderSprite = CreateSprite(_placeholderTexture);
            }

            return _placeholderSprite;
        }
    }
}
