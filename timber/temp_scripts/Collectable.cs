using System;
using System.Collections.Generic;
using Godot;

public class Collectable : Spatial
{
	private IconControl _iconControl;
	private Coord _selfCoord;

	private bool isFlyingToPlayer = false; 
	private Actor targetActor = null;
	private float flySpeed = 5.0f;

	public override void _Ready()
	{
		Node area = FindNode("Area");
		area.Connect("body_entered", this, "onBodyEntered");
		_iconControl = GetNode<IconControl>("IconControl");
		_selfCoord = Grid.ConvertToCoord(GlobalTranslation);
	}

	public override void _Process(float delta)
	{
		if (isFlyingToPlayer)
		{
			Vector3 direction = (targetActor.GlobalTransform.origin - GlobalTransform.origin).Normalized();
			float distance = (targetActor.GlobalTransform.origin - GlobalTransform.origin).Length();

			Vector3 newPosition = GlobalTransform.origin + direction * flySpeed * delta;
			GlobalTransform = new Transform(GlobalTransform.basis, newPosition);

			if (distance < 0.5f)
			{
				OnReachedPlayer();
			}
		}
	}

	private void onBodyEntered(Node body)
	{
		if (body.GetParent() is Actor && body.GetParent().HasNode("HasTeam"))
		{
			HasTeam hasTeam = body.GetParent().GetNode<HasTeam>("HasTeam");
			if (hasTeam.team == "player")
			{
				// ToastManager.SendToast(this, "Collided with a player", type: ToastMessage.ToastType.Notice);
				_updateCurrencyManager();
				targetActor = body.GetParent() as Actor;
				StartFlyToPlayer();
			}
		}
	}

	private void _updateCurrencyManager()
	{
		TempCurrencyManager.IncreaseMoney(10);
	}
	
	private void ResolveOverlap()
	{
		List<Collectable> collectablesOnGrid = CollectableManager.GetCollectableListOnGrid(_selfCoord);
		foreach (var other in collectablesOnGrid)
		{
			if (other == this) continue;

			if ((other.GlobalTransform.origin - GlobalTransform.origin).Length() < 0.5f)
			{
				int animationVariation = collectablesOnGrid.IndexOf(this) % 3; // Example variation based on index
				_iconControl.SetAnimationVariation(animationVariation);
			}
		}
	}
	
	private void StartFlyToPlayer()
	{
		isFlyingToPlayer = true;
	}

	private void OnReachedPlayer()
	{
		isFlyingToPlayer = false;

		_iconControl.SetIconInvisible(() =>
		{
			QueueFree();
		});
	}

	public override void _ExitTree()
	{
		base._ExitTree();
	}
}
