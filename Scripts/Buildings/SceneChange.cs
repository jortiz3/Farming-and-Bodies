using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneChange : MonoBehaviour {

	public string objectOrSceneName;
	public string interactionText;
	public bool isAnObjectInCurrentScene;
	[Tooltip("This should be true if the camera should remain in a key location and look at the player")]
	public bool staticCamera;
	public Vector3 staticLocation;
	public string[] eventsToTrigger;

	private bool canInteract;

	void Update ()
	{
		if (canInteract && Input.GetAxisRaw("Action") == 1)
		{
			if (Manager.currInteraction != null && Manager.currInteraction.Equals(gameObject))
			{
				ChangeScene();
			}
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			canInteract = true;
			global.gameManager.DisplayInteraction(gameObject, interactionText);
		}
	}

	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			canInteract = false;
			global.gameManager.RemoveInteraction(gameObject);
		}
	}

	public void ChangeScene () {
		if (!isAnObjectInCurrentScene)
		{
			GameObject temp = new GameObject();
			temp.AddComponent<SaveLoadScript>();
			temp.GetComponent<SaveLoadScript>().LoadScene(objectOrSceneName, eventsToTrigger);
		}
		else
		{
			List<string> events = new List<string>();
			events.AddRange(eventsToTrigger);
			events.Add("place Player " + objectOrSceneName.Replace(" ", "_"));
			if (staticCamera)
				events.Add("setcamstatic " + staticLocation.x + "," + staticLocation.y + "," + staticLocation.z);
			else
				events.Add("setcamstatic false");
			global.playerScript.FadeToBlack(events.ToArray(), null);
		}
	}
}
