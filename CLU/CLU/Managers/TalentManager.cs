// This file is part of Singular - A community driven Honorbuddy CC
// $Author: raphus $
// $Date: 2012-02-05 08:23:51 +1100 (Sun, 05 Feb 2012) $
// $HeadURL: http://svn.apocdev.com/singular/trunk/Singular/Managers/TalentManager.cs $
// $LastChangedBy: raphus $
// $LastChangedDate: 2012-02-05 08:23:51 +1100 (Sun, 05 Feb 2012) $
// $LastChangedRevision: 583 $
// $Revision: 583 $

namespace CLU.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Styx;
    using Styx.Combat.CombatRoutine;
    //using Styx.Logic.Combat;
    using Styx.WoWInternals;
    using System.Drawing;

    public enum TalentSpec {
        // Just a 'spec' for low levels
        Lowbie = 0,
        // A value representing any spec
        Any = int.MaxValue,
        // Here's how this works
        // In the 2nd byte we store the class for the spec
        // Low byte stores the index of the spec. (0-2 for a total of 3)
        // We can retrieve the class easily by doing spec & 0xFF00 to get the high-byte (or you can just shift right 8bits)
        // And the spec can be retrieved via spec & 0xFF - Simple enough
        // Extra flags are stored in the 3rd byte

        BloodDeathKnight = ((int)WoWClass.DeathKnight << 8) + 0,
        FrostDeathKnight = ((int)WoWClass.DeathKnight << 8) + 1,
        UnholyDeathKnight = ((int)WoWClass.DeathKnight << 8) + 2,

        BalanceDruid = ((int)WoWClass.Druid << 8) + 0,
        FeralDruid = ((int)WoWClass.Druid << 8) + 1,
        GaurdianDruid = ((int)WoWClass.Druid << 8) + 2,
        RestorationDruid = ((int)WoWClass.Druid << 8) + 3,

        BeastMasteryHunter = ((int)WoWClass.Hunter << 8) + 0,
        MarksmanshipHunter = ((int)WoWClass.Hunter << 8) + 1,
        SurvivalHunter = ((int)WoWClass.Hunter << 8) + 2,

        ArcaneMage = ((int)WoWClass.Mage << 8) + 0,
        FireMage = ((int)WoWClass.Mage << 8) + 1,
        FrostMage = ((int)WoWClass.Mage << 8) + 2,

        HolyPaladin = ((int)WoWClass.Paladin << 8) + 0,
        ProtectionPaladin = ((int)WoWClass.Paladin << 8) + 1,
        RetributionPaladin = ((int)WoWClass.Paladin << 8) + 2,

        DisciplinePriest = ((int)WoWClass.Priest << 8) + 0,
        HolyPriest = ((int)WoWClass.Priest << 8) + 1,
        ShadowPriest = ((int)WoWClass.Priest << 8) + 2,

        AssasinationRogue = ((int)WoWClass.Rogue << 8) + 0,
        CombatRogue = ((int)WoWClass.Rogue << 8) + 1,
        SubtletyRogue = ((int)WoWClass.Rogue << 8) + 2,

        ElementalShaman = ((int)WoWClass.Shaman << 8) + 0,
        EnhancementShaman = ((int)WoWClass.Shaman << 8) + 1,
        RestorationShaman = ((int)WoWClass.Shaman << 8) + 2,

        AfflictionWarlock = ((int)WoWClass.Warlock << 8) + 0,
        DemonologyWarlock = ((int)WoWClass.Warlock << 8) + 1,
        DestructionWarlock = ((int)WoWClass.Warlock << 8) + 2,

        ArmsWarrior = ((int)WoWClass.Warrior << 8) + 0,
        FuryWarrior = ((int)WoWClass.Warrior << 8) + 1,
        ProtectionWarrior = ((int)WoWClass.Warrior << 8) + 2,

        // TODO: Check talents.
       BrewmasterMonk = ((int)WoWClass.Monk << 8) + 0,
       MistweaverMonk = ((int)WoWClass.Monk << 8) + 1,
       WindwalkerMonk = ((int)WoWClass.Monk << 8) + 2,
    }

    internal static class TalentManager
    {
        static TalentManager()
        {
            Talents = new List<Talent>();
            Glyphs = new HashSet<string>();
            Lua.Events.AttachEvent("CHARACTER_POINTS_CHANGED", UpdateTalentManager);
            Lua.Events.AttachEvent("GLYPH_UPDATED", UpdateTalentManager);
            Lua.Events.AttachEvent("ACTIVE_TALENT_GROUP_CHANGED", UpdateTalentManager);
        }

        public static WoWSpec CurrentSpec { get; private set; }

        public static List<Talent> Talents { get; private set; }

        public static HashSet<string> Glyphs { get; private set; }

        public static int GetCount(int tab, int index)
        {
            return GetCount(index);
        }

        public static int GetCount(int index)
        {
            return Talents.FirstOrDefault(t => t.Index == index).Count;
        }

        /// <summary>
        ///   Checks if we have a glyph or not
        /// </summary>
        /// <param name = "glyphName">Name of the glyph without "Glyph of". i.e. HasGlyph("Aquatic Form")</param>
        /// <returns></returns>
        public static bool HasGlyph(string glyphName)
        {
            return Glyphs.Count > 0 && Glyphs.Contains(glyphName);
        }


        public static bool HasTalent(int tab, int index)
        {
            return Talents.FirstOrDefault(t => t.Tab == tab && t.Index == index).Count != 0; // TODO: Tab no longer valid
        }



        private static void UpdateTalentManager(object sender, LuaEventArgs args)
        {
            WoWSpec oldSpec = CurrentSpec;

            Update();

            if (CurrentSpec != oldSpec) {
                CLU.TroubleshootLog( "Your spec has been changed. Rebuilding rotation");
                //SpellManager.Update();
                CLU.Instance.QueryClassTree();
            }
        }

        public static void Update()
        {
            // Don't bother if we're < 10
            if (StyxWoW.Me.Level < 10)
            {
                CurrentSpec = WoWSpec.None;
                return;
            }

            // Keep the frame stuck so we can do a bunch of injecting at once.
            using (StyxWoW.Memory.AcquireFrame())
            {
                CurrentSpec = StyxWoW.Me.Specialization;
                CLU.Log("TalentManager - looks like a {0}", CurrentSpec.ToString());

                Talents.Clear();

                var numTalents = Lua.GetReturnVal<int>("return GetNumTalents()", 0);
                for (int index = 1; index <= numTalents; index++)
                {
                    var selected = Lua.GetReturnVal<int>(string.Format("return GetTalentInfo({0})", index), 4);
                    var t = new Talent { Index = index, Count = selected };
                    Talents.Add(t);
                }

                Glyphs.Clear();

                var glyphCount = Lua.GetReturnVal<int>("return GetNumGlyphSockets()", 0);

                if (glyphCount != 0)
                {
                    for (int i = 1; i <= glyphCount; i++)
                    {
                        List<string> glyphInfo = Lua.GetReturnValues(String.Format("return GetGlyphSocketInfo({0})", i));

                        if (glyphInfo != null && glyphInfo[3] != "nil" && !string.IsNullOrEmpty(glyphInfo[3]))
                        {
                            Glyphs.Add(WoWSpell.FromId(int.Parse(glyphInfo[3])).Name.Replace("Glyph of ", ""));
                        }
                    }
                }
            }

        }
        public struct Talent {
            public int Count;

            public int Tab;
            public int Index;
        }
    }
}