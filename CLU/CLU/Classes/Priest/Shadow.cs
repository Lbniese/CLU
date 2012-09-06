using System.Linq;
using CLU.Helpers;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Lists;
using CLU.Settings;
using CLU.Base;

namespace CLU.Classes.Priest
{
    class Shadow : RotationBase
    {

        private const int ItemSetId = 1067; // Tier set ID Regalia of Dying light (Normal)

        public override string Name
        {
            get {
                return "Shadow Priest";
            }
        }

        public override string KeySpell
        {
            get {
                return "Mind Blast"; // Mind Flay
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


                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return
                    @"
----------------------------------------------------------------------
Shadow MoP:
[*] Default Rotation is ""Leveling"" and is recommended until ""Default and MindSpike"" are updated.
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Settings)
This Rotation will:
1. Fade on threat, Shadowform during combat, Dispersion, Power Word: Shield,
	==> Healthstone, Flash Heal if movement enabled.
2. Buffs: Power Word: Fortitude, Shadow Protection, Inner Fire, Vampiric Embrace
3. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Shadowfiend & Dispersion & Archangel
3. AoE with Mind Sear and Divine Star
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
----------------------------------------------------------------------" + twopceinfo + "\n" + fourpceinfo + "\n";
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
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                   Item.UseEngineerGloves())),
                           // Threat
                           Buff.CastBuff("Fade", ret => (CLUSettings.Instance.UseCooldowns || CLUSettings.Instance.Priest.UseFade) && Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "Fade (Threat)"),

                           Item.RunMacroText("/cast Shadowform", ret => !Buff.PlayerHasBuff("Shadowform"), "Shadowform"),

