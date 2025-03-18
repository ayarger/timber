using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using Amazon.S3.Model;
using static System.Net.Mime.MediaTypeNames;

public class actorDeathEvent
{
	public Actor actor;
	public actorDeathEvent(Actor a)
	{
		 actor = a;
	}
}

public class actorHurtEvent
{
	public Actor actor;
	public int damage;
	public bool isCritical;
	public Actor source;
	public actorHurtEvent(Actor a, int d, bool c, Actor s)
	{
		actor = a;
		damage = d;
		isCritical = c;
		source = s;
	}
}

public class Actor : Spatial
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text"; 

	public ActorConfig config { get; protected set; }
	public String actorName { get { return config.name; } }

	public Spatial view { get; protected set; } // Good for scaling operations.
	protected MeshInstance character_view;
	protected MeshInstance character_view_shadow;
	protected ShaderMaterial character_material;
	protected MeshInstance shadow_view;
	protected StateManager state_manager;
   	protected RigidBody rb;

	protected HasTeam has_team;
	public string team
	{
		get
		{
			return has_team.team;
		}
		set
		{
			has_team.team = value;
			//TODO: RESET VARIOUS THINGS
		}
	}
   

	protected IsSelectable selectable;

	protected Texture idle_sprite;
	//Initial animation variables
	public bool initial_load = false;
	public Vector3 initial_view_scale { get; protected set; } = Vector3.One;
	public Vector3 initial_rotation { get; protected set; } = Vector3.Zero;
	public TileData currentTile = null;

	public Dictionary<string, Texture> sprite_textures = new Dictionary<string, Texture>();

	protected float desired_scale_x = 1.0f;
	public float GetDesiredScaleX() { return desired_scale_x; }
	private Subscription<TileDataLoadedEvent> sub;

	bool dying = false;
	bool isInvicible = false;
	bool actorKO = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		previous_position = GlobalTranslation;

		view = (Spatial)GetNode("view");
		state_manager = (StateManager)GetNode("StateManager");
		character_view = (MeshInstance)GetNode("view/mesh");
		character_view_shadow = (MeshInstance)GetNode("view/shadowMesh");
		character_material = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();
		shadow_view = (MeshInstance)GetNode("shadow");
        selectable = GetNodeOrNull<IsSelectable>("IsSelectable");
		rb = GetNode<RigidBody>("RigidBody");
		has_team = GetNode<HasTeam>("HasTeam");
		time = GlobalTranslation.x + GlobalTranslation.z;
		animation_offset = GD.Randf() * 100.0f;
		sub = EventBus.Subscribe<TileDataLoadedEvent>((TileDataLoadedEvent e) =>
		{
			TileData td = Grid.Get(GlobalTranslation);
			currentTile = td;
			td.actor = this;
		});
		// update ActorDict after actor is loaded into the scene
		UpdateActorDict();
	}

	public virtual void Configure(ActorConfig info)
	{
		config = info;
		has_team.team = config.team;
		if (config.team == "player")
		{
			EventBus.Publish<SpawnLightSourceEvent>(new SpawnLightSourceEvent(this));
		}

		if (!String.IsNullOrEmpty(config.pre_ko_sprite_filename))
		{
			ArborResource.Load<Texture>("images/" + config.pre_ko_sprite_filename);
		}else{
			config.pre_ko_sprite_filename = config.idle_sprite_filename;
		}
			
		if (String.IsNullOrEmpty(config.ko_sprite_filename))
		{
			config.ko_sprite_filename = config.idle_sprite_filename;
		}
		ArborResource.Load<Texture>("images/" + config.ko_sprite_filename);
		actorKO = true;

		ArborCoroutine.StartCoroutine(loadTextures(), this);

		ArborResource.UseResource<Texture>(
			"images/" + config.idle_sprite_filename,
			(Texture tex) =>
			{
				ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0).Duplicate();
				ShaderMaterial char_mat_shadow = (ShaderMaterial)character_view_shadow.GetSurfaceMaterial(0).Duplicate();
				ShaderMaterial shadow_mat = (ShaderMaterial)shadow_view.GetSurfaceMaterial(0).Duplicate();

				/* Scale */
				view.Scale = new Vector3(tex.GetWidth(), tex.GetHeight(), 1.0f) * 0.01f;
				view.Scale = view.Scale * config.aesthetic_scale_factor;
				rb.Scale = new Vector3(view.Scale.x, view.Scale.y, rb.Scale.z);
				rb.GlobalTranslation = new Vector3(rb.GlobalTranslation.x, rb.GlobalTranslation.y+rb.Scale.y/2, rb.GlobalTranslation.z);
				initial_load = true;
				initial_view_scale = view.Scale;
				desired_scale_x = initial_view_scale.x;
				initial_rotation = view.Rotation;

				shadow_view.Scale = new Vector3(Mathf.Min(2.0f, view.Scale.x), shadow_view.Scale.y, shadow_view.Scale.z);

				char_mat.SetShaderParam("texture_albedo", tex);
				char_mat_shadow.SetShaderParam("texture_albedo", tex);

				char_mat.SetShaderParam("fade_amount ", 0.8f);
				char_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);

				shadow_mat.SetShaderParam("fade_amount ", 0.8f);
				shadow_mat.SetShaderParam("alpha_cutout_threshold  ", 0.2f);

				//character_material.AlbedoTexture = idle_sprite;
				character_view.SetSurfaceMaterial(0, char_mat);
				character_view_shadow.SetSurfaceMaterial(0, char_mat_shadow);

				shadow_view.SetSurfaceMaterial(0, shadow_mat);

				sprite_textures["idle"] = tex;
				GD.Print("IDLE TEXTURE SET FOR: " + config.name);
			},
			this
		);
		if(config.team == "player"){
			foreach(var config in config.stateConfigs){
			}
			//foreach(var config in config.stateConfigs){
			//	GD.Print("CONFIG: " + config.name);
			//}
		}

		state_manager.Configure(config.stateConfigs);

		StatManager statManager = GetNode<StatManager>("StatManager");
		if(statManager != null)
		{
			statManager.Config(config.statConfig);
		}
	}



