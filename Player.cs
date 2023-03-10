using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using On.VoidSea;
using UnityEngine;

namespace RideableLizards;

public partial class RideableLizards
{
    #region SlugDeets
    public class SlugData
    {
        public readonly List<float> SlugChunksMass; //List,  in case a custom slug has more than 2 bodychunks...`

        public Lizard GrabbedLiz;

        public SlugData(Lizard grabbedLiz)
        {
            GrabbedLiz = grabbedLiz;
            SlugChunksMass = new List<float>();
        }
    }
    private static readonly Dictionary<Player, SlugData> SlugDeets = new Dictionary<Player, SlugData>();
    #endregion
    
    private bool PlayerOnIsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
    {
        if (obj is Lizard liz && !liz.State.dead)
        {
            return false;
        }
        return orig(self, obj);
    }
    
    private class StopPlayerController : ArenaGameSession.PlayerStopController { }
    
    private void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        
        var friend = self.grasps.Select(x =>
            x?.grabbed).FirstOrDefault(y => y is Lizard liz &&
            LikesPlayer(self, liz));

        if (friend != null && friend is Lizard l && !l.State.dead) //If grabbed onto an alive liz
        {
            if (l.grabbedBy.Select(x => x.grabber).First(x => x is Player) == self) //If first player holding a lizard
            {
                self.controller = new StopPlayerController(); //Stop player's inputs
            }

            if (!SlugDeets.ContainsKey(self))
            {
                SlugDeets.Add(self, new SlugData(l));

                foreach (var chunk in self.bodyChunks)
                {
                    SlugDeets[self].SlugChunksMass.Add(chunk.mass);
                    chunk.mass *= 0.1f; //Help the lizard a little
                    Debug.Log($"Orig Slug Chunk {chunk.index} mass: {SlugDeets[self].SlugChunksMass[chunk.index]} | New Slug Chunk {chunk.index} mass: {chunk.mass}");
                }
            }

        }
        else //If not grabbed onto liz
        {
            if (self.controller is StopPlayerController)
            {
                self.controller = null; //Return control to the player
            }

            if (SlugDeets.ContainsKey(self))
            {
                foreach (var chunk in self.bodyChunks)
                {
                    chunk.mass = SlugDeets[self].SlugChunksMass[chunk.index] ; //Help not needed anymore
                    Debug.Log($"Restored Slug Chunk {chunk.index} mass: {chunk.mass}");
                    
                }
                SlugDeets.Remove(self);
            }
        }
    }

}