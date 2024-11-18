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

	private bool isShrinking = false;
	private bool isExpanding = false;
	private float shrinkExpandDuration = 0.3f; 
	private float animationProgress = 0.0f;    
	
	private const float MinScale = 0.0f;
	private const float MaxScale = 1.0f;
	private const float MinProgress = 0.0f;
	private const float MaxProgress = 1.0f;

	private Action shrinkCompleteCallback; // Callback to notify shrink is complete

	public override void _Ready()
	{
		_iconMesh = GetNode<MeshInstance>("../iconMesh");
		offset_y = _iconMesh.Transform.origin.y;
		StartExpand();
	}

	public override void _Process(float delta)
	{
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

	
	private void UpdateFloatingEffect(float delta)
	{
		time += delta * floatSpeed;
		float newY = Mathf.Sin(time) * floatHeight + offset_y;

		Transform meshTransform = _iconMesh.Transform;
		Vector3 currentTranslation = meshTransform.origin;

		currentTranslation.y = newY;
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
