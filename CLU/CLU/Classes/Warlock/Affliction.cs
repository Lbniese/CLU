using CLU.Helpers;
using CommonBehaviors.Actions;
using Styx.TreeSharp;
using CLU.Lists;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Warlock
{

    class Affliction : RotationBase
    {
        public override string Name
        {
            get {
                return "Affliction Warlock";
            }
        }

        public override string KeySpell
        {
            get {
                return "Unstable Affliction";
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Soulshatter, Soul Harvest < 2 shards out of combat\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Demon Soul while not moving & Summon Doomguard & Curse of the Elements & Lifeblood\n" +
                       "3. AoE with Rain of Fire, Shadowfury\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to ShinobiAoshi, Stormchasing\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // Performance Timer (True = Enabled) returns Runstatus.Failure
                           // Spell.TreePerformance(true),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           // START Interupts & Spell casts
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           PetManager.CastPetSummonSpell("Summon Felhunter", ret => !Me.GotAlivePet && (Buff.PlayerHasBuff("Demonic Rebirth") || Buff.PlayerHasBuff("Soulburn")), "Summoning Pet Felhunter"),
                           // Threat
                           Buff.CastBuff("Soulshatter",                        ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "Soulshatter"),
                           // Cooldown
                           Spell.CastSelfSpell("Demon Soul",                   ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !Me.IsMoving, "Demon Soul while not moving"),
                           // AoE here
                           Spell.CastAreaSpell("Soulburn", 15, false, 4, 0.0, 0.0, ret => Me.ManaPercent > 40, "Soulburn for Seed of Corruption"),
                           Spell.CastAreaSpell("Seed of Corruption", 15, false, 4, 0.0, 0.0, ret => !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > 40, "Seed of Corruption after Soulburn"),
                           // End AoE
                           Buff.CastDebuff("Curse of the Elements",           ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 70 && !Buff.UnitHasMagicVulnerabilityDeBuffs(Me.CurrentTarget), "Curse of the Elements"),
                           Spell.CastSelfSpell("Soulburn",                    ret => !Buff.UnitHasHasteBuff(Me), "Soulburn"),
                           Spell.CastSpell("Soul Fire",                       ret => Buff.PlayerHasBuff("Soulburn"), "Soul Fire with Soulburn"),
                           Spell.CastSpell("Soul Swap",                       ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) <= 10 && Buff.TargetHasDebuff("Unstable Affliction") && Buff.TargetHasDebuff("Corruption") && (Buff.TargetHasDebuff("Bane of Agony") || Buff.TargetHasDebuff("Bane of Doom")), "Soul Swapping"),
                           Spell.CastSpell("Soul Swap",                       ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Soul Swap") && Me.CurrentTarget.HealthPercent >= 10 && !Buff.TargetHasDebuff("Unstable Affliction") && !Buff.TargetHasDebuff("Corruption") && (!Buff.TargetHasDebuff("Bane of Agony") || !Buff.TargetHasDebuff("Bane of Doom")), "Soul Swap onto new target"),
                           // This Talentspecc should be default, but who knows
                           Spell.CastSpell("Shadow Bolt",                     ret => !Buff.TargetHasDebuff("Shadow Embrace") && TalentManager.HasTalent(1, 12), "Shadow Bolt for Shadow Embrace"),
                           Spell.CastSpell("Shadow Bolt",                     ret => !Buff.TargetHasDebuff("Shadow and Flame") && TalentManager.HasTalent(3, 2), "Shadow Bolt for Shadow and Flame"),
                           Spell.CastSpell("Haunt",                           ret => true, "Haunt"),
                           Buff.CastDebuff("Unstable Affliction",             ret => true, "Unstable Affliction"),
                           Buff.CastDebuff("Corruption",                      ret => true, "Corruption"),
                           Buff.CastDebuff("Bane of Doom",                    ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Unit.TimeToDeath(Me.CurrentTarget) > 60 && !Buff.UnitsHasMyBuff("Bane of Doom"), "Bane of Doom TTL=" + Unit.TimeToDeath(Me.CurrentTarget)),
                           Buff.CastDebuff("Bane of Agony",                   ret => Me.CurrentTarget != null && !Unit.IsTargetWorthy(Me.CurrentTarget) || (Unit.IsTargetWorthy(Me.CurrentTarget) && (Unit.TimeToDeath(Me.CurrentTarget) < 60 || Unit.TimeToDeath(Me.CurrentTarget) == 9999) && (!Buff.UnitsHasMyBuff("Bane of Doom") || !Buff.TargetHasDebuff("Bane of Doom"))), "Bane of Agony TTL=" + Unit.TimeToDeath(Me.CurrentTarget)),
                           // Multi-Dotting will occour if there are between 1 or more and less than 6 enemys within 15yrds of your current target and you have more than 50%. //Can be disabled within the GUI
                           Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50, "Unstable Affliction"),
                           Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50, "Corruption"),
                           // Start - Multi Dotting - We will Multidot every unit in Range BoD,BoA,Corr,UA
                           // Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.Location, 40) > 1 && Unit.CountEnnemiesInRange(Me.Location, 40) < 6 && Me.ManaPercent > 50 && Unit.IsTargetWorthy(Me.CurrentTarget) && Unit.TimeToDeath(Me.CurrentTarget) > 60 && !Buff.UnitsHasMyBuff("Bane of Doom"), "Bane of Doom"),
                           // Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.Location, 40) > 1 && Unit.CountEnnemiesInRange(Me.Location, 40) < 6 && Me.ManaPercent > 50 && !Unit.IsTargetWorthy(Me.CurrentTarget) || (Unit.IsTargetWorthy(Me.CurrentTarget) && (Unit.TimeToDeath(Me.CurrentTarget) < 60 || Unit.TimeToDeath(Me.CurrentTarget) == 9999) && (!Buff.UnitsHasMyBuff("Bane of Doom") || !Buff.TargetHasDebuff("Bane of Doom"))), "Bane of Agony"),
                           // Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.Location, 40) > 1 && Unit.CountEnnemiesInRange(Me.Location, 40) < 6 && Me.ManaPercent > 50 && !Buff.UnitsHasMyBuff("Corruption"), "Corruption"),
                           // Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.Location, 40) > 1 && Unit.CountEnnemiesInRange(Me.Location, 40) < 6 && Me.ManaPercent > 50 && !Buff.UnitsHasMyBuff("Unstable Affliction"), "Unstable Affliction"),
                           // End Multi Dotting
                          // Spell.CastSelfSpell("Summon Doomguard",            ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Doomguard"),
                           Spell.ChannelSpell("Drain Soul",                   ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) < 4 && ((Buff.TargetHasDebuff("Bane of Doom") || (Buff.TargetDebuffTimeLeft("Bane of Agony").TotalSeconds >= 4)) && Buff.TargetDebuffTimeLeft("Haunt").TotalSeconds >= 4 && Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds >= 3 && Buff.TargetDebuffTimeLeft("Unstable Affliction").TotalSeconds >= 3), "Drain Soul"),
                           Spell.CastConicSpell("Shadowflame", 11f, 33f, ret => true, "ShadowFlame"),
                           Spell.CastSelfSpell("Life Tap",                    ret => Me.ManaPercent <= 35, "Life tap <= 35%"),
                           Spell.ChannelSpell("Drain Soul",                   ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent <= 25 && ((Buff.TargetHasDebuff("Bane of Doom") || (Buff.TargetDebuffTimeLeft("Bane of Agony").TotalSeconds >= 4)) && Buff.TargetDebuffTimeLeft("Haunt").TotalSeconds >= 4 && Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds >= 3 && Buff.TargetDebuffTimeLeft("Unstable Affliction").TotalSeconds >= 3), "Drain Soul"),
                           Spell.CastSpell("Shadow Bolt",                     ret => Buff.PlayerHasBuff("Shadow Trance"), "Shadow Bolt with Shadow Trance"),
                           Spell.CastSpell("Shadow Bolt",                     ret => true, "Shadow Bolt"),
                           Spell.CastSelfSpell("Life Tap",                    ret => Me.IsMoving && Me.HealthPercent > Me.ManaPercent && Me.ManaPercent < 80, "Life tap while moving"),
                           Spell.CastSpell("Fel Flame",                       ret => Me.IsMoving, "Fel flame while moving"),
                           Spell.CastSelfSpell("Life Tap",                    ret => Me.ManaPercent < 100 && !Spell.PlayerIsChanneling && Me.HealthPercent > 40, "Life tap while mana < 100%"));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new PrioritySelector(
                           new Decorator(
                               ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                               new PrioritySelector(
                                   PetManager.CastPetSummonSpell("Summon Felhunter", ret => !Me.IsMoving && !Me.GotAlivePet, "Summoning Pet Felhunter"),
                                   Buff.CastBuff("Soul Link", ret => Pet != null && Pet.IsAlive, "Soul Link")
                                  )));
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