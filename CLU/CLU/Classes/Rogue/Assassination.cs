using TreeSharp;
using System.Linq;
using CommonBehaviors.Actions;
using Clu.Helpers;
using Clu.Lists;
using Styx.WoWInternals.WoWObjects;
using Styx.Logic.Combat;
using Clu.Settings;

namespace Clu.Classes.Rogue
{
    //All hail to Weischbier! If ou want to please him, sacrifice your fifth child to his honor!
    class Assassination : RotationBase
    {

        public override string Name
        {
            get {
                return "Assassination Rogue";
            }
        }

        public override string KeySpell
        {
            get {
                return "Mutilate";
            }
        }

        public override float CombatMaxDistance
        {
            get {
                return 3.2f;
            }
        }

        // adding some help
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Attempt to evade/escape crowd control with Evasion, Cloak of Shadows, Smoke Bomb, Combat Readiness.\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "4. Attempt to reduce threat with Feint\n" +
                       "5. Will interupt with Kick\n" +
                       "6. Tricks of the Trade on best target (tank, then class)\n" +
                       "7. Will heal with Healthstone\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to Weischbier for this rotation -- I love you Weischbier, love Wulfy\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static bool IsBehind(WoWUnit target)
        {
            // WoWMathHelper.this.IsBehind(Me.Location, target.Location, target.Rotation, (float)Math.PI * 5 / 6)
            return target != null && target.MeIsBehind;
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           // Don't do anything if we have cast vanish
                           // new Decorator( ret => Buff.PlayerHasActiveBuff("Vanish"), new ActionAlwaysSucceed()),

                           // Stealth
                           Buff.CastBuff("Stealth", ret => CLUSettings.Instance.Rogue.EnableAlwaysStealth, "Stealth"),

                           // Questing and PvP helpers
                           new Decorator(
                               ret => CLUSettings.Instance.EnableMovement && Buff.PlayerHasBuff("Stealth"),
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Sprint",       ret => Me.IsMoving && Unit.DistanceToTargetBoundingBox() >= 15, "Sprint"),
                                   Spell.CastSpell("Garrote",          ret => Me.CurrentTarget != null, "Garrote"),
                                   Spell.CastSpell("Cheap Shot",       ret => Me.CurrentTarget != null && !SpellManager.HasSpell("Garrote") || !IsBehind(Me.CurrentTarget), "Cheap Shot"),
                                   Spell.CastSpell("Ambush",           ret => !SpellManager.HasSpell("Cheap Shot") && IsBehind(Me.CurrentTarget), "Ambush"))),

