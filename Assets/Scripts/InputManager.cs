using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public enum InputDevice { Joystick, keyboardMouse }
    public InputDevice inputDevice = InputDevice.Joystick;
    public float mouseSensitivity = 1.0f;
    public bool useMouse = false;
    public delegate float AxisHandle();
    public delegate bool ButtonHandle();

    // Input Callbacks
    public static AxisHandle GetPlayerHorizontalAxis;
    public static AxisHandle GetPlayerVerticalAxis;
    public static AxisHandle GetSelectorHorizontalAxis;
    public static AxisHandle GetSelectorVerticalAxis;
    public static ButtonHandle GetActionTrigger;
    public static ButtonHandle GetCancelTrigger;

    private static InputManager _instance = null;


    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        _instance = this;
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

            if (_instance.useMouse)
            {
                GetSelectorHorizontalAxis = () => { return Input.GetAxis("Mouse X") * _instance.mouseSensitivity; };
                GetSelectorVerticalAxis = () => { return Input.GetAxis("Mouse Y") * _instance.mouseSensitivity; };
                GetActionTrigger = () => { return Input.GetMouseButtonDown(1); };
                GetCancelTrigger = () => { return Input.GetMouseButtonDown(0); };
            }
            else
            {
                GetSelectorHorizontalAxis = () => { return Input.GetAxis("Selector Horizontal"); };
                GetSelectorVerticalAxis = () => { return Input.GetAxis("Selector Vertical"); };
                GetActionTrigger = () => { return Input.GetAxis("Selector Trigger") < 0.0f; };
                GetCancelTrigger = () => { return Input.GetAxis("Selector Trigger") > 0.0f; };
            }

            
        }
    }

}
