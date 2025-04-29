using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// TODO: Maybe add support for 2 parameters of the same type
/// <summary>
/// This static class can be used for sending messages between objects without direct references.
/// Anyone can register a callback for a specific message and then someone else can send this message to invoke all registered callbacks.
/// Right now, messages with no parameters or a single parameter (<c>string</c>, <c>int</c>, <c>float</c>, <c>bool</c> or <c>GameObject</c>) are supported.
/// </summary>
public static class Messaging {

	//  Dictionaries for message names and callbacks (with different basic parameters)
	private static Dictionary<string, Action> messageNoParameters = new();
	private static Dictionary<string, Action<string>> messageStringParameter = new();
	private static Dictionary<string, Action<int>> messageIntParameter = new();
	private static Dictionary<string, Action<float>> messageFloatParameter = new();
	private static Dictionary<string, Action<bool>> messageBoolParameter = new();
	private static Dictionary<string, Action<GameObject>> messageGameObjectParameter = new();

	private static bool debugLogs = false; // whether debug messages should be logged to Debug

	#region Register for message
	/// <summary>
	/// Registers the given callback (with no parameters) for a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to register for.</param>
	/// <param name="messageCallback">Callback to register.</param>
	public static void RegisterForMessage(string messageName, Action messageCallback) { // No parameters 
		if (messageNoParameters.ContainsKey(messageName))
			messageNoParameters[messageName] = messageNoParameters[messageName] + messageCallback;
		else
			messageNoParameters[messageName] = messageCallback;
	}
	/// <summary>
	/// Registers the given callback (with a <c>string</c> parameter) for a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to register for.</param>
	/// <param name="messageCallback">Callback to register.</param>
	public static void RegisterForMessage(string messageName, Action<string> messageCallback) { // String parameter
		if (messageStringParameter.ContainsKey(messageName))
			messageStringParameter[messageName] = messageStringParameter[messageName] + messageCallback;
		else
			messageStringParameter[messageName] = messageCallback;
	}
	/// <summary>
	/// Registers the given callback (with a <c>int</c> parameter) for a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to register for.</param>
	/// <param name="messageCallback">Callback to register.</param>
	public static void RegisterForMessage(string messageName, Action<int> messageCallback) { // Int parameter
		if (messageIntParameter.ContainsKey(messageName))
			messageIntParameter[messageName] = messageIntParameter[messageName] + messageCallback;
		else
			messageIntParameter[messageName] = messageCallback;
	}
	/// <summary>
	/// Registers the given callback (with a <c>float</c> parameter) for a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to register for.</param>
	/// <param name="messageCallback">Callback to register.</param>
	public static void RegisterForMessage(string messageName, Action<float> messageCallback) { // Float parameter
		if (messageFloatParameter.ContainsKey(messageName))
			messageFloatParameter[messageName] = messageFloatParameter[messageName] + messageCallback;
		else
			messageFloatParameter[messageName] = messageCallback;
	}
	/// <summary>
	/// Registers the given callback (with a <c>bool</c> parameter) for a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to register for.</param>
	/// <param name="messageCallback">Callback to register.</param>
	public static void RegisterForMessage(string messageName, Action<bool> messageCallback) { // Bool parameter
		if (messageBoolParameter.ContainsKey(messageName))
			messageBoolParameter[messageName] = messageBoolParameter[messageName] + messageCallback;
		else
			messageBoolParameter[messageName] = messageCallback;
	}
	/// <summary>
	/// Registers the given callback (with a <c>GameObject</c> parameter) for a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to register for.</param>
	/// <param name="messageCallback">Callback to register.</param>
	public static void RegisterForMessage(string messageName, Action<GameObject> messageCallback) { // GameObject parameter
		if (messageGameObjectParameter.ContainsKey(messageName))
			messageGameObjectParameter[messageName] = messageGameObjectParameter[messageName] + messageCallback;
		else
			messageGameObjectParameter[messageName] = messageCallback;
	}
	#endregion

