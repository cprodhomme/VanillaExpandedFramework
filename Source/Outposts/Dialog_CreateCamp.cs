﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    public class Dialog_CreateCamp : Window
    {
        private const float LINE_HEIGHT = 100f;
        private readonly Dictionary<WorldObjectDef, Material> cachedMats;
        private readonly Caravan creator;
        private readonly Dictionary<WorldObjectDef, string> validity;
        private Vector2 scrollPosition = new Vector2(0, 0);

        public Dialog_CreateCamp(Caravan creator)
        {
            doCloseButton = true;
            doCloseX = true;
            doWindowBackground = true;
            this.creator = creator;
            validity = new Dictionary<WorldObjectDef, string>();
            cachedMats = new Dictionary<WorldObjectDef, Material>();
            foreach (var outpost in OutpostsMod.Outposts)
            {
                var method = outpost.worldObjectClass.GetMethod("CanSpawnOnWith", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                    validity.Add(outpost, null);
                else
                    validity.Add(outpost, (string) method.Invoke(null, new object[] {creator.Tile, creator.PawnsListForReading}));

                cachedMats.Add(outpost,
                    MaterialPool.MatFrom(outpost.ExpandingIconTexture, ShaderDatabase.WorldOverlayTransparentLit, creator.Faction.Color, WorldMaterials.WorldObjectRenderQueue));
            }
        }

        public override Vector2 InitialSize => new Vector2(800, 1000);

        public override void DoWindowContents(Rect inRect)
        {
            var outRect = inRect.ContractedBy(5f);
            var viewRect = new Rect(0, 0, outRect.width - 50f, OutpostsMod.Outposts.Count * (LINE_HEIGHT + 10));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            var rect = new Rect(10, 0, viewRect.width, LINE_HEIGHT);
            foreach (var outpost in OutpostsMod.Outposts)
            {
                DoOutpostDisplay(rect, outpost);
                rect.y += LINE_HEIGHT + 5f;
                Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);
                rect.y += 5f;
            }

            Widgets.EndScrollView();
        }

        private void DoOutpostDisplay(Rect inRect, WorldObjectDef outpost)
        {
            var image = inRect.LeftPartPixels(50f);
            var text = inRect.RightPartPixels(inRect.width - 60f);
            var tex = outpost.ExpandingIconTexture;
            Widgets.DrawTextureFitted(image, tex, 1f, new Vector2(tex.width, tex.height), new Rect(0f, 0f, 1f, 1f) /*, 0f, cachedMats[outpost]*/);
            var font = Text.Font;
            Text.Font = GameFont.Medium;
            Widgets.Label(text.TopPartPixels(30f), outpost.label.CapitalizeFirst(outpost));
            var button = text.BottomPartPixels(30f).LeftPartPixels(100f);
            Text.Font = GameFont.Tiny;
            Widgets.TextArea(new Rect(text.x, text.y + 30f, text.width, text.height - 60f), outpost.description, true);
            if (Widgets.ButtonText(button, "Outposts.Dialog.Create".Translate()))
            {
                if (validity[outpost].NullOrEmpty())
                {
                    var worldObject = (Outpost) WorldObjectMaker.MakeWorldObject(outpost);
                    worldObject.Name = NameGenerator.GenerateName(creator.Faction.def.settlementNameMaker, Find.WorldObjects.AllWorldObjects.OfType<Outpost>().Select(o => o.Name));
                    worldObject.Tile = creator.Tile;
                    worldObject.SetFaction(creator.Faction);
                    foreach (var pawn in creator.PawnsListForReading.ListFullCopy()) worldObject.AddPawn(pawn);
                    Find.WorldObjects.Add(worldObject);
                    Close();
                    Find.WorldSelector.Select(worldObject);
                }
                else
                {
                    Messages.Message(validity[outpost], MessageTypeDefOf.RejectInput, false);
                }
            }

            Text.Font = font;
        }
    }
}