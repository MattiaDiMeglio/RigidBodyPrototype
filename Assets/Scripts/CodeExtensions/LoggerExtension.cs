using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Runtime.CompilerServices;

public static class LoggerExtension 
{
    private static void DoLog(Action<string, Object> LogFunction,  string prefix, Object myObject, params object[] message) 
    {
#if UNITY_EDITOR
        String objectName;
        switch (prefix){
            case "Error":
                objectName = $"<color=#E42217><b>{myObject.name}</b></color>";
                break;
            case "Warning":
                objectName = $"<color=Yellow><b>{myObject.name}</b></color>";
                break;
            case "Success":
                objectName = $"<color=Green><b>{myObject.name}</b></color>";
                break;
            default:
                objectName = $"<color=LightBlue><b>{myObject.name}</b></color>";
                break;
        }
        LogFunction($"{objectName}: {String.Join(", ", message)}\n", myObject);
#endif
    }

    /// <summary>
    /// New Log.
    /// It'll be called only in editor and not in production
    /// </summary>
    /// <param name="myObject">the calling object</param>
    /// <param name="message">the message or messages to send. Separate the message with ,</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(this Object myObject, params object[] message)
    {
#if UNITY_EDITOR
        DoLog(Debug.Log, "", myObject, message);
#endif
    }

    /// <summary>
    /// New LogError.
    /// It'll be called only in editor and not in production
    /// </summary>
    /// <param name="myObject">the calling object</param>
    /// <param name="message">the message or messages to send. Separate the message with ,</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(this Object myObject, params object[] message)
    {
#if UNITY_EDITOR
        DoLog(Debug.LogError, "Error", myObject, message);
#endif
    }

    /// <summary>
    /// New LogWarning.
    /// It'll be called only in editor and not in production
    /// </summary>
    /// <param name="myObject">the calling object</param>
    /// <param name="message">the message or messages to send. Separate the message with ,</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(this Object myObject, params object[] message)
    {
#if UNITY_EDITOR
        DoLog(Debug.LogWarning, "Warning", myObject, message);
#endif
    }

    /// <summary>
    /// New Log.
    /// To be used in succesful operation logs.
    /// It'll be called only in editor and not in production
    /// </summary>
    /// <param name="myObject">the calling object</param>
    /// <param name="message">the message or messages to send. Separate the message with ,</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogSuccess(this Object myObject, params object[] message)
    {
#if UNITY_EDITOR
        DoLog(Debug.Log, "Success", myObject, message);
#endif
    }
}
