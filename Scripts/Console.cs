using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Console : MonoBehaviour
{
	private class Message
	{
		private string text;
		private Color textColor;

		public Message(string Text)
		{
			text = Text;
			textColor = Color.white;
		}

		//returns true when the message is ready to be removed from view
		public bool Update(Text textObj, float delta)
		{
			textColor.a -= delta;

			if (textColor.a <= 0.1f)
			{
				textObj.color = Color.clear;
				textObj.text = "";
				return true;
			}

			textObj.color = textColor;
			textObj.text = text;
			return false;
		}
	}

	private List<Message> messages;
	private Text[] textcomponents;
	public static GameObject devConsole;

	void Start ()
	{
		textcomponents = transform.FindChild("Display").GetComponentsInChildren<Text>();
		devConsole = transform.FindChild("Dev Console").gameObject;
		messages = new List<Message>();
	}
	
	void Update()
	{
		for (int index = messages.Count - 1; index >= 0; index--)
		{
			if (messages[index].Update(textcomponents[index], Time.deltaTime / 8))
				messages.RemoveAt(index);
		}

		if (devConsole.activeSelf) {
			if (!devConsole.GetComponent<InputField> ().isFocused) {
				devConsole.GetComponent<InputField> ().ActivateInputField();
				devConsole.GetComponent<InputField> ().Select ();
			}
		}
	}

	public void AddMessage(string message)
	{
		messages.Insert(0, new Message(message));
		if (messages.Count > textcomponents.Length)
			messages.RemoveAt(messages.Count - 1);
	}

	public void ToggleDevConsole()
	{
		devConsole.SetActive(!devConsole.activeSelf);
	}

	public void EnterCommandDevConsole()
	{
		AddMessage(global.gameManager.TriggerEvent (devConsole.GetComponent<InputField>().text));
		devConsole.GetComponent<InputField>().text = "";
		ToggleDevConsole ();
	}
}