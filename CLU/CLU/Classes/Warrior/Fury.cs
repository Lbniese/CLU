﻿using Styx.TreeSharp;
using System.Linq;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Settings;
using CLU.Base;
using Styx.CommonBot;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Warrior
{
    using Styx;

    class Fury : RotationBase
    {

        private const int ItemSetId = 1145; // Tier set ID Plate of Resounding Rings

        public override string Name
        {
            get { return "Fury Warrior"; }
        }

        public override string KeySpell
        {
            get { return "Bloodthirst"; }
        }

        public override float CombatMaxDistance
        {
            get { return 3.2f; }
        }

        public override string Help
        {
            get
            {
                return "----------------------------------------------------------------------\n" +
                       "2pc Tier set Bonus?: " + Item.Has2PcTeirBonus(ItemSetId) + "\n" +
                       "4pc Tier set Bonus?: " + Item.Has4PcTeirBonus(ItemSetId) + "\n" +
                       "This Rotation will:\n" +
                       "1. Heal using Victory Rush, Enraged Regeneration\n" +
                       "==> Rallying Cry, Healthstone \n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "5. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to \n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector(
                                // Pause Rotation
                                new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                                // For DS Encounters.
                                EncounterSpecific.ExtraActionButton(),

                                new Decorator(
                                    ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                        new PrioritySelector(
                                        Item.UseTrinkets(),
                                        Spell.UseRacials(),
                                        Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                        Item.UseEngineerGloves())),
                                    // Interupts
                                    Spell.CastInterupt("Pummel", ret => true, "Pummel"),
                                    Spell.CastInterupt("Spell Reflection", ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTarget == Me, "Spell Reflection"),

                                    Spell.CastSelfSpell("Recklessness",        ret => CLUSettings.Instance.UseCooldowns && Me.CurrentTarget != null && ((Buff.TargetDebuffTimeLeft("Colossus Smash").TotalSeconds >= 5 || Spell.SpellCooldown("Colossus Smash").TotalSeconds <= 4) && ((!SpellManager.HasSpell("Avatar") || !Item.Has4PcTeirBonus(ItemSetId))) && ((Me.CurrentTarget.HealthPercent < 20 || Unit.TimeToDeath(Me.CurrentTarget) > 315 || (Unit.TimeToDeath(Me.CurrentTarget) > 165 && Item.Has4PcTeirBonus(ItemSetId)))) || (SpellManager.HasSpell("Avatar") && Item.Has4PcTeirBonus(ItemSetId) && Buff.PlayerHasBuff("Avatar"))) || Unit.TimeToDeath(Me.CurrentTarget) <= 18, "Recklessness"),
                                    Spell.CastSelfSpell("Avatar",              ret => Me.CurrentTarget != null && (CLUSettings.Instance.UseCooldowns && SpellManager.HasSpell("Avatar") && (((Spell.SpellCooldown("Recklessness").TotalSeconds >= 180 || Buff.PlayerHasBuff("Recklessness")) || (Me.CurrentTarget.HealthPercent >= 20 && Unit.TimeToDeath(Me.CurrentTarget) > 195) || (Me.CurrentTarget.HealthPercent < 20 && Item.Has4PcTeirBonus(ItemSetId))) || Unit.TimeToDeath(Me.CurrentTarget) <= 20)), "Avatar"),
                                    Spell.CastSelfSpell("Bloodbath",           ret => Me.CurrentTarget != null && ( SpellManager.HasSpell("Bloodbath") && (((Spell.SpellCooldown("Recklessness").TotalSeconds >= 10 || Buff.PlayerHasBuff("Recklessness")) || (Me.CurrentTarget.HealthPercent >= 20 && (Unit.TimeToDeath(Me.CurrentTarget) <= 165 || (Unit.TimeToDeath(Me.CurrentTarget) <= 315 & !Item.Has4PcTeirBonus(ItemSetId))) && Unit.TimeToDeath(Me.CurrentTarget) > 75)) || Unit.TimeToDeath(Me.CurrentTarget) <= 19)), "Bloodbath"),
                                    Spell.CastSelfSpell("Berserker Rage",      ret => Me.CurrentTarget != null && !(Buff.PlayerHasBuff("Enrage") || (Buff.PlayerCountBuff("Raging Blow!") == 2 && Me.CurrentTarget.HealthPercent >= 20)), "Berserker Rage"),
                                    Spell.CastSelfSpell("Deadly Calm",         ret => CLUSettings.Instance.Warrior.UseDeadlyCalm && CLUSettings.Instance.UseCooldowns && Me.CurrentRage >= 40, "Deadly Calm"),
                                    Spell.CastSelfSpell("Death Wish",          ret => CLUSettings.Instance.UseCooldowns, "Death Wish"),
                                    Spell.CastAreaSpell("Cleave", 5, false, 2, 0.0, 0.0, a => true, "Cleave"),
                                    Spell.CastAreaSpell("Whirlwind", 8, false, 2, 0.0, 0.0, a => true, "Whirlwind"),
                                    //Spell.CastSelfSpell("Inner Rage",          ret => Me.CurrentTarget != null && Unit.EnemyUnits.Count() > 1 && ((Me.CurrentRage >= 75 && Me.CurrentTarget.HealthPercent >= 20) || (Buff.PlayerHasBuff("Incite") || Buff.TargetHasDebuff("Colossus Smash") && ((Me.CurrentRage >= 40 && Me.CurrentTarget.HealthPercent >= 20) || (Me.CurrentRage >= 65 && Me.CurrentTarget.HealthPercent < 20)))), "Inner Rage"),

                                    Spell.CastSpell("Heroic Strike",           ret => Me.CurrentTarget != null && ((((Buff.TargetHasDebuff("Colossus Smash") && Me.CurrentRage >= 40) || (Buff.PlayerHasBuff("Deadly Calm") && Me.CurrentRage >= 30)) && Me.CurrentTarget.HealthPercent >= 20) || Me.CurrentRage >= 110), "Heroic Strike"),
                                    Spell.CastSpell("Bloodthirst",             ret => Me.CurrentTarget != null && !(Me.CurrentTarget.HealthPercent < 20 && Buff.TargetHasDebuff("Colossus Smash") & Me.CurrentRage >= 30), "Bloodthirst"),
                                    Spell.CastSpell("Wild Strike",             ret => Buff.PlayerHasBuff("Bloodsurge") && Me.CurrentTarget.HealthPercent >= 20 && Spell.SpellCooldown("Bloodthirst").TotalSeconds <= 1, "Wild Strike"),
                                    Spell.CastSpell("Colossus Smash",          ret => true, "Colossus Smash"),
                                    Spell.CastSpell("Execute",                 ret => true, "Execute"),
                                    Spell.CastSpell("Storm Bolt",              ret => SpellManager.HasSpell("Storm Bolt"), "Storm Bolt"),
                                    //Item.RunMacroText("/cast Raging Blow",      ret => Buff.PlayerHasBuff("Raging Blow!"), "Raging Blow"),
                                    Spell.CastSpell("Raging Blow",             ret => Buff.PlayerHasActiveBuff("Raging Blow!"), "Raging Blow"),
                                    Spell.CastSpell("Wild Strike",             ret => Me.CurrentTarget != null && (Buff.PlayerHasBuff("Bloodsurge") && Me.CurrentTarget.HealthPercent >= 20), "Wild Strike"),
                                    Spell.CastConicSpell("Shockwave", 11f, 33f, ret => CLUSettings.Instance.Warrior.UseShockwave, "Shockwave"),
                                    Spell.CastConicSpell("Dragon Roar", 11f, 33f, ret => CLUSettings.Instance.Warrior.UseDragonRoar, "Dragon Roar"),
                                    Spell.CastSpell("Heroic Throw",            ret => StyxWoW.Me.Inventory.Equipped.MainHand != null, "Heroic Throw"),
                                    Spell.CastSpell("Bladestorm",              ret => Me.CurrentTarget != null && (SpellManager.HasSpell("Bladestorm") && Spell.SpellCooldown("Colossus Smash").TotalSeconds >= 5 && !Buff.TargetHasDebuff("Colossus Smash") && Spell.SpellCooldown("Bloodthirst").TotalSeconds >= 2 && Me.CurrentTarget.HealthPercent >= 20), "Bladestorm"),
                                    Spell.CastSpell("Wild Strike",             ret => Me.CurrentTarget != null && (Buff.TargetHasDebuff("Colossus Smash") && Me.CurrentTarget.HealthPercent >= 20), "Wild Strike"),
                                    Spell.CastSpell("Impending Victory",       ret => Me.CurrentTarget != null && (SpellManager.HasSpell("Impending Victory") && Me.CurrentTarget.HealthPercent >= 20), "Wild Strike"),
                                    Spell.CastSpell("Wild Strike",             ret => Me.CurrentTarget != null && (Spell.SpellCooldown("Colossus Smash").TotalSeconds > 1 && Me.RagePercent >= 60 && Me.CurrentTarget.HealthPercent >= 20), "Wild Strike"),
                                    Spell.CastSpell("Commanding Shout",        ret => Me.RagePercent < 70 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding, "Commanding Shout for Rage"),
                                    Spell.CastSpell("Battle Shout",            ret => Me.RagePercent < 70 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle, "Battle Shout for Rage"));
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                    new PrioritySelector(
                        Spell.CastSpell("Victory Rush",                ret => Me.HealthPercent < 80 && Buff.PlayerHasBuff("Victorious"), "Victory Rush"),
                        Spell.CastSelfSpell("Enraged Regeneration",    ret => Me.HealthPercent < 45 && !Buff.PlayerHasBuff("Rallying Cry"), "Enraged Regeneration"),
                        Spell.CastSelfSpell("Rallying Cry",            ret => Me.HealthPercent < 45 && !Buff.PlayerHasBuff("Enraged Regeneration"), "Rallying Cry"),
                        Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40 && !Buff.PlayerHasBuff("Rallying Cry") && !Buff.PlayerHasBuff("Enraged Regeneration"), "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                        ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            Buff.CastRaidBuff("Commanding Shout", ret => true, "Commanding Shout"),
                            Buff.CastRaidBuff("Battle Shout",       ret => true, "Battle Shout"),
                            Spell.CastSelfSpell("Berserker Stance", ret => !Buff.PlayerHasBuff("Berserker Stance"), "Berserker Stance")));
            }
        }

        public override Composite Resting
        {
            get { return Rest.CreateDefaultRestBehaviour(); }
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }
    }
}