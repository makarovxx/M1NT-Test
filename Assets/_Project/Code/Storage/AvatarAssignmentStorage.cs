using System;
using System.Collections.Generic;
using _Project.Code.Models;
using UnityEngine;

namespace _Project.Code.Storage
{
    public sealed class AvatarAssignmentStorage
    {
        private readonly LocalStorage _storage;

        public AvatarAssignmentStorage(LocalStorage storage)
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
                    if (assignment is { employeeId: > 0 } && assignment.imageId >= 0)
                    {
                        assignments[assignment.employeeId] = assignment.imageId;
                    }
                }

                return assignments;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not deserialize local avatar assignments: {e.Message}");
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
