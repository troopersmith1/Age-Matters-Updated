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
    public class CompProperties_BiosculpterPod_AgeReversalCycle : CompProperties_BiosculpterPod_BaseCycle
    {
        public CompProperties_BiosculpterPod_AgeReversalCycle() => this.compClass = typeof(CompBiosculpterPod_AgeReversalCycle);
    }

    public class AgeMattersBioSculptingFix
    {
        public Pawn adult;
        public float AdultMinAge => this.adult.RaceProps.lifeStageAges.Count <= 0 ? 0.0f : this.adult.RaceProps.lifeStageAges.Find(stage => stage.def.defName.Contains("Adult")).minAge;
    }
    public class CompBiosculpterPod_AgeReversalCycle : CompBiosculpterPod_Cycle
    {
        public AgeMattersBioSculptingFix FixedMinAge;
        public override void CycleCompleted(Pawn pawn)
        {
            int num1 = 3600000;
            long val1 = (long)(72000000);
            pawn.ageTracker.AgeBiologicalTicks = Math.Max(val1, pawn.ageTracker.AgeBiologicalTicks - (long)num1);
            pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
            int num2 = (int)(pawn.ageTracker.AgeReversalDemandedDeadlineTicks / 60000L);
            Messages.Message((string)"BiosculpterAgeReversalCompletedMessage".Translate(pawn.Named("PAWN"), num2.Named("DEADLINE")), (LookTargets)(Thing)pawn, MessageTypeDefOf.PositiveEvent);
        }
    }

    public class AgeMattersMod : Mod
    {
        public AgeMattersSettings settings;
        public static AgeMattersMod mod;

        public AgeMattersMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AgeMattersSettings>();
            mod = this;
        }

        public override string SettingsCategory()
        {
            return "Age Matters";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Settings - Any changes made here will require a restart to take effect.");
            listingStandard.CheckboxLabeled("Hidden Lifestages", ref settings.HiddenHediff, "Hide the age hediffs (e.g. 'Teenager')");
            listingStandard.Label("Do not use this fix if you have CSL enabled, use the fix included with that mod instead.");
            listingStandard.CheckboxLabeled("Fix Lifestages", ref settings.FixLifestages, "Fix the incorrect Lifestages (Code provided by Dylan");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

    }

    public class AgeMattersSettings : ModSettings
    {
        public bool HiddenHediff = true;
        public bool FixLifestages = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref HiddenHediff, "Hidden", true);
            base.ExposeData();
            Scribe_Values.Look(ref FixLifestages, "LifestageFix", true);
        }
    }

    [DefOf]
    public static class AgeMattersDefOf
    {
        static AgeMattersDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AgeMattersDefOf));
        }

        public static HediffDef AgeMatters_baby;
        public static HediffDef AgeMatters_baby2;
        public static HediffDef AgeMatters_toddler;
        public static HediffDef AgeMatters_child;
        public static HediffDef AgeMatters_teenager;
        public static HediffDef AgeMatters_adult;
        public static HediffDef AgeMatters_adult2;
        public static HediffDef AgeMatters_adult3;
        public static HediffDef AgeMatters_old;
        public static HediffDef AgeMatters_old2;
        public static HediffDef AgeMatters_old3;

    }
}