using System;
using System.Collections.Generic;
using Godot;

public class Collectable : Spatial
{
	private IconControl _iconControl;
	private Coord _selfCoord;

	public bool isFlyingToPlayer = false; 
	public Actor targetActor = null;
	public float flySpeed = 5.0f;
	public int _selfValue = 10;
	
	private enum FlyState
	{
		None,
		PreFly,
		FlyToPlayer
	}
	private FlyState _flyState = FlyState.None;
	private Vector3 _preFlyTarget;
	private float _preFlyDuration = 0.2f; // Duration of the "pre-fly" phase
	private float _preFlyElapsed = 0.0f;

	public override void _Ready()
	{
		// Node area = FindNode("Area");
		// area.Connect("body_entered", this, "onBodyEntered");
		_iconControl = GetNode<IconControl>("IconControl");
	}

	public void SetCoord(Vector3 position)
	{
		_selfCoord =  Grid.ConvertToCoord(position);
		ResolveOverlap();
		// ToastManager.SendToast(this, "Coin transform origin: [" + Transform.origin.x + ", " + Transform.origin.y + ", " + Transform.origin.z + "]", ToastMessage.ToastType.Notice, 1f);
	}
	
	public override void _Process(float delta)
	{
		if (_flyState == FlyState.PreFly)
		{
			_preFlyElapsed += delta;

			Vector3 direction = (_preFlyTarget - GlobalTransform.origin).Normalized();
			float distance = (_preFlyTarget - GlobalTransform.origin).Length();

			Vector3 newPosition = GlobalTransform.origin + direction * flySpeed * delta;
			GlobalTransform = new Transform(GlobalTransform.basis, newPosition);

			if (_preFlyElapsed >= _preFlyDuration)
			{
				_flyState = FlyState.FlyToPlayer;
			}
		}
		else if (_flyState == FlyState.FlyToPlayer && isFlyingToPlayer)
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

	public void StartFlyToPlayer()
	{
		Vector3 offset = new Vector3(
			GD.Randf() * 2 - 1, 
			GD.Randf() * 2 - 1, 
			GD.Randf() * 2 - 1  
		).Normalized() * 2.0f;

		_preFlyTarget = GlobalTransform.origin + offset;
		_flyState = FlyState.PreFly;
		_preFlyElapsed = 0.0f;
		isFlyingToPlayer = true;
	}

	// private void onBodyEntered(Node body)
	// {
	// 	if (targetActor != null)
	// 	{
	// 		return;
	// 	}
	// 	if (body.GetParent() is Actor && body.GetParent().HasNode("HasTeam"))
	// 	{
	// 		HasTeam hasTeam = body.GetParent().GetNode<HasTeam>("HasTeam");
	// 		if (hasTeam.team == "player")
	// 		{
	// 			// ToastManager.SendToast(this, "Collided with a player", type: ToastMessage.ToastType.Notice);
	// 			GetNode<MeshInstance>("shadow").Visible = false;
	// 			CollectableManager.UpdateCurrencyManagerAndClearGridCollectable(_selfCoord);
	// 			targetActor = body.GetParent() as Actor;
	// 			StartFlyToPlayer();
	// 		}
	// 	}
	// }

	public int GetCollectableValue()
	{
		return _selfValue;
	}
	
	// private void ResolveOverlap()
	// {
	// 	List<Collectable> collectablesOnGrid = CollectableManager.GetCollectableListOnGrid(_selfCoord);
	//
	// 	if (collectablesOnGrid.Count > 1)
	// 	{
	// 		collectablesOnGrid.Sort((a, b) => a.GetInstanceId().CompareTo(b.GetInstanceId()));
	// 		// Collectable leader = collectablesOnGrid[0];
	// 		// float radius = 0.3f;
	// 		// float speed = 2.0f;
	// 		for (int i = 0; i < collectablesOnGrid.Count; i++)
	// 		{
	// 			// float angleOffset = (Mathf.Pi * 2 / collectablesOnGrid.Count) * i;
	// 			// collectablesOnGrid[i]._iconControl.StartCircularMotion(new Vector3(0,0,0), radius, speed, angleOffset);
	// 			collectablesOnGrid[i]._iconControl.Initialize();
	// 		}
	// 	}
	// }
	
	private void ResolveOverlap()
	{
		List<Collectable> collectablesOnGrid = CollectableManager.GetCollectableListOnGrid(_selfCoord);
		if (collectablesOnGrid.Count > 0)
		{
			collectablesOnGrid[0].GetNode<MeshInstance>("shadow").Visible = true;
		}
		if (collectablesOnGrid.Count > 1)
		{
			collectablesOnGrid.Sort((a, b) => a.GetInstanceId().CompareTo(b.GetInstanceId()));
			float radius = 0.5f;
			Vector3 center = new Vector3(0, 0, 0);

			for (int i = 0; i < collectablesOnGrid.Count; i++)
			{
				float angleOffset = (Mathf.Pi * 2 / collectablesOnGrid.Count) * i;
				Vector3 targetPosition = center + new Vector3(
					Mathf.Cos(angleOffset) * radius,
					0,
					Mathf.Sin(angleOffset) * radius
				);
				var tween = collectablesOnGrid[i].GetNode<Tween>("Tween");
				if (tween == null)
				{
					tween = new Tween();
					collectablesOnGrid[i].AddChild(tween);
				}
				tween.StopAll();

				tween.InterpolateProperty(collectablesOnGrid[i]._iconControl, "translation", new Vector3(0,0,0), targetPosition, 0.5f, Tween.TransitionType.Sine, Tween.EaseType.InOut);
				tween.Start();

				collectablesOnGrid[i]._iconControl.StartCircularMotion(center, radius, 2.0f, angleOffset);
			}
		}
	}


	
	// private void StartFlyToPlayer()
	// {
	// 	isFlyingToPlayer = true;
	// }

	private void OnReachedPlayer()
	{
		isFlyingToPlayer = false;

		_iconControl.SetIconInvisible();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
	}
}
