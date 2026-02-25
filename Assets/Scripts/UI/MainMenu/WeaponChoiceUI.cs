using EditorAttributes;
using TMPro;
using UnityEngine;

public class WeaponChoiceUI : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text weaponName;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private float offAlpha = 0.5f;

    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.white;

    private WeaponChooserUI mainScript;
    private WeaponDataSO currentWeapon;
    public WeaponDataSO GetWeapon => currentWeapon;

    public bool IsWeaponActive()
    {
        if (GameManager.PlayerData.StartingPrimaryWeapon == currentWeapon ||
            GameManager.PlayerData.StartingSecondaryWeapon == currentWeapon)
        {
            return true;
        }
        return false;
    }
    
    public void SetupChoice(WeaponChooserUI main, WeaponDataSO weapon)
    {
        mainScript = main;
        currentWeapon = weapon;

        weaponName.text = weapon.DisplayName;
    }
    
    public void ToggleSelected(bool value)
    {
        canvasGroup.alpha = value ? 1f : offAlpha;
        canvasGroup.interactable = value;
        weaponName.color = IsWeaponActive() ? activeColor : inactiveColor;
    }

    public void SelectChoice()
    {
        mainScript.SetNewWeapon(currentWeapon);
    }
}
