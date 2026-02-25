using System;
using System.Collections.Generic;
using EditorAttributes;
using TMPro;
using UnityEngine;

public class WeaponChooserUI : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private WeaponTypesSO allChoices;
    [SerializeField] private Transform choicesRoot;
    [SerializeField] private WeaponChoiceUI choicePrefab;
    [SerializeField] private Popup popup;
    [SerializeField] private TMP_Text slotTypeText;
    [SerializeField] private TMP_Text activeLabel;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private WeaponSlotTypes weaponSlotType;
    public WeaponSlotTypes WeaponSlotType => weaponSlotType;

    private List<WeaponChoiceUI> activeChoices = new();

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        foreach (var choice in allChoices.WeaponTypes)
        {
            var choiceUI = Instantiate(choicePrefab, choicesRoot);
            choiceUI.SetupChoice(this, choice);
            activeChoices.Add(choiceUI);
        }
        
        slotTypeText.text = weaponSlotType == WeaponSlotTypes.Primary ? "Primary" : "Secondary";
        
        Refresh();
    }

    public void Refresh()
    {
        // Get starting weapon
        var startingWeapon = weaponSlotType == WeaponSlotTypes.Primary ? GameManager.PlayerData.StartingPrimaryWeapon : GameManager.PlayerData.StartingSecondaryWeapon;

        foreach (var choice in activeChoices)
        {
            choice.ToggleSelected(GameManager.PlayerData.StartingPrimaryWeapon != choice.GetWeapon && GameManager.PlayerData.StartingSecondaryWeapon != choice.GetWeapon);
        }

        if (startingWeapon)
            activeLabel.text = startingWeapon.DisplayName;
        else
            activeLabel.text = "";

    }

    public void SetNewWeapon(WeaponDataSO weapon)
    {
        if(weaponSlotType == WeaponSlotTypes.Primary)
            GameManager.PlayerData.SetStartingPrimaryWeaponData(weapon, GameManager.PlayerData.PrimaryAttachments);
        else if(weaponSlotType == WeaponSlotTypes.Secondary)
            GameManager.PlayerData.SetStartingSecondaryWeaponData(weapon, GameManager.PlayerData.SecondaryAttachments);
        
        popup.Hide();
        
        Refresh();
    }

    public enum WeaponSlotTypes
    {
        Primary,
        Secondary
    }
}
