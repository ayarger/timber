using Godot;

public class IconControl : Node
{
	private MeshInstance _iconMesh;

	private float floatSpeed = 2.0f;  
	private float floatHeight = 0.5f; 
	private float time = 0.0f;
	private float offset_y = 0;

	private bool isShrinking = false;
	private bool isExpanding = false;
	private float shrinkExpandDuration = 0.5f; 
	private float animationProgress = 0.0f;    

	public override void _Ready()
	{
		_iconMesh = GetNode<MeshInstance>("../iconMesh");
		offset_y = _iconMesh.Transform.origin.y;
		StartExpand();
	}

	public override void _Process(float delta)
	{
		// Handle floating effect
		time += delta * floatSpeed;

		float newY = Mathf.Sin(time) * floatHeight + offset_y;

		Transform meshTransform = _iconMesh.Transform;
		Vector3 currentTranslation = meshTransform.origin;

		currentTranslation.y = newY;
		meshTransform.origin = currentTranslation;
		_iconMesh.Transform = meshTransform;

		// Handle shrink/expand animations
		if (isShrinking)
		{
			AnimateShrink(delta);
		}

		if (isExpanding)
		{
			AnimateExpand(delta);
		}
	}

	public void SetIconInvisible()
	{
		StartShrink();
	}
	
	public void SetIconVisible()
	{
		_iconMesh.Visible = true;
		StartExpand();
	}

	// Function to shrink the mesh to size 0
	public void StartShrink()
	{
		isShrinking = true;
		isExpanding = false;
		animationProgress = 0.0f;
	}

	// Function to expand the mesh back to size 1
	public void StartExpand()
	{
		isShrinking = false;
		isExpanding = true;
		animationProgress = 0.0f;
	}

	private void AnimateShrink(float delta)
	{
		animationProgress += delta / shrinkExpandDuration;
		animationProgress = Mathf.Clamp(animationProgress, 0.0f, 1.0f);

		// Interpolate the scale from 1 to 0
		float newScale = Mathf.Lerp(1.0f, 0.0f, animationProgress);
		_iconMesh.Scale = new Vector3(newScale, newScale, newScale);

		if (animationProgress >= 1.0f)
		{
			isShrinking = false;
			_iconMesh.Visible = false;
		}
	}

	private void AnimateExpand(float delta)
	{
		animationProgress += delta / shrinkExpandDuration;
		animationProgress = Mathf.Clamp(animationProgress, 0.0f, 1.0f);

		// Interpolate the scale from 0 to 1
		float newScale = Mathf.Lerp(0.0f, 1.0f, animationProgress);
		_iconMesh.Scale = new Vector3(newScale, newScale, newScale);

		if (animationProgress >= 1.0f)
		{
			isExpanding = false;
		}
	}
}
