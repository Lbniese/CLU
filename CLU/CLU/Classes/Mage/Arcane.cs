using System.Linq;
using CLU.Helpers;
using Styx.TreeSharp;
using System.Drawing;
using CommonBehaviors.Actions;
using CLU.Settings;
using Action = Styx.TreeSharp.Action;
using CLU.Base;

namespace CLU.Classes.Mage
{

    class Arcane : RotationBase
    {
        public override string Name
        {
            get {
                return "Arcane Mage";
            }
        }

        public override string KeySpell
        {
            get {
                return "Arcane Blast";
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Ice Block, Spellsteal\n" +
                       "2. Conjure Mana Gem\n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Flame Orb & Mana Gem & Mirror Image & Arcane Power \n" +
                       "4. AoE with Arcane Explosion & Frost Nova \n" +
                       "5. Best Suited for end game raiding\n" +
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

                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseBagItem("Volcanic Potion", ret => Buff.UnitHasHasteBuff(Me), "Volcanic Potion Heroism/Bloodlust"),
                                   Item.UseEngineerGloves())),
                           // Comment: Dont break Invinsibility!!
                           new Decorator(
                               x => Buff.PlayerHasBuff("Invisibility"), new Action(a => CLU.DebugLog(Color.ForestGreen,"Invisibility active"))),
                           // Interupts & Steal Buffs
                           Spell.CastSpell("Spellsteal",                  ret => Spell.TargetHasStealableBuff() && !Me.IsMoving, "[Steal] Spellsteal"),
                           Spell.CastInterupt("Counterspell",             ret => true, "Counterspell"),
                           // Cooldowns
                           Spell.ChannelSelfSpell("Evocation",            ret => Me.ManaPercent < 35 && !Me.IsMoving, "Evocation"),
                           Spell.CastSpell("Flame Orb",                   ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Flame Orb"),
                           Item.UseBagItem("Mana Gem",                    ret => Me.CurrentTarget != null && Me.ManaPercent < 90 && Unit.IsTargetWorthy(Me.CurrentTarget), "Mana Gem"),
                           Spell.CastSelfSpell("Mirror Image",            ret => Buff.PlayerHasBuff("Arcane Power") && Unit.IsTargetWorthy(Me.CurrentTarget), "Mirror Image"),
                           Spell.CastSelfSpell("Presence of Mind",        ret => !Buff.PlayerHasBuff("Invisibility"), "Presence of Mind"),
                           Spell.CastSelfSpell("Arcane Power",            ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Improved Mana Gem") || Unit.IsTargetWorthy(Me.CurrentTarget), "Arcane Power"),
                           Item.RunMacroText("/cast Conjure Mana Gem",    ret => Buff.PlayerHasBuff("Presence of Mind") && !Item.HaveManaGem(), "Conjure Mana Gem"),
                           // AoE
                           new Decorator(
                               ret => !Me.IsMoving && Unit.EnemyUnits.Count() > 2,
                               new PrioritySelector(
                                   Spell.CastSpell("Arcane Blast", ret => !Buff.PlayerHasBuff("Arcane Blast"), "Arcane Blast"),
                                   Spell.CastAreaSpell("Arcane Explosion", 10, false, 4, 0.0, 0.0, ret => true, "Arcane Explosion"),
                                   Spell.CastAreaSpell("Frost Nova", 10, false, 3, 0.0, 0.0, ret => true, "Frost Nova"))),
                           // Default Rotaion
                           Spell.CastSpell("Arcane Barrage",              ret => Spell.SpellCooldown("Evocation").TotalSeconds > 10 && Me.ManaPercent < 70 && !Buff.PlayerHasActiveBuff("Arcane Missiles!"), "Arcane Barrage"),
                           Spell.ChannelSpell("Arcane Missiles",          ret => Spell.SpellCooldown("Evocation").TotalSeconds > 10 && Me.ManaPercent < 80 && Buff.PlayerHasActiveBuff("Arcane Missiles!"), "Arcane Missiles"),
                           Spell.CastSpell("Arcane Blast",                ret => !Me.IsMoving, "Arcane Blast"),
                           Spell.CastSpell("Arcane Barrage",              ret => Me.IsMoving, "Arcane Barrage (Moving)"),
                           Spell.CastSpell("Fire Blast",                  ret => Me.IsMoving, "Fire Blast (Moving)"),
                           Spell.CastSpell("Ice Lance",                   ret => Me.IsMoving, "Ice Lance (Moving)"));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone",      ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Ice Block",          ret => Me.HealthPercent < 20 && !Buff.PlayerHasActiveBuff("Hypothermia"), "Ice Block"),
                               Buff.CastBuff("Mage Ward",          ret => Me.HealthPercent < 50, "Mage Ward")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               Buff.CastBuff("Mage Armor",                     ret => true, "Mage Armor"),
                               Buff.CastRaidBuff("Dalaran Brilliance",         ret => true, "Dalaran Brilliance"),
                               Buff.CastRaidBuff("Arcane Brilliance",          ret => true, "Arcane Brilliance"), 
                               Item.RunMacroText("/cast Conjure Mana Gem",     ret => !Me.IsMoving && !Item.HaveManaGem(), "Conjure Mana Gem")));
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
