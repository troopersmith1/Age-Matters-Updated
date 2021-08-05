using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace AgeMatters
{
    [StaticConstructorOnStartup]
    public static class HiddenHediff
    {
        static HiddenHediff()
        {
            AgeMattersSettings settings = AgeMattersMod.mod.settings;

            if (settings.HiddenHediff)
            {
                AgeMattersDefOf.AgeMatters_baby.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_baby2.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_toddler.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_child.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_teenager.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_adult.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_adult2.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_adult3.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_old.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_old2.stages.First().becomeVisible = false;
                AgeMattersDefOf.AgeMatters_old3.stages.First().becomeVisible = false;
            }

        }
    }
}
