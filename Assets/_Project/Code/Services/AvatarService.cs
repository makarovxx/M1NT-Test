using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeContacts.Models;
using EmployeeContacts.Storage;
using UnityEngine;

namespace EmployeeContacts.Services
{
    public sealed class AvatarService
    {
        public const int AvatarCount = 5;

        private readonly LocalStorageService _storage;
        private readonly ImageDownloader _downloader;
        private readonly AvatarAssignmentRepository _assignmentRepository;
        private readonly Texture2D _placeholderTexture;
        private readonly Dictionary<int, int> _assignments = new Dictionary<int, int>();

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

        public Texture2D PlaceholderTexture => _placeholderTexture;
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
            for (var index = 0; index < employees.Count; index++)
            {
                var employee = employees[index];
                if (!_assignments.TryGetValue(employee.id, out var imageId))
                {
                    imageId = previousImageId >= 0
                        ? (previousImageId + 1) % AvatarCount
                        : index % AvatarCount;
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

        public bool TryGetImageId(int employeeId, out int imageId)
        {
            return _assignments.TryGetValue(employeeId, out imageId);
        }

        // A newly created texture belongs to the caller and must be destroyed when its UI element is recycled.
        public Texture2D LoadAvatarTexture(int employeeId)
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
    }
}
