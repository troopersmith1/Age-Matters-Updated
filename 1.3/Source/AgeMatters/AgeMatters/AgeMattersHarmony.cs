using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HugsLib;
using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;

namespace AgeMatters
{
    public class Harmony : ModBase
    {
        //This fix provided by Dylan, author of Children, School, and Learning

        public static bool ModCSL_ON = Pawn_AgeTracker_AdultMinAge_Patch.StartsWithIsModActive("Children, school and learning");


        [HarmonyPatch(typeof(Pawn_AgeTracker))]
        [HarmonyPatch("TicksToAdulthood", MethodType.Getter)]
        public static class Pawn_AgeTracker_TicksToAdulthood_Patch
        {
            [HarmonyPostfix]
            public static void TicksToAdulthood_Done(Pawn_AgeTracker __instance, Pawn ___pawn, ref long __result)
            {
                try
                {
                    if (__instance != null && ___pawn != null && Pawn_AgeTracker_AdultMinAge_Patch.IsTheMinAgeFixActive(___pawn))
                    {
                        int lifeStageCount = ___pawn.RaceProps.lifeStageAges.Count;
                        float AdultMinAge = Pawn_AgeTracker_AdultMinAge_Patch.PawnDefaultAdultAge(___pawn, (lifeStageCount <= 0 ? 0f : ___pawn.RaceProps.lifeStageAges[lifeStageCount - 1].minAge));
                        __result = FloorToLong(AdultMinAge * 3600000f) - __instance.AgeBiologicalTicks; //this uses FloorToInt in the basegame, which overflows..
                    }
                }
                catch //(Exception e)
                {
                   // if (Prefs.DevMode == true) { Log.Error("Age Matters: Pawn_AgeTracker TicksToAdulthood: error: " + e.Message); }
                }
            }
            public static long FloorToLong(float f)
            {
                return (long)Math.Floor((double)f);
            }
        }
        [HarmonyPatch(typeof(Pawn_AgeTracker))]
        [HarmonyPatch("AdultMinAge", MethodType.Getter)]
        public static class Pawn_AgeTracker_AdultMinAge_Patch
        {
            public static System.Reflection.MethodInfo GetMinAgeForAdulthoodHARMethodInfo;

            [HarmonyPostfix]
            public static void AdultMinAge_Done(Pawn ___pawn, ref float __result)
            {
                try
                {
                    if (___pawn != null && IsTheMinAgeFixActive(___pawn))
                    {
                        __result = PawnDefaultAdultAge(___pawn, __result);
                    }
                }
                catch //(Exception e)
                {
                   // if (Prefs.DevMode == true) { Log.Error("Age Matters: Pawn_AgeTracker AdultMinAge: error: " + e.Message); }
                }
            }

            public static bool IsTheMinAgeFixActive(Pawn pawn)
            {
                AgeMattersSettings settings = AgeMattersMod.mod.settings;
                try
                {
                    if (settings.FixLifestages && pawn != null && pawn.RaceProps != null && pawn.RaceProps.Humanlike)
                    { 
                        return true;
                    }
                }
                catch { }

                return false;
            }

