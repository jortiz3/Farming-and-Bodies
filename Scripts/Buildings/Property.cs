using UnityEngine;
using System.Collections;

public class Property : MonoBehaviour {
	public NPCScript owner;

	public static Property NextToDie;

	public void SetNextToDie()
	{
		if (NextToDie == null)
			NextToDie = this;
	}

	public static bool KillNextProperty()
	{
		if (NextToDie != null && NextToDie.owner != null)
		{
			//tell owner to die
			NextToDie.owner.Die();
			NextToDie = null;

			//returns true if someone was killed
			return true;
		}
		//returns false if nobody was killed
		return false;
	}
}
