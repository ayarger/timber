

using Godot;

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

    public override string stateType
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
        attackRange = (int)stateConfig.stateStats["attackRange"];
        attackDamage = (int)stateConfig.stateStats["attackDamage"];
        criticalHitRate = stateConfig.stateStats["criticalHitRate"];
        attackWindup = stateConfig.stateStats["attackWindup"];
        attackRecovery = stateConfig.stateStats["attackRecovery"];
        attackCooldown = stateConfig.stateStats["attackCooldown"];
    }

    public Actor findEnemyInRange()
    {
        Actor target = null;
        foreach (var actors in GetAttackableActorList())
        {
            
            var actorInRange = actors as Actor;
            if (actorInRange != null && actorInRange.GetNode<HasTeam>("HasTeam").team != actor.GetNode<HasTeam>("HasTeam").team)
            {
                Coord actorPos = Grid.ConvertToCoord(actorInRange.GlobalTranslation);
                Coord cur = Grid.ConvertToCoord(actor.GlobalTranslation);
                if ((cur - actorPos).Mag() < attackRange)
                {
                    target = actorInRange;
                    break;
                }
            }
        }
        return target;
    }
}