                           // Grinding/Leveling/Farmig/Questing,etc Rotation (designed for High Kill Rate with the least amount of mana)
                           new Decorator(
                               ret => CLUSettings.Instance.Priest.SpriestRotationSelection == ShadowPriestRotation.Leveling,
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Dispersion",        ret => (Me.HealthPercent < 10 || Me.ManaPercent < 10),"Dispersion"),
                                   Spell.CastSelfSpell("Mindbender",        ret => CLUSettings.Instance.UseCooldowns && Unit.EnemyUnits.Count(u => u.IsTargetingMeOrPet) >= 2, "Mindbender"),
                                   // Multi-Dotting will occour if there are between 1 or more and less than 6 enemys within 15yrds of your current target and you have more than 50% mana and we have Empowered Shadow. //Can be disabled within the GUI
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent <= 25, "Shadow Word: Death"),
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent > 25 && Buff.PlayerHasActiveBuff("Empowered Shadow") && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Shadow Word: Pain"),
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 4 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 6 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent > 25 && Buff.PlayerHasActiveBuff("Empowered Shadow") && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Vampiric Touch"),
                                   // End Multi-Dotting
                                   Spell.CastSpell("Shadow Word: Death",    ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent <= 25, "Shadow Word: Death"),
                                   Spell.CastSpell("Mind Spike",            ret => Buff.PlayerHasBuff("Surge of Darkness"), "Mind Spike"), // Free Mindspike
                                   Spell.CastSpell("Mind Blast",            ret => !Me.IsMoving, "Mind Blast"),
                                   Buff.CastDebuff("Vampiric Touch",        ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Vampiric Touch"),
                                   Buff.CastDebuff("Shadow Word: Pain",     ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 15, "Shadow Word: Pain"),
                                   Spell.CastSpell("Divine Star",           ret => Me.CurrentTarget != null && !Me.IsMoving && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 12) > 4 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Divine Star"), // New patch 5.0.4 - could be a mana drainer --wulf
                                   Spell.CastSpell("Mind Sear",             ret => Me.CurrentTarget != null && !Me.IsMoving && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 12) > 4 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Buff.PlayerHasActiveBuff("Empowered Shadow"), "Mind Sear"),
                                   Buff.CastDebuff("Devouring Plague",      ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 20, "Devouring Plague"),
                                   Spell.CastSelfSpell("Archangel",         ret => CLUSettings.Instance.UseCooldowns && Unit.EnemyUnits.Count(u => u.IsTargetingMeOrPet) >= 2, "Archangel"),
                                   Spell.CastSpell("Shadowfiend",           ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Shadowfiend"),
                                   Spell.CastSpell("Mind Spike",            ret => true, "Mind Spike"),
                                   Spell.CastSpecialSpell("Mind Flay",      ret => Buff.TargetDebuffTimeLeft("Mind Flay").TotalSeconds <= Spell.ClippingDuration(), "Mind Flay")
                               )),


                           // Default Rotation
                           new Decorator(
                               ret => CLUSettings.Instance.Priest.SpriestRotationSelection == ShadowPriestRotation.Default,
                               new PrioritySelector(
                                   Spell.StopCast(ret => Me.CastingSpell.Name == "Mind Flay" && (Buff.TargetDebuffTimeLeft("Shadow Word: Pain").TotalSeconds < Buff.DotDelta("Shadow Word: Pain") || Buff.TargetDebuffTimeLeft("Devouring Plague").TotalSeconds < Buff.DotDelta("Devouring Plague") || Buff.TargetDebuffTimeLeft("Vampiric Touch").TotalSeconds < Buff.DotDelta("Vampiric Touch") && Spell.SpellCooldown("Mind Blast").TotalSeconds < Buff.DotDelta("Mind Blast")), "Mind Flay"),
                                   // Multi-Dotting will occour if there are between 1 or more and less than 6 enemys within 15yrds of your current target and you have more than 50% mana and we have Empowered Shadow. //Can be disabled within the GUI
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent <= 25, "Shadow Word: Death"),
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent > 25 && Buff.PlayerHasActiveBuff("Empowered Shadow"), "Shadow Word: Pain"),
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 4 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 6 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent > 25 && Buff.PlayerHasActiveBuff("Empowered Shadow"), "Vampiric Touch"),
                                   // End Multi-Dotting
                                   Spell.CastSpell("Mind Blast",              ret => !Me.IsMoving, "Mind Blast"),
                                   Buff.CastDebuff("Shadow Word: Pain",       ret => true, "Shadow Word: Pain"),
                                   Spell.CastSpell("Mind Sear",               ret => Me.CurrentTarget != null && !Me.IsMoving && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 12) > 4 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Buff.PlayerHasActiveBuff("Empowered Shadow"), "Mind Sear"),
                                   Buff.CastDebuff("Devouring Plague",        ret => true, "Devouring Plague"),
                                   Buff.CastDebuff("Vampiric Touch",          ret => !Me.IsMoving, "Vampiric Touch"),
                                   Spell.CastSelfSpell("Archangel",           ret => CLUSettings.Instance.UseCooldowns && Buff.PlayerCountBuff("Dark Evangelism") > 4 && Buff.TargetDebuffTimeLeft("Vampiric Touch").TotalSeconds > 5 && Buff.TargetDebuffTimeLeft("Devouring Plague").TotalSeconds > 5, "Archangel"),
                                   Spell.CastSpell("Shadow Word: Death",      ret => Me.CurrentTarget != null && (Item.Has2PcTeirBonus(ItemSetId) ? Me.CurrentTarget.HealthPercent <= 100 : Me.CurrentTarget.HealthPercent <= 25), "Shadow Word: Death"),
                                   Spell.CastSpell("Shadowfiend",             ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Shadowfiend"),
                                   Spell.CastSpell("Shadow Word: Death",      ret => Me.ManaPercent < 10, "Shadow Word: Death - Low Mana"),
                                   Spell.CastSpecialSpell("Mind Flay",        ret => !Me.IsMoving && Spell.SpellCooldown("Mind Blast").TotalSeconds > Buff.DotDelta("Mind Blast") && Buff.TargetDebuffTimeLeft("Mind Flay").TotalSeconds <= Spell.ClippingDuration() && Buff.TargetHasDebuff("Shadow Word: Pain") && Buff.TargetHasDebuff("Vampiric Touch") && Buff.TargetHasDebuff("Devouring Plague"), "Mind Flay"),
                                   Spell.CastSpell("Shadow Word: Death",      ret => Me.IsMoving, "Shadow Word: Death - Moving"),
                                   Spell.CastSpell("Devouring Plague",        ret => Me.IsMoving && Me.ManaPercent > 10, "Devouring Plague"),
                                   Spell.CastSelfSpell("Dispersion",          ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && (Me.HealthPercent < 10 || Me.ManaPercent < 10), "Dispersion")
                               )),
                           // MindSpike Experimental Rotation
                           new Decorator(
                               ret => CLUSettings.Instance.Priest.SpriestRotationSelection == ShadowPriestRotation.MindSpike,
                               new PrioritySelector(
                                   Spell.CastSpell("Mind Flay",                    ret => !Me.IsMoving && Buff.PlayerCountBuff("Dark Evangelism") < 5 && Spell.SpellCooldown("Shadowfiend").TotalSeconds <= 3, "Mind Flay"),
                                   Spell.CastSpell("Shadowfiend",                  ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerCountBuff("Dark Evangelism") > 4 && Spell.SpellCooldown("Archangel").TotalSeconds <= 0.1, "Shadowfiend"),
                                   Spell.CastSelfSpell("Archangel",                ret => CLUSettings.Instance.UseCooldowns && Buff.PlayerCountBuff("Dark Evangelism") > 4 && Me.GotAlivePet, "Archangel"),
                                   Item.RunMacroText("/cancelaura Mind Melt",      ret => Buff.PlayerHasActiveBuff("Mind Melt") && Spell.SpellCooldown("Mind Blast").TotalSeconds <= 0.1, "[CancelAura] Mind Melt"),
                                   // Spell.StopCast(ret => Me.CastingSpell.Name == "Mind Flay" && (Buff.TargetDebuffTimeLeft("Shadow Word: Pain").TotalSeconds < (Spell.GCD + 0.5) || Buff.TargetDebuffTimeLeft("Devouring Plague").TotalSeconds < (Spell.GCD + 1) || Buff.TargetDebuffTimeLeft("Vampiric Touch").TotalSeconds < (Spell.CastTime("Vampiric Touch"))) && Buff.PlayerCountBuff("Dark Evangelism") == 5 && Spell.SpellCooldown("Shadowfiend").TotalSeconds > 3, "Mind Flay"),
                                   Spell.CastSpell("Mind Blast",                   ret => !Me.IsMoving, "Mind Blast"),
                                   Spell.CastSpell("Shadow Word: Death",           ret => Me.CurrentTarget != null && (Item.Has2PcTeirBonus(ItemSetId) ? Me.CurrentTarget.HealthPercent <= 100 : Me.CurrentTarget.HealthPercent <= 25) && Me.GotAlivePet, "Shadow Word: Death"),
                                   Spell.CastSpell("Mind Spike",                   ret => !Me.IsMoving && Me.GotAlivePet && Spell.SpellCooldown("Shadowfiend").TotalSeconds > (Spell.GCD + 0.6), "Mind Spike"),  // && !Buffs.UnitHasHasteBuff(Me)
                                   Buff.CastDebuff("Vampiric Touch",               ret => !Me.IsMoving, "Vampiric Touch"),
                                   Buff.CastDebuff("Shadow Word: Pain",            ret => true, "Shadow Word: Pain (GCD+0.5=" + (Spell.GCD + 0.5) + ")"),
                                   Spell.CastSpell("Mind Sear",                    ret => Me.CurrentTarget != null && !Me.IsMoving && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 12) > 4 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Mind Sear"),
                                   Buff.CastDebuff("Devouring Plague",             ret => true, "Devouring Plague (GCD+1=" + (Spell.GCD + 1) + ")"),
                                   Spell.CastSelfSpell("Archangel",                ret => CLUSettings.Instance.UseCooldowns && Buff.PlayerCountBuff("Dark Evangelism") > 4 && Buff.TargetDebuffTimeLeft("Vampiric Touch").TotalSeconds > 5 && Buff.TargetDebuffTimeLeft("Devouring Plague").TotalSeconds > 5, "Archangel"),
                                   Spell.CastSpell("Shadow Word: Death",           ret => Me.CurrentTarget != null && (Item.Has2PcTeirBonus(ItemSetId) ? Me.CurrentTarget.HealthPercent <= 100 : Me.CurrentTarget.HealthPercent <= 25), "Shadow Word: Death"),
                                   // Spell.CastSpell("Shadow Word: Death",       ret => Me.CurrentTarget != null &&Me.CurrentTarget.HealthPercent <= 25 || Me.CurrentTarget.MaxHealth == 1, "Shadow Word: Death"), // will cast on cooldown on the dummy.
                                   Spell.CastSelfSpell("Shadowfiend",              ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Shadowfiend"),
                                   Spell.CastSpell("Shadow Word: Death",           ret => Me.ManaPercent < 10, "Shadow Word: Death - Low Mana"),
                                   Spell.CastSpell("Mind Flay",                    ret => !Me.IsMoving, "Mind Flay"),
                                   Spell.CastSpell("Shadow Word: Death",           ret => Me.IsMoving, "Shadow Word: Death - Moving"),
                                   Buff.CastDebuff("Devouring Plague",             ret => Me.IsMoving && Me.ManaPercent > 10, "Devouring Plague"),
                                   Spell.CastSelfSpell("Dispersion",               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && (Me.HealthPercent < 10 || Me.ManaPercent < 10), "Dispersion")
                               ))
                       );
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSelfSpell("Power Word: Shield",  ret => !Buff.PlayerHasBuff("Weakened Soul") && Me.HealthPercent < CLUSettings.Instance.Priest.ShieldHealthPercent, "Power Word: Shield"),
                               Item.UseBagItem("Healthstone",             ret => Me.HealthPercent < CLUSettings.Instance.Priest.UseHealthstone, "Healthstone"),
                               Spell.CastSpell("Flash Heal",              ret => Me.HealthPercent < CLUSettings.Instance.Priest.ShadowFlashHealHealth, "Emergency flash heal")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new PrioritySelector(
                           new Decorator(
                               ret => !Me.Mounted && !Me.Dead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                               new PrioritySelector(
                                   // Item.RunMacroText("/cast Shadowform", ret => !Buff.PlayerHasBuff("Shadowform"), "Shadowform"),
                                   Buff.CastRaidBuff("Power Word: Fortitude",   ret => CLUSettings.Instance.Priest.UsePowerWordFortitude, "Power Word: Fortitude"),
                                   Buff.CastRaidBuff("Shadow Protection",       ret => CLUSettings.Instance.Priest.UseShadowProtection, "Shadow Protection"),
                                   Buff.CastBuff("Inner Fire",                  ret => CLUSettings.Instance.Priest.UseInnerFire, "Inner Fire"),
                                   Buff.CastBuff("Vampiric Embrace",            ret => true, "Vampiric Embrace"))));
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
