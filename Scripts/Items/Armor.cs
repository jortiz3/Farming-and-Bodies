using UnityEngine;
using System.Collections;

public class Armor : ItemScript
{
	public int Protection()
	{
		return global.itemDatabase.GetItem(transform.parent.name).typeSpecificStat;
	}
}
