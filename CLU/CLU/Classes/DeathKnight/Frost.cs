using System.Linq;
using CLU.Helpers;
using CLU.Lists;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using CLU.Base;

namespace CLU.Classes.DeathKnight
{
    class Frost : RotationBase
    {
        public override string Name
        {
            get {
                return "Frost Deathknight";
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
                return
                    @"
----------------------------------------------------------------------
Frost:
[*] Handles Killing Machine differently; Dual Wield (Frost Strike); 2Handed (Obliterate)
[*] Unholy runes are gamed to force RE procs or Blood Tap or Plague Leech on blood/frost
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Setting)
[*] Death Siphon (only if movement enabled.)
This Rotation will:
1. Heal using AMS, IBF, Healthstone, Deathstrike
2. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Pillar of Frost & Raise Dead & Death and Decay & Empower Rune Weapon 
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
Credits to Weischbier, because he owns the buisness and I want him to have my babys! -- Sincerely Wulf (Bahahaha :P)
----------------------------------------------------------------------";
            }
        }

        public override string KeySpell
        {
            get {
                return "Frost Strike";
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
                           // Items
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           
                           //Interrupts
                           Spell.CastInterupt("Mind Freeze", 		ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Mind Freeze"), //Why does nobody check for the range of melee kicks? // CurrentTarget Null check, we are accessing the objects property ;) --wulf
                           Spell.CastInterupt("Strangulate", 		ret => true, "Strangulate"),
                           Spell.CastInterupt("Asphyxiate", 		ret => true, "Asphyxiate"),// Replaces Strangulate -- Darth Vader like ability
                           //Diseases
                           Common.ApplyDiseases(ret => Me.CurrentTarget),
                           //Cooldowns
                           new Decorator(ret => Unit.IsTargetWorthy(Me.CurrentTarget) && Me.IsWithinMeleeRange,//Check for the damn range, we don't want to pop anything when the destination is shit away
                                         new PrioritySelector(
                                             Buff.CastBuff("Raise Dead", 				ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Pillar of Frost") && Buff.PlayerBuffTimeLeft("Pillar of Frost") <= 10 && Buff.PlayerHasBuff("Unholy Strength"), "Raise Dead"),
                                             Buff.CastBuff("Pillar of Frost", 			ret => Me.CurrentTarget != null, "Pillar of Frost"),
                                             Spell.CastSelfSpell("Empower Rune Weapon", ret => Me.CurrentTarget != null && CLUSettings.Instance.DeathKnight.UseEmpowerRuneWeapon && Common.RuneCalculus > 8 && !Buff.UnitHasHasteBuff(Me), "Empower Rune Weapon")
                                         )
                                        ),
                           //Aoe
                           Spell.CastAreaSpell("Howling Blast", 10, false, 3, 0.0, 0.0, 	ret => (Me.FrostRuneCount >= 1 || Me.DeathRuneCount >= 1) && Unit.EnemyUnits.Count() >= 3, "Howling Blast"),
                           Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, 	ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget), "Death and Decay"),
                           Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, 	ret => Me.CurrentTarget != null && (!BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && (Spell.RuneCooldown(4) == 0 && Spell.RuneCooldown(3) <= 1) || (Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) <= 1) && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget)), "Death and Decay"),
                           Common.SpreadDiseasesBehavior(ret => Me.CurrentTarget), // Used to spread your Diseases based upon your Tier one Talent. -- wulf
                           Spell.CastAreaSpell("Plague Strike", 10, false, 3, 0.0, 0.0, 	ret => Spell.SpellCooldown("Death and Decay").TotalSeconds > 6 && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3, "Plague Strike"),
                           //Operation: Do Damage[Eyes only]
                           Spell.CastSpell("Soul Reaper", ret => Me.CurrentTarget, 		ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 35, "Soul Reaping"),
                           Spell.CastSpell("Frost Strike", ret => Me.CurrentTarget,     ret => !Common.IsWieldingTwoHandedWeapon() && Buff.PlayerHasBuff("Killing Machine"), "Frost Strike (Dual Wield Killing Machine)"),
                           Spell.CastSpell("Obliterate", ret => Me.CurrentTarget,       ret => Common.IsWieldingTwoHandedWeapon() && Buff.PlayerHasBuff("Killing Machine"), "Frost Strike (2 Hand Killing Machine)"),
                           Spell.CastSpell("Frost Strike", ret => Me.CurrentTarget, 	ret => Me.RunicPowerPercent >= 90, "Frost Strike (Dumping Runic Power)"),
                           //Utility Talents like: Plague Leech; Blood Tap;
                           
                           Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, 		ret => Buff.PlayerCountBuff("Blood Tap") >= 11 && (Spell.RuneCooldown(1) > 1 && Spell.RuneCooldown(2) > 1 && Spell.RuneCooldown(5) > 1 && Spell.RuneCooldown(6) > 1 && (Spell.RuneCooldown(3) > 1 && Spell.RuneCooldown(4) == 0 || Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) > 1)), "Blood Tap (Refreshed a depleted Rune)"), //Don't waste it on Unholy Runes
                           //Do Damage continue1
                           Spell.CastSpell("Howling Blast", 					ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast (Rime)"),
                           //Utility Talent: Blood Tap;
                           Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret => Buff.PlayerCountBuff("Blood Tap") < 11 && (Spell.RuneCooldown(1) > 1 && Spell.RuneCooldown(2) > 1 && Spell.RuneCooldown(5) > 1 && Spell.RuneCooldown(6) > 1 && (Spell.RuneCooldown(3) > 1 && Spell.RuneCooldown(4) == 0 || Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) > 1)), "Blood Tap (Refreshed a depleted Rune)"),  //Don't waste it on Unholy Runes
                           //Do Damage continue2
                           Spell.CastSpell("Obliterate", 						ret => true, "Obliterate (Because we can)"),
                           Spell.CastSpell("Howling Blast", 					ret => true, "Howling Blast (Because we can)"),
                           Buff.CastBuff("Horn of Winter",						ret => true,"Horn of Winter (Because we can)")
                       );
            }
        }

        public override Composite Medic
        {
            get {
                return
                    new PrioritySelector(
                        Buff.CastBuff("Anti-Magic Shell",ret => Me.CurrentTarget != null && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.DeathKnight.UseAntiMagicShell && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.ChanneledCastingSpellId != 0) && Me.CurrentTarget.IsTargetingMeOrPet, "AMS"),
                        new Decorator(
                            ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                            new PrioritySelector(
                                Spell.CastSelfSpell("Death Pact",			ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostPetSacrificePercent && Me.Minions.FirstOrDefault(q => q.CreatureType == WoWCreatureType.Undead || q.CreatureType == WoWCreatureType.Totem) != null && CLUSettings.Instance.DeathKnight.FrostUsePetSacrifice, "Death Pact"),
                                Spell.CastSelfSpell("Raise Dead",			ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostPetSacrificePercent && !Buff.PlayerHasBuff("Icebound Fortitude") && CLUSettings.Instance.DeathKnight.FrostUsePetSacrifice, "Raise Dead"),
                                Spell.CastSelfSpell("Icebound Fortitude",	ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostIceboundFortitudePercent && CLUSettings.Instance.DeathKnight.UseIceboundFortitude, "Icebound Fortitude "),
                                Spell.CastSpell("Death Strike",				ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikeEmergencyPercent, "Death Strike"),
                                Spell.CastSpell("Death Siphon",             ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathSiphonPercent && CLUSettings.Instance.EnableMovement, "Death Siphon"), // Only if movement is enabled..i.e. we are questing, gathering, etc...
                                // Conversion lasts until canceled!!..I am reluctant to put this in atm as its so underpowered
                                // Buff.CastBuff("Conversion",                 ret => (Buff.PlayerHasBuff("Unholy Presence") && Buff.PlayerHasBuff("Anti-Magic Shell")/*Since conversion stops extra rune regen from Frost Presence but not from AMS we will go this way only for Unholy Dks*/), "Conversion (Restoring 3% HP every 1s for 10RP"),//Tricky One
                                Item.UseBagItem("Healthstone",				ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.HealthstonePercent, "Healthstone"))
                        )
                    );
            }
        }

        public override Composite PreCombat
        {
            get {
                return
                    new Decorator(
                        ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            Buff.CastRaidBuff("Horn of Winter", ret => CLUSettings.Instance.DeathKnight.UseHornofWinter && Me.CurrentTarget != null && !Me.CurrentTarget.IsFriendly, "Horn of Winter")));
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