using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class LoadoutChoiceUI : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text loadoutName;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private float offAlpha = 0.5f;

    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.white;

    private LoadoutChooserUI mainScript;
    private SerializableInterface<ILoadout> currentLoadoutValue = new();
    public ILoadout CurrentLoadoutValue => currentLoadoutValue.Instance;

    public bool IsLoadoutValueActive()
    {
        if (CurrentLoadoutValue is WeaponDataSO)
            return IsWeaponActive();
        else if (CurrentLoadoutValue is AttachmentDataSO)
            return IsAttachmentActive();

        return false;
    }
    
    private bool IsWeaponActive()
    {
        if (GameManager.PlayerData.StartingPrimaryWeapon == (WeaponDataSO)CurrentLoadoutValue ||
            GameManager.PlayerData.StartingSecondaryWeapon == (WeaponDataSO)CurrentLoadoutValue)
        {
            return true;
        }
        return false;
    }
    private bool IsAttachmentActive()
    {
        if (mainScript.SlotType == LoadoutChooserUI.SlotTypes.PrimaryAttachment &&
            GameManager.PlayerData.StartingPrimaryWeapon != null && 
            GameManager.PlayerData.StartingPrimaryAttachment == (AttachmentDataSO)CurrentLoadoutValue)
        {
            return true;
        }
        if (mainScript.SlotType == LoadoutChooserUI.SlotTypes.SecondaryAttachment && 
            GameManager.PlayerData.StartingSecondaryWeapon != null && 
            GameManager.PlayerData.StartingSecondaryAttachment == (AttachmentDataSO)CurrentLoadoutValue)
        {
            return true;
        }
        return false;
    }
    
    public void SetupChoice(LoadoutChooserUI main, ILoadout loadoutValue) 
    {
        mainScript = main;
        currentLoadoutValue.Instance = loadoutValue;

        loadoutName.text = loadoutValue.GetDisplayName;
    }
    
    public void ToggleSelected(bool value)
    {
        canvasGroup.alpha = value ? 1f : offAlpha;
        canvasGroup.interactable = true;
        loadoutName.color = IsLoadoutValueActive() ? activeColor : inactiveColor;
    }

    public void SelectChoice()
    {
        mainScript.SetNewLoadoutValue(CurrentLoadoutValue, IsLoadoutValueActive()); 
    }
}
