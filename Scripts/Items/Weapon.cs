using UnityEngine;
using System.Collections;

public class Weapon : ItemScript
{
	public int Damage()
	{
		return global.itemDatabase.GetItem(transform.parent.name).typeSpecificStat;
	}
}