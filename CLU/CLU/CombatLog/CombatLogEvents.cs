﻿#region Revision info

/*
 * $Author$
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 * $LastChangedBy$
 * $ChangesMade$
 */

#endregion Revision info

using System.Globalization;

namespace CLU.CombatLog
{
    using System;
    using System.Collections.Generic;
    using Base;
    using Helpers;
    using Managers;
    using Settings;
    using Styx;
    using Styx.CommonBot;
    using Styx.CommonBot.POI;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;

    public class CombatLogEvents
    {
        private static CombatLogEvents instance;

        public static CombatLogEvents Instance
        {
            get
            {
                return instance ?? (instance = new CombatLogEvents());
            }
        }

        private static bool _combatLogAttached;

        public static readonly Dictionary<string, DateTime> Locks = new Dictionary<string, DateTime>();

        public static readonly double ClientLag = CLUSettings.Instance.EnableClientLagDetection ? StyxWoW.WoWClient.Latency * 2 / 1000.0 : 1;

        public void CombatLogEventsOnStarted(object o)
        {
            try
            {
                CLULogger.TroubleshootLog("CombatLogEvents: Connected to the Grid");

                // means spell was cast (did not hit target yet)
                CLULogger.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_SUCCEEDED");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_SUCCEEDED", this.OnSpellFired_ACK);

                // user got stunned, silenced, kicked...
                CLULogger.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_INTERRUPTED");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_INTERRUPTED", this.OnSpellFired_NACK);

