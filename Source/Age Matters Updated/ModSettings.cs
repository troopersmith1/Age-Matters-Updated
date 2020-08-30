using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace AgeMattersUpdated
{
  public class RWModSettings : ModSettings
  {
    public int AgingRate = 1;

    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look(ref AgingRate, "Aging_Rate");
    }
  }

  public class RWMod : Mod
  {
    public RWModSettings settings;
    public static RWMod mod;

    public RWMod(ModContentPack con) : base(con)
    {
      settings = GetSettings<RWModSettings>();
      mod = this;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
      Listing_Standard listing = new Listing_Standard();
      listing.Begin(inRect);
      listing.Gap(24f);

      listing.Settings_SliderLabeled("AgingRateLabel".Translate(), "", ref mod.settings.AgingRate, 1, 25);
      listing.End();

      base.DoSettingsWindowContents(inRect);
    }

    public override void WriteSettings()
    {
      UpdateChanges();

      base.WriteSettings();
    }

    public override string SettingsCategory()
    {
            //return "SettingsLabel".Translate();
            return "Age Matters Updated";
    }

    public static void UpdateChanges()
    {

            HediffDef youthDef = DefDatabase<HediffDef>.GetNamed("Youth");
            youthDef.CompProps<HediffCompProperties_SeverityPerDay>().severityPerDay = RWMod.mod.settings.AgingRate * -0.0014f;

        }
  }
}