            public static float PawnDefaultAdultAge(Pawn pawn, float previousResult)
            {
                float resultNow = 20f;
                try
                {
                    if (pawn != null)
                    {
                        float resultSoFar = previousResult;
                        float resultHAR = -999f;
                        float resultCSL = -999f;
                        float resultBoth = -999f;
                        int lifeStageCount = 1;
                        float upperLimit = 999999F; //no longer really a limit.. (just in case, was 590f before with FloorToInt) 
                        try { lifeStageCount = pawn.RaceProps.lifeStageAges.Count; } catch { }
                        resultCSL = GetChildRaceAgeAdultFallbackGiven(pawn, -999, false);
                        try
                        {


                            /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
                            //resultCSL & resultHAR would benefit from caching of some sort (on pawn?!)
                            /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


                            /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
                            /*!!!!! This normally happens on mod controller init in public static variables before harmony.PatchAll is called -- START !!!!!!!*/
                            bool ModAlienRace_ON;
                            bool ModAlienRaces_HarmonyPatchesFound = false;
                            Type ModAlienRaces_HarmonyPatches;
                            ModAlienRaces_HarmonyPatches = null;
                            ModAlienRace_ON = StartsWithIsModActive("Humanoid Alien Races");
                            if (ModAlienRace_ON) { try { ModAlienRaces_HarmonyPatches = GetTypeFromActiveAssemblies("AlienRace", "AlienRace.HarmonyPatches", out ModAlienRaces_HarmonyPatchesFound); } catch { ModAlienRaces_HarmonyPatchesFound = false; } }
                            /*!!!!! This normally happens on mod controller init in public static variables before harmony.PatchAll is called -- END !!!!!!!*/
                            /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

                            if (ModAlienRaces_HarmonyPatchesFound)
                            {
                                //AlienRaces active 
                                if (GetMinAgeForAdulthoodHARMethodInfo == null) { GetMinAgeForAdulthoodHARMethodInfo = ModAlienRaces_HarmonyPatches.GetMethod("GetMinAgeForAdulthood", new Type[] { typeof(Pawn), typeof(float) }); }
                                resultHAR = (float)GetMinAgeForAdulthoodHARMethodInfo.Invoke((object)null, new object[] { pawn, resultSoFar });
                            }
                        }
                        catch //(Exception e)
                        {
                           // if (Prefs.DevMode == true) { Log.Error("Age Matters: GetMinAgeForAdulthoodHAR: error: " + e.Message); }
                        }
                        if (resultHAR > resultCSL)
                        {
                            resultBoth = resultHAR;
                        }
                        else
                        {
                            resultBoth = resultCSL; //take mine most of the time here, HAR is mostly for other things, just as a fallback..
                        }
                        if (resultBoth >= 0 && resultBoth < upperLimit)
                        {
                            //ok result from both
                            resultNow = resultBoth;
                        }
                        else
                        {
                            if (resultSoFar < 0 || resultSoFar > upperLimit)
                            {
                                //fixed 20 age ..
                                resultNow = PawnDefaultAdultAgeIfNoneFound(pawn);
                            }
                            else
                            {
                                //ok..
                            }
                        }
                    }
                }
                catch //(Exception e)
                {
                   // if (Prefs.DevMode == true) { Log.Error("Age Matters: PawnDefaultAdultAge: error: " + e.Message); }
                }
                try { if (resultNow <= 0) { resultNow = 0.0001f; /*never return fully 0 because divide etc..*/ } } catch { }
                return resultNow;
            }
            public static float PawnDefaultAdultAgeIfNoneFound(Pawn pawn)
            {
                return 20f;
            }

            /*Helper stuff age - normally in other class*/
            public static float GetChildRaceAgeAdultFallbackGiven(Pawn pawn, float fallbackAge, bool log)
            {
                float Age = fallbackAge;
                float tmpAge = -1;
                try
                {
                    tmpAge = GetChildRaceAgeByString(pawn, "Adult", log);
                    if (tmpAge >= 0)
                    {
                        Age = tmpAge;
                        if (log == true) { Log.Message("Pawn: " + pawn.Name + " AdultAgeFound: " + Age.ToString()); }
                    }
                }
                catch //(Exception e)
                {
                   // if (Prefs.DevMode == true) { Log.Error("Age Matters: GetChildRaceAgeAdultFallbackGiven: error: " + e.Message); }
                }
                return Age;
            }
            public static float GetChildRaceAgeByString(Pawn pawn, string ageName, bool log)
            {
                float Age = -1;
                try
                {
                    if (pawn.kindDef != null && pawn.kindDef.RaceProps != null && pawn.kindDef.RaceProps.lifeStageAges != null)
                    {
                        foreach (LifeStageAge lfsA in pawn.kindDef.RaceProps.lifeStageAges)
                        {
                            if (lfsA.def.defName.ToLower().Contains(ageName.ToLower()))
                            {
                                Age = lfsA.minAge;
                                if (log == true) { Log.Message("Pawn: " + pawn.Name + " AgeFound: " + ageName + " - " + Age.ToString()); }
                                break;
                            }
                        }
                        if (Age == -1)
                        {
                            if (log == true) { Log.Message("Pawn: " + pawn.Name + "LifeStage: " + ageName + " not found! Race: " + pawn.kindDef.race.defName); }
                        }
                    }
                    else
                    {
                        if (log == true) { Log.Message("Pawn: " + pawn.Name + " Race not defined correctly!"); }
                    }
                }
                catch //(Exception e)
                {
                    //if (Prefs.DevMode == true) { Log.Error("Age Matters: GetChildRaceAgeByString: error: " + e.Message); }
                }
                return Age;
            }

