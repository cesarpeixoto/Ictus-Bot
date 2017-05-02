using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MenuItensBase<T> where T : struct, System.IConvertible
{
    protected int _enumLength = 0;
    protected Transform _rootTransform;
    protected GameObject[] _windows = null;
    protected int _currentWindowIndex = 0;
    protected int _previousWindowIndex = 0;

    public MenuItensBase(Transform transform)
    {
        if (!typeof(T).IsEnum)
            Debug.LogError("T precisa ser do tipo System.Enum, para configuração dos controles de Menu ou SubMenu");

        var enumValues = (T[])Enum.GetValues(typeof(T));
        _enumLength = enumValues.Length;
        _rootTransform = transform;
        RegisterWindowsInSystem();
    }

    public virtual void CloseRootMenu()
    {
        _rootTransform.gameObject.SetActive(false);
        _currentWindowIndex = 0;
        _previousWindowIndex = 0;
    }

    public virtual void OpenRootMenu()
    {
        _rootTransform.gameObject.SetActive(true);
    }


    public virtual void ChangeWindowTo(T newWindow)
    {
        int newWindowIndex = Convert.ToInt32(newWindow);
        if (_windows[_currentWindowIndex] != null)
        {
            _windows[_currentWindowIndex].gameObject.SetActive(false);
            _previousWindowIndex = _currentWindowIndex;
        }

        if (_windows[newWindowIndex] != null)
        {
            _windows[newWindowIndex].gameObject.SetActive(true);
            _currentWindowIndex = newWindowIndex;
        }
    }

    public virtual void ReturnWindow()
    {
        if(_currentWindowIndex > 0)
        {
            int newWindowIndex = _previousWindowIndex;
            if (_windows[_currentWindowIndex] != null)
            {
                _windows[_currentWindowIndex].gameObject.SetActive(false);
                _previousWindowIndex = _currentWindowIndex;
            }

            if (_windows[newWindowIndex] != null)
            {
                _windows[newWindowIndex].gameObject.SetActive(true);
                _currentWindowIndex = newWindowIndex;
            }
        }
    }

    protected virtual void RegisterWindowsInSystem()
    {
        _windows = new GameObject[_enumLength];
        var enumValues = (T[])Enum.GetValues(typeof(T));

        for (int i = 0; i < _enumLength; i++)
        {
            string name = enumValues[i].ToString();
            _windows[i] = GetChildrenByName(name);
            //Debug.Log("GameObject: " + _windows[i] + " Name: " + name + " Index: " + i);
        }
    }

    protected virtual GameObject GetChildrenByName(string name)
    {
        for (int i = 0; i < _rootTransform.childCount; i++)
        {
            if (_rootTransform.GetChild(i).name == name)
                return _rootTransform.GetChild(i).gameObject;
        }
        return null;
    }


}
