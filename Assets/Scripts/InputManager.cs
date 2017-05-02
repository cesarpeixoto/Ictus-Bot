using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public enum InputDevice { Joystick, keyboardMouse }
    public InputDevice inputDevice = InputDevice.Joystick;
    public float mouseSensitivity = 1.0f;
    public delegate float AxisHandle();
    public delegate bool ButtonHandle();

    // Input Callbacks
    public static AxisHandle GetPlayerHorizontalAxis;
    public static AxisHandle GetPlayerVerticalAxis;
    public static AxisHandle GetSelectorHorizontalAxis;
    public static AxisHandle GetSelectorVerticalAxis;
    public static ButtonHandle GetActionTrigger;
    public static ButtonHandle GetCancelTrigger;

    private static float _mouseSensitivity = 1.0f;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        _mouseSensitivity = mouseSensitivity;
        SetInputDevice(inputDevice);
    }

    public static void SetInputDevice(InputDevice controller)
    {
        if(controller == InputDevice.Joystick)
        {
            GetPlayerHorizontalAxis = ()=> { return Input.GetAxis("Xbox LHorizontal"); };
            GetPlayerVerticalAxis = ()=> { return Input.GetAxis("Xbox LVertical"); };
            GetSelectorHorizontalAxis = ()=> { return Input.GetAxis("Xbox RHorizontal"); };
            GetSelectorVerticalAxis = () => { return Input.GetAxis("Xbox RVertical"); };
            GetActionTrigger = () => { return Input.GetAxis("Xbox RLTrigger") < 0.0f; };
            GetCancelTrigger = () => { return Input.GetAxis("Xbox RLTrigger") > 0.0f; };
        }
        else
        {
            GetPlayerHorizontalAxis = () => { return Input.GetAxis("Horizontal"); };
            GetPlayerVerticalAxis = () => { return Input.GetAxis("Vertical"); };
            GetSelectorHorizontalAxis = () => { return Input.GetAxis("Mouse X") * _mouseSensitivity; };
            GetSelectorVerticalAxis = () => { return Input.GetAxis("Mouse Y") * _mouseSensitivity; };
            GetActionTrigger = () => { return Input.GetMouseButtonDown(1); };
            GetCancelTrigger = () => { return Input.GetMouseButtonDown(0); };
        }
    }

}
