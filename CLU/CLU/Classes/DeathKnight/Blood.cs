using System.Linq;
using Clu.Helpers;
using Clu.Lists;
using Clu.Settings;
using CommonBehaviors.Actions;
using Styx;
using TreeSharp;

namespace Clu.Classes.Deathknight
{

    class Blood : RotationBase
    {
        // public static readonly HealerBase Healer = HealerBase.Instance;

        public override string Name
        {
            get {
                return "Blood Deathknight";
            }
        }

        public override string KeySpell
        {
            get {
                return "Heart Strike";
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
                       "1. Heal using AMS, DeathPact, Lichbourne Heal, IBF, Healthstone, RuneTap, and VB\n" +
                       "2. Intelligently Heal with Deathstrikes\n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Empower Rune Weapon \n" +
                       "4. Maintain HoW only if similar buff is not present\n" +
                       "5. Maintain Bone Shield\n" +
                       "6. Use Death and Decay and Pestilence for AoE\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to Weischbier, ossirian, gniegsch and cowdude and Singular\n" +
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
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Item.UseEngineerGloves(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"))),
                           // Encounter Specific (Uncomment the two lines below for HC Deathwing) //thanks to gniegsch fo his input!
                           // Spell.CastSelfSpell("Bone Shield", ret => Me.CurrentTarget != null && Me.CurrentTarget.ChanneledCastingSpellId == 109632 && Me.CurrentTarget.IsTargetingMeOrPet, "Bone Shield for Impale"),
                           // Spell.CastSpell("Death Strike", ret =>Me.CurrentTarget != null && Me.CurrentTarget.ChanneledCastingSpellId == 109632 && Me.CurrentTarget.IsTargetingMeOrPet, "DS for shield"),
                           // Interupts
                           Spell.CastInterupt("Mind Freeze",              ret => true, "Mind Freeze"),
                           Spell.CastInterupt("Strangulate",              ret => true, "Strangulate"),
                           Buff.CastBuff("Anti-Magic Shell",              ret => Me.CurrentTarget != null && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.DeathKnight.UseAntiMagicShell && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.ChanneledCastingSpellId != 0) && Me.CurrentTarget.IsTargetingMeOrPet, "AMS"),
                           Spell.CastSelfSpell("Bone Shield",             ret => Spell.SpellCooldown("Death Strike").TotalSeconds > 3 && CLUSettings.Instance.DeathKnight.UseBoneShieldonCooldown, "Bone Shield"),
                           // START AoE + Disease spread
                           Spell.CastAreaSpell("Pestilence", 10, false, CLUSettings.Instance.DeathKnight.BloodPestilenceCount, 0.0, 0.0, ret => Buff.TargetHasDebuff("Blood Plague") && Buff.TargetHasDebuff("Frost Fever") && (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Pestilence"),
                           Spell.CastAreaSpell("Death and Decay", 10, true, CLUSettings.Instance.DeathKnight.BloodDeathAndDecayCount, 0.0, 0.0, ret => !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Death and Decay"),
                           // START Main Rotation
                           Buff.CastDebuff("Outbreak",                    ret => !Buff.TargetHasDebuff("Frost Fever") || !Buff.TargetHasDebuff("Blood Plague"), "Outbreak for diseases"),
                           Spell.CastSpell("Death Strike", 				  ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikePercent || (Me.UnholyRuneCount + Me.FrostRuneCount + Me.DeathRuneCount >= 4) || (Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikeBloodShieldPercent && (Buff.PlayerBuffTimeLeft("Blood Shield") < CLUSettings.Instance.DeathKnight.DeathStrikeBloodShieldTimeRemaining)), "Death Strike"), // !Buff.PlayerHasBuff("Blood Shield")
                           Spell.CastSelfSpell("Blood Tap",               ret => Me.HealthPercent <= CLUSettings.Instance.DeathKnight.RuneTapPercent && Me.BloodRuneCount >= 1 && (Me.UnholyRuneCount == 0 || Me.FrostRuneCount == 0) && CLUSettings.Instance.DeathKnight.UseBloodTapforRuneTap, "Blood Tap"),
                           Spell.CastSelfSpell("Rune Tap",                ret => Me.HealthPercent <= CLUSettings.Instance.DeathKnight.RuneTapPercent && Me.BloodRuneCount >= 1 && CLUSettings.Instance.DeathKnight.UseRuneTap, "Rune Tap"),
                           Buff.CastDebuff("Plague Strike",               ret => Me.CurrentTarget != null && Spell.SpellCooldown("Outbreak").TotalSeconds > 3 && Me.HealthPercent > 50 && !Buff.UnitHasDamageReductionDebuff(Me.CurrentTarget), "Plague Strike for Scarlet Fevor"),
                           Buff.CastDebuff("Icy Touch",                   ret => Me.CurrentTarget != null && Spell.SpellCooldown("Outbreak").TotalSeconds > 3 && Me.HealthPercent > 50 && !Buff.UnitHasAttackSpeedDebuff(Me.CurrentTarget), "Icy Touch for Frost Fever"),
                           Spell.CastAreaSpell("Blood Boil", 10, false, CLUSettings.Instance.DeathKnight.BloodBloodBoilCount, 0.0, 0.0, ret => Me.BloodRuneCount >= 1, "Blood Boil"),
                           Spell.CastSpell("Heart Strike",                ret => (Me.BloodRuneCount > 0 || Me.DeathRuneCount > 2), "Heart Strike"),
                           Spell.CastSpell("Rune Strike",                 ret => (Me.CurrentRunicPower >= CLUSettings.Instance.DeathKnight.RuneStrikePercent || Me.HealthPercent > 90) && (Me.UnholyRuneCount == 0 || Me.FrostRuneCount == 0) && !Buff.PlayerHasBuff("Lichborne"), "Rune Strike"),
                           Spell.CastSpell("Death Coil",                  ret => Me.CurrentTarget != null && !Spell.CanCast("Rune Strike", Me.CurrentTarget) && Me.CurrentRunicPower >= 90, "Death Coil"),
                           Spell.CastSpell("Horn of Winter",              ret => (Me.CurrentRunicPower < 34 || !Buff.UnitHasStrAgiBuff(Me)), "Horn of Winter for RP"));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSelfSpell("Death Pact",              ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.BloodPetSacrificePercent && Me.Minions.FirstOrDefault(q => q.CreatureType == WoWCreatureType.Undead || q.CreatureType == WoWCreatureType.Totem) != null && CLUSettings.Instance.DeathKnight.BloodUsePetSacrifice, "Death Pact"),
                               Spell.CastSelfSpell("Rune Tap",                ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.RuneTapWoTNPercent && Buff.PlayerHasBuff("Will of the Necropolis") && CLUSettings.Instance.DeathKnight.UseRuneTapWoTN, "Rune Tap & WotN "),
                               Spell.HealMe("Death Coil",                     ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathCoilHealPercent && Buff.PlayerHasBuff("Lichborne"), "Death Coil Heal"),
                               Spell.CastSelfSpell("Bone Shield",             ret => CLUSettings.Instance.DeathKnight.UseBoneShieldDefensively && Spell.SpellCooldown("Death Strike").TotalSeconds > 3 && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Dancing Rune Weapon") && !Buff.PlayerHasBuff("Lichborne"), "Bone Shield"),
                               Spell.CastSelfSpell("Vampiric Blood",          ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.VampiricBloodPercent && !Buff.PlayerHasBuff("Dancing Rune Weapon") && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Lichborne") && CLUSettings.Instance.DeathKnight.UseVampiricBlood, "Vampiric Blood"),
                               Spell.CastSpell("Dancing Rune Weapon",         ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DancingRuneWeaponPercent && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Lichborne") && CLUSettings.Instance.DeathKnight.UseDancingRuneWeapon, "Dancing Rune Weapon"),
                               Spell.CastSelfSpell("Lichborne",               ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.LichbornePercent && Me.CurrentRunicPower >= 60 && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Dancing Rune Weapon") && CLUSettings.Instance.DeathKnight.UseLichborne, "Lichborne"),
                               Spell.CastSelfSpell("Raise Dead",              ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.BloodPetSacrificePercent && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Dancing Rune Weapon") && !Buff.PlayerHasBuff("Lichborne") && CLUSettings.Instance.DeathKnight.BloodUsePetSacrifice, "Raise Dead"),
                               Spell.CastSelfSpell("Icebound Fortitude",      ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.BloodIceboundFortitudePercent && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Dancing Rune Weapon") && !Buff.PlayerHasBuff("Lichborne") && CLUSettings.Instance.DeathKnight.UseIceboundFortitude, "Icebound Fortitude "),
                               Spell.CastSelfSpell("Empower Rune Weapon",     ret => Me.CurrentTarget != null && CLUSettings.Instance.DeathKnight.UseEmpowerRuneWeapon && Unit.IsTargetWorthy(Me.CurrentTarget) && (Spell.RuneCooldown(1) + Spell.RuneCooldown(2) + Spell.RuneCooldown(3) + Spell.RuneCooldown(4) + Spell.RuneCooldown(5) + Spell.RuneCooldown(6)) > 8 && !Buff.UnitHasHasteBuff(Me), "Empower Rune Weapon"),
                               Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.HealthstonePercent, "HealthstonePercent")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new PrioritySelector(
                           new Decorator(
                               ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                               new PrioritySelector(
                                   Buff.CastBuff("Bone Shield",        ret => (Buff.PlayerCountBuff("Bone Shield") < 2 || !Buff.PlayerHasBuff("Bone Shield")) && (CLUSettings.Instance.DeathKnight.UseBoneShieldDefensively || CLUSettings.Instance.DeathKnight.UseBoneShieldonCooldown), "Bone Shield"),
                                   Buff.CastBuff("Horn of Winter",     ret => !Buff.UnitHasStrAgiBuff(Me) && CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter"))));
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