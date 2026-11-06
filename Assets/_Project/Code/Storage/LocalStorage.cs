using System;
using System.IO;
using UnityEngine;

namespace _Project.Code.Storage
{
    public sealed class LocalStorage
    {
        private const string EmployeesFile = "employees.json";
        private const string FavoritesFile = "favorites.json";
        private const string AssignmentsFile = "employee_images.json";
        private const string ImagesDirectory = "images";
        private const string ImageFilePrefix = "image_";
        private const string ImageFileExtension = ".jpg";
        private readonly string _rootPath;

        public LocalStorage(string rootPath = null)
        {
            _rootPath = string.IsNullOrWhiteSpace(rootPath) ? GetDefaultRootPath() : rootPath;
        }

        public bool TryReadText(StorageFile file, out string content)
        {
            content = null;

            var path = GetPath(file);

            if (!File.Exists(path))
                return false;

            try
            {
                content = File.ReadAllText(path);
                return !string.IsNullOrWhiteSpace(content);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to read file '{path}': {exception.Message}");
                return false;
            }
        }

        public bool TryWriteText(StorageFile file, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            var path = GetPath(file);

            try
            {
                Directory.CreateDirectory(_rootPath);
                File.WriteAllText(path, content);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save file '{path}': {exception.Message}");
                return false;
            }
        }

        public bool TryReadImage(int imageId, out byte[] bytes)
        {
            bytes = null;

            var path = GetImagePath(imageId);

            if (!File.Exists(path))
                return false;

            try
            {
                bytes = File.ReadAllBytes(path);
                return bytes.Length > 0;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to read image {imageId}: {exception.Message}");
                return false;
            }
        }

        public bool TryWriteImage(int imageId, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return false;

            var path = GetImagePath(imageId);

            try
            {
                Directory.CreateDirectory(Path.Combine(_rootPath, ImagesDirectory));
                File.WriteAllBytes(path, bytes);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save image {imageId}: {exception.Message}");
                return false;
            }
        }

        private string GetPath(StorageFile file)
        {
            string fileName;
            switch (file)
            {
                case StorageFile.Employees:
                    fileName = EmployeesFile;
                    break;
                case StorageFile.Favorites:
                    fileName = FavoritesFile;
                    break;
                case StorageFile.EmployeeImageAssignments:
                    fileName = AssignmentsFile;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(file));
            }

            return Path.Combine(_rootPath, fileName);
        }

        private string GetImagePath(int imageId)
        {
            return Path.Combine(_rootPath, ImagesDirectory, $"{ImageFilePrefix}{imageId}{ImageFileExtension}");
        }

        private string GetDefaultRootPath()
        {
#if UNITY_EDITOR
            return Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "LocalData"));
#else
            return Application.persistentDataPath;
#endif
        }
    }
}