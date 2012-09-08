using Styx.TreeSharp;
using System.Linq;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Settings;
using CLU.Base;

namespace CLU.Classes.Warrior
{
    class Fury : RotationBase
    {

        private const int ItemSetId = 1073; // Tier set ID Colossal Dragonplate (Normal)

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
                                    // Correct Stance 
                                    Spell.CastSelfSpell("Berserker Stance", ret => !Buff.PlayerHasBuff("Berserker Stance"), "Berserker Stance"),

                                    Spell.CastSelfSpell("Death Wish",          ret => CLUSettings.Instance.UseCooldowns, "Death Wish"),
                                    Spell.CastAreaSpell("Cleave", 5, false, 2, 0.0, 0.0, a => true, "Cleave"),
                                    Spell.CastAreaSpell("Whirlwind", 8, false, 2, 0.0, 0.0, a => true, "Whirlwind"),
                                    Spell.CastSelfSpell("Inner Rage",          ret => Me.CurrentTarget != null && Unit.EnemyUnits.Count() > 1 && ((Me.CurrentRage >= 75 && Me.CurrentTarget.HealthPercent >= 20) || (Buff.PlayerHasBuff("Incite") || Buff.TargetHasDebuff("Colossus Smash") && ((Me.CurrentRage >= 40 && Me.CurrentTarget.HealthPercent >= 20) || (Me.CurrentRage >= 65 && Me.CurrentTarget.HealthPercent < 20)))), "Inner Rage"),
                                    Spell.CastSpell("Heroic Strike",           ret => Me.CurrentTarget != null && (Me.CurrentRage >= 85 || (Buff.PlayerHasBuff("Volatile Outrage") && Buff.PlayerHasBuff("Inner Rage") && Me.CurrentRage >= 75)) && Me.CurrentTarget.HealthPercent >= 20, "Heroic Strike"),
                                    Spell.CastSpell("Heroic Strike",           ret => Buff.PlayerHasBuff("Battle Trance"), "Heroic Strike"),
                                    Spell.CastSpell("Heroic Strike",           ret => Me.CurrentTarget != null && (Buff.PlayerHasBuff("Incite") || Buff.TargetHasDebuff("Colossus Smash")) && (((Me.CurrentRage >= 50 || (Me.CurrentRage >= 40 && Buff.PlayerHasBuff("Volatile Outrage") && Buff.PlayerHasBuff("Inner Rage"))) && Me.CurrentTarget.HealthPercent >= 20) || ((Me.CurrentRage >= 75 || (Me.CurrentRage >= 65 && Buff.PlayerHasBuff("Volatile Outrage") && Buff.PlayerHasBuff("Inner Rage"))) && Me.CurrentTarget.HealthPercent < 20)), "Heroic Strike"),
                                    Spell.CastSpell("Execute",                 ret => Buff.PlayerBuffTimeLeft("Executioner") < 1.5, "Execute"),
                                    Spell.CastSpell("Colossus Smash",          ret => true, "Colossus Smash"),
                                    Spell.CastSpell("Execute",                 ret => Buff.PlayerCountBuff("Executioner") < 5, "Execute"),
                                    Spell.CastSpell("Bloodthirst",             ret => true, "Bloodthirst"),
                                    Spell.CastSelfSpell("Berserker Rage",      ret => (!Buff.PlayerHasBuff("Death Wish") || !Buff.PlayerHasBuff("Enrage") || !Buff.PlayerHasBuff("Unholy Frenzy")) && Me.CurrentRage > 15, "Berserker Rage"),
                                    Spell.CastSpell("Raging Blow",             ret => true, "Raging Blow"),
                                    Spell.CastSpell("Slam",                    ret => Buff.PlayerHasActiveBuff("Bloodsurge"), "Slam"),
                                    Spell.CastSpell("Execute",                 ret => Me.CurrentRage >= 50, "Execute"),
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
                            Buff.CastRaidBuff("Battle Shout",       ret => true, "Battle Shout")));
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