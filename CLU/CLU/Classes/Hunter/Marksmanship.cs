using Clu.Helpers;
using Clu.Settings;
using CommonBehaviors.Actions;
using TreeSharp;

namespace Clu.Classes.Hunter
{
    class Marksmanship : RotationBase
    {

        public override string Name
        {
            get {
                return "Marksmanship Hunter";
            }
        }

        // public static readonly HealerBase Healer = HealerBase.Instance;

        public override string KeySpell
        {
            get {
                return "Aimed Shot";
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        // adding some help
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Trap Launcher or Feign Death will halt the rotation\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Rapid Fire & Call of the Wild\n" +
                       "3.AoE with Multishot and Explosive Trap\n" +
                       "4. Best Suited for T13 end game raiding\n" +
                       "Fox		-haste\n" +
                       "Cat		+str/agil\n" +
                       "Core Hound	(+haste &-cast speed) exotic \n" +
                       "Silithid		(+health) exotic\n" +
                       "Wolf		(+crit)\n" +
                       "Shale Spider	(+5% stats) exotic\n" +
                       "Raptor		(-armor)\n" +
                       "Carrion Bird	(-physical damage)\n" +
                       "Sporebat	(-cast speed)\n" +
                       "Ravager		(-Phys Armor)\n" +
                       "Wind Serpent	(-Spell Armor)\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           // Trinkets & Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && (Unit.IsTargetWorthy(Me.CurrentTarget) && !Buff.PlayerHasBuff("Feign Death")),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           // Main Rotation
                           new Decorator(
                               ret => !Buff.PlayerHasBuff("Feign Death"),
                               new PrioritySelector(
                                   // HandleMovement? Lets Misdirect to Focus, Pet, or Tank
                                   new Decorator(
                                       ret => CLUSettings.Instance.EnableMovement,
                                       new PrioritySelector(
                                           Spell.CastSpell("Misdirection", u => Unit.BestMisdirectTarget, ret => Me.CurrentTarget != null && !Buff.PlayerHasBuff("Misdirection") && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Misdirection")
                                       )),
                                   Buff.CastDebuff("Hunter's Mark",           ret => Me.CurrentTarget != null && !Buff.TargetHasDebuff("Hunter's Mark", Me.CurrentTarget) && (Me.CurrentTarget.CurrentHealth > 310000 || Me.CurrentTarget.MaxHealth == 1), "Hunter's Mark"),
                                   Spell.CastSelfSpell("Feign Death",         ret => Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Feign Death Threat"),
                                   Spell.CastSpell("Concussive Shot",         ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTargetGuid == Me.Guid, "Concussive Shot"),
                                   new Decorator(
                                       ret => Me.IsMoving,
                                       new Sequence(
                                           // Waiting for a bit just incase we are only moving outa the fire!
                                           // new ActionSleep(1500),
                                           Buff.CastBuff("Aspect of the Fox", ret => Me.IsMoving, "[Aspect] of the Fox - Moving"))),
                                   Buff.CastBuff("Aspect of the Hawk", ret => !Me.IsMoving, "[Aspect] of the Hawk"),
                                   PetManager.CastPetSpell("Call of the Wild",      ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < Spell.MeleeRange, "Call of the Wild"),
                                   Spell.CastSpell("Kill Shot",               ret => true, "Kill Shot"),
                                   Spell.HunterTrapBehavior("Explosive Trap", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Lists.BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) > 0),
                                   Spell.CastSpell("Multi-Shot",              ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 20) > 4, "Multi-Shot"),
                                   Spell.CastSpell("Steady Shot",             ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 20) > 4, "Steady Shot"),
                                   Spell.CastSpell("Raptor Strike",           ret => Me.CurrentTarget != null && !Lists.BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.CurrentTarget.IsWithinMeleeRange, "Raptor Strike (Melee)"),
                                   Buff.CastDebuff("Wing Clip",               ret => Me.CurrentTarget != null && !Lists.BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.CurrentTarget.IsWithinMeleeRange, "Wing Clip (Melee)"),
                                   // Main rotation
                                   Buff.CastDebuff("Serpent Sting",           ret => Me.CurrentTarget != null && Buff.TargetDebuffTimeLeft("Serpent Sting").TotalSeconds <= 0.5 && (Me.CurrentTarget.HealthPercent <= 90 || Me.CurrentTarget.MaxHealth == 1), "Serpent Sting"),
                                   Spell.CastSpell("Chimera Shot",            ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent <= 90 || Me.CurrentTarget.MaxHealth == 1), "Chimera Shot"),
                                   Buff.CastBuff("Rapid Fire",                ret => Me.CurrentTarget != null && !Buff.UnitHasHasteBuff(Me) && Unit.IsTargetWorthy(Me.CurrentTarget), "Rapid Fire"),
                                   Buff.CastBuff("Readiness",                 ret => Buff.PlayerHasActiveBuff("Rapid Fire"), "Readiness"),
                                   Spell.CastSpell("Steady Shot",             ret => Buff.PlayerActiveBuffTimeLeft("Improved Steady Shot").TotalSeconds < 3, "Steady Shot (Improved Steady Shot) [" + Buff.PlayerActiveBuffTimeLeft("Improved Steady Shot").TotalSeconds + "]"),
                                   Item.RunMacroText("/cast Aimed Shot!",     ret => Buff.PlayerHasActiveBuff("Fire!"), "Aimed Shot (Fire!)"),
                                   Spell.CastSpell("Arcane Shot",             ret => Me.CurrentTarget != null && (Me.FocusPercent >= 66 || Spell.SpellCooldown("Chimera Shot").TotalSeconds >= 4) && (Me.CurrentTarget.HealthPercent < 90 && !Buff.UnitHasHasteBuff(Me)), "Arcane Shot"),
                                   Spell.CastSpell("Aimed Shot",              ret => Me.CurrentTarget != null && (Me.FocusPercent >= 80 || Spell.SpellCooldown("Chimera Shot").TotalSeconds > 5) && (Buff.UnitHasHasteBuff(Me) || (Me.CurrentTarget.HealthPercent > 90)), "Aimed Shot (RF, Lust or Hero or TW"),
                                   Spell.CastSpell("Steady Shot",             ret => true, "Steady Shot"))));
            }
        }

        public override Composite Medic
        {
            get {
                return new PrioritySelector(
                           new Decorator(
                               ret => Me.HealthPercent < 100 && !Buff.PlayerHasBuff("Feign Death") && CLUSettings.Instance.EnableSelfHealing,
                               new PrioritySelector(
                                   Item.UseBagItem("Healthstone",             ret => Me.HealthPercent < 40, "Healthstone"),
                                   Spell.CastSelfSpell("Deterrence",          ret => Me.HealthPercent < 40 && Me.HealthPercent > 1, "Deterrence"))),
                           new Decorator(
                               ret => !Buff.PlayerHasBuff("Feign Death"),
                               new PrioritySelector(
                                   PetManager.CastPetSpell("Mend Pet",             ret => Me.GotAlivePet && (Me.Pet.HealthPercent < 70 || Me.Pet.HappinessPercent < 90) && !PetManager.PetHasBuff("Mend Pet"), "Mend Pet"),
                                   PetManager.CastPetSpell("Heart of the Phoenix", ret => !Me.GotAlivePet, "Heart of the Phoenix"))));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink") && !Buff.PlayerHasBuff("Feign Death") && !Buff.PlayerHasBuff("Trap Launcher"),
                           new PrioritySelector(
                               new Decorator(
                                   ret => !Me.GotAlivePet || Pet == null,
                                   new PrioritySelector(
                                       PetManager.CastPetSummonSpell("Call Pet 1", ret => Pet == null, "Calling Pet"),
                                       new WaitContinue(2, ret => Me.GotAlivePet || Me.Combat, new ActionAlwaysSucceed()),
                                       Spell.CastSelfSpell("Revive Pet", ret => !Me.GotAlivePet, "Revive Pet")))));
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