using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : MonoBehaviour {
	
	// original target to follow; which is the player in our case
	public Transform player;
	// the current target that the camera is following
	private Transform target;
	public float distanceMin = 3.0f;
	public float distanceMax = 12.0f;
	// The current distance in the x-z plane to the target
	private float distance = 10.0f;
	// the height we want the camera to be above the target
	private float height = 5.0f;

	[Range(0.1f, 3)]
	public float mouseRotationSensitivity = 1f;

	private Vector3 euler;

	//this value is used to invert the Y axis input
	private int invertYVal;

	//used to determine whether the player is controlling the camera
	private bool control_mouse;
	//the timer to keep track of when the autorotate should kick in
	private float control_timer;
	//time in seconds until the camera control goes back to following the player's direction
	private float control_maxTime = 3f;

	//the player models are placed oddly witht the position, so i needed to add this
	private static Vector3 targetOffset = new Vector3(0f, 1.33f, 0f);

	//used to toggle the automatic camera-follow/rotate behavior
	private bool autoRotationEnabled;

	//if there is a cutscene playing, this value will not be null
	[HideInInspector]
	public CutScene currentCutScene;

	//this factor is used to determine if the camera is facing the front or back of the player
	private int viewToggleFactor;

	private bool staticCamera;
	private Vector3 staticLocation;

	void Start() {
		control_mouse = false;
		target = player;

		viewToggleFactor = 1;
		invertYVal = -1;
		autoRotationEnabled = false;
	}

	public void ToggleYRotationInvert()
	{
		invertYVal *= -1;
	}

	public void ToggleRotationConformity()
	{
		autoRotationEnabled = !autoRotationEnabled;
	}

	public void EnableStaticCamera(Vector3 location)
	{
		staticCamera = true;
		staticLocation = location;
	}

	public void DisableStaticCamera()
	{
		staticCamera = false;
	}
		
	void LateUpdate () {
		// Early out if we don't have a target
		if (!target)
			return;

		if (currentCutScene != null)
		{
			if (currentCutScene.MoveCamera(transform))
			{
				RevertTarget();
			}
			else if (target != currentCutScene.CurrentTarget)
			{
				target = currentCutScene.CurrentTarget;
			}
		}
		else if (staticCamera)
		{
			if (!transform.position.Equals(staticLocation))
				transform.position = staticLocation;
		}
		else
		{
			//at the frame the right mouse button is first pressed
			if (Input.GetMouseButtonDown(1))
			{
				//reset the timer, set the control, and hide the cursor
				control_timer = 0;
				control_mouse = true;
				Screen.showCursor = false;

				//euler = transform.eulerAngles;
				//eulerAngles.x is 360-270 above horizon and 0-90 below, so we convert it to +- 0-90
				//euler.x = transform.eulerAngles.x <= 90 ? -transform.eulerAngles.x : 360 - transform.eulerAngles.x;
			}
			//right mouse button is currently pressed
			else if (Input.GetMouseButton(1))
			{
				//update the rotation based on mouse drag info
				euler.y += Input.GetAxis("Mouse X") * distance * mouseRotationSensitivity;
				euler.x += Input.GetAxis("Mouse Y") * distance * mouseRotationSensitivity * invertYVal;
			}

			if (global.uicanvas.currentState.Equals("") || global.uicanvas.currentState.Equals("Dialogue") ||
			    global.uicanvas.currentState.Equals("Interaction")) {
				//changes distance using the scroll wheel
				distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

				if (distance > distanceMin) {
					RaycastHit hit;
					if (Physics.Raycast (new Ray(transform.position - (transform.TransformDirection(Vector3.forward) * 2),
					                             transform.TransformDirection(Vector3.forward)), out hit, distance / 4)) {
						if (!hit.transform.gameObject.tag.Equals("Player") && !hit.collider.isTrigger) {
							distance -= hit.distance;
						}
					}
				}
			}

			height = distance / 2f;
			height = Mathf.Clamp(height, 2.3f, 15f);

			if (control_mouse)
			{
				//the right mouse button was just released
				if (Input.GetMouseButtonUp(1))
					Screen.showCursor = true;
				//the right mouse button is currently up
				if (!Input.GetMouseButton(1))
					control_timer += Time.deltaTime;

				if (control_timer >= control_maxTime)
					control_mouse = false;

				euler.x = Mathf.Clamp(euler.x, -20, 40);

				transform.rotation = Quaternion.Euler(euler);
				transform.position = target.position + new Vector3(0, height, 0);
				transform.position -= transform.forward * distance;
			}
			else
			{
				// Calculate the current rotation angles
				float wantedRotationAngle = target.eulerAngles.y * viewToggleFactor;
				if (!autoRotationEnabled)
					wantedRotationAngle = transform.eulerAngles.y;

				// changes current rotation to rotate towards the wanted angle by one step
				Quaternion currentRotation = Quaternion.RotateTowards (transform.rotation,
				                                                       Quaternion.Euler(0, wantedRotationAngle, 0),
				                                                       1f);
				// Damp the height
				float wantedHeight = target.position.y + height;
				float currentHeight = Mathf.Lerp (transform.position.y, wantedHeight, 3f * Time.deltaTime);

				
				// Set the position of the camera on the x-z plane to distance behind the target
				transform.position = target.position;
				transform.position -= currentRotation * Vector3.forward * distance;
				
				// Set the height of the camera
				transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
			}
		}
		

		// Always look at the target
		transform.LookAt (target.position + targetOffset);
	}

	public void ChangeTargetToNPC(Transform npcTransform)
	{
		target = npcTransform;
		Transform interaction = npcTransform.FindChild("Interaction");
		if (interaction != null && interaction.GetComponent<Dialogue>() != null)
		{
			if (interaction.GetComponent<Dialogue>().frontCamView)
				viewToggleFactor = -1;
		}
		distance = 3f;
		height = 4f;
	}

	public void RevertTarget()
	{
		target = player;
		currentCutScene = null;
		CutScene.DisableWideScreen ();
		viewToggleFactor = 1;
		distance = 10.0f;
		height = 5.0f;
	}

	public void SetCutScene(CutScene newCutScene)
	{
		currentCutScene = newCutScene;
		target = currentCutScene.CurrentTarget;
		CutScene.EnableWideScreen ();
	}
}


