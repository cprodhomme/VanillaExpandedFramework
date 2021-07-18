﻿using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class GenStep_CustomStructureGen : GenStep
    {
        public List<StructureLayoutDef> structureLayoutDefs = new List<StructureLayoutDef>();

        /* Ruin */
        public bool shouldRuin = false;
        public List<ThingDef> filthTypes = new List<ThingDef>();
        public List<string> ruinSymbolResolvers = new List<string>();

        public override int SeedPart
        {
            get
            {
                return 916595355;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            StructureLayoutDef structureLayoutDef = structureLayoutDefs.RandomElement();

            RectUtils.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = CellRect.CenteredOn(map.Center, w, h);

            foreach (List<string> item in structureLayoutDef.layouts)
            {
                GenUtils.GenerateRoomFromLayout(item, cellRect, map, structureLayoutDef);
            }

            if (shouldRuin)
            {
                CGO.factionSettlement = new FactionSettlement
                {
                    filthTypes = filthTypes
                };

                ResolveParams rp = new ResolveParams
                {
                    faction = map.ParentFaction,
                    rect = cellRect
                };
                foreach (string resolver in ruinSymbolResolvers)
                {
                    BaseGen.symbolStack.Push(resolver, rp, null);
                }
            }

            // Flood refog
            this.SetAllFogged(map);
            foreach (IntVec3 loc in map.AllCells)
            {
                map.mapDrawer.MapMeshDirty(loc, MapMeshFlag.FogOfWar);
            }
        }

        internal void SetAllFogged(Map map)
        {
            CellIndices cellIndices = map.cellIndices;
            if (map.fogGrid?.fogGrid != null)
            {
                foreach (IntVec3 c in map.AllCells)
                {
                    map.fogGrid.fogGrid[cellIndices.CellToIndex(c)] = true;
                }
                if (Current.ProgramState == ProgramState.Playing)
                {
                    map.roofGrid.Drawer.SetDirty();
                }
            }
        }
    }
}