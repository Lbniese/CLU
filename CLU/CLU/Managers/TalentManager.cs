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
    using System.Globalization;
    using System.Linq;
    using Styx;
    using Styx.WoWInternals;
  

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

        /// <summary>
        ///   Checks if we have the talent or not
        /// </summary>
        /// <param name = "index">The index in the Talent Pane from left to right.</param>
        /// <returns></returns>
        public static bool HasTalent(int index)
        {
            return Talents.FirstOrDefault(t => t.Index == index).Count != 0; 
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

                Talents.Clear();

                var numTalents = Lua.GetReturnVal<int>("return GetNumTalents()", 0);
                CLU.TroubleshootLog("TalentManager - numTalents {0}", numTalents.ToString(CultureInfo.InvariantCulture));
                
                for (int index = 0; index <= numTalents; index++)
                {
                    
                    var selected = Lua.GetReturnVal<int>(string.Format("return GetTalentInfo({0})", index), 4);
                    //var talentName = Lua.GetReturnValues("return select(1, GetSpellInfo(" + index + "))")[0];
                    //CLU.TroubleshootLog("TalentManager - Name {10}", talentName);
                    switch (selected)
                    {
                        case 1:
                            {
                                var t = new Talent { Index = index }; //Name = talentName
                                Talents.Add(t);
                            }
                            break;
                    }
                }

                foreach (var talent in Talents)
                {
                    if (talent.Name != null)
                    {
                        CLU.TroubleshootLog("TalentManager - talent {0} == Name", talent.Index);
                    }
                }

                //Glyphs.Clear();

                //var glyphCount = Lua.GetReturnVal<int>("return GetNumGlyphSockets()", 0);

                //if (glyphCount != 0)
                //{
                //    for (int i = 1; i <= glyphCount; i++)
                //    {
                //        List<string> glyphInfo = Lua.GetReturnValues(String.Format("return GetGlyphSocketInfo({0})", i));

                //        if (glyphInfo != null && glyphInfo[3] != "nil" && !string.IsNullOrEmpty(glyphInfo[3]))
                //        {
                //            Glyphs.Add(WoWSpell.FromId(int.Parse(glyphInfo[3])).Name.Replace("Glyph of ", ""));
                //        }
                //    }
                //}
            }

        }
        public struct Talent {
            public int Count;
            public int Index;
            public string Name;
        }
    }
}