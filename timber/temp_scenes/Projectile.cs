using Godot;

public class Projectile : Actor
{
	public Vector3 targetPosition;
	public int damage;
	public Actor owner;

	public override void _Ready()
	{
		base._Ready();
		state_manager.defaultState = "ProjectileState";
	}

	float rotationSpeed = 5, speed = 5f;
	public override void _Process(float delta)//different projectile movements: projectile motion, 
	{
		base._Process(delta);
	}
	

	public override void Configure(ActorConfig info)
	{
		config = info;
		GetNode<HasTeam>("HasTeam").team = config.team;

		// if (config.pre_ko_sprite_filename != null && config.pre_ko_sprite_filename != "")
		// 	ArborResource.Load<Texture>("images/" + config.pre_ko_sprite_filename);
		// if (config.ko_sprite_filename != null && config.ko_sprite_filename != "")
		// 	ArborResource.Load<Texture>("images/" + config.ko_sprite_filename);
		
		ArborResource.UseResource<Texture>(
			"images/" + config.idle_sprite_filename,
			(Texture tex) =>
			{
				GD.Print("Loaded texture: " + tex);
				ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();

				view.Scale = new Vector3(tex.GetWidth(), tex.GetHeight(), 1.0f) * 0.02f;
				view.Scale = view.Scale * config.aesthetic_scale_factor;
				initial_load = true;
				initial_view_scale = view.Scale;
				desired_scale_x = initial_view_scale.x;
				initial_rotation = view.Rotation;

				char_mat.SetShaderParam("texture_albedo", tex);
				char_mat.SetShaderParam("fade_amount ", 0.8f);
				char_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);

				character_view.SetSurfaceMaterial(0, char_mat);
			},
			this
		);

		StateManager _stateManager = GetNode<Node>("StateManager") as StateManager;
		state_manager.Configure(config.stateConfigs);

		// IdleState _idleState = _stateManager.states["IdleState"] as IdleState;
		// _idleState.has_idle_animation = false;

	}

	public void setTarget(Vector3 target)
	{
		ProjectileState projectileState = state_manager.states["ProjectileState"] as ProjectileState;
		projectileState.setTarget(target);
	}

	public void setDamage(int d)
	{
		damage = d;
	}

}
