using System;

namespace _Project.Code.Models
{
    [Serializable]
    public class EmployeeImageAssignmentsData
    {
        public EmployeeImageAssignment[] assignments;
    }

    [Serializable]
    public class EmployeeImageAssignment
    {
        public int employeeId;
        public int imageId;
    }
}
