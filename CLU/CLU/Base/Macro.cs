using System;
using Styx.WoWInternals;
using Styx;
using Styx.CommonBot;

namespace CLU.Base
{
    internal static class Macro
    {
        /* Putting all the Macro logic here */

        /// <summary>
        /// Toggle macro for rotation pause
        /// </summary>
        public static bool Manual
        {
            get
            {
                return Convert.ToBoolean(Lua.GetReturnVal<int>("return Manual and 0 or 1", 0));
            }
        }

        /// <summary>
        /// Toggle macro for burst rotation
        /// </summary>
        public static bool Burst
        {
            get
            {
                return Convert.ToBoolean(Lua.GetReturnVal<int>("return Burst and 0 or 1", 0));
            }
        }

        /// <summary>
        /// Toggle macro for Obliterate to Necrotic Strike rotations
        /// </summary>
        public static bool rotationSwap
        {
            get
            {
                return Convert.ToBoolean(Lua.GetReturnVal<int>("return rotationSwap and 0 or 1", 0));
            }
        }

        /// <summary>
        /// Toggle macro for Necrotic Strike preassure to stack rotations
        /// </summary>
        public static bool necroSwap
        {
            get
            {
                return Convert.ToBoolean(Lua.GetReturnVal<int>("return necroSwap and 0 or 1", 0));
            }
        }

        /// <summary>
        /// Resets individual MultiCastMacro int's to 0
        /// </summary>
        /// <param name="Which">Spell name</param>
        public static void resetMacro(string Which)
        {
            Lua.DoString(Which + " = 0;");
        }

        /// <summary>
        /// Resets All MultiCastMacro int's within to 0
        /// </summary>
        public static void resetAllMacros()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                resetMacro("MultiCastFT");
                resetMacro("MultiCastMT");
            }
        }

        static int MultiCastMacroMT = 0;
        static int MultiCastMacroFT = 0;
        static string whatSpell;
        static WoWSpell _Spell;

        /// <summary>
        /// Checks to see if the MultiCastMacro is a 0 or 1, if 1 is present ~> run MT or FT call, also checks for LoS and if we can cast the spell or not
        /// </summary>
        public static void isMultiCastMacroInUse()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                MultiCastMacroMT = Lua.GetReturnVal<int>("return MultiCastMT", 0);
                MultiCastMacroFT = Lua.GetReturnVal<int>("return MultiCastFT", 0);
                whatSpell = Lua.GetReturnVal<String>("return spellName", 0);
            }
            if (MultiCastMacroMT > 0 || MultiCastMacroFT > 0)
            {
                if (whatSpell == null)
                {
                    CLU.Log("Invalid Spell!");
                    resetAllMacros();
                }
                else
                {
                    SpellManager.Spells.TryGetValue(whatSpell, out _Spell);
                    if (!_Spell.IsValid || _Spell.CooldownTimeLeft > SpellManager.GlobalCooldownLeft)
                    {
                        CLU.Log("Can't Cast Spell!");
                        resetAllMacros();
                    }
                }
            }
            if (MultiCastMacroMT > 0)
            {
                if (SpellManager.GlobalCooldown || StyxWoW.Me.IsCasting)
                    return;
                if (StyxWoW.Me.CurrentTarget.InLineOfSight)
                {
                    if (SpellManager.CanCast(whatSpell))
                    {
                        Lua.DoString("RunMacroText(\"/cast " + whatSpell + "\")");
                        SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                        resetMacro("MultiCastMT");
                    }
                }
                else
                {
                    resetMacro("MultiCastMT");
                }
            }
            else if (MultiCastMacroFT > 0)
            {
                if (SpellManager.GlobalCooldown || StyxWoW.Me.IsCasting)
                    return;
                if (StyxWoW.Me.FocusedUnit.InLineOfSight)
                {
                    if (SpellManager.CanCast(whatSpell))
                    {
                        Lua.DoString("RunMacroText(\"/cast [@focus] " + whatSpell + "\")");
                        SpellManager.ClickRemoteLocation(StyxWoW.Me.FocusedUnit.Location);
                        resetMacro("MultiCastFT");
                    }
                }
                else
                {
                    resetMacro("MultiCastFT");
                }
            }
        }
    }
}
