using System;
using System.Net;

namespace EmployeeContacts.Models
{
    [Serializable]
    public class EmployeeData
    {
        // JsonUtility maps public fields by their exact JSON names.
        public int id;
        public string first_name;
        public string last_name;
        public string email;
        public string gender;
        public string ip_address;

        public string FullName => $"{first_name} {last_name}";

        public bool IsValid()
        {
            return id > 0
                && !string.IsNullOrWhiteSpace(first_name)
                && !string.IsNullOrWhiteSpace(last_name)
                && !string.IsNullOrWhiteSpace(email)
                && email.Contains("@")
                && !string.IsNullOrWhiteSpace(gender)
                && IPAddress.TryParse(ip_address, out _);
        }
    }
}
