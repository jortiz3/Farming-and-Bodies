using UnityEngine;
using System.Collections;

public class Destination : MonoBehaviour
{
	void OnTriggerEnter(Collider col)
	{
		if (col.tag.Equals ("Player")) 
		{
			global.gameManager.BroadcastActionCompleted("Go to " + transform.parent.name);
		}
	}
}
