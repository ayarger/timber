using System;
using Godot;
using MoonSharp.Interpreter;

public class IconControl : Node
{
	private MeshInstance _iconMesh;

	private float floatSpeed = 2.0f;  
	private float floatHeight = 0.5f; 
	private float time = 0.0f;
	private float offset_y = 0;
	private float offset_x = 0;

	private bool isShrinking = false;
	private bool isExpanding = false;
	private float shrinkExpandDuration = 0.3f; 
	private float animationProgress = 0.0f;    
	
	private const float MinScale = 0.0f;
	private const float MaxScale = 1.0f;
	private const float MinProgress = 0.0f;
	private const float MaxProgress = 1.0f;
	
	private bool isCircularMotion = false;
	private float circleRadius = 0.5f;
	private float circleSpeed = 2.0f; 
	private float angle = 0.0f;       
	private Vector3 centerPosition;


	private Action shrinkCompleteCallback;

	public override void _Ready()
	{
		_iconMesh = GetNode<MeshInstance>("../iconMesh");
		offset_y = _iconMesh.Transform.origin.y;
		StartExpand();
	}

	public override void _Process(float delta)
	{
		// if (isCircularMotion)
		// {
		// 	UpdateCircularMotion(delta);
		// }
		// else
		// {
		// 	UpdateFloatingEffect(delta);
		// }
		
		UpdateFloatingEffect(delta);

		if (isShrinking)
		{
			AnimateShrink(delta);
		}

		if (isExpanding)
		{
			AnimateExpand(delta);
		}
	}

	private void UpdateCircularMotion(float delta)
	{

		time += delta * circleSpeed;
		float angle = Mathf.Wrap(time, 0, Mathf.Pi * 2);
		float x = Mathf.Cos(angle) * circleRadius;
		float y = Mathf.Sin(angle) * circleRadius;
		Transform meshTransform = _iconMesh.Transform;
		meshTransform.origin = centerPosition + new Vector3(x, y, 0);
		_iconMesh.Transform = meshTransform;
	}

	public void StartCircularMotion(Vector3 center, float radius, float speed, float angleOffset = 0.0f)
	{
		isCircularMotion = true;
		centerPosition = center;
		circleRadius = radius;
		circleSpeed = speed;
		angle = angleOffset;
	}

	public void StopCircularMotion()
	{
		isCircularMotion = false;
	}
	
	private float floatSpeedVariation;
	private float floatHeightVariation;
	private float phaseOffset;

	public void Initialize()
	{
		float speedVar = (float)GD.RandRange(-0.5, 0.5);
		float heightVar = (float)GD.RandRange(-0.2, 0.2);
		float phase = (float)GD.RandRange(0, Mathf.Pi * 2);

		floatSpeedVariation = speedVar;
		floatHeightVariation = heightVar;
		phaseOffset = phase;
		isCircularMotion = true;
	}


	private void UpdateFloatingEffect(float delta)
	{
		time += delta * (floatSpeed + floatSpeedVariation);
		float newY = Mathf.Sin(time + phaseOffset) * (floatHeight + floatHeightVariation) + offset_y;
		Transform meshTransform = _iconMesh.Transform;
		Vector3 currentTranslation = meshTransform.origin;
		currentTranslation.y = newY;
		if (isCircularMotion)
		{
			float newX = Mathf.Sin(time) * Mathf.Cos(time) * (floatHeight * 0.5f) + offset_x;
			currentTranslation.x = newX;
		}
		meshTransform.origin = currentTranslation;
		_iconMesh.Transform = meshTransform;
	}



	public void SetIconInvisible(Action callback = null)
	{
		shrinkCompleteCallback = callback; // Set the callback if provided
		StartShrink();
	}
	
	public void SetIconVisible()
	{
		_iconMesh.Visible = true;
		StartExpand();
	}

	public void StartShrink()
	{
		ResetAnimationState();
		isShrinking = true;
	}

	public void StartExpand()
	{
		ResetAnimationState();
		isExpanding = true;
	}

	private void AnimateShrink(float delta)
	{
		animationProgress += delta / shrinkExpandDuration;
		animationProgress = Mathf.Clamp(animationProgress, MinProgress, MaxProgress);

		float newScale = Mathf.Lerp(MaxScale, MinScale, animationProgress);
		_iconMesh.Scale = new Vector3(newScale, newScale, newScale);

		if (animationProgress >= MaxProgress)
		{
			isShrinking = false;
			_iconMesh.Visible = false;
			shrinkCompleteCallback?.Invoke();
		}
	}

	private void AnimateExpand(float delta)
	{
		animationProgress += delta / shrinkExpandDuration;
		animationProgress = Mathf.Clamp(animationProgress, MinProgress, MaxProgress);

		float newScale = Mathf.Lerp(MinScale, MaxScale, animationProgress);
		_iconMesh.Scale = new Vector3(newScale, newScale, newScale);

		if (animationProgress >= MaxProgress)
		{
			isExpanding = false;
		}
	}
	
	private void ResetAnimationState()
	{
		isShrinking = false;
		isExpanding = false;
		animationProgress = MinProgress;
	}
	
	public void SetAnimationVariation(int variationIndex)
	{
		switch (variationIndex)
		{
			case 0:
				floatSpeed = 1.5f;
				floatHeight = 0.3f;
				shrinkExpandDuration = 0.5f;
				break;

			case 1:
				floatSpeed = 3.0f;
				floatHeight = 0.8f;
				shrinkExpandDuration = 0.2f;
				break;

			case 2:
				floatSpeed = 2.0f;
				floatHeight = 0.5f;
				shrinkExpandDuration = 0.3f;
				break;

			default:
				GD.Print("Unknown variation index, applying default settings.");
				floatSpeed = 2.0f;
				floatHeight = 0.5f;
				shrinkExpandDuration = 0.3f;
				break;
		}
	}


}
