using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseInspector : Editor
{
	private ItemDatabase id;

	void OnEnable()
	{
		id = (ItemDatabase)target;
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI();

		if (GUI.changed)
		{
			for (int i = 0; i < id.items.Length; i++)
				id.items[i].DatabaseID = i;
			EditorUtility.SetDirty(id);
		}
	}

	//this makes it so that when play is pressed, it starts from the menu scene
	[MenuItem("Edit/Play-Stop, But From Prelaunch Scene %0")]
	public static void PlayFromPrelaunchScene()
	{
		if ( EditorApplication.isPlaying == true )
		{
			EditorApplication.isPlaying = false;
			return;
		}
		
		EditorApplication.SaveCurrentSceneIfUserWantsTo();
		EditorApplication.OpenScene("Assets/Scenes/MainMenu.unity");
		EditorApplication.isPlaying = true;
	}
}
