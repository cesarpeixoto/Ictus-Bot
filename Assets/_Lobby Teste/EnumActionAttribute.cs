// Solução encontrada em https://forum.unity3d.com/threads/ability-to-add-enum-argument-to-button-functions.270817/, em 27/04/2017.

/// <summary>
/// Mark a method with an integer argument with this to display the argument as an enum popup in the UnityEvent
/// drawer. Use: [EnumAction(typeof(SomeEnumType))]
/// </summary>
/// 

using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class EnumActionAttribute : PropertyAttribute
{
    public Type enumType;

    public EnumActionAttribute(Type enumType)
    {
        this.enumType = enumType;
    }
}
