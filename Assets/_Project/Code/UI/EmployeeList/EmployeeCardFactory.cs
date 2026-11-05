using UnityEngine;
namespace EmployeeContacts.UI.EmployeeList { public sealed class EmployeeCardFactory { private readonly EmployeeCardView prefab; private readonly Transform parent; public EmployeeCardFactory(EmployeeCardView value, Transform root) { prefab=value; parent=root; } public EmployeeCardView Create() => Object.Instantiate(prefab,parent); } }
