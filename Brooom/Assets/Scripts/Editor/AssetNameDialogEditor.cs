using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


// A modal dialog for selecting an asset name when creating it
public class AssetNameDialogEditor : EditorWindow {

	private string assetName;
	private bool shouldClose = false;
	private string errorMessage = "";

	private Action onOKButton;

	// Shows the dialog (modal) and returns the chosen name when it is closed
	public static string ShowDialog() {
		string chosenName = null;

		AssetNameDialogEditor window = CreateInstance<AssetNameDialogEditor>();
		window.titleContent = new GUIContent("Create asset");
		window.assetName = "";
		window.onOKButton += () => chosenName = window.assetName;
		window.ShowModal();

		return chosenName;
	}

	private void OnGUI() {
		if (shouldClose)
			Close();
		Rect rect = DrawControl();
		AdjustWindowSize(rect);
	}

	private Rect DrawControl() {
		var rect = EditorGUILayout.BeginVertical();

		// Fields for name
		EditorGUILayout.LabelField("Please enter a name:");
		GUI.SetNextControlName("inName");
		assetName = EditorGUILayout.TextField("", assetName);
		GUI.FocusControl("inName");
		EditorGUILayout.LabelField(errorMessage);
		EditorGUILayout.Space(3);

		// Buttons
		DrawButtons();
		EditorGUILayout.Space(5);

		EditorGUILayout.EndVertical();

		return rect;
	}


	private void DrawButtons() {
		// Draw OK and Cancel buttons side by side
		Rect rect = EditorGUILayout.GetControlRect();
		rect.width /= 2;
		if (GUI.Button(rect, "OK")) {
			if (!string.IsNullOrEmpty(assetName)) {
				onOKButton?.Invoke();
				errorMessage = "";
				shouldClose = true;
			} else {
				errorMessage = "The name cannot be empty.";
			}
		}
		rect.x += rect.width;
		if (GUI.Button(rect, "Cancel")) {
			assetName = null;
			shouldClose = true;
		}
	}

	private void AdjustWindowSize(Rect rect) {
		// Adjust size to the content
		if (rect.width != 0 && minSize != rect.size)
			minSize = maxSize = rect.size;
	}

}
