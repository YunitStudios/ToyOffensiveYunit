using System;
using System.Collections.Generic;
using EditorAttributes;
using TMPro;
using UnityEngine;

public class LoadoutChooserUI : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private WeaponTypesSO weaponChoices;
    [SerializeField] private AttachmentTypesSO attachmentChoices;
    [SerializeField] private Transform choicesRoot;
    [SerializeField] private LoadoutChoiceUI choicePrefab;
    [SerializeField] private Popup popup;
    [SerializeField] private TMP_Text slotTypeText;
    [SerializeField] private TMP_Text activeLabel;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private SlotTypes slotType;
    public SlotTypes SlotType => slotType;

    private List<LoadoutChoiceUI> activeChoices = new();

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        List<ILoadout> allChoices = new();
        switch (slotType)
        {
            case SlotTypes.PrimaryWeapon or SlotTypes.SecondaryWeapon:
                allChoices = new List<ILoadout>(weaponChoices.WeaponTypes);
                break;
            case SlotTypes.PrimaryAttachment or SlotTypes.SecondaryAttachment:
                allChoices = new List<ILoadout>(attachmentChoices.AttachmentTypes);
                break;
        }
        foreach (var choice in allChoices)
            CreateChoicePrefab(choice);

        string slotTypeName = slotType switch
        {
            SlotTypes.PrimaryWeapon => "Primary Weapon",
            SlotTypes.SecondaryWeapon => "Secondary Weapon",
            SlotTypes.PrimaryAttachment => "Primary Attachment",
            SlotTypes.SecondaryAttachment => "Secondary Attachment",
            _ => slotType.ToString()
        };
        slotTypeText.text = slotTypeName;
        
        Refresh();
    }

    private void CreateChoicePrefab(ILoadout choice)
    {
        var choiceUI = Instantiate(choicePrefab, choicesRoot);
        choiceUI.SetupChoice(this, choice);
        activeChoices.Add(choiceUI);
    }

    public void Refresh()
    {
        string loadoutValueName = slotType switch
        {
            SlotTypes.PrimaryWeapon => GameManager.PlayerData.StartingPrimaryWeapon ? GameManager.PlayerData.StartingPrimaryWeapon.DisplayName : "None",
            SlotTypes.SecondaryWeapon => GameManager.PlayerData.StartingSecondaryWeapon ? GameManager.PlayerData.StartingSecondaryWeapon.DisplayName : "None",
            SlotTypes.PrimaryAttachment => GameManager.PlayerData.StartingPrimaryAttachment ? GameManager.PlayerData.StartingPrimaryAttachment.DisplayName : "None",
            SlotTypes.SecondaryAttachment => GameManager.PlayerData.StartingSecondaryAttachment ? GameManager.PlayerData.StartingSecondaryAttachment.DisplayName : "None",
            _ => "None"
        };
        
        foreach (var choice in activeChoices)
        {
            choice.ToggleSelected(!choice.IsLoadoutValueActive());
        }

        activeLabel.text = loadoutValueName; 

    }

    public void SetNewLoadoutValue(ILoadout loadoutValue, bool alreadySelected = false)
    {

        popup.Hide();

        // Dont change anything if this weapon is already selected
        if (alreadySelected)
            return;

        if (loadoutValue is WeaponDataSO weapon)
        {
            if (slotType == SlotTypes.PrimaryWeapon)
                GameManager.PlayerData.SetStartingPrimaryWeapon(weapon);
            else if (slotType == SlotTypes.SecondaryWeapon)
                GameManager.PlayerData.SetStartingSecondaryWeapon(weapon);
        }
        else if(loadoutValue is AttachmentDataSO attachment)
        {
            if (slotType == SlotTypes.PrimaryAttachment)
                GameManager.PlayerData.SetStartingPrimaryAttachment(attachment);
            else if (slotType == SlotTypes.SecondaryAttachment)
                GameManager.PlayerData.SetStartingSecondaryAttachment(attachment);
        }
        
        
        Refresh();
    }

    public enum SlotTypes
    {
        PrimaryWeapon,
        PrimaryAttachment,
        SecondaryWeapon,
        SecondaryAttachment
    }
}
