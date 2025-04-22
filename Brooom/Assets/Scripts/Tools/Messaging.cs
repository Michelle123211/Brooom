using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Supports messages with basic types: none, string, int, float, 
// TODO: Maybe add support for 2 parameters of the same type
public static class Messaging {
	//  Dictionaries for message names and actions (with different basic parameters)
	private static Dictionary<string, Action> messageNoParameters = new();
	private static Dictionary<string, Action<string>> messageStringParameter = new();
	private static Dictionary<string, Action<int>> messageIntParameter = new();
	private static Dictionary<string, Action<float>> messageFloatParameter = new();
	private static Dictionary<string, Action<bool>> messageBoolParameter = new();
	private static Dictionary<string, Action<GameObject>> messageGameObjectParameter = new();

	private static bool debugLogs = false; // whether debug messages should be logged to Debug

	#region Register for message
	// Methods for registering a callback on a message of the given name
	public static void RegisterForMessage(string messageName, Action messageCallback) { // No parameters 
		if (messageNoParameters.ContainsKey(messageName))
			messageNoParameters[messageName] = messageNoParameters[messageName] + messageCallback;
		else
			messageNoParameters[messageName] = messageCallback;
	}
	public static void RegisterForMessage(string messageName, Action<string> messageCallback) { // String parameter
		if (messageStringParameter.ContainsKey(messageName))
			messageStringParameter[messageName] = messageStringParameter[messageName] + messageCallback;
		else
			messageStringParameter[messageName] = messageCallback;
	}
	public static void RegisterForMessage(string messageName, Action<int> messageCallback) { // Int parameter
		if (messageIntParameter.ContainsKey(messageName))
			messageIntParameter[messageName] = messageIntParameter[messageName] + messageCallback;
		else
			messageIntParameter[messageName] = messageCallback;
	}
	public static void RegisterForMessage(string messageName, Action<float> messageCallback) { // Float parameter
		if (messageFloatParameter.ContainsKey(messageName))
			messageFloatParameter[messageName] = messageFloatParameter[messageName] + messageCallback;
		else
			messageFloatParameter[messageName] = messageCallback;
	}
	public static void RegisterForMessage(string messageName, Action<bool> messageCallback) { // Bool parameter
		if (messageBoolParameter.ContainsKey(messageName))
			messageBoolParameter[messageName] = messageBoolParameter[messageName] + messageCallback;
		else
			messageBoolParameter[messageName] = messageCallback;
	}
	public static void RegisterForMessage(string messageName, Action<GameObject> messageCallback) { // GameObject parameter
		if (messageGameObjectParameter.ContainsKey(messageName))
			messageGameObjectParameter[messageName] = messageGameObjectParameter[messageName] + messageCallback;
		else
			messageGameObjectParameter[messageName] = messageCallback;
	}
	#endregion

	#region Unregister from message
	// Methods for unregistering a callback on a message of the given name
	public static void UnregisterFromMessage(string messageName, Action messageCallback) { // No parameters 
		if (messageNoParameters.ContainsKey(messageName))
			messageNoParameters[messageName] = messageNoParameters[messageName] - messageCallback;
	}
	public static void UnregisterFromMessage(string messageName, Action<string> messageCallback) { // String parameter
		if (messageStringParameter.ContainsKey(messageName))
			messageStringParameter[messageName] = messageStringParameter[messageName] - messageCallback;
	}
	public static void UnregisterFromMessage(string messageName, Action<int> messageCallback) { // Int parameter
		if (messageIntParameter.ContainsKey(messageName))
			messageIntParameter[messageName] = messageIntParameter[messageName] - messageCallback;
	}
	public static void UnregisterFromMessage(string messageName, Action<float> messageCallback) { // Float parameter
		if (messageFloatParameter.ContainsKey(messageName))
			messageFloatParameter[messageName] = messageFloatParameter[messageName] - messageCallback;
	}
	public static void UnregisterFromMessage(string messageName, Action<bool> messageCallback) { // Bool parameter
		if (messageBoolParameter.ContainsKey(messageName))
			messageBoolParameter[messageName] = messageBoolParameter[messageName] - messageCallback;
	}
	public static void UnregisterFromMessage(string messageName, Action<GameObject> messageCallback) { // GameObject parameter
		if (messageGameObjectParameter.ContainsKey(messageName))
			messageGameObjectParameter[messageName] = messageGameObjectParameter[messageName] - messageCallback;
	}
	#endregion

	#region Send message
	//  Methods for sending a message (of the given name, with the given parameter)
	public static void SendMessage(string messageName) { // No parameters
		if (debugLogs)
			Debug.Log($"Message {messageName} sent.");
		if (messageNoParameters.ContainsKey(messageName))
			messageNoParameters[messageName]?.Invoke();
	}
	public static void SendMessage(string messageName, string messageParameter) { // String parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter}.");
		if (messageStringParameter.ContainsKey(messageName))
			messageStringParameter[messageName]?.Invoke(messageParameter);
	}
	public static void SendMessage(string messageName, int messageParameter) { // Int parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter}.");
		if (messageIntParameter.ContainsKey(messageName))
			messageIntParameter[messageName]?.Invoke(messageParameter);
	}
	public static void SendMessage(string messageName, float messageParameter) { // Float parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter}.");
		if (messageFloatParameter.ContainsKey(messageName))
			messageFloatParameter[messageName]?.Invoke(messageParameter);
	}
	public static void SendMessage(string messageName, bool messageParameter) { // Bool parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter}.");
		if (messageBoolParameter.ContainsKey(messageName))
			messageBoolParameter[messageName]?.Invoke(messageParameter);
	}
	public static void SendMessage(string messageName, GameObject messageParameter) { // GameObject parameter
		if (debugLogs)
			Debug.Log($"Message {messageName} sent with parameter {messageParameter.name}.");
		if (messageGameObjectParameter.ContainsKey(messageName))
			messageGameObjectParameter[messageName]?.Invoke(messageParameter);
	}
	#endregion
}
