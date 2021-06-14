using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GUIStack {

    public static GUIStackImplementation<bool> Enabled => GUIEnabledStack.Instance;
    public static GUIStackImplementation<Color> Color => GUIColorStack.Instance;
    public static GUIStackImplementation<Color> ContentColor => GUIContentColorStack.Instance;
    public static GUIStackImplementation<Color> BackgroundColor => GUIBackgroundColorStack.Instance;
    public static GUIStackImplementation<int> IndentLevel => GUIIndentStack.Instance;
    
}

public abstract class GUIStackImplementation<T> {

    private Stack<T> stack;

    protected GUIStackImplementation () {
        stack = new Stack<T>();
    }

    protected abstract void SetValue (T t);
    protected abstract T GetValue ();

    public void Push (T t) {
        stack.Push(GetValue());
        SetValue(t);
    }

    public T Pop () {
        var value = stack.Pop();
        SetValue(value);
        return value;
    }

}

public class GUIEnabledStack : GUIStackImplementation<bool> {
    
    private static GUIEnabledStack _instance;
    public static GUIStackImplementation<bool> Instance { get {
        if(_instance == null) _instance = new GUIEnabledStack();
        return _instance;
    } } 

    protected override void SetValue (bool t) => GUI.enabled = t;
    protected override bool GetValue () => GUI.enabled;

}

public class GUIColorStack : GUIStackImplementation<Color> {

    private static GUIColorStack _instance;
    public static GUIStackImplementation<Color> Instance { get {
        if(_instance == null) _instance = new GUIColorStack();
        return _instance;
    } } 
    
    protected override void SetValue (Color t) => GUI.color = t;
    protected override Color GetValue () => GUI.color;

}

public class GUIContentColorStack : GUIStackImplementation<Color> {

    private static GUIContentColorStack _instance;
    public static GUIStackImplementation<Color> Instance { get {
        if(_instance == null) _instance = new GUIContentColorStack();
        return _instance;
    } } 
    
    protected override void SetValue (Color t) => GUI.contentColor = t;
    protected override Color GetValue () => GUI.contentColor;

}

public class GUIBackgroundColorStack : GUIStackImplementation<Color> {

    private static GUIBackgroundColorStack _instance;
    public static GUIStackImplementation<Color> Instance { get {
        if(_instance == null) _instance = new GUIBackgroundColorStack();
        return _instance;
    } } 
    
    protected override void SetValue (Color t) => GUI.backgroundColor = t;
    protected override Color GetValue () => GUI.backgroundColor;

}

public class GUIIndentStack : GUIStackImplementation<int> {

    private static GUIIndentStack _instance;
    public static GUIStackImplementation<int> Instance { get {
        if(_instance == null) _instance = new GUIIndentStack();
        return _instance;
    } } 
    
    protected override void SetValue (int t) => EditorGUI.indentLevel = t;
    protected override int GetValue () => EditorGUI.indentLevel;

}