            /*Helper stuff other mods - normally in other class*/
            public static bool StartsWithIsModActive(string modName)
            {
                foreach (ModMetaData allInstalledMod in ModLister.AllInstalledMods)
                {
                    if (allInstalledMod.Active && allInstalledMod.Name.ToLower().StartsWith(modName.ToLower()))
                        return true;
                }
                return false;
            }
            public static Type GetTypeFromActiveAssemblies(string modDLLName, string typeName, out bool typeFound)
            {
                foreach (System.Reflection.Assembly runningAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (runningAssembly.FullName.Contains(modDLLName))
                    {
                        try
                        {
                            typeFound = true;
                            return runningAssembly.GetType(typeName, true, false);
                        }
                        catch { }
                    }
                }
                typeFound = false;
                return null;
            }
        }
        //slightly different than CalculateInitialGrowth below!
        [HarmonyPatch(typeof(Pawn_AgeTracker), "RecalculateLifeStageIndex")]
        public static class Pawn_AgeTracker_RecalculateLifeStageIndex_Lifestage_Transpiler_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                try
                {
                    bool CSLNotActive = !ModCSL_ON;
                    AgeMattersSettings settings = AgeMattersMod.mod.settings;
                    bool TryFixLifestages = settings.FixLifestages;
                    if (TryFixLifestages && CSLNotActive)
                    {
                        bool instructionsFoundMinAgeLifeStage = false;
                        var codes = new List<CodeInstruction>(instructions);
                        for (int i = 0; i < codes.Count; i++)
                        {
                            if (codes[i].opcode == System.Reflection.Emit.OpCodes.Ldarg_0)
                            {
                                if ((i + 1) < codes.Count && codes[i + 1].opcode == System.Reflection.Emit.OpCodes.Ldfld && codes[i + 1].operand.ToString().EndsWith("pawn"))
                                {
                                    if ((i + 2) < codes.Count && codes[i + 2].opcode == System.Reflection.Emit.OpCodes.Callvirt && codes[i + 2].operand.ToString().EndsWith("get_RaceProps()"))
                                    {
                                        if ((i + 3) < codes.Count && codes[i + 3].opcode == System.Reflection.Emit.OpCodes.Ldfld && codes[i + 3].operand.ToString().EndsWith("lifeStageAges"))
                                        {
                                            if ((i + 12) < codes.Count && codes[i + 12].opcode == System.Reflection.Emit.OpCodes.Ldfld && codes[i + 12].operand.ToString().EndsWith("minAge"))
                                            {
                                                if ((i + 15) < codes.Count && codes[i + 15].opcode == System.Reflection.Emit.OpCodes.Call && codes[i + 15].operand.ToString().Contains("Lerp"))
                                                {
                                                    //found the right place!
                                                    //nop until minAge before this.growth + Lerp(float32, float32, float32)
                                                    for (int m = 2; m <= 12; m++)
                                                    {
                                                        codes[i + m].opcode = System.Reflection.Emit.OpCodes.Nop;
                                                    }
                                                    //call my func with 0.0f, pawn, growth instead of Lerp(float32, float32, float32)
                                                    codes[i + 15].opcode = System.Reflection.Emit.OpCodes.Call;
                                                    codes[i + 15].operand = AccessTools.Method(typeof(Pawn_AgeTracker_RecalculateLifeStageIndex_Lifestage_Transpiler_Patch), nameof(Pawn_AgeTracker_RecalculateLifeStageIndex_Lifestage_Transpiler_Patch.GetLifeStageIndexCurrentAge));
                                                    instructionsFoundMinAgeLifeStage = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (instructionsFoundMinAgeLifeStage == true)
                        {
                            if (Prefs.DevMode == true) { Log.Message("Age Matters: Transpiler(RecalculateLifeStageIndex): Patched!"); }
                        }
                        else
                        {
                            if (Prefs.DevMode == true) { Log.Error("Age Matters: Transpiler(RecalculateLifeStageIndex): Not patched! (maybe another mod messed things up?)"); } //todo real readable error + tips other mods etc.
                        }
                        return codes.AsEnumerable();
                    }
                }
                catch //(Exception e)
                {
                    // if (Prefs.DevMode == true) { Log.Error("Age Matters: Transpiler(RecalculateLifeStageIndex): error: " + e.Message); }
                }
                return instructions; //no modification
            }
            public static float GetLifeStageIndexCurrentAge(float lerpA, Pawn pawn, float growthGiven)
            {
                if (Pawn_AgeTracker_AdultMinAge_Patch.IsTheMinAgeFixActive(pawn))
                {
                    if (growthGiven < 1.0f)
                    {
                        try
                        {//vanilla esque version with growth
                            int lifeStageCount = pawn.RaceProps.lifeStageAges.Count;
                            return Mathf.Lerp(0.0f, Pawn_AgeTracker_AdultMinAge_Patch.PawnDefaultAdultAge(pawn, (lifeStageCount <= 0 ? 0f : pawn.RaceProps.lifeStageAges[lifeStageCount - 1].minAge)), growthGiven); //vanilla version..
                        }
                        catch { }
                    }
                    return pawn.ageTracker.AgeBiologicalYearsFloat; //otherwise use years!
                }
                else
                { //fallback close to vanilla..
                    int lifeStageCount = pawn.RaceProps.lifeStageAges.Count;
                    return Mathf.Lerp(0.0f, (lifeStageCount <= 0 ? 0f : pawn.RaceProps.lifeStageAges[lifeStageCount - 1].minAge), growthGiven); //vanilla version (more or less)..
                }
            }
        }
        [HarmonyPatch(typeof(Pawn_AgeTracker), "CalculateInitialGrowth")]
        public static class Pawn_AgeTracker_CalculateInitialGrowth_Lifestage_Transpiler_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                try
                {
                    bool CSLNotActive = !ModCSL_ON;
                    AgeMattersSettings settings = AgeMattersMod.mod.settings;
                    bool TryFixLifestages = settings.FixLifestages;
                    if (TryFixLifestages && CSLNotActive)
                    {
                        bool instructionsFoundMinAgeLifeStage = false;
                        var codes = new List<CodeInstruction>(instructions);
                        for (int i = 0; i < codes.Count; i++)
                        {
                            if (codes[i].opcode == System.Reflection.Emit.OpCodes.Ldarg_0)
                            {
                                if ((i + 1) < codes.Count && codes[i + 1].opcode == System.Reflection.Emit.OpCodes.Ldfld && codes[i + 1].operand.ToString().EndsWith("pawn"))
                                {
                                    if ((i + 2) < codes.Count && codes[i + 2].opcode == System.Reflection.Emit.OpCodes.Callvirt && codes[i + 2].operand.ToString().EndsWith("get_RaceProps()"))
                                    {
                                        if ((i + 3) < codes.Count && codes[i + 3].opcode == System.Reflection.Emit.OpCodes.Ldfld && codes[i + 3].operand.ToString().EndsWith("lifeStageAges"))
                                        {
                                            if ((i + 12) < codes.Count && codes[i + 12].opcode == System.Reflection.Emit.OpCodes.Ldfld && codes[i + 12].operand.ToString().EndsWith("minAge"))
                                            {
                                                //found the right place!
                                                codes[i + 2].opcode = System.Reflection.Emit.OpCodes.Call;
                                                codes[i + 2].operand = AccessTools.Method(typeof(Pawn_AgeTracker_CalculateInitialGrowth_Lifestage_Transpiler_Patch), nameof(Pawn_AgeTracker_CalculateInitialGrowth_Lifestage_Transpiler_Patch.GetMinAgeAdultLifestage));
                                                //nop rest until minAge before DIV
                                                for (int m = 3; m <= 12; m++)
                                                {
                                                    codes[i + m].opcode = System.Reflection.Emit.OpCodes.Nop;
                                                }
                                                instructionsFoundMinAgeLifeStage = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (instructionsFoundMinAgeLifeStage == true)
                        {
                            if (Prefs.DevMode == true) { Log.Message("Age Matters: Transpiler(CalculateInitialGrowth): Patched!"); }
                        }
                        else
                        {
                            if (Prefs.DevMode == true) { Log.Error("Age Matters: Transpiler(CalculateInitialGrowth): Not patched! (maybe another mod messed things up?)"); }
                        }
                        return codes.AsEnumerable();
                    }
                }
                catch //(Exception e)
                {
                    // if (Prefs.DevMode == true) { Log.Error("Age Matters: Transpiler(CalculateInitialGrowth): error: " + e.Message); }
                }
                return instructions; //no modification
            }
            public static float GetMinAgeAdultLifestage(Pawn pawn)
            {
                int lifeStageCount = pawn.RaceProps.lifeStageAges.Count;
                if (Pawn_AgeTracker_AdultMinAge_Patch.IsTheMinAgeFixActive(pawn))
                {
                    return Pawn_AgeTracker_AdultMinAge_Patch.PawnDefaultAdultAge(pawn, (lifeStageCount <= 0 ? 0f : pawn.RaceProps.lifeStageAges[lifeStageCount - 1].minAge));
                }
                else
                {
                    return (lifeStageCount <= 0 ? 0f : pawn.RaceProps.lifeStageAges[lifeStageCount - 1].minAge); //vanilla version (more or less)..
                }
            }
        }






        //This is called on map load for map.mapPawns.PawnsInFaction(Faction.OfPlayer)
        //LifestageCorrectionGrowth(pawn, true, 0.0125f); //now only do something when things deviate a bit more
        public override void MapLoaded(Map map)
        {
            foreach (Pawn pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
            {
                LifestageCorrectionGrowth(pawn, true, 0.0125f);
            }
        }
        public static void LifestageCorrectionGrowth(Pawn pawn, bool hasToDeviateMore, float minDeviation = 0.049f, bool log = true)
        {
            try
            {
                bool CSLNotActive = !ModCSL_ON;
                AgeMattersSettings settings = AgeMattersMod.mod.settings;
                bool TryFixLifestages = settings.FixLifestages;
                bool cachedTryFixGrowthSmallerThanShouldBeEnabled = true; //!!!!!!!!! these are normally based on mod-settings
                bool cachedTryFixGrowthLargerThanShouldBeEnabled = true; //!!!!!!!!! these are normally based on mod-settings
                if (TryFixLifestages && CSLNotActive)
                {
                    if (pawn != null && pawn.ageTracker != null && pawn.ageTracker.CurLifeStage != null && pawn.health != null && pawn.health.hediffSet != null && pawn.Spawned && (pawn.RaceProps != null && pawn.RaceProps.Humanlike))
                    { //all humanlikes now.. 
                      //these calculations are just used to see if changes are needed and to log (only triggers CalculateInitialGrowth and RecalculateLifeStageIndex if needed now with all fixes in place)
                        float growthCurrent = pawn.ageTracker.Growth;
                        int lifeStageCount = pawn.RaceProps.lifeStageAges.Count;
                        float AdultMinAge = Pawn_AgeTracker_AdultMinAge_Patch.PawnDefaultAdultAge(pawn, (lifeStageCount <= 0 ? 0f : pawn.RaceProps.lifeStageAges[lifeStageCount - 1].minAge));
                        float growthCaclulated = Mathf.Clamp01(pawn.ageTracker.AgeBiologicalYearsFloat / AdultMinAge);
                        if ((growthCurrent < 1.0f && growthCurrent < growthCaclulated && cachedTryFixGrowthSmallerThanShouldBeEnabled)
                        || (growthCaclulated < 1.0f && growthCurrent > growthCaclulated && cachedTryFixGrowthLargerThanShouldBeEnabled))
                        {
                            //try to see if growth should have changed.. 
                            bool updateValueNow = false;
                            if (hasToDeviateMore)
                            {
                                if (Mathf.Abs(growthCurrent - growthCaclulated) > minDeviation) { updateValueNow = true; } //large enough deviation!
                            }
                            else { updateValueNow = true; }
                            if (updateValueNow)
                            {
                                Traverse.Create(pawn.ageTracker).Method("CalculateInitialGrowth").GetValue(); //let the base-game trigger it to interact more with other mods.. (no real calc here now needed with other fixes)
                                Traverse.Create(pawn.ageTracker).Method("RecalculateLifeStageIndex").GetValue(); //call RecalculateLifeStageIndex
                                if (Prefs.DevMode == true && log == true) { Log.Message("Age Matter: LifestageCorrectionGrowth(active): Pawn: " + pawn.Name.ToString() + " minAge: " + AdultMinAge.ToString() + " Growth is: " + growthCurrent.ToString() + " Growth should be: " + growthCaclulated.ToString() + " correcting.. calling CalculateInitialGrowth() then RecalculateLifeStageIndex() now growth is:" + pawn.ageTracker.Growth.ToString()); }
                            }
                        }
                    }
                }
                else
                    if (Prefs.DevMode == true) { Log.Message("Age Matters Lifestage Fix Not Active"); }
            }
            catch //(Exception e)
            {
               // if (Prefs.DevMode == true) { Log.Error("CSL: LifestageCorrectionGrowth: error: " + e.Message); }
            }
        }
    }
}