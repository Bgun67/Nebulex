using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponsCatalogScriptableObject", order = 1)]

public class Weapons_Catalog : ScriptableObject
{
    public GameObject[] guns;
    public GameObject[] scopes;
}
