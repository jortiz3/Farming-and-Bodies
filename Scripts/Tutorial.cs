using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tutorial : MonoBehaviour {

	public string textToDisplay;
	private bool hasBeenDisplayed;

	private static Transform tutorialUI;

	void Start()
	{
		if (tutorialUI == null)
			tutorialUI = GameObject.Find("Canvas").transform.FindChild("Tutorial");
		hasBeenDisplayed = false;
	}

	// this script will go on the interaction portion of an object
	public bool DisplayText()
	{
		if (hasBeenDisplayed)
			return false;

		//enable ui element for tut text
		tutorialUI.FindChild ("Tutorial Text").GetComponent<Text> ().text = textToDisplay;
		hasBeenDisplayed = true;
		return true;
	}

	public void HideText()
	{
		if (Manager.currInteraction != null && Manager.currInteraction.Equals(gameObject))
			global.gameManager.RemoveTutorial(gameObject);
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player" && !hasBeenDisplayed)
		{
			if (Manager.currInteraction == null)
			{
				global.gameManager.DisplayTutorial(gameObject);
			}
			if (Manager.currInteraction.Equals(gameObject))
			{
				DisplayText();
			}
		}
	}
	
	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			HideText();
		}
	}
}
