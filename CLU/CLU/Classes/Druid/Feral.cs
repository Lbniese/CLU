using Clu.Helpers;
using Clu.Lists;
using Clu.Settings;
using CommonBehaviors.Actions;
using TreeSharp;

namespace Clu.Classes.Druid
{
    class Feral : RotationBase
    {
        private const int ItemSetId = 1058; // Tier set ID

        public override string Name
        {
            get {
                return "Feral Druid";
            }
        }

        public override float CombatMaxDistance
        {
            get {
                return 3.2f;
            }
        }

        public override string KeySpell
        {
            get {
                return "Mangle";
            }
        }

        public override string Help
        {
            get {

                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       twopceinfo + "\n" +
                       fourpceinfo + "\n" +
                       "This Rotation will:\n" +
                       "1. Heal using Frenzied Regeneration, Survival Instincts, Barkskin\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Enrage & Berserk & Berserking & Lifeblood\n" +
                       "3. Checks for T13 2pc Kitty Bonus\n" +
                       "4. Ignores Shred for Ultraxion\n" +
                       "5. Hot swap rotation from bear to cat if you change form\n" +
                       "6. This Rotation will not get you a beer\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to Singular, mrwowbuddy and cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static bool IsBehind
        {
            get {
                return Me.CurrentTarget != null && Me.CurrentTarget.MeIsBehind;
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

                           // HandleMovement? If so, Choose our form!
                           new Decorator(
                               ret => CLUSettings.Instance.EnableMovement,
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Bear Form", 		ret => Me.HealthPercent <= 20 && !Buff.PlayerHasBuff("Bear Form"), "Bear Form"),
                                   Spell.CastSelfSpell("Cat Form", 			ret => Me.HealthPercent > 20 && !Buff.PlayerHasBuff("Cat Form"), "Cat Form"))),
                           new Decorator(
                               ret => Buff.PlayerHasBuff("Bear Form"),
                               new PrioritySelector(
                                   // Cooldownss
                                   new Decorator(
                                       ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                       new PrioritySelector(
                                           Item.UseTrinkets(),
                                           Spell.UseRacials(),
                                           Spell.CastSelfSpell("Enrage", 	ret => !Spell.CanCast("Berserk", Me) && Me.CurrentRage < 80, "Enrage"),
                                           Spell.CastSelfSpell("Berserk", 	ret => Buff.PlayerHasBuff("Pulverize") && Buff.TargetCountDebuff("Lacerate") >= 1, "Berserk"),
                                           Item.UseEngineerGloves(),
                                           Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"))),
                                   // Interupts
                                   Spell.CastInterupt("Skull Bash",           ret => true, "Skull Bash"),
                                   // 1.5 Keep up at least 1 stack of Lacerate up during beserk
                                   Buff.CastDebuff("Lacerate",                ret => (Buff.TargetDebuffTimeLeft("Lacerate").TotalSeconds < 2) && Buff.PlayerHasBuff("Berserk"), "Lacerate (Beserk)"),
                                   // 2. Mangle on cooldown
                                   Spell.CastSpell("Mangle (Bear)",           ret => true, "Mangle"),
                                   // 3.1 AoE Swipe
                                   Spell.CastAreaSpell("Swipe (Bear)", 8, false, 3, 0.0, 0.0, ret => true, "Swipe"),
                                   // 1. Keep Demoralizing Roar up
                                   Buff.CastDebuff("Demoralizing Roar",       ret => Me.CurrentTarget != null && !Buff.UnitHasDamageReductionDebuff(Me.CurrentTarget), "Demoralizing Roar"),
                                   // 3. Thrash
                                   Spell.CastSpell("Thrash",                  ret => true, "Thrash"),
                                   // 3.2 Maul & Clearcasting AoE
                                   Spell.CastAreaSpell("Maul", 5, false, 3, 0.0, 0.0, ret => Buff.PlayerHasBuff("Clearcasting"), "Maul"),
                                   // 4. Keep up at least 1 stack of Lacerate
                                   Buff.CastDebuff("Lacerate",                ret => Me.CurrentTarget != null && (Buff.TargetDebuffTimeLeft("Lacerate").TotalSeconds < 2) && !Spell.CanCast("Pulverize", Me.CurrentTarget), "Lacerate"),
                                   // 5. Keep up the Pulverize buff
                                   Spell.CastSpell("Pulverize",               ret => Buff.PlayerHasBuff("Clearcasting") && !Buff.PlayerHasBuff("Pulverize") && (Buff.TargetCountDebuff("Lacerate") >= 1), "Pulverize (Clear Casting)"),
                                   Spell.CastSpell("Pulverize",               ret => !Buff.PlayerHasBuff("Pulverize") && (Buff.TargetCountDebuff("Lacerate") > 2), "Pulverize"),
                                   // 6. Faerie Fire
                                   Spell.CastSpell("Faerie Fire (Feral)",     ret => Me.CurrentTarget != null && Buff.TargetCountDebuff("Faerie Fire") < 3 && !Buff.UnitHasArmorReductionDebuff(Me.CurrentTarget), "Faerie Fire (Feral)"),
                                   // 7. Keep up a 3 stack of Lacerate
                                   Buff.CastDebuff("Lacerate",                ret => Buff.TargetCountDebuff("Lacerate") < 3, "Lacerate (< 3)"),
                                   // 8. Spend excess rage on Maul
                                   Spell.CastSpell("Maul",                    ret => Me.RagePercent > 70, "Maul"),
                                   Spell.CastSpell("Lacerate",                ret => true, "Lacerate"))),
                           //--------------------------------------------------------------------------------------------------------------------------------------------------------------- cat / bear
                           new Decorator(
                               ret => Buff.PlayerHasBuff("Cat Form"),
                               new PrioritySelector(
                                   // Cooldowns
                                   new Decorator(
                                       ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                       new PrioritySelector(
                                           Item.UseTrinkets(),
                                           Spell.UseRacials(),
                                           Item.UseEngineerGloves(),
                                           Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"))),  // Thanks Kink
                                   // Interupts
                                   Spell.CastInterupt("Skull Bash",               ret => true, "Skull Bash"),
                                   // 1. Tiger's Fury
                                   Spell.CastSelfSpell("Tiger's Fury",            ret => Me.CurrentEnergy <= (Item.Has2PcTeirBonus(ItemSetId) ? 45 : 35) && !Buff.PlayerHasBuff("Clearcasting"), "Tigers Fury"),
                                   // Kitty Cooldown
                                   Spell.CastSelfSpell("Berserk",                 ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && (Buff.PlayerHasBuff("Tiger's Fury") || Buff.TargetDebuffTimeLeft("Tiger's Fury").TotalSeconds > 6), "Berserk"),
                                   Spell.CastSelfSpell("Berserking",              ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerHasBuff("Tiger's Fury"), "Berserking"),
                                   // 2. AoE Swipe
                                   Spell.CastAreaSpell("Swipe (Cat)", 8, false, 3, 0.0, 0.0, ret => true, "Swipe"),
                                   // 2. Mangle
                                   Spell.CastSpell("Mangle (Cat)",                ret => Me.CurrentTarget != null && !Buff.UnitHasBleedDamageDebuff(Me.CurrentTarget), "Mangle"),
                                   Item.RunMacroText("/cast Ravage",              ret => Buff.PlayerHasBuff("Stampede") && Buff.TargetDebuffTimeLeft("Stampede").TotalSeconds <= 1, "Ravage"),
                                   Spell.CastSpell("Ferocious Bite",              ret => Me.CurrentTarget != null && Me.ComboPoints >= 1 && Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 2.1 && ((Me.CurrentTarget.HealthPercent <= (Item.Has2PcTeirBonus(ItemSetId) ? 60 : 25)) || Me.CurrentTarget.MaxHealth == 1), "Ferocious Bite Rip"),
                                   Spell.CastSpell("Ferocious Bite",              ret => Me.CurrentTarget != null && Me.ComboPoints == 5 && Buff.TargetHasDebuff("Rip") && ((Me.CurrentTarget.HealthPercent <= (Item.Has2PcTeirBonus(ItemSetId) ? 60 : 25)) || Me.CurrentTarget.MaxHealth == 1), "Ferocious Bite Rip"),
                                   // 2. Extend Rip
                                   Spell.CastSpell("Shred",                       ret => (IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 4, "Shred (Extend Rip)"),
                                   Spell.CastSpell("Mangle (Cat)",                ret => (!IsBehind || !BossList.CanShred.Contains(Me.CurrentTarget.Entry)) && Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 4 && (Me.CurrentTarget.HealthPercent > (Item.Has2PcTeirBonus(ItemSetId) ? 60 : 25) || Me.CurrentTarget.MaxHealth == 1), "Mangle (Extend Rip) [NotBehind]"),
                                   Buff.CastDebuff("Rip",                         ret => Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 2 && (Buff.PlayerHasBuff("Berserk") || (Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= Spell.SpellCooldown("Tiger's Fury").TotalSeconds)), "Rip"),
                                   Spell.CastSpell("Ferocious Bite",              ret => Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds > 5 && Buff.PlayerBuffTimeLeft("Savage Roar") >= 3 && Buff.PlayerHasBuff("Berserk"), "Ferocious Bite"),
                                   Buff.CastDebuff("Rake",                        ret => Buff.PlayerHasBuff("Tiger's Fury") && (Buff.TargetDebuffTimeLeft("Rake").TotalSeconds < 8.9), "Rake (TF.up & Rake < 9)"),
                                   Buff.CastDebuff("Rake",                        ret => Buff.TargetDebuffTimeLeft("Rake").TotalSeconds < 2.9 && (Buff.PlayerHasBuff("Berserk") || Me.CurrentEnergy > 70 || ((Spell.SpellCooldown("Tiger's Fury").TotalSeconds + 0.1) > Buff.TargetDebuffTimeLeft("rake").TotalSeconds)), "Rake"),
                                   Spell.CastSpell("Shred",                       ret => (IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && Buff.PlayerHasBuff("Clearcasting"), "Shred (OoC)"),
                                   Spell.CastSpell("Mangle (Cat)",                ret => (!IsBehind || !BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && Buff.PlayerHasBuff("Clearcasting"), "Mangle (OoC) [NotBehind]"),
                                   // Keep up Savage Roar
                                   Spell.CastSpell("Savage Roar",                 ret => Me.ComboPoints > 0 && Buff.PlayerBuffTimeLeft("Savage Roar") <= 1, "Savage Roar"),
                                   Item.RunMacroText("/cast Ravage",              ret => Buff.PlayerHasBuff("Stampede") && Spell.SpellCooldown("Tiger's Fury").TotalSeconds < 1, "Ravage"),
                                   Spell.CastSpell("Ferocious Bite",              ret => Me.CurrentTarget != null && Me.ComboPoints == 5 && Me.CurrentTarget.HealthPercent < 10, "Ferocious Bite"),
                                   Spell.CastSpell("Ferocious Bite",              ret => Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds >= 14 && Buff.PlayerBuffTimeLeft("Savage Roar") >= 10, "Ferocious Bite"),
                                   Item.RunMacroText("/cast Ravage",              ret => Buff.PlayerHasBuff("Stampede") && Buff.PlayerHasBuff("Tiger's Fury") && !Buff.PlayerHasBuff("Clearcasting") && Me.CurrentEnergy > 80, "Ravage"),
                                   Spell.CastSpell("Shred",                       ret => (IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && (Buff.PlayerHasBuff("Berserk") || Buff.PlayerHasBuff("Tiger's Fury")), "Shred (TF or Beserk)"),
                                   Spell.CastSpell("Mangle (Cat)",                ret => (!IsBehind || !BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && (Buff.PlayerHasBuff("Berserk") || Buff.PlayerHasBuff("Tiger's Fury")), "Mangle (TF or Beserk) [NotBehind]"),
                                   Spell.CastSpell("Shred",                       ret => (IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && (Me.ComboPoints < 5 || Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 3) || (Me.ComboPoints == 0 && Buff.PlayerBuffTimeLeft("Savage Roar") < 2), "Shred"),
                                   Spell.CastSpell("Mangle (Cat)",                ret => (!IsBehind || !BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && (Me.ComboPoints < 5 || Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 3) || (Me.ComboPoints == 0 && Buff.PlayerBuffTimeLeft("Savage Roar") < 2) && Me.CurrentEnergy > 40, "Mangle [NotBehind]"),
                                   Spell.CastSpell("Shred",                       ret => (IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && Spell.SpellCooldown("Tiger's Fury").TotalSeconds <= 3, "Shred"),
                                   Spell.CastSpell("Mangle (Cat)",                ret => (!IsBehind || !BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && Spell.SpellCooldown("Tiger's Fury").TotalSeconds <= 3, "Mangle (TF) [NotBehind]"),
                                   Spell.CastSpell("Shred",                       ret => (IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && Me.CurrentEnergy > 80, "Shred (Capping)"),
                                   Spell.CastSpell("Mangle (Cat)",                ret => (!IsBehind || !BossList.CanShred.Contains(Unit.CurrentTargetEntry)) && Me.CurrentEnergy > 80, "Mangle (Capping) [NotBehind]"),
                                   Buff.CastDebuff("Faerie Fire (Feral)",         ret => Me.CurrentTarget != null && Buff.TargetCountDebuff("Faerie Fire") < 3 && !Buff.UnitHasArmorReductionDebuff(Me.CurrentTarget   ), "Faerie Fire (Feral)"))));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               new Decorator(
                                   ret => Buff.PlayerHasBuff("Bear Form"),
                                   new PrioritySelector(
                                       Spell.CastSelfSpell("Frenzied Regeneration",   ret => Me.HealthPercent <= 25 && !Buff.PlayerHasBuff("Survival Instincts"), "Frenzied Regeneration"),
                                       Spell.CastSelfSpell("Survival Instincts",      ret => Me.HealthPercent <= 40 && !Buff.PlayerHasBuff("Frenzied Regeneration"), "Survival Instincts"),
                                       Spell.CastSelfSpell("Barkskin",                ret => Me.HealthPercent <= 80, "Barkskin"),
                                       Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone"))),
                               new Decorator(
                                   ret => Buff.PlayerHasBuff("Cat Form"),
                                   new PrioritySelector(
                                       Spell.CastSelfSpell("Survival Instincts",      ret => Me.HealthPercent <= 40, "Survival Instincts"),
                                       Spell.CastSelfSpell("Barkskin",                ret => Me.HealthPercent <= 80, "Barkskin"),
                                       Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone")))));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               Buff.CastRaidBuff("Mark of the Wild", ret => !Buff.PlayerHasBuff("Blessing of Kings"), "Mark of the Wild")));
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