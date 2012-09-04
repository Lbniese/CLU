using CLU.Helpers;
using TreeSharp;
using System.Drawing;
using CommonBehaviors.Actions;
using CLU.Lists;
using CLU.Settings;
using Action = TreeSharp.Action;
using CLU.Base;

namespace CLU.Classes.Mage
{

    class Fire : RotationBase
    {
        public override string Name
        {
            get {
                return "Fire Mage";
            }
        }

        public override string KeySpell
        {
            get {
                return "Pyroblast";
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
                       "==> Flame Orb & Mana Gem & Mirror Image & Combustion \n" +
                       "4. AoE with Blast Wave & Dragon's Breath & Fire Blast & Flamestrike\n" +
                       "5. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to jamjar0207\n" +
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
                           // Comment: Dont break Evocation!!
                           new Decorator(
                               x => Buff.PlayerHasBuff("Evocation"), new Action(a => CLU.DebugLog(Color.ForestGreen,"Evocation active"))),
                           Buff.CastBuff("Molten Armor",                      ret => !Buff.PlayerHasBuff("Mage Armor") && !Buff.PlayerHasBuff("Molten Armor"), "No Armor Buff - Molten Armor"),
                           Buff.CastBuff("Molten Armor",                      ret => Me.ManaPercent > 45 && Buff.PlayerHasBuff("Mage Armor"), "Molten Armor Now We Have Enough Mana Returned From Mage Armor"),
                           // Threat
                           Buff.CastBuff("Invisibility",                      ret => Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Buff.PlayerHasActiveBuff("Hypothermia"), "Invisibility"),
                           // Interupts & Steal Buffs
                           Spell.CastSpell("Spellsteal",                      ret => Spell.TargetHasStealableBuff() && !Me.IsMoving, "[Steal] Spellsteal"),
                           Spell.CastInterupt("Counterspell",                 ret => true, "Counterspell"),
                           // Cooldowns
                           Item.RunMacroText("/cast Conjure Mana Gem",        ret => !Item.HaveManaGem(), "[Create] Mana Gem"),
                           Item.UseBagItem("Mana Gem",                        ret => Me.CurrentTarget != null && Me.ManaPercent < 90 && Unit.IsTargetWorthy(Me.CurrentTarget), "Mana Gem"),
                           Spell.ChannelSelfSpell("Evocation",                ret => Me.ManaPercent < 10 && !Me.IsMoving, "Evocation For Mana"),
                           Spell.CastSpell("Scorch",                          ret => Me.CurrentTarget != null && !Buff.TargetHasDebuff("Critical Mass", Me.CurrentTarget) && !Buff.TargetHasDebuff("Shadow and Flame", Me.CurrentTarget), "Scorch"),
                           Buff.CastDebuff("Combustion",                      ret => Me.CurrentTarget != null && Buff.TargetHasDebuff("Living Bomb") && Buff.TargetHasDebuff("Ignite") && Buff.TargetHasDebuff("Pyroblast!") && Unit.IsTargetWorthy(Me.CurrentTarget), "Combustion"),
                           Spell.CastSelfSpell("Mirror Image",                ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Mirror Image"),
                           Spell.CastSpell("Flame Orb",                       ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Flame Orb"),
                           Spell.CastConicSpell("Dragon's Breath", 12f, 33f,  ret => true, "Dragon's Breath"),
                           // AoE
                           new Decorator(
                               ret => !Me.IsMoving && Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 2,
                               new PrioritySelector(
                                   Spell.CastAreaSpell("Blast Wave", 8, true, 3, 0.0, 0.0, ret => !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Blast Wave"),
                                   Spell.CastSpell("Fire Blast", ret => (Buff.TargetHasDebuff("Living Bomb") || Buff.TargetHasDebuff("Ignite")) && Buff.PlayerHasBuff("Impact"), "Fire Blast with Impact"),
                                   Spell.CastSpellAtLocation("Flamestrike", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Buff.TargetHasDebuff("Flamestrike") && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > 30 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 3, "Flamestrike")
                               )),
                           // Default Rotaion
                           Buff.CastDebuff("Living Bomb",                     ret => true, "Living Bomb"),
                           Spell.CastSpell("Pyroblast",                       ret => Buff.PlayerHasActiveBuff("Hot Streak"), "Hot Streak"),
                           Spell.CastSpell("Fireball",                        ret => true, "Fireball"),
                           Spell.CastSpell("Scorch",                          ret => Me.IsMoving, "Scorch while Moving"),
                           Buff.CastBuff("Mage Armor",                        ret => Me.ManaPercent < 5 && !Buff.PlayerHasBuff("Mage Armor"), "Mage Armor We Are Low On Mana")
                       );
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone",          ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Ice Block",              ret => Me.HealthPercent < 20 && !Buff.PlayerHasActiveBuff("Hypothermia"), "Ice Block"),
                               Buff.CastBuff("Mage Ward",              ret => Me.HealthPercent < 50, "Mage Ward")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               Buff.CastBuff("Molten Armor",                   ret => true, "Molten Armor"),
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
