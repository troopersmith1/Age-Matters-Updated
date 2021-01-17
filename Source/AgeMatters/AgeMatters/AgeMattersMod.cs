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
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

    }

    public class AgeMattersSettings : ModSettings
    {
        public bool HiddenHediff = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref HiddenHediff, "Hidden", true);
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