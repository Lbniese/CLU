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

        public static TalentSpec CurrentSpec
        {
            get;
            private set;
        }

        public static List<Talent> Talents
        {
            get;
            private set;
        }

        public static HashSet<string> Glyphs
        {
            get;
            private set;
        }

        public static int GetCount(int tab, int index)
        {
            return Talents.FirstOrDefault(t => t.Tab == tab && t.Index == index).Count;
        }

        public static bool HasTalent(int tab, int index)
        {
            return Talents.FirstOrDefault(t => t.Tab == tab && t.Index == index).Count != 0;
        }

        /// <summary>
        ///   Checks if we have a glyph or not
        /// </summary>
        /// <param name = "glyphName">Name of the glyph without "Glyph of". i.e. HasGlyph("Aquatic Form")</param>
        /// <returns>true of we have the glyph</returns>
        public static bool HasGlyph(string glyphName)
        {
            return Glyphs.Count > 0 && Glyphs.Contains(glyphName);
        }

        private static void UpdateTalentManager(object sender, LuaEventArgs args)
        {
            TalentSpec oldSpec = CurrentSpec;

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
            if (StyxWoW.Me.Level < 10) {
                CurrentSpec = TalentSpec.Lowbie;
                return;
            }

            var getSpecialization = Lua.GetReturnVal<int>("return GetSpecialization()", 0);
            if (getSpecialization != 0)
            {
                switch (StyxWoW.Me.Class)
                {
                    case WoWClass.Warrior:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.ArmsWarrior;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.ProtectionWarrior;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.FuryWarrior;
                                break;
                        }
                        break;
                    case WoWClass.Paladin:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.HolyPaladin;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.ProtectionPaladin;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.RetributionPaladin;
                                break;
                        }
                        break;
                    case WoWClass.Hunter:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.BeastMasteryHunter;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.MarksmanshipHunter;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.SurvivalHunter;
                                break;
                        }
                        break;
                    case WoWClass.Rogue:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.AssasinationRogue;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.CombatRogue;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.SubtletyRogue;
                                break;
                        }
                        break;
                    case WoWClass.Priest:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.DisciplinePriest;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.HolyPriest;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.ShadowPriest;
                                break;
                        }
                        break;
                    case WoWClass.DeathKnight:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.BloodDeathKnight;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.FrostDeathKnight;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.UnholyDeathKnight;
                                break;
                        }
                        break;
                    case WoWClass.Shaman:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.ElementalShaman;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.EnhancementShaman;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.RestorationShaman;
                                break;
                        }
                        break;
                    case WoWClass.Mage:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.ArcaneMage;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.FireMage;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.FrostMage;
                                break;
                        }
                        break;
                    case WoWClass.Warlock:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.AfflictionWarlock;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.DemonologyWarlock;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.DestructionWarlock;
                                break;
                        }
                        break;
                    case WoWClass.Druid:
                        switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.BalanceDruid;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.FeralDruid;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.GaurdianDruid;
                                break;
                            case 4:
                                CurrentSpec = TalentSpec.RestorationDruid;
                                break;
                        }
                        break;
                    //TODO: Monk Settings
                    /*case WoWClass.Monk:
                         switch (getSpecialization)
                        {
                            case 1:
                                CurrentSpec = TalentSpec.BrewmasterMonk;
                                break;
                            case 2:
                                CurrentSpec = TalentSpec.MistweaverMonk;
                                break;
                            case 3:
                                CurrentSpec = TalentSpec.WindwalkerMonk;
                                break;
                        }
                        break;*/

                    default:
                        break;
                }
            }
            //CLU.DiagnosticLog( "getSpecialization =  " + getSpecialization);

            WoWClass myClass = StyxWoW.Me.Class;
            //int treeOne = 0, treeTwo = 0, treeThree = 0;




            //// Keep the frame stuck so we can do a bunch of injecting at once.
            //using (StyxWoW.Memory.AcquireFrame()) {
            //    Talents.Clear();
            //    for (int tab = 1; tab <= 3; tab++) {
            //        var numTalents = Lua.GetReturnVal<int>("return GetNumTalents(" + tab + ")", 0);
            //        for (int index = 1; index <= numTalents; index++) {
            //            var rank = Lua.GetReturnVal<int>(string.Format("return GetTalentInfo({0}, {1})", tab, index), 4);
            //            var t = new Talent { Tab = tab, Index = index, Count = rank };
            //            Talents.Add(t);

            //            switch (tab) {
            //            case 1:
            //                treeOne += rank;
            //                break;
            //            case 2:
            //                treeTwo += rank;
            //                break;
            //            case 3:
            //                treeThree += rank;
            //                break;
            //            }
            //        }
            //    }

                Glyphs.Clear();

                var glyphCount = Lua.GetReturnVal<int>("return GetNumGlyphSockets()", 0);

                if (glyphCount != 0) {
                    for (int i = 1; i <= glyphCount; i++) {
                        List<string> glyphInfo = Lua.GetReturnValues(String.Format("return GetGlyphSocketInfo({0})", i));

                        if (glyphInfo != null && glyphInfo[3] != "nil" && !string.IsNullOrEmpty(glyphInfo[3])) {
                            Glyphs.Add(WoWSpell.FromId(int.Parse(glyphInfo[3])).Name.Replace("Glyph of ", string.Empty));
                        }
                    }
                }
            //}

            //if (treeOne == 0 && treeTwo == 0 && treeThree == 0) {
            //    CurrentSpec = TalentSpec.Lowbie;
            //    return;
            //}

            //int max = Math.Max(Math.Max(treeOne, treeTwo), treeThree);
            //CLU.TroubleshootLog( " Best Tree: " + max);
            //int specMask = ((int)StyxWoW.Me.Class << 8);

            //if (max == treeOne) {
            //    CurrentSpec = (TalentSpec)(specMask + 0);
            //} else if (max == treeTwo) {
            //    CurrentSpec = (TalentSpec)(specMask + 1);
            //} else {
            //    CurrentSpec = (TalentSpec)(specMask + 2);
            //}

        }

        public struct Talent {
            public int Count;

            public int Index;

            public int Tab;
        }


        //local currentSpec = GetSpecialization()
        //local currentSpecName = currentSpec and select(2, GetSpecializationInfo(currentSpec)) or "None"
        //print("Your current spec:", currentSpecName)
    }
}