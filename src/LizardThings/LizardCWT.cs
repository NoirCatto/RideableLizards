using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RideableLizards.LizardThings;

public static class LizardCWT
{
    public static readonly ConditionalWeakTable<AbstractCreature, LizardData> LizardDeets = new ConditionalWeakTable<AbstractCreature, LizardData>();

    public static bool TryGetLizardData(this Lizard liz, out LizardData lizardData) => TryGetLizardData(liz.abstractCreature, out lizardData);
    public static bool TryGetLizardData(this AbstractCreature crit, out LizardData lizData)
    {
        if (crit.creatureTemplate.ancestor?.type == CreatureTemplate.Type.LizardTemplate)
        {
            lizData = LizardDeets.GetValue(crit, _ => new LizardData(crit));
            return true;
        }

        lizData = null;
        return false;
    }
}

public class LizardData(AbstractCreature owner)
{
    private const bool DEBUG = false; //show debug visualizer

    public readonly DebugDestinationVisualizer DestinationVisualizer = !DEBUG ? null : new DebugDestinationVisualizer
        (owner.world.game.abstractSpaceVisualizer, owner.world, owner.abstractAI?.RealAI.pathFinder, Color.green);

    public readonly AbstractCreature Owner = owner;
    public Lizard Liz => Owner.realizedCreature as Lizard;
    public Player Rider => Riders.FirstOrDefault();
    public IEnumerable<Player> Riders
    {
        get
        {
            return Liz?.grabbedBy.Select(x => x?.grabber).OfType<Player>().Where(player => Liz.LikesPlayer(player));
        }
    }

    public bool Jumping;
    public int JumpCounter;
    public const int JumpLinger = 15;
    public LizardJumpModule.JumpFinder JumpFinder;
    public bool BitCreature;

    public void Update(bool eu)
    {
        if (DEBUG)
        {
            DestinationVisualizer.Update();
            var room = Liz?.room;
            if (room != null && DestinationVisualizer.room != room)
                DestinationVisualizer.ChangeRooms(room);
        }

        var lizGraphics = (LizardGraphics)Liz?.graphicsModule;
        if (lizGraphics != null)
        {
            if (Jumping)
            {
                if (JumpCounter < JumpLinger)
                {
                    JumpCounter++;
                }
                else
                {
                    JumpCounter = 0;
                    Jumping = false;
                }
            }
        }

        if (BitCreature)
        {
            if (Rider == null || !Rider.input[0].pckp)
                BitCreature = false;
        }
    }

}