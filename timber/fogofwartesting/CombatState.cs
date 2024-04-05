using Godot;
using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class CombatState : ActorState
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    protected int attackRange = 2;//number of grids
    protected int attackDamage = 10;
    protected float criticalHitRate = 0.3f;

    protected float attackWindup = 0.5f;//animation before attack
    protected float attackRecovery = 0.125f;//anim after attack
    protected float attackCooldown = 1f;

    protected bool attackable = true;
    protected bool attacking = false;

    public Actor TargetActor;

    public override string name
    {
        get { return "CombatState"; }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
        base._Ready();
    }

    public override void Config(StateConfig stateConfig)
    {
        CombatConfig c = stateConfig as CombatConfig;
        attackRange = c.attackRange;
        attackDamage = c.attackDamage;
        criticalHitRate = c.criticalHitRate;
        attackWindup = c.attackWindup;
        attackRecovery = c.attackRecovery;
        attackCooldown = c.attackCooldown;
    }


    public abstract Coord FindClosestTileInRange(Coord cur);

    public abstract bool WithinRange(Coord pos);
}
