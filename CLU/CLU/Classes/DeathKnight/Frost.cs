using System.Linq;
using Clu.Helpers;
using Clu.Lists;
using Clu.Settings;
using CommonBehaviors.Actions;
using Styx;
using TreeSharp;

namespace Clu.Classes.DeathKnight
{
    class Frost : RotationBase
    {
        public override string Name
        {
            get {
                return "MasterFrost Deathknight";
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
                       "Masterfrost: HB OBL Mastery. It is the best dps but hard.\n" +
                       "[*] Mastery > Haste\n" +
                       "[*] Unholy runes are gamed to force RE procs on blood/frost\n" +
                       "[*] HB is prioritised unless resources start to stack high\n" +
                       "[*] during high resources, OBL prioritises to keep runes used\n" +
                       "This Rotation will:\n" +
                       "1. Heal using AMS, IBF, Healthstone and Deathstrike < 40%\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Pillar of Frost & Raise Dead & Death and Decay & Empower Rune Weapon \n" +
                       "3. Maintain HoW only if similar buff is not present\n" +
                       "4. Ensure we are in Unholy Presence (Commented out)\n" +
                       "5. Brez players (non-specific) using Raise Ally\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to Weischbier, ossirian, imdasandman and cowdude\n" +
                       "----------------------------------------------------------------------\n";
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

                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           Buff.CastBuff("Pillar of Frost", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Pillar of Frost"),
                           // Interupts
                           Spell.CastInterupt("Mind Freeze",      ret => true, "Mind Freeze"),
                           Spell.CastInterupt("Strangulate",      ret => true, "Strangulate"),
                           Buff.CastBuff("Anti-Magic Shell",      ret => Me.CurrentTarget != null && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.DeathKnight.UseAntiMagicShell && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.ChanneledCastingSpellId != 0) && Me.CurrentTarget.IsTargetingMeOrPet, "AMS"),
                           Spell.CastSelfSpell("Blood Tap",       ret => Spell.RuneCooldown(1) > 2 && Spell.RuneCooldown(2) > 2, "Blood Tap"),
                           Buff.CastBuff("Raise Dead",            ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerHasBuff("Pillar of Frost") && Buff.PlayerBuffTimeLeft("Pillar of Frost") <= 10 && Buff.PlayerHasBuff("Unholy Strength"), "Raise Dead"),
                           Spell.CastSpell("Outbreak",            ret => Buff.TargetDebuffTimeLeft("Blood Plague").TotalSeconds < 0.5 || Buff.TargetDebuffTimeLeft("Frost Fever").TotalSeconds < 0.5, "Outbreak"),
                           Spell.CastSpell("Howling Blast",       ret => Buff.TargetDebuffTimeLeft("Frost Fever").TotalSeconds < 0.5, "Howling Blast (Frost Fever)"),
                           Spell.CastSpell("Plague Strike",       ret => Buff.TargetDebuffTimeLeft("Blood Plague").TotalSeconds < 0.5, "Plague Strike"),
                           // Start AoE------------------------------------------------------------------------------------------------
                           Spell.CastAreaSpell("Howling Blast", 10, false, 3, 0.0, 0.0, ret => (Me.FrostRuneCount >= 1 || Me.DeathRuneCount >= 1) && Unit.EnemyUnits.Count() >= 3, "Howling Blast"),
                           Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget), "Death and Decay"),
                           Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, ret => Me.CurrentTarget != null && (!BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && (Spell.RuneCooldown(4) == 0 && Spell.RuneCooldown(3) <= 1) || (Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) <= 1) && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget)), "Death and Decay"),
                           Spell.CastAreaSpell("Plague Strike", 10, false, 3, 0.0, 0.0, ret => Spell.SpellCooldown("Death and Decay").TotalSeconds > 6 && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3, "Plague Strike"),
                           // End AoE------------------------------------------------------------------------------------------------
                           Spell.CastSpell("Obliterate",          ret => Me.DeathRuneCount >= 1 && Me.FrostRuneCount >= 1 && Me.UnholyRuneCount >= 1 && Unit.EnemyUnits.Count() == 1, "Obliterate 1st"),
                           Spell.CastSpell("Frost Strike",        ret => Me.CurrentRunicPower >= 120, "Frost Strike (Runic Power 120)"),
                           Spell.CastSpell("Obliterate",          ret => (Me.DeathRuneCount == 2 && Me.FrostRuneCount == 2) || (Me.DeathRuneCount == 2 && Me.UnholyRuneCount == 2) || (Me.UnholyRuneCount == 2 && Me.FrostRuneCount == 2) && Unit.EnemyUnits.Count() == 1, "Obliterate 2nd"),
                           Spell.CastSpell("Frost Strike",        ret => Me.CurrentRunicPower >= 110, "Frost Strike (Runic Power 110)"),
                           Spell.CastSpell("Howling Blast",       ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast (Rime)"),
                           Spell.CastSpell("Frost Strike",        ret => Me.CurrentRunicPower >= 100, "Frost Strike (Runic Power 100)"),
                           Spell.CastSpell("Obliterate",          ret => Buff.PlayerHasBuff("Killing Machine"), "Obliterate (Killing Machine)"),
                           Spell.CastSpell("Obliterate",          ret => Me.UnholyRuneCount == 2, "Obliterate (2x Unholy Runes)"),
                           Spell.CastSpell("Obliterate",          ret => (Spell.RuneCooldown(4) == 0 && Spell.RuneCooldown(3) <= 1) || (Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) <= 1), "Obliterate (Unholy Rune less than 1 second)"),
                           Spell.CastSpell("Obliterate",          ret => (Spell.RuneCooldown(4) == 0 && Spell.RuneCooldown(3) < 4) || (Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) < 4) && (Me.FrostRuneCount + Me.DeathRuneCount == 1), "Obliterate (Unholy Rune less than 4 seconds)"),
                           // Start AoE------------------------------------------------------------------------------------------------
                           Spell.CastAreaSpell("Frost Strike", 10, false, 3, 0.0, 0.0, ret => Buff.PlayerHasBuff("Killing Machine") && Unit.EnemyUnits.Count() >= 3, "Frost Strike"),
                           // End AoE------------------------------------------------------------------------------------------------
                           Spell.CastSpell("Howling Blast",       ret => true, "Howling Blast"),
                           Spell.CastSpell("Howling Blast",       ret => Me.CurrentRunicPower < 60 && !Buff.UnitHasHasteBuff(Me), "Howling Blast (under 80 Runic Power)"),
                           // Start More AoE------------------------------------------------------------------------------------------------
                           Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.UnholyRuneCount == 1 && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget), "Death and Decay"),
                           Spell.CastAreaSpell("Plague Strike", 10, false, 3, 0.0, 0.0, ret => Spell.SpellCooldown("Death and Decay").TotalSeconds > 6 && Me.UnholyRuneCount == 1 && Unit.EnemyUnits.Count() >= 3, "Plague Strike"),
                           // End More AoE------------------------------------------------------------------------------------------------
                           Spell.CastSpell("Obliterate",          ret => Me.CurrentRunicPower >= 60 && Buff.UnitHasHasteBuff(Me), "Obliterate (over 80 Runic Power)"),
                           Spell.CastSpell("Frost Strike",        ret => true, "Frost Strike"),
                           Spell.CastSelfSpell("Empower Rune Weapon", ret => Me.CurrentTarget != null && CLUSettings.Instance.DeathKnight.UseEmpowerRuneWeapon && Unit.IsTargetWorthy(Me.CurrentTarget) && (Spell.RuneCooldown(1) + Spell.RuneCooldown(2) + Spell.RuneCooldown(3) + Spell.RuneCooldown(4) + Spell.RuneCooldown(5) + Spell.RuneCooldown(6)) > 8 && !Buff.UnitHasHasteBuff(Me), "Empower Rune Weapon"),
                           Spell.CastSpell("Horn of Winter",      ret => (Me.CurrentRunicPower < 32 || !Buff.UnitHasStrAgiBuff(Me)), "Horn of Winter for RP"));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSelfSpell("Death Pact",              ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostPetSacrificePercent && Me.Minions.FirstOrDefault(q => q.CreatureType == WoWCreatureType.Undead || q.CreatureType == WoWCreatureType.Totem) != null && CLUSettings.Instance.DeathKnight.FrostUsePetSacrifice, "Death Pact"),
                               Spell.CastSelfSpell("Raise Dead",              ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostPetSacrificePercent && !Buff.PlayerHasBuff("Icebound Fortitude") && CLUSettings.Instance.DeathKnight.FrostUsePetSacrifice, "Raise Dead"),
                               Spell.CastSelfSpell("Icebound Fortitude",      ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostIceboundFortitudePercent && CLUSettings.Instance.DeathKnight.UseIceboundFortitude, "Icebound Fortitude "),
                               Spell.CastSpell("Death Strike",                ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikeEmergencyPercent, "Death Strike"),
                               Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.HealthstonePercent, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return
                    new Decorator(
                        ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            Buff.CastBuff("Horn of Winter", ret => !Buff.UnitHasStrAgiBuff(Me) && CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter")));
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