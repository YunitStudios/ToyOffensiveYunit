using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class InputTrigger : MonoBehaviour
{
    [SerializeField] private InputActionReference input;
    [SerializeField] private UnityEvent onInputDown;


    private void OnEnable()
    {
        input.action.performed += Trigger;
    }
    private void OnDisable()
    {
        input.action.performed -= Trigger;
    }

    private void Trigger(InputAction.CallbackContext callback)
    {
        print("trigger");
        onInputDown?.Invoke();
    }
}
