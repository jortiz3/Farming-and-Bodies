using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crop : ItemScript
{
	public float SecondsToGrow = 3;
	private float stageTimer;
	private float timePerStage;

	public Vector3 StartScale = Vector3.zero;
	public Vector3 FinishScale = Vector3.zero;

	//these are the colors that the object changes to after a certain amount of time of "growing"
	public Color[] colors = { Color.red, Color.yellow, Color.green};
	private Color previousColor;

	//this is the current color to be applied to the renderer
	private int color;

	//gets the renderer for the object so we can change the colors later
	private Renderer objrend;

	//stores the plot in which the crop was planted
	private Plot currentPlot;

	private bool canGrow;
	private bool fullyGrown;

	private Transform timerCanvas;
	private Slider timerSlider;

	void Start()
	{
		if (ItemParentInHierarchy == null)
			ItemParentInHierarchy = GameObject.FindGameObjectWithTag("ItemParent").transform;

		timerCanvas = transform.parent.FindChild("Timer Canvas");

		if (timerCanvas != null)
		{
			timerCanvas.localPosition = new Vector3(0, 10, 0);
			timerSlider = timerCanvas.FindChild("Timer Slider").GetComponent<Slider>();
			timerSlider.value = 0;
		}

		stageTimer = 0;
		timePerStage = SecondsToGrow / colors.Length;

		//sets the color to the first color
		color = 0;
		objrend = transform.parent.GetComponent<Renderer>();
		objrend.material.color = colors[color];
		previousColor = objrend.material.color;

		if (FinishScale == Vector3.zero)
			FinishScale = transform.parent.localScale;

		if (StartScale != Vector3.zero)
		{
			transform.localScale = transform.localScale + (transform.parent.localScale - StartScale);
			transform.parent.localScale = StartScale;
		}
		else
			StartScale = transform.parent.localScale;

		fullyGrown = false;

		//starts growing the object if it is on a plot
		if (currentPlot != null)
			canGrow = true;
		else
			HideTimer();
	}

	void Update()
	{
		if (timerCanvas != null)
			timerCanvas.rotation = Camera.main.transform.rotation;

		if (!fullyGrown && canGrow)
		{
			if (color < colors.Length)
			{
				stageTimer += Time.deltaTime;
				//changes the color
				objrend.material.color = Color.Lerp(previousColor, colors[color], stageTimer);

				if (stageTimer >= timePerStage)
				{
					objrend.material.color = colors[color];
					previousColor = objrend.material.color;
					color++;
					stageTimer = 0;
				}
			}
			
			//updates the current scale of the object
			if (FinishScale != StartScale)
			{
				Vector3 scaleStep = (FinishScale - StartScale) * Time.deltaTime / SecondsToGrow;
				transform.parent.localScale += scaleStep;
				transform.localScale -= scaleStep;
			}

			//updates the timer bar if there is one
			if (timerSlider != null)
				timerSlider.value += Time.deltaTime / SecondsToGrow;

			//if the timer bar is full, we are done growing
			if (timerSlider.value >= 1)
			{
				//randomly increases quantity by a value of [0, 3)
				Quantity += Random.Range(0, 3);
				fullyGrown = true;
				HideTimer();
				//sends a message to anything that requires a crop to grow
				global.gameManager.BroadcastActionCompleted("Grow " + transform.parent.name);
			}
		}
	}

	public void SetPlot(Plot p)
	{
		currentPlot = p;
		p.Plant(this);
	}

	public void HideTimer()
	{
		if (timerCanvas != null)
			timerCanvas.gameObject.SetActive(false);
	}

	public override void PickUp ()
	{
		//if the item gets added to the inventory
		if (global.playerInventory.CanAddItem(GetItem()))
		{
			Item tempItem = global.itemDatabase.GetItem(transform.parent.name);
			tempItem.quantity = Quantity;
			if (currentPlot != null)
			{
				if (currentPlot.Harvest(tempItem.levelreq, fullyGrown))
					tempItem.stolenFrom = currentPlot.owner.name; 
				currentPlot = null;
			}
			else
			{
				global.gameManager.BroadcastActionCompleted("Pickup " + tempItem.name);
			}
			global.playerInventory.AddItem(tempItem);
			Destroy(transform.parent.gameObject);
		}
	}
}
