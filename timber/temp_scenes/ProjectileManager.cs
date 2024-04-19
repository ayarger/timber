using Godot;
using System;
using System.Collections.Generic;

// Press T to toggle placement
// Press X to toggle removal

public class ProjectileManager : Node
{

	public List<Projectile> projectiles = new List<Projectile>();

	public static ProjectileManager instance;

	public override void _Ready()
	{
		instance = this;
	}

	//Position to spawn at; Offset from bottom of actor; target position; actor who created projectile; damage
	public void SpawnProjectile(Vector3 position, Vector3 offset, Vector3 target, Actor owner, int damage)
	{
		// ToastManager.SendToast(this, "Tower coord: [" + cur.x + "," + cur.z + "]", ToastMessage.ToastType.Notice, 1f);
		ActorConfig config = new ActorConfig();
		config.name = "Projectile";
		config.idle_sprite_filename = "acorn.png";
		config.aesthetic_scale_factor = 0.05f;

		HasTeam team = owner.GetNode<HasTeam>("HasTeam");
		if (team != null)
        {
			config.team = team.team;
        }
		else config.team = "player";

		Projectile new_projectile = SpawnProjectileOfType(config, position);
		new_projectile.view.Translation += offset;
		new_projectile.setTarget(target);
		new_projectile.setDamage(damage);
		new_projectile.owner = owner;
		projectiles.Add(new_projectile);

	}

	public Projectile SpawnProjectileOfType(ActorConfig config, Vector3 position)
	{
		/* Spawn actor scene */
		PackedScene projectile_scene = (PackedScene)ResourceLoader.Load("res://temp_scenes/Projectile.tscn");
		Spatial new_projectile = (Spatial)projectile_scene.Instance();
		new_projectile.Name = config.name;
		AddChild(new_projectile);

		Projectile projectile_script = new_projectile as Projectile;

		new_projectile.GlobalTranslation = position;

		projectile_script.Configure(config);

		return projectile_script;

	}

	//void RemoveTower(Vector3 cursorPos)
	//{
	//	Vector3 pos = new Vector3(cursorPos.x, 0, cursorPos.z);
	//	Coord cur = Grid.ConvertToCoord(pos);
	//	status = TowerManagerStatus.idle;

	//	for (int i = 0; i < tower_spawn_positions.Count; i++)
	//	{
	//		Tower tower = tower_spawn_positions[i];
	//		if (Grid.Get(cur).actor != null && Grid.Get(cur).actor.Equals(tower))
	//		{
	//			tower_spawn_positions.RemoveAt(i);
	//			tower.QueueFree();
	//			Grid.Get(cur).actor = null;
	//			Grid.Get(cur).value = '.';
	//			break;
	//		}
	//	}
	//}


}