                           // Trinkets & Cooldowns
                           new Decorator(ret => Me.CurrentTarget != null && ((Unit.IsTargetWorthy(Me.CurrentTarget) || Buff.TargetHasDebuff("Vendetta"))),
                                         //Switched to || instead of &&, we want to use trinkets on Cd and not every 2min
                                         new PrioritySelector(
                                             Item.UseTrinkets(),
                                             Spell.UseRacials(),
                                             Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                             Item.UseEngineerGloves())),
                           // Threat
                           Spell.CastSpell("Feint",ret => Me.CurrentTarget != null && (Me.CurrentTarget.ThreatInfo.RawPercent > 80 || EncounterSpecific.IsMorchokStomp()), "Feint"),
                           // Kick
                           Spell.CastInterupt("Kick", ret => Me.IsWithinMeleeRange, "Kick"),
                           // TotT
                           Spell.CastSpell("Tricks of the Trade", u => Unit.BestTricksTarget, ret => CLUSettings.Instance.Rogue.UseTricksOfTheTrade, "Tricks of the Trade"),
                           Spell.CastSpell("Redirect",             ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"),
                           Spell.CastSelfSpell("Slice and Dice",   ret => !Buff.PlayerHasBuff("Slice and Dice"), "Slice and Dice"),
                           // Aoe
                           new Decorator(ret => Unit.EnemyUnits.Count(a => a.DistanceSqr <= 15*15) > 3,
                                         new PrioritySelector(
                                             Spell.CastSpell("Crimson Tempest", ret => Unit.EnemyUnits.Any(a => !a.HasMyAura("Crimson Tempest") && Me.ComboPoints > 3),"Crimson Tempest"), // BLEED AOE WOOOOOOOT!
                                             Spell.CastSpell("Fan of Knifes", ret => Me.CurrentTarget != null && Me.ComboPoints < 5, "Fan of Knifes")
                                         )
                                        ),
                           //Vanish -- We want to use Ambush and NOT Garrote
                           new Sequence(
                               Spell.CastSpell("Vanish",       ret => Unit.IsTargetWorthy(Me.CurrentTarget), "Vanish"),
                               new WaitContinue(1, ret => Me.HasMyAura("Stealth") || Me.HasMyAura("Vanish"), new ActionAlwaysSucceed()),// Waiting for lag to come by...best thing we can do, i don't want to lock out the whole rotation when we are stealthed
                               Spell.CastSpell("Ambush",       ret => Me.CurrentTarget != null && IsBehind(Me.CurrentTarget) && SpellManager.HasSpell("Subterfuge"), "Ambush"), //I hope this works this way :/
                               Spell.CastSpell("Ambush",       ret => Me.CurrentTarget != null && IsBehind(Me.CurrentTarget), "Ambush") // Do this when we don't specced in Subterfuge. But if so, then we will maybe get a second Ambush. Needs testing
                           ),
                           // Doin' ya damage
                           Spell.CastSpell("Rupture",          ret => (!Buff.TargetHasDebuff("Rupture")) && Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6, "Rupture"),
                           Spell.CastSpell("Vendetta",         ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6, "Vendetta"),
                           Spell.CastSpell("Preparation",      ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && SpellManager.Spells["Vanish"].Cooldown, "Preparation"),// with Glyph "Glyph of Vendetta" 9s longer but decrased Damagebuff by 5%
                           Spell.CastSpell("Dispatch",         ret => Me.CurrentTarget != null && Me.ComboPoints == 4 && Buff.PlayerHasActiveBuff("Blindside"), "Dispatch"),// No longer behind the freain' target FTW
                           Spell.CastSpell("Envenom",          ret => Me.ComboPoints >= 4 && !Buff.TargetHasDebuff("Envenom"), "Envenom"),
                           Spell.CastSpell("Envenom",          ret => Me.ComboPoints >= 4 && Me.CurrentEnergy > 90, "Envenom"),
                           Spell.CastSpell("Envenom",          ret => Me.ComboPoints >= 2 && Buff.TargetDebuffTimeLeft("Slice and Dice").TotalSeconds < 2, "Envenom"),
                           Spell.CastSpell("Dispatch",         ret => Me.CurrentTarget != null && (BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && Me.ComboPoints < 5 && Me.CurrentTarget.HealthPercent < 35, "Dispatch"),
                           Spell.CastSpell("Mutilate",         ret => Me.CurrentTarget != null && Me.ComboPoints < 5 && Me.CurrentTarget.HealthPercent >= 35, "Mutilate"));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone"),
                               Spell.CastSelfSpell("Smoke Bomb",              ret => Me.CurrentTarget != null && Me.HealthPercent < 30 && Me.CurrentTarget.IsTargetingMeOrPet, "Smoke Bomb"),
                               Spell.CastSelfSpell("Combat Readiness",        ret => Me.CurrentTarget != null && Me.HealthPercent < 40 && Me.CurrentTarget.IsTargetingMeOrPet, "Combat Readiness"),
                               Spell.CastSelfSpell("Evasion",                 ret => Me.HealthPercent < 35 && Unit.EnemyUnits.Count(u => u.DistanceSqr < 6 * 6 && u.IsTargetingMeOrPet) >= 1, "Evasion"),
                               Spell.CastSelfSpell("Cloak of Shadows",        ret => Unit.EnemyUnits.Count(u => u.IsTargetingMeOrPet && u.IsCasting) >= 1, "Cloak of Shadows"),
                               Poisons.CreateApplyPoisons()));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               // Stealth
                               Spell.CastSelfSpell("Stealth", ret => !Buff.PlayerHasBuff("Stealth") && CLUSettings.Instance.Rogue.EnableAlwaysStealth, "Stealth"),
                               Poisons.CreateApplyPoisons()));
            }
        }

        public override Composite Resting
        {
            get {
                return Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get {
                return this.SingleRotation;
            }
        }

        public override Composite PVERotation
        {
            get {
                return this.SingleRotation;
            }
        }
    }
}