using Godot;

// TODO: switch to HasStat
// When towers are spawned, their team is set to "construction" and has no combatState
// they will attract "player" actors with ConstructionState to attack them, 
// and when construction progress bar is filled, 
// the tower switches to "player" team, and starts functioning.

public class Tower : Actor
{
	public enum TowerType
	{
		Normal,
	}
	public enum TowerStatus
	{
		Functioning,
		AwaitConstruction,
		InConstruction,
	}

	public Coord curr_coord;
	public TowerStatus towerStatus;

	public HasStats _HasStats;
	public Timer timer;

	public override void _Ready()
	{
		base._Ready();
		SetProcessInput(true);
		timer = GetNode<Timer>("Timer");
		_HasStats = GetNode<HasStats>("HasStats");
		_HasStats.AddStat("construction_progress", 0, 50, 0, true);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Scancode == (uint)KeyList.Z)
		{
			GD.Print("Test Tower reaction.");
		}
		
	}

	public void HandleTowerStatusChange(TowerStatus newTowerStatus)
	{
		
		if (newTowerStatus == TowerStatus.AwaitConstruction)
		{
			towerStatus = TowerStatus.AwaitConstruction;
		}

		else if (newTowerStatus == TowerStatus.InConstruction)
		{
			towerStatus = TowerStatus.InConstruction;
			
			// Debug
			ToastManager.SendToast(this, "Switch to InConstruction", ToastMessage.ToastType.Notice);
		}
		
		else if (newTowerStatus == TowerStatus.Functioning)
		{
			towerStatus = TowerStatus.Functioning;
			config.team = "player";
			GetNode<HasTeam>("HasTeam").team = config.team;
			
			// enable combatstate
			ToastManager.SendToast(this, "Switch to Functioning", ToastMessage.ToastType.Notice);
			// state_manager.SetProcess(true);
		}
	} 

	public override void Configure(ActorConfig info)
	{
		config = info;
		GetNode<HasTeam>("HasTeam").team = config.team;
		EventBus.Publish(new SpawnLightSourceEvent(this, true));

		// if (config.pre_ko_sprite_filename != null && config.pre_ko_sprite_filename != "")
		// 	ArborResource.Load<Texture>("images/" + config.pre_ko_sprite_filename);
		// if (config.ko_sprite_filename != null && config.ko_sprite_filename != "")
		// 	ArborResource.Load<Texture>("images/" + config.ko_sprite_filename);

		Texture tex = (Texture)ResourceLoader.Load("res://temp_scripts/TempTowerSprite.png");

		ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();

		shadow_view = (MeshInstance)GetNode("shadow");
		view.Scale = new Vector3(tex.GetWidth(), tex.GetHeight(), 1.0f) * 0.01f;
		view.Scale = view.Scale * config.aesthetic_scale_factor;
		initial_load = true;
		initial_view_scale = view.Scale;
		desired_scale_x = initial_view_scale.x;
		initial_rotation = view.Rotation;
		shadow_view.Scale = new Vector3(Mathf.Min(2.0f, view.Scale.x), shadow_view.Scale.y, shadow_view.Scale.z);

		char_mat.SetShaderParam("texture_albedo", tex);
		char_mat.SetShaderParam("fade_amount ", 0.8f);
		char_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);

		character_view.SetSurfaceMaterial(0, char_mat);

		StateManager _stateManager = GetNode<Node>("StateManager") as StateManager;
		// IdleState _idleState = _stateManager.states["Idle"] as IdleState;
		// _idleState.has_idle_animation = false;
		
		state_manager.Configure(config.stateConfigs);

		StatManager statManager = GetNode<StatManager>("StatManager");
		if (statManager != null)
		{
			statManager.Config(config.statConfig);
		}
	}

	public override void Hurt(int damage, bool isCritical, Actor source)
	{
		if (config.team == "construction")
		{
			if (towerStatus == TowerStatus.AwaitConstruction)
			{
				HandleTowerStatusChange(TowerStatus.InConstruction);
			}
			
			// No critical attack for construction
			DamageTextManager.DrawText(damage, this, "construction");
			if(_HasStats != null)
			{
				_HasStats.GetStat("construction_progress").IncreaseCurrentValue(damage);
				if(_HasStats.GetStat("construction_progress").currVal >= _HasStats.GetStat("construction_progress").maxVal)
				{
					HandleTowerStatusChange(TowerStatus.Functioning);
					return;
				}
			}
			return;
		}

		// Avoid civil war
		if (source.GetNode<HasTeam>("HasTeam").team == "player") return;
		
		base.Hurt(damage, isCritical, source);
	}

	public override void _ExitTree()
	{
		Grid.Get(curr_coord).actor = null;
		Grid.Get(curr_coord).value = '.';
		foreach (Node child in GetChildren())
		{
			if (child is IsSelectable childScript)
			{
				childScript.OnRemovingParent();
			}
		}
		EventBus.Publish(new RemoveLightSourceEvent(GlobalTranslation));
		base._ExitTree();
	}
}

// public class EventTowerStatusChange
// {
// 	public Tower.TowerStatus towerStatus;
// 	public Tower targetTower;
//
// 	public EventTowerStatusChange (Tower _targetTower, Tower.TowerStatus _towerStatus)
// 	{
// 		targetTower = _targetTower;
// 		towerStatus = _towerStatus;
// 	}
// }
