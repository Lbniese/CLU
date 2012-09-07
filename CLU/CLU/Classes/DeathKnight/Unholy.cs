using System.Linq;
using CLU.Helpers;
using CLU.Lists;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using CLU.Base;
using CLU.Managers;

namespace CLU.Classes.DeathKnight
{

    class Unholy : RotationBase
    {
        public override string Name
        {
            get {
                return "Unholy Deathknight";
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
                       "1. Heal using AMS, IBF, Healthstone and Deathstrike < 40%\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Unholy Frenzy & Summon Gargoyle & Death and Decay & Empower Rune Weapon \n" +
                       "3. Maintain HoW only if similar buff is not present\n" +
                       "4. Ensure we are in Unholy Presence\n" +
                       "5. Use Death and Decay and Pestilence and Blood boil for AoE\n" +
                       "6. Brez players (non-specific) using Raise Ally\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to Weischbier, ossirian, kbrebel04, Toney001 and cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override string KeySpell
        {
            get {
                return "Scourge Strike";
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
                                   Buff.CastBuff("Lifeblood",                          ret => true, "Lifeblood"),  // Thanks Kink
                                   Item.UseEngineerGloves(),
                                   Buff.CastBuffonUnit("Unholy Frenzy", u => Unit.BestUnholyFrenzyTarget, ret => Me.CurrentRunicPower >= 60 && !Buff.UnitHasHasteBuff(Unit.BestUnholyFrenzyTarget), "Unholy Frenzy"),
                                   // Buff.CastBuff("Unholy Frenzy",                       ret => Me.CurrentRunicPower >= 60 && !Buffs.UnitHasHasteBuff(Me), "Unholy Frenzy"),
                                   Buff.CastBuff("Summon Gargoyle",                    ret => Me.CurrentRunicPower >= 60 && Buff.UnitHasHasteBuff(Me), "Gargoyle"),
                                   Spell.UseRacials(),
                                   Spell.CastSelfSpell("Empower Rune Weapon", ret => CLUSettings.Instance.DeathKnight.UseEmpowerRuneWeapon && (Me.BloodRuneCount + Me.FrostRuneCount + Me.UnholyRuneCount + Me.DeathRuneCount == 0) && !Buff.UnitHasHasteBuff(Me), "Empower Rune Weapon"))),
                           // Interupts
                           Spell.CastInterupt("Mind Freeze",                  ret => true, "Mind Freeze"),
                           Spell.CastInterupt("Strangulate",                  ret => true, "Strangulate"),
                           Buff.CastBuff("Anti-Magic Shell", ret => Me.CurrentTarget != null && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.DeathKnight.UseAntiMagicShell && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.ChanneledCastingSpellId != 0), "AMS"), // TODO: Put this back in when its fixed. && Me.CurrentTarget.IsTargetingMeOrPet
                           Spell.CastSelfSpell("Blood Tap",                   ret => PetManager.PetCountBuff("Shadow Infusion") == 5 && (Me.BloodRuneCount + Me.UnholyRuneCount + Me.DeathRuneCount == 0), "Blood Tap for Dark Transformation"),
                           Spell.CastSelfSpell("Blood Tap",                   ret => Me.FrostRuneCount == 1 && (Me.UnholyRuneCount == 0 || Me.BloodRuneCount == 0), "Blood Tap"),
                           // Start Disease ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                           Common.ApplyDiseases(ret => Me.CurrentTarget),
                           // End Disease --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                           Spell.CastSpell("Dark Transformation",             ret => true, "Dark Transformation"),
                           // Start AoE ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                           Spell.CastAreaSpell("Pestilence", 10, false, 3, 0.0, 0.0, ret => Buff.TargetHasDebuff("Blood Plague") && Buff.TargetHasDebuff("Frost Fever") && (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Pestilence"),
                           Spell.CastAreaSpell("Blood Boil", 10, false, 3, 0.0, 0.0, ret => Buff.TargetHasDebuff("Blood Plague") && Buff.TargetHasDebuff("Frost Fever") && (from enemy in Unit.EnemyUnits where enemy.HasAura("Blood Plague") && enemy.HasAura("Frost Fever") select enemy).Count() > 2, "Blood Boil"),
                           // End AoE ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                           Spell.CastSpellAtLocation("Death and Decay", u => Me, ret => Me.CurrentTarget != null && BossList.OverrideDnD.Contains(Unit.CurrentTargetEntry) && (Me.UnholyRuneCount == 2) && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Me.CurrentRunicPower < 110 && (Me.CurrentTarget.CurrentHealth > 1000000 || Me.CurrentTarget.MaxHealth == 1) && Unit.CountEnnemiesInRange(Me.Location, 1000) >= 1, "Death and Decay"),
                           Spell.CastAreaSpell("Death and Decay", 10, true, 1, 0.0, 0.0, ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && (Me.UnholyRuneCount == 2) && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Me.CurrentRunicPower < 110 && (Me.CurrentTarget.CurrentHealth > 1000000 || Me.CurrentTarget.MaxHealth == 1), "Death and Decay"),
                           Spell.CastSpell("Death Coil",                      ret => Spell.SpellCooldown("Summon Gargoyle").TotalSeconds >= 1 && !Spell.CanCast("Summon Gargoyle", Me) && PetManager.PetCountBuff("Shadow Infusion") < 5 && !PetManager.PetHasActiveBuff("Dark Transformation"), "Death Coil (Shadow Infusion)"),
                           Spell.CastSpell("Scourge Strike",                  ret => Me.UnholyRuneCount == 2 && Me.CurrentRunicPower < 110, "Scourge Strike"),
                           Spell.CastSpell("Festering Strike",                ret => Me.BloodRuneCount == 2 && Me.FrostRuneCount == 2 && Me.CurrentRunicPower < 110, "Festering Strike"),
                           Spell.CastSpell("Death Coil",                      ret => (Me.CurrentRunicPower >= 94 && !PetManager.PetHasActiveBuff("Dark Transformation")) || (Me.CurrentRunicPower >= 94 && PetManager.PetBuffTimeLeft("Dark Transformation").TotalSeconds > 4), "Death Coil (RP Burn)"),
                           Spell.CastSpell("Death Coil",                      ret => Buff.PlayerHasActiveBuff("Sudden Doom"), "Death Coil (Sudden Doom)"),
                           Spell.CastSpellAtLocation("Death and Decay", u => Me, ret => Me.CurrentTarget != null && BossList.OverrideDnD.Contains(Unit.CurrentTargetEntry) && !Me.IsMoving && !Me.CurrentTarget.IsMoving && (Me.CurrentTarget.CurrentHealth > 310000 || Me.CurrentTarget.MaxHealth == 1) && Unit.CountEnnemiesInRange(Me.Location, 1000) >= 1, "Death and Decay"),
                           Spell.CastAreaSpell("Death and Decay", 10, true, 1, 0.0, 0.0, ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && !Me.IsMoving && !Me.CurrentTarget.IsMoving && (Me.CurrentTarget.CurrentHealth > 310000 || Me.CurrentTarget.MaxHealth == 1), "Death and Decay"),
                           Spell.CastSpell("Scourge Strike",                  ret => true, "Scourge Strike"),
                           Spell.CastSpell("Festering Strike",                ret => true, "Festering Strike"),
                           Spell.CastSpell("Death Coil",                      ret => PetManager.PetCountBuff("Shadow Infusion") < 5 && !PetManager.PetHasActiveBuff("Dark Transformation"), "Death Coil (Shadow Infusion)"),
                           // Start AoE ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                           Spell.CastAreaSpell("Icy Touch", 10, false, 3, 0.0, 0.0, ret => Me.FrostRuneCount > 0, "Icy Touch"),
                           // End AoE ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                           Spell.CastSpell("Horn of Winter",                  ret => (Me.CurrentRunicPower < 32 || !Buff.UnitHasStrAgiBuff(Me)), "Horn of Winter for RP"));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Buff.CastBuff("Conversion", ret => (Me.HasAura("Unholy Presence") && Me.HasAura("Anti-Magic Shell")/*Since conversion stops extra rune regen from Frost Presence but not from AMS we will go this way only for Unholy Dks*/), "Conversion (Restoring 3% HP every 1s for 10RP"),//Tricky One
                               Spell.CastSelfSpell("Raise Dead",                  ret => (Me.Pet == null || Me.Pet.Dead), "Raise Dead"),
                               Spell.CastSelfSpell("Icebound Fortitude",          ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.UnholyIceboundFortitudePercent && CLUSettings.Instance.DeathKnight.UseIceboundFortitude, "Icebound Fortitude "),
                               Item.UseBagItem("Healthstone",                     ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.HealthstonePercent, "Healthstone"),
                               Spell.CastSpell("Death Strike",                    ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikeEmergencyPercent, "Death Strike"),
                               Spell.CastSpell("Death Siphon", ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathSiphonPercent && CLUSettings.Instance.EnableMovement, "Death Siphon"), // Only if movement is enabled..i.e. we are questing, gathering, etc
                               // Conversion lasts until canceled!!..I am reluctant to put this in atm as it seems underpowered
                               // Buff.CastBuff("Conversion",                 ret => (Buff.PlayerHasBuff("Unholy Presence") && Buff.PlayerHasBuff("Anti-Magic Shell")/*Since conversion stops extra rune regen from Frost Presence but not from AMS we will go this way only for Unholy Dks*/), "Conversion (Restoring 3% HP every 1s for 10RP"),//Tricky One
                               Spell.CastSelfSpell("Death Pact",                  ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.UnholyPetSacrificePercent && Me.Minions.FirstOrDefault(q => q.CreatureType == WoWCreatureType.Undead || q.CreatureType == WoWCreatureType.Totem) != null && CLUSettings.Instance.DeathKnight.UnholyUsePetSacrifice    , "Death Pact")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return
                    new Decorator(
                        ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            Spell.CastSelfSpell("Raise Dead",              ret => (Me.Pet == null || Me.Pet.Dead), "Raise Dead"),
                            Buff.CastRaidBuff("Horn of Winter",            ret => CLUSettings.Instance.DeathKnight.UseHornofWinter && Me.CurrentTarget != null && !Me.CurrentTarget.IsFriendly, "Horn of Winter")));
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