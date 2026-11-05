using System;
using System.Collections.Generic;
using EmployeeContacts.Models;
using UnityEngine;

namespace EmployeeContacts.Storage
{
    public sealed class AvatarAssignmentRepository
    {
        private readonly LocalStorageService _storage;

        public AvatarAssignmentRepository(LocalStorageService storage)
        {
            _storage = storage;
        }

        public Dictionary<int, int> Load()
        {
            if (!_storage.TryReadText(StorageFile.EmployeeImageAssignments, out var json))
            {
                return new Dictionary<int, int>();
            }

            try
            {
                var data = JsonUtility.FromJson<EmployeeImageAssignmentsData>(json);
                var assignments = new Dictionary<int, int>();
                if (data?.assignments == null)
                {
                    return assignments;
                }

                foreach (var assignment in data.assignments)
                {
                    if (assignment != null && assignment.employeeId > 0 && assignment.imageId >= 0)
                    {
                        assignments[assignment.employeeId] = assignment.imageId;
                    }
                }

                return assignments;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not deserialize local avatar assignments: {exception.Message}");
                return new Dictionary<int, int>();
            }
        }

        public bool Save(Dictionary<int, int> assignments)
        {
            var employeeIds = new List<int>(assignments.Keys);
            employeeIds.Sort();
            var serializedAssignments = new EmployeeImageAssignment[employeeIds.Count];

            for (var index = 0; index < employeeIds.Count; index++)
            {
                var employeeId = employeeIds[index];
                serializedAssignments[index] = new EmployeeImageAssignment
                {
                    employeeId = employeeId,
                    imageId = assignments[employeeId]
                };
            }

            var json = JsonUtility.ToJson(new EmployeeImageAssignmentsData
            {
                assignments = serializedAssignments
            }, true);
            return _storage.TryWriteText(StorageFile.EmployeeImageAssignments, json);
        }
    }
}