public class CutScene
{
	private List<CameraDirection> directions;
	private int currentDirection;
	private bool loop;
	private bool finished;

	public CameraDirection CurrentDirection
	{
		get {
			if (directions != null && !finished)
				return directions[currentDirection];
			else
				return null;
		}
	}

	public Transform CurrentTarget
	{
		get {
			if (directions != null && !finished)
				return directions[currentDirection].target;
			else
				return null;
		}
	}

	public CutScene(List<CameraDirection> Directions, bool LoopThroughDirections)
	{
		directions = Directions;
		currentDirection = 0;
		loop = LoopThroughDirections;
		finished = false;
	}

	public static void EnableWideScreen()
	{
		global.uicanvas.transform.FindChild ("WideScreen").gameObject.SetActive(true);
	}

	public static void DisableWideScreen()
	{
		global.uicanvas.transform.FindChild ("WideScreen").gameObject.SetActive(false);
	}

	/// <summary>
	/// Moves the camera's transform position to the target position. Returns true if the camera reached the target and there are no following directions.
	/// </summary>
	public bool MoveCamera(Transform camTransform)
	{
		if (directions [currentDirection].MoveCamera (camTransform))
		{
			return GetNextDirection();
		}
		return false;
	}

	/// <summary>
	/// Gets the next direction.
	/// </summary>
	/// <returns><c>true</c>, if next direction was gotten, <c>false</c> otherwise.</returns>
	private bool GetNextDirection()
	{
		currentDirection++;
		if (currentDirection >= directions.Count)
		{
			if (loop)
			{
				currentDirection = 0;
			}
			else
			{
				finished = true;
				return finished;
			}
		}
		return false;
	}
}

public class CameraDirection
{
	private Vector3 desiredCameraPosition;
	private Transform targetForCameraToLookAt;
	private int translationTime;

	public Transform target
	{
		get { return targetForCameraToLookAt; }
	}

	public CameraDirection(Vector3 moveToPosition, Transform lookatTransform, int TranslationTime)
	{
		desiredCameraPosition = moveToPosition;
		targetForCameraToLookAt = lookatTransform;
		translationTime = TranslationTime;
	}

	/// <summary>
	/// Moves the camera's transform position to the target position. Returns true if camera is close enough to the target.
	/// </summary>
	public bool MoveCamera(Transform camTransform)
	{
		camTransform.position = Vector3.MoveTowards (camTransform.position, desiredCameraPosition, translationTime * Time.deltaTime);
		
		if (Vector3.Distance(camTransform.position, desiredCameraPosition) < 1)
		{
			return true;
		}
		return false;
	}
}
