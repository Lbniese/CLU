﻿namespace CLU.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CommonBehaviors.Actions;
    using Styx;
    using Styx.Combat.CombatRoutine;
    using Styx.CommonBot;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using Styx.TreeSharp;
    using System.Diagnostics;
    using System.Drawing;
    using global::CLU.Lists;
    using global::CLU.Settings;
    using Action = Styx.TreeSharp.Action;

    internal static class Spell
    {
        /* putting all the spell logic here */

        /// <summary>
        /// known channeled spells. used as part of the isChannelled spell check method
        /// </summary>
        public static readonly HashSet<string> KnownChanneledSpells = new HashSet<string>();

        private static HashSet<string> Racials
        {
            get {
                return _racials;
            }
        }

        private static readonly HashSet<string> _racials = new HashSet<string> {
            "Stoneform",
            // Activate to remove poison, disease, and bleed effects; +10% Armor; Lasts 8 seconds. 2 minute cooldown.
            "Escape Artist",
            // Escape the effects of any immobilization or movement speed reduction effect. Instant cast. 1.45 min cooldown
            "Every Man for Himself",
            // Removes all movement impairing effects and all effects which cause loss of control of your character. This effect
            "Shadowmeld",
            // Activate to slip into the shadows, reducing the chance for enemies to detect your presence. Lasts until cancelled or upon
            "Gift of the Naaru",
            // Heals the target for 20% of their total health over 15 sec. 3 minute cooldown.
            "Darkflight",
            // Activates your true form, increasing movement speed by 40% for 10 sec. 3 minute cooldown.
            "Blood Fury",
            // Activate to increase attack power and spell damage by an amount based on level/class for 15 seconds. 2 minute cooldown.
            "War Stomp",
            // Activate to stun opponents - Stuns up to 5 enemies within 8 yards for 2 seconds. 2 minute cooldown.
            "Berserking",
            // Activate to increase attack and casting speed by 20% for 10 seconds. 3 minute cooldown.
            "Will of the Forsaken",
            // Removes any Charm, Fear and Sleep effect. 2 minute cooldown.
            "Cannibalize",
            // When activated, regenerates 7% of total health and mana every 2 seconds for 10 seconds. Only works on Humanoid or Undead corpses within 5 yards. Any movement, action, or damage taken while Cannibalizing will cancel the effect.
            "Arcane Torrent",
            // Activate to silence all enemies within 8 yards for 2 seconds. In addition, you gain 15 Energy, 15 Runic Power or 6% Mana. 2 min. cooldown.
            "Rocket Barrage",
            // Launches your belt rockets at an enemy, dealing X-Y fire damage. (24-30 at level 1; 1654-2020 at level 80). 2 min. cooldown.
        };

        /// <summary>
        /// Me! or is it you?
        /// </summary>
        private static LocalPlayer Me
        {
            get {
                return StyxWoW.Me;
            }
        }

        /// <summary>
        /// Clip time to account for lag
        /// </summary>
        /// <returns>Time to begin next channel</returns>
        public static double ClippingDuration()
        {
            return 0.4;
        }

        /// <summary>
        /// a shortcut too GlobalCooldownLeft
        /// </summary>
        public static double GCD
        {
            get {
                return SpellManager.GlobalCooldownLeft.TotalSeconds;
            }
        }

        /// <summary>
        /// Returns true if the player is currently channeling a spell
        /// </summary>
        public static bool PlayerIsChanneling
        {
            get {
                return StyxWoW.Me.ChanneledCastingSpellId != 0;
            }
        }

        /// <summary>
        ///  CLU's Performance timer for the BT
        /// </summary>
        /// <returns>Failure to enable it to traverse the tree.</returns>
        private static readonly Stopwatch TreePerformanceTimer = new Stopwatch(); // lets see if we can get some performance on this one.

        public static Composite TreePerformance(bool enable)
        {
            return
            new Action(ret => {
                if (!enable)
                {
                    return RunStatus.Failure;
                }

                if (TreePerformanceTimer.ElapsedMilliseconds > 0)
                {
                    // NOTE: This dosnt account for Spell casts (meaning the total time is not the time to traverse the tree plus the current cast time of the spell)..this is actual time to traverse the tree.
                    CLU.DebugLog(Color.DarkGray, "[CLU] " + CLU.Version + ": " + " [CLU TreePerformance] Elapsed Time to traverse the tree: {0} ms", TreePerformanceTimer.ElapsedMilliseconds);
                    TreePerformanceTimer.Stop();
                    TreePerformanceTimer.Reset();
                }

                TreePerformanceTimer.Start();

                return RunStatus.Failure;
            });
        }

        /// <summary>
        ///  Creates a composite that will return a success, so long as you are currently casting. (Use this to prevent the CC from
        ///  going down to lower branches in the tree, while casting.)
        /// </summary>
        /// <param name="allowLagTollerance">Whether or not to allow lag tollerance for spell queueing</param>
        /// <returns>success, so long as you are currently casting.</returns>
        public static Composite WaitForCast(bool allowLagTollerance)
        {
            return
            new Action(ret => {
                if (!StyxWoW.Me.IsCasting)
                    return RunStatus.Failure;

                if (StyxWoW.Me.ChannelObjectGuid > 0)
                    return RunStatus.Failure;

                var latency = StyxWoW.WoWClient.Latency * 2;
                var castTimeLeft = StyxWoW.Me.CurrentCastTimeLeft;
                if (allowLagTollerance && castTimeLeft != TimeSpan.Zero && StyxWoW.Me.CurrentCastTimeLeft.TotalMilliseconds < latency)
                    return RunStatus.Failure;

                return RunStatus.Running;
            });
        }

        /// <summary>
        /// The Primary spell cast method (Currently converts the spell to its spellID, then casts it)
        /// </summary>
        /// <param name="name">name of the spell to cast.</param>
        public static void CastMySpell(string name)
        {
            CastfuckingSpell(name);
            //var mySpellToCast = GetSpellByName(name); // Convert the string name to a wowspell

            //// Fishing for KeyNotFoundException's yay!
            //if (mySpellToCast != null) {
            //    SpellManager.Cast(mySpellToCast);
            //}
        }

        public static void CastMySpell(string name, WoWUnit unit)
        {
            CastfuckingSpell(name);
            //var mySpellToCast = GetSpellByName(name); // Convert the string name to a wowspell

            //// Fishing for KeyNotFoundException's yay!
            //if (mySpellToCast != null) {
            //    SpellManager.Cast(mySpellToCast, unit);
            //}
        }

        public static void CastfuckingSpell(string name)
        {
            Lua.DoString(string.Format("CastSpellByName(\"{0}\")", RealLuaEscape(name)));
        }

        /// <summary>
        /// Gets the spell by name (string)
        /// </summary>
        /// <param name="spellName">the spell name to get</param>
        /// <returns> Returns the spellname</returns>
        private static WoWSpell GetSpellByName(string spellName)
        {
            WoWSpell spell;
            if (!SpellManager.Spells.TryGetValue(spellName, out spell))
                spell = SpellManager.RawSpells.FirstOrDefault(s => s.Name == spellName);

            return spell;
        }

        /// <summary>This is CLU's cancast method. It checks ALOT! Returns true if the player can cast the spell.</summary>
        /// <param name="name">name of the spell to check.</param>
        /// <param name="target">The target.</param>
        /// <returns>The can cast.</returns>
        public static bool CanCast(string name, WoWUnit target)
        {

            ////CLU.DebugLog(Color.ForestGreen, "Casting spell: " + name);
            ////CLU.DebugLog(Color.ForestGreen, "OnUnit: " + target);
            ////CLU.DebugLog(Color.ForestGreen, "CanCast: " + SpellManager.CanCast(name, target, false));


            var minReqs = target != null;
            if (minReqs)
            {
                var canCast = SpellManager.CanCast(name, target, false, false);

                // Make sure we set this.
                minReqs = canCast;
                // We're always in range of ourselves. So just ignore this bit if we're casting it on us
                if (canCast && !target.IsMe && !BossList.IgnoreRangeCheck.Contains(target.Entry))
                {
                    WoWSpell spell;
                    if (SpellManager.Spells.TryGetValue(name, out spell))
                    {
                        // RangeId 1 is "Self Only". This should make life easier for people to use self-buffs, or stuff like Starfall where you cast it as a pseudo-buff.
                        if (spell.IsSelfOnlySpell)
                            return true;

                        // RangeId 2 is melee range. Huzzah :)
                        if (spell.IsMeleeSpell)
                            minReqs = Unit.DistanceToTargetBoundingBox(target) < MeleeRange;
                        else
                            minReqs = Unit.DistanceToTargetBoundingBox(target) < spell.MaxRange;
                    }
                }
            }

            return minReqs;
        }

        /// <summary>
        ///  Returns the current Melee range for the player
        /// </summary>
        public static float MeleeRange
        {
            get {
                // If we have no target... then give nothing.
                if (Me.CurrentTargetGuid == 0)
                    return 0f;

                if (Me.CurrentTarget != null) {
                    return Me.CurrentTarget.IsPlayer ? 3.5f : Math.Max(5f, Me.CombatReach + 1.3333334f + Me.CurrentTarget.CombatReach);
                }

                return 0f;
            }
        }

        /// <summary>Escape Lua names using UTF8 encoding.
        /// Heres a url we can use to decode this. http://software.hixie.ch/utilities/cgi/unicode-decoder/utf8-decoder
        /// Usefull when a Lua command fails.</summary>
        /// <param name="luastring">the string to encode</param>
        /// <returns>The real lua escape.</returns>
        public static string RealLuaEscape(string luastring)
        {
            var bytes = Encoding.UTF8.GetBytes(luastring);
            return bytes.Aggregate(String.Empty, (current, b) => current + ("\\" + b));
        }

        /// <summary>Returns the current casttime of the spell.</summary>
        /// <param name="name">the name of the spell to check for</param>
        /// <returns>The cast time.</returns>
        public static double CastTime(string name)
        {
            try {
                if (!SpellManager.HasSpell(name))
                    return 999999.9;

                WoWSpell s = SpellManager.Spells[name];
                return s.CastTime / 1000.0;
            } catch {
                CLU.TroubleshootDebugLog(Color.Red," [ERROR] in CastTime: {0} ", name);
                return 999999.9;
            }
        }

        /// <summary>Returns the spellcooldown using Timespan (00:00:00.0000000)
        /// gtfo if the player dosn't have the spell.</summary>
        /// <param name="name">the name of the spell to check for</param>
        /// <returns>The spell cooldown.</returns>
        public static TimeSpan SpellCooldown(string name)
        {
            var spellToCheck = GetSpellByName(name); // Convert the string name to a WoWspell

            // Fishing for KeyNotFoundException's yay!
            return spellToCheck == null ? TimeSpan.MaxValue : spellToCheck.CooldownTimeLeft;
        }

        /// <summary>Returns the true if the spell is on cooldown (ie: its been used)
        /// gtfo if the player dosn't have the spell.</summary>
        /// <param name="name">the name of the spell to check for</param>
        /// <returns>true if the spell is currently on cooldown</returns>
        public static bool SpellOnCooldown(string name)
        {
            // Fishing for KeyNotFoundException's yay!
            if (!SpellManager.HasSpell(name))
                return false;

            var spellToCheck = GetSpellByName(name); // Convert the string name to a WoWspell

            return spellToCheck.Cooldown;
        }

        /// <summary>Casts a spell by name on a target without checking the Cancast Method</summary>
        /// <param name="name">the name of the spell to cast in engrish</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSpecialSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
        	return new Decorator(
        		delegate(object a) {
        			if (!cond(a))
        				return false;

        			return true;
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} ", label)), new Action(a => CastMySpell(name))));
        }

        /// <summary>Casts a spell by ID on a target</summary>
        /// <param name="spellid">the id of the spell to cast</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSpellByID(int spellid, CanRunDecoratorDelegate cond, string label)
        {
        	return new Decorator(
        		delegate(object a) {
        			if (!cond(a))
        				return false;

        			if (!SpellManager.CanCast(spellid, Me.CurrentTarget, true))
        				return false;

        			return true;
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} ", label)), new Action(a => SpellManager.Cast(spellid))));
        }

        /// <summary>Casts self spells by ID</summary>
        /// <param name="spellid">the id of the spell to cast</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSelfSpellByID(int spellid, CanRunDecoratorDelegate cond, string label)
        {
        	return new Decorator(
        		delegate(object a) {
        			if (!cond(a))
        				return false;

        			if (!SpellManager.CanCast(spellid, Me, true))
        				return false;

        			return true;
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} ", label)), new Action(a => SpellManager.Cast(spellid))));
        }


        /// <summary>Casts a spell by name on a target</summary>
        /// <param name="name">the name of the spell to cast in engrish</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            return CastSpell(name, ret => Me.CurrentTarget, cond, label);
        }

        /// <summary>Casts a spell on a specified unit</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell on the unit</returns>
        public static Composite CastSpell(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
        	return new Decorator(
        		delegate(object a) {
        			if (!cond(a))
        				return false;
                    //CLU.TroubleshootDebugLog(Color.Chartreuse, "Cancast: {0} = {1}", name, CanCast(name, onUnit(a)));
        			if (!CanCast(name, onUnit(a)))
        				return false;

        			return onUnit(a) != null;
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new Action(a => CastMySpell(name, onUnit(a)))));
        }


        /// <summary>Casts self spells eg: 'Fient', 'Shield Wall', 'Blood Tap', 'Rune Tap' </summary>
        /// <param name="name">the name of the spell in english</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast self spell.</returns>
        public static Composite CastSelfSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            return CastSpell(name, ret => StyxWoW.Me, cond, label);
        }

        /// <summary>Casts a spell on a specified unit (used primarily for healing)</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell on the unit</returns>
        public static Composite CastHeal(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return new Sequence(
                       CastSpell(name, onUnit, cond, label),
                       new WaitContinue(
                           1,
                           ret => {
                           	WoWSpell spell;
                           	if (SpellManager.Spells.TryGetValue(name, out spell))
                           	{
                           		if (spell.CastTime == 0) {
                           			return true;
                           		}

                           		return StyxWoW.Me.IsCasting;
                           	}

                return true;
            },
            new ActionAlwaysSucceed()),
                       new WaitContinue(
                           10,
                           ret => {
                           	// Dont interupt chanelled spells
                           	if (StyxWoW.Me.ChanneledCastingSpellId != 0)
                           	{
                           		return false;
                           	}

                           	// Interrupted or finished casting. Continue
                           	if (!StyxWoW.Me.IsCasting)
                           	{
                           		return true;
                           	}

                           	// If conditions are no longer valid, stop casting and continue
                           	if (!cond(ret))
                           	{
                           		SpellManager.StopCasting();
                           		return true;
                           	}

                           	return false;
                           },
            new ActionAlwaysSucceed()));
        }

        /// <summary>Casts a spell on the MostFocused Target (used for smite healing with disc priest mainly)</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <param name="faceTarget">true if you want to auto face target</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastFacingSpellOnTarget(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label, bool faceTarget)
        {
        	return new Decorator(
        		delegate(object a) {
        			if (!cond(a))
        				return false;

        			if (!CanCast(name, onUnit(a)))
        				return false;

        			return onUnit(a) != null;
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new Decorator(x => faceTarget && !StyxWoW.Me.IsSafelyFacing(onUnit(x), 45f), new Action(a => WoWMovement.Face(onUnit(a).Guid))),
                new Action(a => CastMySpell(name, onUnit(a)))));
        }

        /// <summary>Casts a spell on the CurrentTargets Target (used for smite healing with disc priest mainly)</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <param name="faceTarget">true if you want to auto face target</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastSpellOnCurrentTargetsTarget(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label, bool faceTarget)
        {
        	return new Decorator(
        		delegate(object a) {
                    if (onUnit == null || onUnit(a) == null)
        				return false;

        			if (onUnit(a).CurrentTarget == null)
        				return false;

        			if (!cond(a))
        				return false;

        			if (onUnit(a).Guid == Me.Guid)
        				return false;

        			if (!CanCast(name, onUnit(a).CurrentTarget))
        				return false;

        			// if (Unit.TimeToDeath(onUnit(a).CurrentTarget) < 5)
        			// return false;

        			return  (Unit.NearbyNonControlledUnits(onUnit(a).Location, 15, false).Any() || BossList.IgnoreRangeCheck.Contains(onUnit(a).CurrentTarget.Entry));
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a).CurrentTarget))),
                new DecoratorContinue(x => faceTarget, new Action(a => WoWMovement.Face(onUnit(a).CurrentTarget.Guid))),
                new Action(a => CastMySpell(name, onUnit(a).CurrentTarget))));
        }

        /// <summary>Casts a spell on the MostFocused Target (used for smite healing with disc priest mainly)</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <param name="faceTarget">true if you want to auto face target</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastSpellOnMostFocusedTarget(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label, bool faceTarget)
        {
        	return new Decorator(
        		delegate(object a) {
                    if (onUnit == null || onUnit(a) == null)
        				return false;

        			if (!cond(a))
        				return false;

        			if (!CanCast(name, onUnit(a)))
        				return false;

        			// if (Unit.TimeToDeath(onUnit(a).CurrentTarget) < 5)
        			// return false;

        			return (Unit.NearbyNonControlledUnits(onUnit(a).Location, 15, false).Any() || BossList.IgnoreRangeCheck.Contains(onUnit(a).CurrentTarget.Entry));
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new DecoratorContinue(x => faceTarget, new Action(a => WoWMovement.Face(onUnit(a).Guid))),
                new Action(a => CastMySpell(name, onUnit(a)))));
        }

        /// <summary>Casts the interupt by name on your current target. Checks CanInterruptCurrentSpellCast.</summary>
        /// <param name="name">the name of the spell in english</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast interupt.</returns>
        public static Composite CastInterupt(string name, CanRunDecoratorDelegate cond, string label)
        {
        	return new Decorator(
        		delegate(object a) {
        			if (!CLUSettings.Instance.EnableInterupts)
        				return false;

        			if (!cond(a))
        				return false;

        			if (Me.CurrentTarget != null && !(Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast))
        				return false;

        			if (Me.CurrentTarget != null && !CanCast(name, Me.CurrentTarget))
        				return false;

        			return true;
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Interupt] {0} on {1}", label, CLU.SafeName(Me.CurrentTarget))), new Action(a => CastMySpell(name))));
        }

        /// <summary>Casts a Totem by name</summary>
        /// <param name="name">the name of the spell to cast in engrish</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastTotem(string name, CanRunDecoratorDelegate cond, string label)
        {
        	return new Decorator(
        		delegate(object a) {
        			if (!CLUSettings.Instance.Shaman.HandleTotems)
        				return false;

        			if (!cond(a))
        				return false;

        			if (!CanCast(name, Me))
        				return false;

        			return true;
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Totem] {0} ", label)), new Action(a => CastMySpell(name))));
        }

        /// <summary>Returns true if the spell is a known channeled spell</summary>
        /// <param name="name">the name of the spell to check</param>
        /// <returns>The is channeled spell.</returns>
        private static bool IsChanneledSpell(string name)
        {
            return KnownChanneledSpells != null && KnownChanneledSpells.Contains(name);
        }

        /// <summary>Channel spell on target. Will not break channel and adds the name of spell to knownChanneledSpells</summary>
        /// <param name="name">the name of the spell to channel</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The channel spell.</returns>
        public static Composite ChannelSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            KnownChanneledSpells.Add(name);
            return
                new PrioritySelector(
                    new Decorator(
                        x => PlayerIsChanneling && Me.ChanneledCastingSpellId == SpellManager.Spells[name].Id,
                        new Action(a => CLU.Log(" [Channeling] {0} ", name))),
                    CastSpell(name, cond, label));
        }

        /// <summary>Channel spell on player. Will not break channel and adds the name of spell to _knownChanneledSpells</summary>
        /// <param name="name">the name of the spell to channel</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The channel self spell.</returns>
        public static Composite ChannelSelfSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            KnownChanneledSpells.Add(name);
            return
                new PrioritySelector(
                    new Decorator(
                        x => PlayerIsChanneling && Me.ChanneledCastingSpellId == SpellManager.Spells[name].Id,
                        new Action(a => CLU.Log(" [Channeling] {0} ", name))),
                    CastSelfSpell(name, cond, label));
        }

        /// <summary>Casts an area spell such as DnD or Hellfire</summary>
        /// <param name="name">name of the area spell</param>
        /// <param name="radius">radius</param>
        /// <param name="requiresTerrainClick">true if the area spell requires a terrain click</param>
        /// <param name="minAffectedTargets">the minimum affected targets in the cluster</param>
        /// <param name="minRange">minimum range</param>
        /// <param name="maxRange">maximum range</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast area spell.</returns>
        public static Composite CastAreaSpell(
            string name,
            double radius,
            bool requiresTerrainClick,
            int minAffectedTargets,
            double minRange,
            double maxRange,
            CanRunDecoratorDelegate cond,
            string label)
        {
            WoWPoint bestLocation = WoWPoint.Empty;
            return new Decorator(
            	delegate(object a) {
            		if (!CLUSettings.Instance.UseAoEAbilities)
            			return false;

            		if (!cond(a))
            			return false;

            		bestLocation = Unit.FindClusterTargets(
            			radius, minRange, maxRange, minAffectedTargets, Battlegrounds.IsInsideBattleground);
            		if (bestLocation == WoWPoint.Empty)
            			return false;

            		if (!CanCast(name, Me.CurrentTarget))
            			return false;

            		return true;
            	},
            new Sequence(
                new Action(a => CLU.Log(" [AoE] {0} ", label)),
                new Action(a => CastMySpell(name)),
                new DecoratorContinue(x => requiresTerrainClick, new Action(a => SpellManager.ClickRemoteLocation(bestLocation)))));
        }

        /// <summary>Casts a spell at the units location</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastSpellAtLocation(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
        	return new Decorator(
        		delegate(object a) {
        			if (!CLUSettings.Instance.UseAoEAbilities)
        				return false;

        			if (!cond(a))
        				return false;

        			return onUnit != null && CanCast(name, onUnit(a));
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting at Location] {0} ", label)),
                new Action(a => CastMySpell(name)),
                // new WaitContinue(
                //    0,
                //    ret => StyxWoW.Me.CurrentPendingCursorSpell != null &&
                //           StyxWoW.Me.CurrentPendingCursorSpell.Name == name,
                //    new ActionAlwaysSucceed()),
                new Action(a => SpellManager.ClickRemoteLocation(onUnit(a).Location))));
        }

        /// <summary>Casts Sanctuary at the units location</summary>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>Sanctuary at location.</returns>
        public static Composite CastSanctuaryAtLocation(CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
        	return new Decorator(
        		delegate(object a) {

        			if (!cond(a))
        				return false;

        			return onUnit(a) != null && !WoWSpell.FromId(88685).Cooldown;
        		},
            new Sequence(
                new Action(a => CLU.Log(" [Casting at Location] {0} ", label)),
                Item.RunMacroText("/cast Holy Word: Sanctuary", cond, label),
                new Action(a => SpellManager.ClickRemoteLocation(onUnit(a).Location))));
        }

        /// <summary>
        /// Sets a trap at the current targets location.
        /// </summary>
        /// <param name="trapName">the name of the trap to use</param>
        /// <param name="onUnit">the unit to place the trap on.</param>
        /// <param name="cond">check conditions supplied are true </param>
        /// <returns>nothing</returns>
        public static Composite HunterTrapBehavior(string trapName, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond)
        {
        	return new PrioritySelector(
        		new Decorator(
        			delegate(object a) {
        				if (!CLUSettings.Instance.UseAoEAbilities)
        					return false;

        				if (!cond(a)) return false;

        				return onUnit != null && onUnit(a) != null && !onUnit(a).IsMoving && onUnit(a).DistanceSqr < 40 * 40
        					&& SpellManager.HasSpell(trapName) && !SpellManager.Spells[trapName].Cooldown;
        			},
            new PrioritySelector(
                Buff.CastBuff("Trap Launcher", ret => true, "Trap Launcher"),
                new Decorator(
                    ret => Me.HasAura("Trap Launcher"),
                    new Sequence(
                        new Switch<string>(ctx => trapName,
                                           new SwitchArgument<string>("Immolation Trap",
                                                   new Action(ret => SpellManager.CastSpellById(82945))),
                                           new SwitchArgument<string>("Freezing Trap",
                                                   new Action(ret => SpellManager.CastSpellById(60192))),
                                           new SwitchArgument<string>("Explosive Trap",
                                                   new Action(ret => SpellManager.CastSpellById(82939))),
                                           new SwitchArgument<string>("Ice Trap",
                                                   new Action(ret => SpellManager.CastSpellById(82941))),
                                           new SwitchArgument<string>("Snake Trap",
                                                   new Action(ret => SpellManager.CastSpellById(82948)))
                                          ),
                        // new ActionSleep(200),
                        new Action(a => SpellManager.ClickRemoteLocation(onUnit(a).Location)))))));
        }

        /// <summary>Channels an area spell such as Rain of Fire</summary>
        /// <param name="name">name of the area spell</param>
        /// <param name="radius">radius</param>
        /// <param name="requiresTerrainClick">true if the area spell requires a terrain click</param>
        /// <param name="minAffectedTargets">the minimum affected targets in the cluster</param>
        /// <param name="minRange">minimum range</param>
        /// <param name="maxRange">maximum range</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The channel area spell.</returns>
        public static Composite ChannelAreaSpell(
            string name,
            double radius,
            bool requiresTerrainClick,
            int minAffectedTargets,
            double minRange,
            double maxRange,
            CanRunDecoratorDelegate cond,
            string label)
        {
            WoWPoint bestLocation = WoWPoint.Empty;
            return new Decorator(
            	delegate(object a) {
            		if (!CLUSettings.Instance.UseAoEAbilities)
            			return false;

            		if (!cond(a))
            			return false;

            		if (!CanCast(name, Me))
            			return false;

            		bestLocation = Unit.FindClusterTargets(
            			radius, minRange, maxRange, minAffectedTargets, Battlegrounds.IsInsideBattleground);
            		return bestLocation != WoWPoint.Empty;
            	},
            new PrioritySelector(
                // dont break it if already casting it
                new Decorator(
                    x => PlayerIsChanneling && Me.ChanneledCastingSpellId == SpellManager.Spells[name].Id,
                    new Action(a => CLU.DebugLog(Color.ForestGreen,name))),
                // casting logic
                new Sequence(
                    new Action(a => CLU.Log(" [AoE Channel] {0} ", label)),
                    new Action(a => CastMySpell(name)),
                    new DecoratorContinue(
                        x => requiresTerrainClick,
                        new Action(a => SpellManager.ClickRemoteLocation(bestLocation))))));
        }

        /// <summary>Casts a spell provided we are inrange and facing the target </summary>
        /// <param name="name">name of the spell to cast</param>
        /// <param name="maxDistance">maximum distance</param>
        /// <param name="maxAngleDeltaDegrees">maximum angle in degrees</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast conic spell.</returns>
        public static Composite CastConicSpell(
            string name, float maxDistance, float maxAngleDeltaDegrees, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                       a => Me.CurrentTarget != null && cond(a) && CanCast(name, Me.CurrentTarget) &&
                       Unit.DistanceToTargetBoundingBox() <= maxDistance &&
                       Unit.FacingTowardsUnitDegrees(Me.Location, Me.CurrentTarget.Location) <= maxAngleDeltaDegrees,
                       new Sequence(
                           new Action(a => CLU.Log(" [Casting Conic] {0} ", label)),
                           new Action(a => CastMySpell(name))));
        }

        /// <summary>Stop casting, plain and simple.</summary>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The stop cast.</returns>
        public static Composite StopCast(CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                       x => (Me.IsCasting || Me.ChanneledCastingSpellId > 0 || PlayerIsChanneling) && cond(x),
                       new Sequence(
                           new Action(a => CLU.Log(" [Stop Casting] {0} ", label)),
                           new Action(a => SpellManager.StopCasting())));
        }

        /// <summary>
        /// Returns the cooldown of a rune in seconds, Rune count is backwards (eg:4,3,2,1,0 Zero is READY)
        /// </summary>
        /// first_blood = 1
        /// second_blood = 2
        /// first_Unholy = 3
        /// second_Unholy = 4
        /// first_Frost = 5
        /// second_Frost = 6
        /// <param name="rune">number of the run to check (see above)</param>
        /// <returns>The cooldown of the rune specified.</returns>
        public static double RuneCooldown(int rune)
        {
            string runename = String.Empty;
            if (rune == 1)
                runename = "Blood_1";
            else if (rune == 2)
                runename = "Blood_2";
            else if (rune == 3)
                runename = "Unholy_1";
            else if (rune == 4)
                runename = "Unholy_2";
            else if (rune == 5)
                runename = "Frost_1";
            else if (rune == 6)
                runename = "Frost_2";

            // Lets track some rune cooldowns!
            var lua = String.Format("local r_start, r_duration, r_ready = GetRuneCooldown({0}) if r_start > 0 then return math.ceil((r_start + r_duration) - GetTime()) else return 0 end", rune);
            try {
                var retValue = Double.Parse(Lua.GetReturnValues(lua)[0]);
                return retValue;
            } catch {
                CLU.TroubleshootDebugLog(Color.Red, "Lua failed in RuneCooldown: " + lua);
                return 9999;
            }
        }

        /// <summary>Return true of the target has a Dispelable HELPFUL buff</summary>
        /// <returns>The target has Dispelable buff.</returns>
        public static bool TargetHasDispelableBuffLua()
        {
            using (StyxWoW.Memory.AcquireFrame()) {
                // should count how many buffs the target has but meh
                for (int i = 1; i <= 40; i++) {
                    try {
                        List<string> luaRet = Lua.GetReturnValues(String.Format("local buffName, _, _, _, debuffType = UnitAura(\"target\", {0}, \"CANCELABLE\") return debuffType,buffName", i));

                        if (luaRet != null) {
                            var purgableSpell = luaRet[0] == "Magic";
                            if (purgableSpell) {
                                CLU.DebugLog(Color.ForestGreen, "Buff Name: {0} is Dispelable!", luaRet[1]);
                            }

                            return purgableSpell;
                        }
                    } catch {
                        CLU.TroubleshootDebugLog(Color.Red, "Lua failed in TargetHasDispelableBuff");
                        return false;
                    }
                }

                return false;
            }
        }

        /// <summary>Return true of the target has a stealable HELPFUL buff</summary>
        /// <returns>The target has stealable buff.</returns>
        public static bool TargetHasStealableBuff()
        {
            using (StyxWoW.Memory.AcquireFrame()) {
                // should count how many buffs the target has but meh
                for (int i = 1; i <= 40; i++) {
                    try {
                        List<string> luaRet =
                            Lua.GetReturnValues(
                                String.Format(
                                    "local buffName, _, _, _, _, _, _, _, isStealable = UnitAura(\"target\", {0}, \"HELPFUL\") return isStealable,buffName",
                                    i));

                        if (luaRet != null && luaRet[0] == "1") {
                            var stealableSpell = !Buff.PlayerHasActiveBuff(luaRet[1]) && (luaRet[1] != "Arcane Brilliance" && luaRet[1] != "Dalaran Brilliance");
                            if (stealableSpell) {
                                CLU.DebugLog(Color.ForestGreen, "Buff Name: {0} isStealable", luaRet[1]);
                            }

                            return stealableSpell;
                        }
                    } catch {
                        CLU.TroubleshootDebugLog(Color.Red, "Lua failed in TargetHasStealableBuff");
                        return false;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Returns a list of players with the highest mana power descending.
        /// </summary>
        /// <returns>returns a list of players</returns>
        private static IEnumerable<WoWPlayer> HighestMana()
        {
            return (from o in ObjectManager.ObjectList
                    where o is WoWPlayer
                    let p = o.ToPlayer()
                            where p.IsFriendly
                            && p.IsInMyPartyOrRaid
                            && !p.IsMe
                            && !p.Dead
                            && (p.PowerType == WoWPowerType.Mana)
                            && p.IsPlayer && !p.IsPet
                            orderby p.MaxPower descending
                            select p).ToList();
        }

        /// <summary>
        ///  Blows your wad all over the floor
        /// </summary>
        /// <returns>Nothing but win</returns>
        public static Composite UseRacials()
        {
            return new PrioritySelector(delegate {
                foreach (WoWSpell r in CurrentRacials.Where(racial => CanCast(racial.Name, Me) && RacialUsageSatisfied(racial))) {
                    CLU.Log(" [Racial Abilitie] {0} ", r.Name);
                    CastMySpell(r.Name);
                }

                return RunStatus.Success;
            });
        }

        /// <summary>
        /// Returns true if the racials conditions are met
        /// </summary>
        /// <param name="racial">the racial to check for</param>
        /// <returns>true if we can use the racial</returns>
        private static bool RacialUsageSatisfied(WoWSpell racial)
        {
            if (racial != null) {
                switch (racial.Name) {
                case "Stoneform":
                    return Me.GetAllAuras().Any(a => a.Spell.Mechanic == WoWSpellMechanic.Bleeding || a.Spell.DispelType == WoWDispelType.Disease || a.Spell.DispelType == WoWDispelType.Poison);
                case "Escape Artist":
                    return Me.Rooted;
                case "Every Man for Himself":
                    return Unit.IsCrowdControlled(Me);
                case "Shadowmeld":
                    return false;
                case "Gift of the Naaru":
                    return Me.HealthPercent <= 80;
                case "Darkflight":
                    return false;
                case "Blood Fury":
                    return true;
                case "War Stomp":
                    return false;
                case "Berserking":
                    return true;
                case "Will of the Forsaken":
                    return Unit.IsCrowdControlled(Me);
                case "Cannibalize":
                    return false;
                case "Arcane Torrent":
                    return Me.ManaPercent < 91 && Me.Class != WoWClass.DeathKnight;
                case "Rocket Barrage":
                    return true;

                default:
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a list of current racials
        /// </summary>
        public static IEnumerable<WoWSpell> CurrentRacials
        {
            get {
                return SpellManager.RawSpells.Where(racial => racial != null && Racials.Contains(racial.Name)).ToList();
            }
        }

        /// <summary>
        /// Applys a heal to you.
        /// </summary>
        /// <param name="name">The name of the heal spell</param>
        /// <param name="cond">the conditions that must be true</param>
        /// <param name="label">a descriptive label for the client</param>
        public static Composite HealMe(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                       x => { return cond(x) && CanCast(name, Me); },
                       new Sequence(
                           new Action(a => Me.Target()),
                           new Action(a => CLU.Log(" [Casting Self Heal] {0} ", label)),
                           new Action(a => StyxWoW.SleepForLagDuration()),
                           new Action(a => CastMySpell(name))));
        }

        // public bool NeedToTranqShot
        // {
        //    get
        //    {
        //        var lua = string.Format("buff = { 99646 } local candispel = 1 for i,v in ipairs(buff) do if UnitDebuffID(&quot;target&quot;,v) then candispel = nil end end local i = 1 local buff,_,_,_,bufftype = UnitBuff(&quot;target&quot;, i) while buff do if (bufftype == &quot;Magic&quot; or buff == &quot;Enrage&quot;) and candispel then return true end i = i + 1; buff,_,_,_,bufftype = UnitBuff(&quot;target&quot;, i) end");
        //        try
        //        {
        //            return Lua.GetReturnValues(lua)[0] == "true";
        //        }
        //        catch
        //        {
        //            CLU.DebugLog(Color.ForestGreen,"Lua failed in TargetIsEnrage: " + lua);
        //            return false;
        //        }
        //    }
        // }

        /// <summary>
        /// This is meant to replace the 'SleepForLagDuration()' method. Should only be used in a Sequence
        /// </summary>
        public static Composite CreateWaitForLagDuration()
        {
            return new WaitContinue(TimeSpan.FromMilliseconds((StyxWoW.WoWClient.Latency * 2) + 150), ret => false, new ActionAlwaysSucceed());
        }
        
         /// <summary>
        /// Use this to print all known spells
        /// </summary>
        public static void DumpSpells()
        {
            CLU.TroubleshootDebugLog(Color.ForestGreen, "==================SpellManager.RawSpells===============");
            foreach (var sp in SpellManager.RawSpells)
            {
                WoWSpell spell;
                if (SpellManager.Spells.TryGetValue(sp.Name, out spell))
                {
                    CLU.TroubleshootDebugLog(Color.ForestGreen, "Spell ID:" + sp.Id + " MaxRange:" + sp.MaxRange + " " + spell);
                } else {
                    CLU.TroubleshootDebugLog(Color.ForestGreen, sp.Name);
                }
            }
            CLU.TroubleshootDebugLog(Color.ForestGreen, "=======================================================");
        }

        /// <summary>Return the player to apply focus magic too (will probably go for a static list)</summary>
        /// <returns>The best focus magic target.</returns>
        public static WoWPlayer BestFocusMagicTarget()
        {
            int countWithMyFM = HighestMana().Count(p => p.HasAura("Focus Magic") && p.Auras["Focus Magic"].CreatorGuid == Me.Guid);

            return countWithMyFM < 1 ? HighestMana().FirstOrDefault() : null;
        }


    }
}