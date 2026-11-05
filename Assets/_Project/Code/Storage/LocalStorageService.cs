using System;
using System.IO;
using UnityEngine;

namespace EmployeeContacts.Storage
{
    public enum StorageFile
    {
        Employees,
        Favorites,
        EmployeeImageAssignments
    }

    public sealed class LocalStorageService
    {
        private const string EmployeesFileName = "employees.json";
        private const string FavoritesFileName = "favorites.json";
        private const string AssignmentsFileName = "employee_images.json";
        private const string ImagesDirectoryName = "images";
        private const string ImageFilePrefix = "image_";
        private const string ImageFileExtension = ".jpg";

        private readonly string _rootPath;

        public LocalStorageService(string rootPath = null)
        {
            _rootPath = string.IsNullOrWhiteSpace(rootPath) ? GetDefaultRootPath() : rootPath;
        }

        public string RootPath => _rootPath;

        public string GetImagePath(int imageId)
        {
            return Path.Combine(_rootPath, ImagesDirectoryName, ImageFilePrefix + imageId + ImageFileExtension);
        }

        public bool TryReadText(StorageFile file, out string content)
        {
            content = null;
            var path = GetPath(file);

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                content = File.ReadAllText(path);
                return !string.IsNullOrWhiteSpace(content);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not read local file '{path}': {exception.Message}");
                return false;
            }
        }

        public bool TryWriteText(StorageFile file, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                Debug.LogWarning($"Refused to save an empty '{file}' file.");
                return false;
            }

            var path = GetPath(file);
            try
            {
                EnsureDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, content);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not save local file '{path}': {exception.Message}");
                return false;
            }
        }

        public bool TryReadImage(int imageId, out byte[] bytes)
        {
            bytes = null;
            var path = GetImagePath(imageId);

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                bytes = File.ReadAllBytes(path);
                return bytes.Length > 0;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not read avatar '{path}': {exception.Message}");
                return false;
            }
        }

        public bool TryWriteImage(int imageId, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogWarning($"Refused to save an empty avatar with image ID {imageId}.");
                return false;
            }

            var path = GetImagePath(imageId);
            try
            {
                EnsureDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, bytes);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not save avatar '{path}': {exception.Message}");
                return false;
            }
        }

        private string GetPath(StorageFile file)
        {
            switch (file)
            {
                case StorageFile.Employees:
                    return Path.Combine(_rootPath, EmployeesFileName);
                case StorageFile.Favorites:
                    return Path.Combine(_rootPath, FavoritesFileName);
                case StorageFile.EmployeeImageAssignments:
                    return Path.Combine(_rootPath, AssignmentsFileName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(file), file, "Unknown storage file.");
            }
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string GetDefaultRootPath()
        {
#if UNITY_EDITOR
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "LocalData"));
#else
            return Application.persistentDataPath;
#endif
        }
    }
}
