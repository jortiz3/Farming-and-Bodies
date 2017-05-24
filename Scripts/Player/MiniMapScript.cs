using UnityEngine;
using System.Collections;

public class MiniMapScript : MonoBehaviour {

	private Camera cam;

	void Awake()
	{
		cam = GetComponent<Camera> ();
	}

	void OnGUI()
	{
		if (Camera.main.GetComponent<PlayerCamera>().currentCutScene != null)
		{
			cam.enabled = false;
			return;
		}

		cam.enabled = true;
		GUI.Box(new Rect(cam.pixelRect.x, (Screen.height - cam.pixelRect.yMax), cam.pixelWidth, cam.pixelHeight), "") ;
	}
}
