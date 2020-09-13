using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace AgeMattersUpdated
{
  [StaticConstructorOnStartup]
  public static class DialogSettings
  {
    private static float RoundToNearestHalf(float val)
    {
      return (float)Math.Round(val * 2, MidpointRounding.AwayFromZero) / 2;
    }

    private static float RoundToNearestHundredth(float val)
    {
      return (float)Math.Round(val * 100, MidpointRounding.AwayFromZero) / 100;
    }

    public static Color textColor = Color.white;
    public static Color highlightColor = new Color(0f, 0f, 0f, 0.25f);

    public static void Settings_IntegerBox(this Listing_Standard lister, string text, ref int value, float labelLength, float padding, int min = int.MinValue, int max = int.MaxValue)
    {
      lister.Gap(12f);
      Rect rect = lister.GetRect(Text.LineHeight);

      Rect rectLeft = new Rect(rect.x, rect.y, labelLength, rect.height);
      Rect rectRight = new Rect(rect.x + labelLength + padding, rect.y, rect.width - labelLength - padding, rect.height);

      Color color = GUI.color;
      Widgets.Label(rectLeft, text);

      var align = Text.CurTextFieldStyle.alignment;
      Text.CurTextFieldStyle.alignment = TextAnchor.MiddleLeft;
      string buffer = value.ToString();
      Widgets.TextFieldNumeric(rectRight, ref value, ref buffer, min, max);

      Text.CurTextFieldStyle.alignment = align;
      GUI.color = color;
    }

    public static void Settings_Numericbox(this Listing_Standard lister, string text, ref float value, float labelLength, float padding, float min = -1E+09f, float max = 1E+09f)
    {
      lister.Gap(12f);
      Rect rect = lister.GetRect(Text.LineHeight);

      Rect rectLeft = new Rect(rect.x, rect.y, labelLength, rect.height);
      Rect rectRight = new Rect(rect.x + labelLength + padding, rect.y, rect.width - labelLength - padding, rect.height);

      Color color = GUI.color;
      Widgets.Label(rectLeft, text);

      var align = Text.CurTextFieldStyle.alignment;
      Text.CurTextFieldStyle.alignment = TextAnchor.MiddleLeft;
      string buffer = value.ToString();
      Widgets.TextFieldNumeric<float>(rectRight, ref value, ref buffer, min, max);

      Text.CurTextFieldStyle.alignment = align;
      GUI.color = color;
    }

    public static void Settings_SliderLabeled(this Listing_Standard lister, string label, string endSymbol, ref float value, float min, float max, float multiplier = 1f, int decimalPlaces = 2, float hardMaximum = -1f, string valueDisplayWhenMax = "")
    {
      lister.Gap(12f);
      Rect rect = lister.GetRect(24f);
      string labelValue = string.Format("{0}" + endSymbol, (Math.Round(value * multiplier, decimalPlaces) * 100));
      if (endSymbol == "%")
        labelValue = string.Format("{0}" + endSymbol, value * 100);
      if (!valueDisplayWhenMax.NullOrEmpty() && hardMaximum > 0) // when value is max, show a string instead (infinite, max, etc)
        if (value >= hardMaximum)
          labelValue = valueDisplayWhenMax;
      value = Widgets.HorizontalSlider(rect, RoundToNearestHundredth(value), min, max, false, null, label, labelValue);
      if (hardMaximum > 0 && value >= max)
        value = hardMaximum;
    }

    public static void Settings_SliderLabeled(this Listing_Standard lister, string label, string endSymbol, ref int value, int min, int max, int hardMaximum = -1, string valueDisplayWhenMax = "")
    {
      lister.Gap(12f);
      Rect rect = lister.GetRect(24f);
      string labelValue = string.Format("{0}" + endSymbol, value);
      if (!valueDisplayWhenMax.NullOrEmpty() && hardMaximum > 0) // when value is max, show a string instead (infinite, max, etc)
        if (value == hardMaximum)
          labelValue = valueDisplayWhenMax;
      value = (int)Widgets.HorizontalSlider(rect, value, min, max, false, null, label, labelValue);
      if (hardMaximum > 0 && value == max)
        value = hardMaximum;
    }

    public static void Settings_Header(this Listing_Standard lister, string header, Color highlight, GameFont fontSize = GameFont.Medium, TextAnchor anchor = TextAnchor.MiddleLeft)
    {
      var textSize = Text.Font;
      Text.Font = fontSize;

      Rect rect = lister.GetRect(Text.CalcHeight(header, lister.ColumnWidth));
      GUI.color = highlight;
      GUI.DrawTexture(rect, BaseContent.WhiteTex);
      GUI.color = textColor;

      var anchorTmp = Text.Anchor;
      Text.Anchor = anchor;
      Widgets.Label(rect, header);
      Text.Font = textSize;
      Text.Anchor = anchorTmp;
      lister.Gap(12f);
    }

    public static bool Settings_Button(this Listing_Standard lister, string label, Rect rect, Color customColor, bool background = true, bool active = true)
    {
      var anchor = Text.Anchor;
      Color color = GUI.color;

      if (background)
      {
        Texture2D atlas = ButtonBGAtlas;
        if (Mouse.IsOver(rect))
        {
          atlas = ButtonBGAtlasMouseover;
          if (Input.GetMouseButton(0))
          {
            atlas = ButtonBGAtlasClick;
          }
        }
        Widgets.DrawAtlas(rect, atlas);
      }
      else
      {
        GUI.color = customColor;
        if (Mouse.IsOver(rect))
          GUI.color = Color.cyan;
      }
      if (background)
        Text.Anchor = TextAnchor.MiddleCenter;
      else
        Text.Anchor = TextAnchor.MiddleLeft;
      bool wordWrap = Text.WordWrap;
      if (rect.height < Text.LineHeight * 2f)
        Text.WordWrap = false;
      Widgets.Label(rect, label);
      Text.Anchor = anchor;
      GUI.color = color;
      Text.WordWrap = wordWrap;
      lister.Gap(2f);
      return Widgets.ButtonInvisible(rect, false);
    }

    public static bool Settings_ButtonLabeled(this Listing_Standard lister, string header, string buttonLabel, Color highlightColor, float buttonWidth = 30f, bool background = true, bool active = true)
    {
      var anchor = Text.Anchor;
      Color color = GUI.color;
      Rect rect = lister.GetRect(20f);
      Rect buttonRect = new Rect(rect.width - buttonWidth, rect.y, buttonWidth, rect.height);

      Text.Anchor = TextAnchor.MiddleLeft;
      Widgets.Label(rect, header);

      if (background)
      {
        Texture2D atlas = ButtonBGAtlas;
        if (Mouse.IsOver(buttonRect))
        {
          atlas = ButtonBGAtlasMouseover;
          if (Input.GetMouseButton(0))
          {
            atlas = ButtonBGAtlasClick;
          }
        }
        Widgets.DrawAtlas(buttonRect, atlas);
      }
      else
      {
        GUI.color = Color.white;
        if (Mouse.IsOver(buttonRect))
          GUI.color = highlightColor;
      }
      if (background)
        Text.Anchor = TextAnchor.MiddleCenter;
      else
        Text.Anchor = TextAnchor.MiddleRight;
      bool wordWrap = Text.WordWrap;
      if (buttonRect.height < Text.LineHeight * 2f)
        Text.WordWrap = false;

      Widgets.Label(buttonRect, buttonLabel);
      Text.Anchor = anchor;
      GUI.color = color;
      Text.WordWrap = wordWrap;
      lister.Gap(2f);
      return Widgets.ButtonInvisible(buttonRect, false);
    }

    public static void Draw_Label(Rect rect, string label, Color highlight, Color textColor, GameFont fontSize = GameFont.Medium, TextAnchor anchor = TextAnchor.MiddleLeft)
    {
      var textSize = Text.Font;
      Text.Font = fontSize;
      GUI.color = highlight;
      GUI.DrawTexture(rect, BaseContent.WhiteTex);
      GUI.color = textColor;

      var anchorTmp = Text.Anchor;
      Text.Anchor = anchor;
      Widgets.Label(rect, label);
      Text.Font = textSize;
      Text.Anchor = anchorTmp;
    }

    public static readonly Texture2D ButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG", true);

    public static readonly Texture2D ButtonBGAtlasMouseover = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover", true);

    public static readonly Texture2D ButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick", true);

    public static readonly Texture2D LightHighlight = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.04f));


  }

}
