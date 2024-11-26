using Godot;
using System;
using System.Collections.Generic;
using Amazon.CloudFront.Model;

public class CollectableManager : Node
{
	private static Dictionary<Coord, List<Collectable>> allCollectables = new Dictionary<Coord, List<Collectable>>();
	private static Dictionary<Coord, Area> allAreas = new Dictionary<Coord, Area>();
	
	private static bool isUpdatingCurrencyManager = false;
	private static AudioStreamPlayer audioPlayer;
	public override void _Ready()
	{
		SetProcessInput(true);
		audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey && eventKey.Pressed && OS.GetScancodeString(eventKey.Unicode) == "c" )
		{
			SpawnCollectable(SelectionSystem.GetTilePosition());
		}
	}

	void SpawnCollectable(Vector3 cursorPos)
	{
		Coord cur = Grid.ConvertToCoord(new Vector3(cursorPos.x , 0, cursorPos.z));
		Vector3 spawnPos = Grid.ConvertToWorldPos(cur);
		
		if (Grid.Get(cur).actor != null)
		{
			ToastManager.SendToast(this, "Cannot throw a coin on a non-empty grid.", ToastMessage.ToastType.Warning, 2f);
			return;
		}

		if (Grid.Get(cur).value != '.')
		{
			ToastManager.SendToast(this, "Cannot throw a coin into the void.", ToastMessage.ToastType.Warning, 2f);
			return;
		}
		
		// ToastManager.SendToast(this, "Coin coord: [" + cur.x + "," + cur.z + "]", ToastMessage.ToastType.Notice, 1f);
		// ToastManager.SendToast(this, "Coin coord: [" + spawnPos.x + ", " + spawnPos.y + ", " + spawnPos.z + "]", ToastMessage.ToastType.Notice, 1f);
	
		SpawnCollectableScene(spawnPos);
		EventBus.Publish(new TileDataLoadedEvent());
	}
	
	Collectable SpawnCollectableScene(Vector3 position)
	{
		PackedScene collectable_scene = (PackedScene)ResourceLoader.Load("res://scenes/collectables/TestCollectable.tscn");
		Spatial new_collectable = (Spatial)collectable_scene.Instance();
		AddChild(new_collectable);

		Collectable collectable_script = new_collectable as Collectable;

		new_collectable.GlobalTranslation = position;
		var coord = Grid.ConvertToCoord(position);
		if (!allCollectables.ContainsKey(coord))
		{
			allCollectables[coord] = new List<Collectable>();
		}
		allCollectables[coord].Add(collectable_script);		
		collectable_script.SetCoord(position);
		
		if (!allAreas.ContainsKey(coord))
		{
			CreateArea(position, coord);
		}
		return collectable_script;
	}

	private void CreateArea(Vector3 position, Coord coord)
	{
		Area newArea = new Area();

		CollisionShape collisionShape = new CollisionShape();
		BoxShape shape = new BoxShape();
		collisionShape.Shape = shape;
		collisionShape.Scale = new Vector3(2, 2, 2);
		collisionShape.Translation = new Vector3(0, 0.5f, -0.5f);
		newArea.AddChild(collisionShape);
		newArea.GlobalTranslation = position;
		AddChild(newArea);
		allAreas[coord] = newArea;
		ToastManager.SendToast(this, "Area at " + position.x + ", " + position.y + ", " + position.z, type: ToastMessage.ToastType.Notice);
		newArea.Connect("body_entered", this, nameof(OnBodyEntered));
	}
	
	private void OnBodyEntered(Node body)
	{
		if (body.GetParent() is Actor && body.GetParent().HasNode("HasTeam"))
		{
			HasTeam hasTeam = body.GetParent().GetNode<HasTeam>("HasTeam");
			if (hasTeam.team == "player")
			{
				ToastManager.SendToast(this, "Collided with a player", type: ToastMessage.ToastType.Notice);
				foreach (var kvp in allAreas)
				{
					Area triggeredArea = kvp.Value;
					if (triggeredArea.GetOverlappingBodies().Contains(body))
					{
						// found the area triggered
						Coord coord = Grid.ConvertToCoord(triggeredArea.GlobalTranslation);
						if (!allCollectables.ContainsKey(coord))
						{
							ToastManager.SendToast(this, "Empty coll dict at " + coord.x + ", " + coord.z, type: ToastMessage.ToastType.Notice);
							return;
						}
						for (int i = 0; i < allCollectables[coord].Count; i++)
						{
							Collectable collectable = allCollectables[coord][i];
							if (collectable.targetActor != null)
							{
								return;
							}
							collectable.GetNode<MeshInstance>("shadow").Visible = false;
							UpdateCurrencyManager(coord);
							collectable.targetActor = body.GetParent() as Actor;
							collectable.StartFlyToPlayer();
						}
						triggeredArea.QueueFree();
						allAreas.Remove(coord);

						return;
					}
				}
			}
		}
	}
	
	public static List<Collectable> GetCollectableListOnGrid(Coord coord)
	{
		return allCollectables[coord];
	}

	public static void UpdateCurrencyManager(Coord coord)
	{
		// let the player collect all coins in that grid. 
		if (!isUpdatingCurrencyManager)
		{
			isUpdatingCurrencyManager = true;
			PlayCollectionSound();
			var total_amount = 0;
			foreach (var collectable in allCollectables[coord])
			{
				total_amount += collectable.GetCollectableValue();
			}
			TempCurrencyManager.IncreaseMoney(total_amount);
			allCollectables[coord].Clear();
			allCollectables.Remove(coord);
			isUpdatingCurrencyManager = false;
		}
	}

	public static void PlayCollectionSound()
	{
		audioPlayer.Play();
	}
}
