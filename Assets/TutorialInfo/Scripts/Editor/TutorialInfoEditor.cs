using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TutorialInfo))]
public class TutorialInfoEditor : Editor 
{
	void OnEnable()
	{

	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck ();

		base.OnInspectorGUI ();
	}
}
