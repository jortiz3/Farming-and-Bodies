using UnityEngine;
using System.Collections;
using System;

public class Waypoints : MonoBehaviour 
{
	//Name of the icon image
	public string iconName = "wayIcon.psd";
	//Radius of each way point - use for checking the collision detection with the enemy
	public float radius = 2.0f;
	//if true, then the path goes to the end waypoint, then goes back through the previous waypoints
	//if false, once the end is reached, go towards first node
	public bool lineOrCircle = true;
	//Current waypoint index
	private int int_wayIndex;
	//Next waypoint index
	private int int_nextIndex;
	//Traversing forwards or backwards
	private bool forwards;
	//Movement direction of the enemy to next waypoint
	private Vector3 v3_direction;
	//Checking if the enemy hit the waypoint
	private bool b_isHitRadius;
	//Waypoint array
	public Waypoint[] wayArray = new Waypoint[0];

	//Set up all parameters before Initailize
	public void Awake() 
	{
		int_wayIndex = 0;
		int_nextIndex = 1;
		//Set the direction to zero
		v3_direction = Vector3.zero;
		//To ignore the first waypoint at the beginning of the game
		b_isHitRadius = true;

		forwards = true;
	}

	public Vector3 StartPosition()
	{
		return wayArray[0].t.position;
	}

	public Waypoint CurrentWaypoint()
	{
		return wayArray[int_wayIndex];
	}

	public bool ReachedWaypoint()
	{
		return b_isHitRadius;
	}

	//Return the direction of the enemy toward the next waypoint
	public Vector3 GetDirection(Transform _AI)
	{
		if (Vector3.Distance(_AI.position, wayArray[int_nextIndex].t.position) <= radius) 
		{
			//Only check once when the AI hit the way point
			if (!b_isHitRadius) 
			{
				b_isHitRadius = true;
				//Update the current way index
				int_wayIndex = int_nextIndex;

				if (forwards)
				{
					if (int_nextIndex < wayArray.Length - 1)
						int_nextIndex ++;
					else
					{
						if (lineOrCircle)
						{
							int_nextIndex = wayArray.Length - 2;
							forwards = false;
						}
						else
						{
							int_nextIndex = 0;
						}
					}
				}
				else
				{
					if (int_nextIndex > 0)
						int_nextIndex--;
					else
					{
						int_nextIndex = 1;
						forwards = true;
					}

				}
			}
		}
		else 
		{
			b_isHitRadius = false;
		}
		
		//Get Direction from the current position of the character to the next way point
		//Make sure that the y position equal to the waypoint y position -- wayArray[int_nextIndex].t.position.y
		Vector3 v3_currentPosition = new Vector3(_AI.position.x, wayArray[int_nextIndex].t.position.y, _AI.position.z);
		v3_direction = (wayArray[int_nextIndex].t.position - v3_currentPosition).normalized;
		
		return v3_direction;
	}

	//To get the direction from current position of the enemy to the player
	public Vector3 GetDirectionToPlayer (Transform _AI, Transform _player)
	{
		//Make sure that the y position equal to the waypoint y position
		Vector3 v3_currentPosition = new Vector3(_AI.position.x, wayArray[int_wayIndex].t.position.y, _AI.position.z);
		Vector3 v3_playerPosition = new Vector3(_player.position.x, wayArray[int_wayIndex].t.position.y, _player.position.z);
		v3_direction = (v3_playerPosition - v3_currentPosition).normalized;
		
		return v3_direction;
		
	}
	
	//Checking if the enemy is away from the target waypoint in the specific distance or not
	public bool AwayFromWaypoint (Transform _AI, float _distance)
	{
		if (Vector3.Distance(_AI.position, wayArray[int_nextIndex].t.position) >= _distance) 
		{
			return true;
		} 
		else 
		{
			return false;
		}
	}

	//Draw Gizmos and Directional line
	public void OnDrawGizmos() 
	{
		//Get all Transform of this game objects include the children and the transform of this gameobject
		Transform[] waypointGizmos = new Transform[wayArray.Length];
		for (int w = 0; w < waypointGizmos.Length; w++)
			waypointGizmos[w] = wayArray[w].t;

		if(waypointGizmos.Length > 0) 
		{
			//Draw line by the order of each waypoint 0,1,2,3,...
			for (int i = 0; i < waypointGizmos.Length; i++) 
			{
				if (waypointGizmos[i] != null)
				{
					int n = (i + 1) % waypointGizmos.Length;
					if (waypointGizmos[n] != null)
					{
						if (lineOrCircle && n != 0 || !lineOrCircle)
						{
							Gizmos.color = Color.red;
							Gizmos.DrawLine(waypointGizmos[i].position, waypointGizmos[n].position);
							Gizmos.DrawIcon(waypointGizmos[i].position, iconName);
							Gizmos.color = Color.green;
						}
					}
					Gizmos.DrawWireSphere(waypointGizmos[i].position, radius);
				}
			}
		}
	}
}

[Serializable]
public class Waypoint
{
	public Transform t;
	public float waitTime;
}
