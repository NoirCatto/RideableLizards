using System;
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
        if (crit.creatureTemplate.ancestor.type == CreatureTemplate.Type.LizardTemplate)
        {
            lizData = LizardDeets.GetValue(crit, _ => new LizardData(crit));
            return true;
        }

        Log.Once($"Creature {crit.ToString()} is NOT a Lizard!!!");
        lizData = null;
        return false;
    }
}

public class LizardData(AbstractCreature owner)
{
    public AbstractCreature Owner = owner;

    public Player Rider => Riders.FirstOrDefault();
    public IEnumerable<Player> Riders
    {
        get
        {
            var self = (Lizard)Owner.realizedCreature;
            return self?.grabbedBy.Select(x => x?.grabber).OfType<Player>().Where(player => self.LikesPlayer(player));
        }
    }
}