	#region Unregister from message
	/// <summary>
	/// Unregisters the given callback (with no parameters) from a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to unregister from.</param>
	/// <param name="messageCallback">Callback to unregister.</param>
	public static void UnregisterFromMessage(string messageName, Action messageCallback) { // No parameters 
		if (messageNoParameters.ContainsKey(messageName))
			messageNoParameters[messageName] = messageNoParameters[messageName] - messageCallback;
	}
	/// <summary>
	/// Unregisters the given callback (with <c>string</c> parameter) from a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to unregister from.</param>
	/// <param name="messageCallback">Callback to unregister.</param>
	public static void UnregisterFromMessage(string messageName, Action<string> messageCallback) { // String parameter
		if (messageStringParameter.ContainsKey(messageName))
			messageStringParameter[messageName] = messageStringParameter[messageName] - messageCallback;
	}
	/// <summary>
	/// Unregisters the given callback (with <c>int</c> parameter) from a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to unregister from.</param>
	/// <param name="messageCallback">Callback to unregister.</param>
	public static void UnregisterFromMessage(string messageName, Action<int> messageCallback) { // Int parameter
		if (messageIntParameter.ContainsKey(messageName))
			messageIntParameter[messageName] = messageIntParameter[messageName] - messageCallback;
	}
	/// <summary>
	/// Unregisters the given callback (with <c>float</c> parameter) from a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to unregister from.</param>
	/// <param name="messageCallback">Callback to unregister.</param>
	public static void UnregisterFromMessage(string messageName, Action<float> messageCallback) { // Float parameter
		if (messageFloatParameter.ContainsKey(messageName))
			messageFloatParameter[messageName] = messageFloatParameter[messageName] - messageCallback;
	}
	/// <summary>
	/// Unregisters the given callback (with <c>bool</c> parameter) from a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to unregister from.</param>
	/// <param name="messageCallback">Callback to unregister.</param>
	public static void UnregisterFromMessage(string messageName, Action<bool> messageCallback) { // Bool parameter
		if (messageBoolParameter.ContainsKey(messageName))
			messageBoolParameter[messageName] = messageBoolParameter[messageName] - messageCallback;
	}
	/// <summary>
	/// Unregisters the given callback (with <c>GameObject</c> parameter) from a message of the given name.
	/// </summary>
	/// <param name="messageName">Name of the message to unregister from.</param>
	/// <param name="messageCallback">Callback to unregister.</param>
	public static void UnregisterFromMessage(string messageName, Action<GameObject> messageCallback) { // GameObject parameter
		if (messageGameObjectParameter.ContainsKey(messageName))
			messageGameObjectParameter[messageName] = messageGameObjectParameter[messageName] - messageCallback;
	}
	#endregion

	#region Send message
	/// <summary>
	/// Sends a message of the given name (with no parameters) which invokes all callbacks registered for it.
	/// </summary>
	/// <param name="messageName">Name of the message to be sent.</param>
	public static void SendMessage(string messageName) { // No parameters
		if (debugLogs)
			Debug.Log($"Message {messageName} sent.");
		if (messageNoParameters.ContainsKey(messageName))
			messageNoParameters[messageName]?.Invoke();
	}
	/// <summary>
	/// Sends a message of the given name with the given <c>string</c> parameter which invokes all callbacks registered for it.
	/// </summary>
	/// <param name="messageName">Name of the message to be sent.</param>
	/// <param name="messageParameter">Parameter attached to the message which will be passed into callbacks.</param>
	public static void SendMessage(string messageName, string messageParameter) { // String parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter}.");
		if (messageStringParameter.ContainsKey(messageName))
			messageStringParameter[messageName]?.Invoke(messageParameter);
	}
	/// <summary>
	/// Sends a message of the given name with the given <c>int</c> parameter which invokes all callbacks registered for it.
	/// </summary>
	/// <param name="messageName">Name of the message to be sent.</param>
	/// <param name="messageParameter">Parameter attached to the message which will be passed into callbacks.</param>
	public static void SendMessage(string messageName, int messageParameter) { // Int parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter}.");
		if (messageIntParameter.ContainsKey(messageName))
			messageIntParameter[messageName]?.Invoke(messageParameter);
	}
	/// <summary>
	/// Sends a message of the given name with the given <c>float</c> parameter which invokes all callbacks registered for it.
	/// </summary>
	/// <param name="messageName">Name of the message to be sent.</param>
	/// <param name="messageParameter">Parameter attached to the message which will be passed into callbacks.</param>
	public static void SendMessage(string messageName, float messageParameter) { // Float parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter}.");
		if (messageFloatParameter.ContainsKey(messageName))
			messageFloatParameter[messageName]?.Invoke(messageParameter);
	}
	/// <summary>
	/// Sends a message of the given name with the given <c>bool</c> parameter which invokes all callbacks registered for it.
	/// </summary>
	/// <param name="messageName">Name of the message to be sent.</param>
	/// <param name="messageParameter">Parameter attached to the message which will be passed into callbacks.</param>
	public static void SendMessage(string messageName, bool messageParameter) { // Bool parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter}.");
		if (messageBoolParameter.ContainsKey(messageName))
			messageBoolParameter[messageName]?.Invoke(messageParameter);
	}
	/// <summary>
	/// Sends a message of the given name with the given <c>GameObject</c> parameter which invokes all callbacks registered for it.
	/// </summary>
	/// <param name="messageName">Name of the message to be sent.</param>
	/// <param name="messageParameter">Parameter attached to the message which will be passed into callbacks.</param>
	public static void SendMessage(string messageName, GameObject messageParameter) { // GameObject parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter.name}.");
		if (messageGameObjectParameter.ContainsKey(messageName))
			messageGameObjectParameter[messageName]?.Invoke(messageParameter);
	}
	#endregion
}
