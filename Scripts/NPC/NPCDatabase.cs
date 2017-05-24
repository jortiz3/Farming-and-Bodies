using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class NPCDatabase {

	class NPC{
		public int id;
		public string name;
		public NPCScript npcScript;

		public NPC(int ID, NPCScript NPC) {
			id = ID;
			name = NPC.name;
			npcScript = NPC;
		}
	}

	class SpecialNPCStatus
	{
		public string key;
		public string npcName;

		public SpecialNPCStatus(string Key, string Name) {
			key = Key;
			npcName = Name;
		}
	}

	private static List<SpecialNPCStatus> special;
	private static List<NPC> alive;
	private static List<NPC> dead;

	public static void RegisterNPC(NPCScript npc) {
		if (alive == null)
			alive = new List<NPC> ();

		NPC found = GetDBNPC (npc.name);

		if (found == null)
			alive.Add (new NPC (alive.Count, npc));
		else
			found.npcScript = npc;
	}

	public static void RegisterDeadNPC(NPCScript npc) {
		if (dead == null)
			dead = new List<NPC>();
		dead.Add (GetDBNPC (npc.name));
		if (special != null) {
			for (int i = special.Count - 1; i >= 0; i--) {
				if (special[i].npcName.Equals(npc.name))
					special.RemoveAt(i);
			}
		}
		alive.Remove (GetDBNPC (npc.name));
	}

	private static NPC GetDBNPC(string name) {
		for (int i = 0; i < alive.Count; i++)
			if (alive[i].name.Equals(name))
				return alive[i];
		return null;
	}

	public static NPCScript GetNPCFromDB(string name) {
		for (int i = 0; i < alive.Count; i++)
			if (alive[i].name.Equals(name))
				return alive[i].npcScript;
		return null;
	}

	public static string GetNPCName(int id){
		for (int i = 0; i < alive.Count; i++)
			if (alive[i].id == id)
				return alive[i].name;
		return "null";
	}

	//returns all of the statuses that a particular npc holds
	public static string[] GetNPCStatus(NPCScript npc) {
		List<string> s = new List<string>();
		for (int i = 0; i < special.Count; i++) {
			if (special[i].npcName.Equals(npc.name))
				s.Add(special[i].key);
		}
		return s.ToArray ();
	}

	public static string GetNPCNameSpecial(string key) {
		if (special != null) {
			for(int i = 0; i < special.Count; i++) {
				if (special[i].key.Equals(key)) {
					return special[i].npcName;
				}
			}
		}
		return "null";
	}

	public static void SetNPCNameSpecial(string key, string npcName) {
		if (special == null)
			special = new List<SpecialNPCStatus>();

		for(int i = 0; i < special.Count; i++) {
			if (special[i].key.Equals(key)) {
				special[i].npcName = npcName;
				return;
			}
		}

		special.Add (new SpecialNPCStatus (key, npcName));
	}

	public static string[] GetDeadNames() {
		if (dead == null)
			return null;
		string[] names = new string[dead.Count];
		for (int i = 0; i < names.Length; i++)
			names[i] = dead[i].name;
		return names;
	}

	public static void SetAllNPCsActive() {
		foreach(NPC n in alive) {
			if (n != null && n.npcScript != null)
				n.npcScript.gameObject.SetActive(true);
		}
	}
}