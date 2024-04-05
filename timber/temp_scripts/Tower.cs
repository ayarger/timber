using Godot;

public class Tower : Actor
{
	public enum TowerType
	{
		Normal,
	}
	
	public override void _Ready()
	{
		base._Ready();
		SetProcessInput(true);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Scancode == (uint)KeyList.Z)
		{
			GD.Print("Test Tower reaction.");
		}
		
	}

	public override void Configure(ActorConfig info)
	{
		config = info;
		GetNode<HasTeam>("HasTeam").team = config.team;
		if (config.team == "player")
		{
			EventBus.Publish<SpawnLightSourceEvent>(new SpawnLightSourceEvent(this, true));
		}

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
		IdleState _idleState = _stateManager.states["Idle"] as IdleState;
		_idleState.has_idle_animation = false;

		StatManager statManager = GetNode<StatManager>("StatManager");
		if (statManager != null)
		{
			statManager.Config(config.statConfig);
		}
	}

	public override void _ExitTree()
	{
		EventBus.Publish(new RemoveLightSourceEvent(GlobalTranslation));
		foreach (Node child in GetChildren())
		{
			if (child is IsSelectable childScript)
			{
				childScript.OnRemovingParent();
			}
		}
		base._ExitTree();
	}

}