//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		time += delta;
		ProcessColor();
		SetShaderParams();

		ComputeSharedAnimationData();
	}

	Vector3 previous_position = Vector3.Zero;
	void ComputeSharedAnimationData()
	{

		/* Determine which direction the actor would generally prefer to look in. */
		Vector3 position_delta = GlobalTranslation - previous_position;
		//GD.Print("COMPUTE ANIM!!! " + GlobalTranslation.x + " vs " + previous_position.x);

		if (position_delta.x > 0.01f)
			desired_scale_x = initial_view_scale.x;
		if (position_delta.x < -0.01f)
			desired_scale_x = -initial_view_scale.x;


		previous_position = GlobalTranslation;
	}

	float animation_offset = 0;

	float time = 0.0f;

	void ProcessColor()
	{
		Color desired_color = new Color(1, 1, 1, 1.0f);

		/* Fade if cursor behind actor (helps player see behind tall actors) */
		if (view.Scale.y >= 2.0f)
		{
			float distance_to_cursor = GlobalTranslation.DistanceTo(SelectionSystem.GetTilePosition());
			float z_axis_difference = SelectionSystem.GetTilePosition().z - GlobalTranslation.z;

			if (distance_to_cursor <= 4.1f && z_axis_difference < -0.55f)
				desired_color.a = 0.35f;
			else
				desired_color.a = 1.0f;
		}

		//character_material.AlbedoColor += (desired_color - character_material.AlbedoColor) * 0.1f;
	}

	void SetShaderParams()
	{
		ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0);

		char_mat.SetShaderParam("fowTexture",FogOfWar.actorInstance.GetTexture());
		char_mat.SetShaderParam("world_position", GlobalTranslation);
		char_mat.SetShaderParam("screenWidth", FogOfWar.instance.screenWidth);
		char_mat.SetShaderParam("screenHeight", FogOfWar.instance.screenHeight);
		char_mat.SetShaderParam("screenPosX", FogOfWar.instance.screenPosX);
		char_mat.SetShaderParam("screenPosZ", FogOfWar.instance.screenPosZ);

		ShaderMaterial char_mat_shadow = (ShaderMaterial)character_view_shadow.GetSurfaceMaterial(0);

		char_mat_shadow.SetShaderParam("fowTexture", FogOfWar.actorInstance.GetTexture());
		char_mat_shadow.SetShaderParam("world_position", GlobalTranslation);
		char_mat_shadow.SetShaderParam("screenWidth", FogOfWar.instance.screenWidth);
		char_mat_shadow.SetShaderParam("screenHeight", FogOfWar.instance.screenHeight);
		char_mat_shadow.SetShaderParam("screenPosX", FogOfWar.instance.screenPosX);
		char_mat_shadow.SetShaderParam("screenPosZ", FogOfWar.instance.screenPosZ);

		ShaderMaterial shadow_mat = (ShaderMaterial)shadow_view.GetSurfaceMaterial(0);

		shadow_mat.SetShaderParam("fowTexture", FogOfWar.actorInstance.GetTexture());
		shadow_mat.SetShaderParam("world_position", GlobalTranslation);
		shadow_mat.SetShaderParam("screenWidth", FogOfWar.instance.screenWidth);
		shadow_mat.SetShaderParam("screenHeight", FogOfWar.instance.screenHeight);
		shadow_mat.SetShaderParam("screenPosX", FogOfWar.instance.screenPosX);
		shadow_mat.SetShaderParam("screenPosZ", FogOfWar.instance.screenPosZ);
	}

	IEnumerator loadTextures(){

		foreach(var sprites in config.sprite_filenames){
			string key = sprites.Key;
			string filename = sprites.Value;
			ArborResource.Load<Texture>("images/" + filename);
			//store the texture in the dictionary		}
		}
		//HARDCODE
		if(config.name == "Spot"){
			ArborResource.Load<Texture>("images/" + "spot_step_left.png");
			ArborResource.Load<Texture>("images/" + "spot_step_right.png");
			ArborResource.Load<Texture>("images/" + "spot_attack.png");
			yield return ArborResource.WaitFor("images/" + "spot_step_left.png");
			sprite_textures["walk_left"] = ArborResource.Get<Texture>("images/" + "spot_step_left.png");
			yield return ArborResource.WaitFor("images/" + "spot_step_right.png");
			sprite_textures["walk_right"] = ArborResource.Get<Texture>("images/" + "spot_step_right.png");
			yield return ArborResource.WaitFor("images/" + "spot_attack.png");
			sprite_textures["attack"] = ArborResource.Get<Texture>("images/" + "spot_attack.png");
		}

		foreach(var sprites in config.sprite_filenames){
			yield return ArborResource.WaitFor("images/" + sprites.Value);
			Texture tex = ArborResource.Get<Texture>("images/" + sprites.Value);
			sprite_textures[sprites.Key] = tex;
		}
	}

	public virtual void Hurt(int damage, bool isCritical, Actor source = null)
	{
		int damage_to_deal = isCritical ? damage * 2 : damage;

        if (isCritical) // for testing
		{
			DamageTextManager.DrawText(damage_to_deal, this, "criticalDamage");
		}
		else
		{
			DamageTextManager.DrawText(damage, this, "damage");
		}

		//animate hurt - change to red color for a duration
		//deal damage
		HasStats stat = GetNode<HasStats>("HasStats");
		if (stat == null)
			return;

		stat.GetStat("health").DecreaseCurrentValue(damage_to_deal);
		if(stat.GetStat("health").currVal <= 0)
		{
			Kill(source);
			return;
		}

            //draws aggro
		if (state_manager.states.ContainsKey("CombatState"))
		{
			ChaseState c = (state_manager.states["ChaseState"] as ChaseState);
			if (source != null && !state_manager.IsStateActive("CombatState"))
			{
				if (!state_manager.IsStateActive("MovementState") && !state_manager.IsStateActive("ChaseState"))
				{
					c.TargetActor = source;
					state_manager.EnableState("ChaseState");
				}
			}
		}
        

		ArborCoroutine.StartCoroutine(HurtAnimation(), this);
	}

	public void Kill(Actor source = null)
	{
		if (dying)
			return;

		dying = true;

        if (IsQueuedForDeletion()) return;
		if(currentTile != null) currentTile.actor = null;
		bool endGame = config.name == "Spot";

		//Flag Lua Engine
		NLuaScriptManager.Instance.KillActor(this);
		ArborCoroutine.StopCoroutinesOnNode(this);
        PackedScene scene = (PackedScene)ResourceLoader.Load("res://scenes/ActorKO.tscn");
        ActorKO new_ko = (ActorKO)scene.Instance();
        GetParent().AddChild(new_ko);
        new_ko.GlobalTranslation = GlobalTranslation;
        new_ko.GlobalRotation = GlobalRotation;
        new_ko.Scale = Scale;

        if (actorKO)
		{
            new_ko.Configure(ArborResource.Get<Texture>("images/" + config.pre_ko_sprite_filename), ArborResource.Get<Texture>("images/" + config.ko_sprite_filename), endGame, source);
        }
		else
		{
            new_ko.Configure(ArborResource.Get<Texture>("images/" + config.idle_sprite_filename), ArborResource.Get<Texture>("images/" + config.idle_sprite_filename), endGame, source);
        }

        QueueFree();
	}

	public void SetActorTexture(string texture_name){
		if(sprite_textures.ContainsKey(texture_name))
		{
			Texture tex = sprite_textures[texture_name];
			character_material.SetShaderParam("texture_albedo", tex);
			character_view.SetSurfaceMaterial(0, character_material);
		}else{
			if(!sprite_textures.ContainsKey("idle"))
				return;
			texture_name = "idle";
			Texture tex = sprite_textures[texture_name];
			character_material.SetShaderParam("texture_albedo", tex);
			character_view.SetSurfaceMaterial(0, character_material);
		}
	}

   public void UpdateActorDict()
	{
		ConsoleManager console_manager = GetParent().GetNode<ConsoleManager>("../CanvasLayer/TempConsole/ConsoleManager");
		console_manager.ActorDict[this.Name.ToLower()] = this;
	}

   public override void _ExitTree()
   {
	   EventBus.Unsubscribe(sub);
	   base._ExitTree();
   }

	IEnumerator HurtAnimation()//TODO add actual animation
	{
		//turn color
		ShaderMaterial char_mat = (ShaderMaterial)character_view.GetSurfaceMaterial(0);
		char_mat.SetShaderParam("apply_red_tint", 1);
		yield return ArborCoroutine.WaitForSeconds(0.2f);
		char_mat.SetShaderParam("apply_red_tint", 0);
		//unturn color
	}

	public void setInvicible(bool invicible)
	{
		isInvicible = invicible;
	}

	public ActorConfig GetActorConfig()
	{
		return config;
	}

	public bool IsStateActive(string state)
	{
		return state_manager.IsStateActive(state);
	}
}
