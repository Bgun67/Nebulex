using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loadout Properties", menuName = "ScriptableObjects/Loadout Properties", order = 4)]
public class LoadoutProperties : ScriptableObject
{

    [field: SerializeField] public List<WeaponProperties> PrimaryWeapons { get; private set; } = new List<WeaponProperties>();
    [field: SerializeField] public List<WeaponProperties> SecondaryWeapons { get; private set; } = new List<WeaponProperties>();
    [field: SerializeField] public List<WeaponProperties> TertiaryWeapons { get; private set; } = new List<WeaponProperties>();
    [field: SerializeField] public List<string> Scopes { get; private set; } = new List<string>();
    
}