                // misc fails, due to stopcast, spell spam, etc.
                CLULogger.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_FAILED");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_FAILED", this.OnSpellFired_FAIL);
                CLULogger.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_FAILED_QUIET");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_FAILED_QUIET", this.OnSpellFired_FAIL);
                CLULogger.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_STOP");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_STOP", this.OnSpellFired_FAIL);
                CLULogger.TroubleshootLog("CombatLogEvents: Connect PARTY_MEMBERS_CHANGED");
                Lua.Events.AttachEvent("PARTY_MEMBERS_CHANGED", this.HandlePartyMembersChanged);

                if ((CLU.LocationContext != GroupLogic.Battleground && CLU.LocationContext != GroupLogic.PVE) || TalentManager.CurrentSpec == WoWSpec.DruidFeral)
                    AttachCombatLogEvent();
            }
            catch (Exception ex)
            {
                CLULogger.DiagnosticLog("HandlePartyMembersChanged : {0}", ex);
            }
        }

        public void CombatLogEventsOnStopped(object o)
        {
            // Lua.Events.DetachEvent("CHARACTER_POINTS_CHANGED", UpdateActiveRotation);
            // Lua.Events.DetachEvent("ACTIVE_TALENT_GROUP_CHANGED", UpdateActiveRotation)
            Lua.Events.DetachEvent("UNIT_SPELLCAST_SUCCEEDED", this.OnSpellFired_ACK);
            Lua.Events.DetachEvent("UNIT_SPELLCAST_INTERRUPTED", this.OnSpellFired_NACK);
            Lua.Events.DetachEvent("UNIT_SPELLCAST_FAILED", this.OnSpellFired_FAIL);
            Lua.Events.DetachEvent("UNIT_SPELLCAST_FAILED_QUIET", this.OnSpellFired_FAIL);
            Lua.Events.DetachEvent("UNIT_SPELLCAST_STOP", this.OnSpellFired_FAIL);
            Lua.Events.DetachEvent("PARTY_MEMBERS_CHANGED", this.HandlePartyMembersChanged);

            DetachCombatLogEvent();
        }

        public void BotBaseChange(object o)
        {
            BotChecker.Initialize();
        }

        private static void AttachCombatLogEvent()
        {
            if (_combatLogAttached)
                return;

            // DO NOT EDIT THIS UNLESS YOU KNOW WHAT YOU'RE DOING!
            // This ensures we only capture certain combat log events, not all of them.
            // This saves on performance, and possible memory leaks. (Leaks due to Lua table issues.)
            Lua.Events.AttachEvent("COMBAT_LOG_EVENT_UNFILTERED", HandleCombatLog);
            if (
                !Lua.Events.AddFilter(
                    "COMBAT_LOG_EVENT_UNFILTERED",
                    "return args[2] == 'SPELL_CAST_SUCCESS' or args[2] == 'SPELL_AURA_APPLIED' or args[2] == 'SPELL_DAMAGE' or args[2] == 'SPELL_AURA_REFRESH' or args[2] == 'SPELL_AURA_REMOVED'or args[2] == 'SPELL_MISSED' or args[2] == 'RANGE_MISSED' or args[2] =='SWING_MISSED'"))
            {
                CLULogger.TroubleshootLog(
                    "ERROR: Could not add combat log event filter! - Performance may be horrible, and things may not work properly!");
            }

            CLULogger.TroubleshootLog("Attached combat log");
            _combatLogAttached = true;
        }

        private static void DetachCombatLogEvent()
        {
            if (!_combatLogAttached)
                return;

            CLULogger.TroubleshootLog("Detached combat log");
            Lua.Events.DetachEvent("COMBAT_LOG_EVENT_UNFILTERED", HandleCombatLog);
            _combatLogAttached = false;
        }

        private void HandlePartyMembersChanged(object sender, LuaEventArgs args)
        {
            try
            {
                if (CLU.IsHealerRotationActive && StyxWoW.IsInGame)
                {
                    CLULogger.TroubleshootLog("CombatLogEvents: Party Members Changed - Re-Initialize list Of HealableUnits");
                    switch (CLUSettings.Instance.SelectedHealingAquisition)
                    {
                        case HealingAquisitionMethod.Proximity:
                            HealableUnit.HealableUnitsByProximity();
                            break;

                        case HealingAquisitionMethod.RaidParty:
                            HealableUnit.HealableUnitsByPartyorRaid();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CLULogger.DiagnosticLog("HandlePartyMembersChanged : {0}", ex);
            }
        }

        public void Player_OnMapChanged(BotEvents.Player.MapChangedEventArgs args)
        {
            try
            {
                if ((CLU.LocationContext == GroupLogic.Battleground || CLU.LocationContext == GroupLogic.PVE) && TalentManager.CurrentSpec != WoWSpec.DruidFeral)
                    DetachCombatLogEvent();
                else
                    AttachCombatLogEvent();

                //Why would we create same behaviors all over ?
                if (CLU.LastLocationContext == CLU.LocationContext)
                {
                    return;
                }

                CLULogger.TroubleshootLog("Context changed. New context: " + CLU.LocationContext + ". Rebuilding behaviors.");
                CLU.Instance.CreateBehaviors();

                if (CLU.IsHealerRotationActive && StyxWoW.IsInGame)
                {
                    CLULogger.TroubleshootLog("CombatLogEvents: Party Members Changed - Re-Initialize list Of HealableUnits");
                    switch (CLUSettings.Instance.SelectedHealingAquisition)
                    {
                        case HealingAquisitionMethod.Proximity:
                            HealableUnit.HealableUnitsByProximity();
                            break;

                        case HealingAquisitionMethod.RaidParty:
                            HealableUnit.HealableUnitsByPartyorRaid();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CLULogger.DiagnosticLog("Player_OnMapChanged : {0}", ex);
            }
        }

        private void OnSpellFired_ACK(object sender, LuaEventArgs raw)
        {
            this.OnSpellFired(true, true, raw);
            if (CLUSettings.Instance.EnableWoWStats) WoWStats.Instance.UnitSpellcastSucceeded(sender, raw); //added to cut down on Lua attatched events...seeing as we already attatch UNIT_SPELLCAST_SUCCEEDED in this class --wulf
        }

        private void OnSpellFired_NACK(object sender, LuaEventArgs raw)
        {
            this.OnSpellFired(false, true, raw);
        }

        private void OnSpellFired_FAIL(object sender, LuaEventArgs raw)
        {
            this.OnSpellFired(false, false, raw);
        }

        private void OnSpellFired(bool success, bool spellCast, LuaEventArgs raw)
        {
            var args = raw.Args;
            var player = Convert.ToString(args[0]);

            if (player != "player")
            {
                return;
            }

            // get the english spell name, not the localized one!
            var spellId = Convert.ToInt32(args[4]);
            var spellName = WoWSpell.FromId(spellId).Name;
            var sourceGuid = ulong.Parse(args[3].ToString().Replace("0x", string.Empty), NumberStyles.HexNumber);

            if (!success && spellCast)
            {
                CLULogger.DiagnosticLog("Woops, '{0}' cast failed: {1}", spellName, raw.EventName);
            }

            // if the spell is locked, let's extend it (spell travel time + client lag) / or reset it...
            if (Locks.ContainsKey(spellName))
            {
                if (success)
                {
                    // yay!
                    Locks[spellName] = DateTime.Now.AddSeconds(ClientLag + 4.0);
                }
                else
                {
                    if (spellCast)
                    {
                        // interrupted while casting
                        Locks[spellName] = DateTime.Now;
                    }
                    else
                    {
                        // failed to cast it. moar spam!
                        Locks[spellName] = DateTime.Now;
                    }
                }
            }

            // We need to 'sleep' for these spells. Otherwise, we'll end up double-casting them. Which will cause issues
            switch (spellName)
            {
                case "Rejuvenation":
                case "Lifebloom":
                case "Regrowth":
                case "Nourish":
                case "Healing Touch":
                case "Remove Corruption":
                case "Holy Light":
                case "Holy Radiance":
                case "Divine Light":
                case "Holy Shock":
                    CLULogger.DiagnosticLog("Sleeping for heal success. ({0})", spellName);
                    StyxWoW.SleepForLagDuration();
                    break;

                case "Nature's Swiftness":
                    CLULogger.DiagnosticLog("PrevNaturesSwiftness. ({0})", spellName);
                    if (sourceGuid == StyxWoW.Me.Guid)
                    {
                        Classes.Druid.Common.PrevNaturesSwiftness = spellId == 132158;
                    }
                    break;
            }
        }

        // Thanks to Singular Devs for the CombatLogEventArgs class and SpellImmunityManager.
        private static void HandleCombatLog(object sender, LuaEventArgs args)
        {
            var e = new CombatLogEventArgs(args.EventName, args.FireTimeStamp, args.Args);

            //var missType = Convert.ToString(e.Args[14]);

            switch (e.Event)
            {
                case "SWING_MISSED":
                    if (e.Args[11].ToString() == "EVADE")
                    {
                        CLULogger.TroubleshootLog("Mob is evading swing. Blacklisting it!");
                        Blacklist.Add(e.DestGuid, TimeSpan.FromMinutes(30));
                        if (StyxWoW.Me.CurrentTargetGuid == e.DestGuid)
                        {
                            StyxWoW.Me.ClearTarget();
                        }

                        BotPoi.Clear("Blacklisting evading mob");
                        StyxWoW.SleepForLagDuration();
                    }
                    else if (e.Args[11].ToString() == "IMMUNE")
                    {
                        WoWUnit unit = e.DestUnit;
                        if (unit != null && !unit.IsPlayer)
                        {
                            CLULogger.TroubleshootLog("{0} is immune to {1} spell school", unit.Name, e.SpellSchool);
                            SpellImmunityManager.Add(unit.Entry, e.SpellSchool);
                        }
                    }
                    break;

                case "SPELL_MISSED":
                case "RANGE_MISSED":
                    if (e.Args[14].ToString() == "EVADE")
                    {
                        CLULogger.TroubleshootLog("Mob is evading ranged attack. Blacklisting it!");
                        Blacklist.Add(e.DestGuid, TimeSpan.FromMinutes(30));
                        if (StyxWoW.Me.CurrentTargetGuid == e.DestGuid)
                        {
                            StyxWoW.Me.ClearTarget();
                        }

                        BotPoi.Clear("Blacklisting evading mob");
                        StyxWoW.SleepForLagDuration();
                    }
                    else if (e.Args[14].ToString() == "IMMUNE")
                    {
                        WoWUnit unit = e.DestUnit;
                        if (unit != null && !unit.IsPlayer)
                        {
                            CLULogger.TroubleshootLog("{0} is immune to {1} spell school", unit.Name, e.SpellSchool);
                            SpellImmunityManager.Add(unit.Entry, e.SpellSchool);
                        }
                    }
                    break;

                case "SPELL_AURA_REFRESH":
                    if (e.SourceGuid == StyxWoW.Me.Guid)
                    {
                        if (e.SpellId == 1822)
                        {
                            Classes.Druid.Common.RakeMultiplier = 1;

                            //TF
                            if (StyxWoW.Me.HasAura(5217))
                                Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.15;

                            //Savage Roar
                            if (StyxWoW.Me.HasAura(127538))
                                Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.3;

                            //Doc
                            if (StyxWoW.Me.HasAura(108373))
                                Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.25;
                        }
                        if (e.SpellId == 1079)
                        {
                            Classes.Druid.Common.ExtendedRip = 0;
                            Classes.Druid.Common.RipMultiplier = 1;

                            //TF
                            if (StyxWoW.Me.HasAura(5217))
                                Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.15;

                            //Savage Roar
                            if (StyxWoW.Me.HasAura(127538))
                                Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.3;

                            //Doc
                            if (StyxWoW.Me.HasAura(108373))
                                Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.25;
                        }
                    }
                    break;

                case "SPELL_AURA_APPLIED":
                    if (e.SourceGuid == StyxWoW.Me.Guid)
                    {
                        if (e.SpellId == 1822)
                        {
                            Classes.Druid.Common.RakeMultiplier = 1;

                            //TF
                            if (StyxWoW.Me.HasAura(5217))
                                Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.15;

                            //Savage Roar
                            if (StyxWoW.Me.HasAura(127538))
                                Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.3;

                            //Doc
                            if (StyxWoW.Me.HasAura(108373))
                                Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.25;
                        }
                        if (e.SpellId == 1079)
                        {
                            Classes.Druid.Common.ExtendedRip = 0;
                            Classes.Druid.Common.RipMultiplier = 1;

                            //TF
                            if (StyxWoW.Me.HasAura(5217))
                                Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.15;

                            //Savage Roar
                            if (StyxWoW.Me.HasAura(127538))
                                Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.3;

                            //Doc
                            if (StyxWoW.Me.HasAura(108373))
                                Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.25;
                        }
                    }
                    break;

                case "SPELL_AURA_REMOVED":
                    if (e.SourceGuid == StyxWoW.Me.Guid)
                    {
                        if (e.SpellId == 1822)
                        {
                            Classes.Druid.Common.RakeMultiplier = 0;
                        }
                        if (e.SpellId == 1079)
                        {
                            Classes.Druid.Common.ExtendedRip = 0;
                            Classes.Druid.Common.RipMultiplier = 0;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Dumps the spells that are locked to the spelllockwatcher GUI
        /// </summary>
        /// <returns> a list of spells within the spelllock dictionary</returns>
        public Dictionary<string, double> DumpSpellLocks()
        {
            var ret = new Dictionary<string, double>();
            var now = DateTime.Now;

            foreach (var x in Locks)
            {
                var s = x.Value.Subtract(now).TotalSeconds;
                if (s < 0) s = 0;
                s = Math.Round(s, 3);
                ret[x.Key] = s;
            }

            return ret;
        }
    }
}