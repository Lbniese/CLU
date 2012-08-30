﻿using Clu.Helpers;
using TreeSharp;
using System.Drawing;
using CommonBehaviors.Actions;
using Clu.Lists;
using Clu.Settings;
using Action = TreeSharp.Action;

namespace Clu.Classes.Mage
{
    class Frost : RotationBase
    {
        public override string Name
        {
            get {
                return "Frost Mage";
            }
        }

        public override string KeySpell
        {
            get {
                return "Summon Water Elemental";
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
                       "Credits to Obliv\n" +
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
                           // Rotation based on SimCraft - Build 15211
                           Spell.CastInterupt("Counterspell",             ret => true, "Counterspell"),
                           Item.RunMacroText("/cast Conjure Mana Gem",    ret => !Item.HaveManaGem(), "Conjure Mana Gem"),
                           Spell.ChannelSelfSpell("Evocation",            ret => Me.ManaPercent < 40 && !Me.IsMoving && (Buff.PlayerHasActiveBuff("Icy Veins") || Buff.UnitHasHasteBuff(Me)), "Evocation"),
                           Item.UseBagItem("Mana Gem",                    ret => Me.CurrentTarget != null && Me.ManaPercent < 90 && Unit.IsTargetWorthy(Me.CurrentTarget), "Mana Gem"),
                           Spell.CastSelfSpell("Cold Snap",               ret => Spell.SpellCooldown("Deep Freeze").TotalSeconds > 15 && Spell.SpellCooldown("Flame Orb").TotalSeconds > 30 && Spell.SpellCooldown("Icy Veins").TotalSeconds > 30, "Cold Snap"),
                           Spell.CastSpell("Flame Orb",                   ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Frostfire Orb"),
                           Spell.CastSelfSpell("Mirror Image",            ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Mirror Image"),
                           Spell.CastSelfSpell("Icy Veins",               ret => !Buff.PlayerHasActiveBuff("Icy Veins") && !Buff.UnitHasHasteBuff(Me) && (Buff.PlayerCountBuff("Stolen Time") > 7 || Spell.SpellCooldown("Cold Snap").TotalSeconds < 22), "Icy Veins"),
                           Spell.CastSpell("Deep Freeze",                 ret => Buff.PlayerHasActiveBuff("Fingers of Frost"), "Deep Freeze (Fingers of Frost)"),
                           Spell.CastSpell("Frostfire Bolt",              ret => Buff.PlayerHasActiveBuff("Brain Freeze"), "Frostfire Bolt (Brain Freeze)"),
                           // AoE
                           new Decorator(
                               ret => Me.CurrentTarget != null && !Me.IsMoving && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 2,
                               new PrioritySelector(
                                   Spell.CastSpellAtLocation("Flamestrike", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Buff.TargetHasDebuff("Flamestrike") && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > 30 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 3, "Flamestrike"),
                                   Spell.ChannelAreaSpell("Blizzard", 11.0, true, 4, 0.0, 0.0, ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > 30 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 3, "Blizzard")
                               )),
                           PetManager.CastPetSpellAtLocation("Freeze", u => Me.CurrentTarget, ret => true, "Freeze (Pet)"),
                           Spell.CastSpell("Ice Lance",                   ret => Buff.PlayerCountBuff("Fingers of Frost") > 1, "Ice Lance"),
                           // PetSpellCooldown NOT returning correctly although it does return the correct value within the method, when you call it here it returns zero for some reason...still investigating
                           // K    ice_lance,if=buff.fingers_of_frost.react&pet.water_elemental.cooldown.freeze.remains<gcd
                           Spell.CastSpell("Ice Lance", ret => Buff.PlayerHasActiveBuff("Fingers of Frost") && (PetManager.PetSpellCooldown("Freeze").TotalSeconds < Spell.GCD), "Ice Lance (water_elemental_Freeze=" + PetManager.PetSpellCooldown("Freeze").TotalSeconds + " < " + Spell.GCD + ")"),
                           Spell.CastSpell("Frostbolt",                   ret => true, "Frostbolt (FreezeCD=" + PetManager.PetSpellCooldown("Freeze").TotalSeconds + ")"),
                           Spell.CastSpell("Ice Lance",                   ret => Me.IsMoving, "Ice Lance (Moving)"),
                           Spell.CastSpell("Fire Blast",                  ret => Me.IsMoving, "Fire Blast (Moving)")
                       );
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               PetManager.CastPetSummonSpell("Summon Water Elemental",  ret => !Me.GotAlivePet, "Water Elemental"),
                               Buff.CastBuff("Molten Armor",                      ret => true, "Molten Armor"),
                               Item.UseBagItem("Healthstone",                     ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Ice Block",                         ret => Me.HealthPercent < 20 && !Buff.PlayerHasActiveBuff("Hypothermia"), "Ice Block"),
                               Buff.CastBuff("Mage Ward",                         ret => Me.HealthPercent < 50, "Mage Ward")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               PetManager.CastPetSummonSpell("Summon Water Elemental", ret => !Me.GotAlivePet, "Water Elemental"),
                               Buff.CastBuff("Molten Armor",               ret => true, "Molten Armor"),
                               Buff.CastRaidBuff("Dalaran Brilliance",     ret => !Buff.PlayerHasBuff("Arcane Brilliance"), "Dalaran Brilliance"),
                               Buff.CastRaidBuff("Arcane Brilliance",      ret => !Buff.PlayerHasBuff("Dalaran Brilliance"), "Arcane Brilliance"), // as most people say, the main difference between Dalaran and Arcane Brilliance, is that Dalaran Brilliance has a totally different casting animation (looks way cooler) that allows you to stand out from most mages and has a 10 yard increase in range.
                               Item.RunMacroText("/cast Conjure Mana Gem", ret => !Me.IsMoving && !Item.HaveManaGem(), "Conjure Mana Gem")));
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