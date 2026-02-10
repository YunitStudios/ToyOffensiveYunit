using EditorAttributes;
using UnityEngine;
using TMPro;

public class AmmoDisplay : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private TMP_Text currentAmmoObj;
    [SerializeField] private TMP_Text maxAmmoObj;

    //[Title("\n<b><color=#ffd180>Attributes", 15, 5, false)]


    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)]
    [SerializeField] private VoidEventChannelSO onAmmoChanged;

    private void OnEnable()
    {
        onAmmoChanged.OnEventRaised += UpdateDisplay;
    }

    private void OnDisable()
    {
        onAmmoChanged.OnEventRaised -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        currentAmmoObj.text = ""+GameManager.PlayerData.PrimaryWeapon.CurrentAmmoInMag;
        maxAmmoObj.text = "" + GameManager.PlayerData.NormalAmmoCount;
    }


}
