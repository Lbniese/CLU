﻿#region Revision info
/*
 * $Author$
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 * $LastChangedBy$
 * $ChangesMade$
 */
#endregion

using System.Linq;
using CLU.Helpers;
using CLU.Lists;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using Styx;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Druid
{
    using global::CLU.Managers;
    using Styx.CommonBot;

    class Balance : RotationBase
    {
        public override string Name
        {
            get
            {
                return "Balance Druid";
            }
        }

        public override string Revision
        {
            get
            {
                return "$Rev$";
            }
        }

        public override string KeySpell
        {
            get
            {
                return "Starfire";
            }
        }

        public override int KeySpellId
        {
            get { return 2912; }
        }

        public override float CombatMinDistance
        {
            get
            {
                return 30f;
            }
        }

        private static int MushroomCount
        {
            get
            {
                return ObjectManager.GetObjectsOfType<WoWUnit>().Where(o => o.Entry == 47649 && o.Distance <= 40).Count(o => o.CreatedByUnitGuid == Me.Guid);
            }
            // Thanks to Singular for the logic and code.
        }

        public override string Help
        {
            get
            {
                return "\n" +
                "----------------------------------------------------------------------\n" +
                "Has Incarnation talened: " + TalentManager.HasTalent(11) + "\n" +
                "This Rotation will:\n" + TalentManager.HasTalent(11) +
                "1. Attempt to heal with healthstone\n" +
                "2. Raid buff Mark of the Wild\n" +
                "3. AutomaticCooldowns has: \n" +
                "==> UseTrinkets \n" +
                "==> UseRacials \n" +
                "==> UseEngineerGloves \n" +
                "==> Force of Nature & Volcanic Potion & Faerie Fire\n" +
                "4. AoE with Wild Mushroom, Starfall, \n" +
                "5. Best Suited for end game raiding\n" +
                "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                "Credits to kbrebel04 for helping with this rotation\n" +
                "----------------------------------------------------------------------\n";
            }
        }

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector(
                    // Pause Rotation
                    new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                    Spell.WaitForCast(true),

                    // For DS Encounters.
                    EncounterSpecific.ExtraActionButton(),

                    // HandleMovement? If so, Choose our form!
                    new Decorator(
                        ret => CLUSettings.Instance.EnableMovement,
                        new PrioritySelector(
                            Spell.CastSelfSpell(
                                "Moonkin Form", ret => !Buff.PlayerHasBuff("Moonkin Form") && !CLU.IsMounted, "Moonkin Form"))),

                        new Decorator(ret => Buff.PlayerHasBuff("Moonkin Form") && !Buff.PlayerHasActiveBuff("Shadowmeld"),
                            new PrioritySelector(
                                new Decorator(ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                                    new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Racials.UseRacials(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                    Item.UseBagItem("Volcanic Potion", ret => Buff.UnitHasHasteBuff(Me), "Volcanic Potion Heroism/Bloodlust"),
                                    Spell.CastSpell("Sunfire", ret => Buff.PlayerHasBuff("Eclipse Visual (Lunar)") && Buff.PlayerHasBuff("Incarnation: Chosen of Elune") && !Buff.TargetHasDebuff("Sunfire"), "Sunfire Opener"),
                                    Spell.CastSpell("Moonfire", ret => Buff.PlayerHasBuff("Eclipse Visual (Lunar)") && Buff.PlayerHasBuff("Incarnation: Chosen of Elune") && Buff.TargetHasDebuff("Sunfire") && !Buff.TargetHasDebuff("Moonfire"), "Moonfire Opener"),
                                    Spell.CastSelfSpell("Incarnation", ret => TalentManager.HasTalent(11) && (Buff.PlayerHasBuff("Eclipse Visual (Solar)") || Buff.PlayerHasBuff("Eclipse Visual (Lunar)")), "Incarnation: Chosen of Elune"),
                                    Spell.CastSelfSpell("Nature's Vigil", ret => TalentManager.HasTalent(18) && Buff.PlayerHasBuff("Incarnation: Chosen of Elune"), "Nature's Vigil"),
                                    Spell.CastSelfSpell("Celestial Alignment", ret => (Buff.PlayerHasBuff("Eclipse (Lunar)") && Me.CurrentEclipse <= 20 && Me.CurrentEclipse >= -20 || Buff.PlayerHasBuff("Eclipse (Solar)") && Me.CurrentEclipse < 5) && (Buff.PlayerHasBuff("Incarnation: Chosen of Elune") || !TalentManager.HasTalent(11)), "Celestial Alignment"),
                                    Item.UseEngineerGloves())),

                                //AoE Rotation
                                new Decorator(ret => CLUSettings.Instance.UseAoEAbilities && Unit.NearbyNonControlledUnits(Me.CurrentTarget.Location, 10f, false).Count(a => !a.IsFriendly && a.Combat) >= 3,
                                        new PrioritySelector(
                                        Spell.CastSpell("Starfall", ret => !Buff.PlayerHasBuff("Starfall"), "Starfall"),
                                        Spell.CastSpell("Wild Mushroom: Detonate", ret => MushroomCount == 3, "Detonate Shrooms!"),
                                        Spell.CastSpell("Wild Mushroom: Detonate", ret => MushroomCount > 0 && Buff.PlayerHasBuff("Eclipse (Solar)"), "Detonate Shrooms!"),
                    //Spell.CastOnUnitLocation("Force of Nature", u => Me.CurrentTarget, ret => TalentManager.HasTalent(12) && Me.CurrentTarget != null && Unit.UseCooldowns() && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Force of Nature"),
                                        Spell.CastOnUnitLocation("Wild Mushroom", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 6) >= 3 && MushroomCount < 3, "Wild Mushroom"),
                                        Spell.CastSpell("Moonfire", ret => !Buff.TargetHasBuff("Moonfire"), "Moonfire (Moving)"),
                                        Spell.CastSpell("Sunfire", ret => !Buff.TargetHasBuff("Sunfire"), "Sunfire (Moving)"),
                                        Spell.CastSpell("Moonfire", ret => true, "Moonfire"),
                                        Spell.CastSpell("Sunfire", ret => true, "Sunfire"))),
                    //Celestial Alignment Rotation
                                new Decorator(ret => Buff.PlayerHasBuff("Celestial Alignment") && (!CLUSettings.Instance.UseAoEAbilities || Unit.CountEnnemiesInRange(Me.Location, 8f) < 3),
                                        new PrioritySelector(
                                        Spell.CastSpell("Starfall", ret => !Buff.PlayerHasBuff("Starfall"), "Starfall"),
                                        Spell.CastSpell("Moonfire", ret => Buff.PlayerHasBuff("Celestial Alignment") && Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds < 11 && (Buff.PlayerBuffTimeLeft("Celestial Alignment") >= 12 || Buff.PlayerBuffTimeLeft("Celestial Alignment") <= 3), "Moonfire Celestial"),
                                        Spell.CastSpell("Starsurge", ret => !Me.IsMoving, "Starsurge"),
                                        Spell.CastSpell("Starfire", ret => !Me.IsMoving, "Starfire"))),

                                //Single Target Rotation
                                new Decorator(ret => !Buff.PlayerHasBuff("Celestial Alignment") && (!CLUSettings.Instance.UseAoEAbilities || Unit.NearbyNonControlledUnits(Me.CurrentTarget.Location, 10f, false).Count(a => !a.IsFriendly && a.Combat) < 3),
                                        new PrioritySelector(
                                        Spell.CastSpell("Starfall", ret => !Buff.PlayerHasBuff("Starfall"), "Starfall"),
                                        Spell.CastSpell("Wild Mushroom: Detonate", ret => MushroomCount == 3 && Buff.PlayerHasBuff("Eclipse (Solar)") && Buff.PlayerHasBuff("Nature's Grace"), "Detonate Shrooms!"),
                                        Spell.CastSpell("Wrath", ret => !Buff.PlayerHasBuff("Eclipse Visual (Lunar)") && Buff.PlayerBuffTimeLeft("Starfall") > 8 && Me.CurrentEclipse == -75 && !Buff.TargetHasDebuff("Moonfire") && !Buff.TargetHasDebuff("Sunfire") && !Spell.SpellOnCooldown("Starsurge"), "Wrath"),
                                        Spell.CastSpell("Moonfire", ret => (Buff.PlayerHasBuff("Eclipse Visual (Lunar)") || Me.CurrentEclipse == -100) && (!Buff.TargetHasDebuff("Moonfire") || Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds <= 9), "Moonfire Start Lunar"),
                                        Spell.CastSpell("Sunfire", ret => (Buff.PlayerHasBuff("Eclipse Visual (Solar)") || Me.CurrentEclipse == 100) && (!Buff.TargetHasDebuff("Sunfire") || Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds <= 10), "Sunfire Start Solar"),
                                        Spell.CastSpell("Moonfire", ret => !Buff.PlayerHasBuff("Incarnation: Chosen of Elune") && Buff.PlayerHasBuff("Eclipse (Lunar)") && (!Buff.TargetHasDebuff("Moonfire") || Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds < 3), "Moonfire High"),
                                        Spell.CastSpell("Sunfire", ret => !Buff.PlayerHasBuff("Incarnation: Chosen of Elune") && Buff.PlayerBuffTimeLeft("Starfall") < 8 && Buff.PlayerHasBuff("Eclipse (Solar)") && Me.CurrentEclipse > -75 && (!Buff.TargetHasDebuff("Sunfire") || Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds < 3), "Sunfire High"),
                                        Spell.CastSpell("Moonfire", ret => !Buff.PlayerHasBuff("Incarnation: Chosen of Elune") && Buff.PlayerHasBuff("Eclipse (Solar)") && Me.CurrentEclipse > -60 && (!Buff.TargetHasDebuff("Moonfire") || Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds < 3), "Moonfire High"),
                                        Spell.CastSpell("Sunfire", ret => !Buff.PlayerHasBuff("Incarnation: Chosen of Elune") && Buff.PlayerHasBuff("Eclipse (Lunar)") && Me.CurrentEclipse < 60 && Buff.TargetHasDebuff("Sunfire") && Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds < 3, "Sunfire High"),
                                        Spell.CastSpell("Sunfire", ret => Buff.PlayerHasBuff("Eclipse (Lunar)") && !Buff.PlayerHasBuff("Incarnation: Chosen of Elune") && !Buff.TargetHasDebuff("Sunfire"), "Sunfire Opener"),
                    //Spell.CastSpell("Sunfire", ret => Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds < 3, "Sunfire Start Solar"),
                    //Spell.CastSpell("Moonfire", ret => Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds < 3, "Moonfire High"),
                    //Eclipse Generators
                                        Spell.CastSpell("Starsurge", ret => !Me.IsMoving && (Buff.PlayerHasBuff("Eclipse (Lunar)") || Me.CurrentEclipse <= 0 && Me.CurrentEclipse > -65 && Buff.PlayerHasBuff("Eclipse (Solar)")), "Starsurge"),
                                        Spell.CastSpell("Wrath", ret => !Me.IsMoving && !Buff.PlayerHasBuff("Nature's Grace") && Buff.PlayerHasBuff("Eclipse (Solar)") && Buff.TargetHasDebuff("Sunfire") && Me.CurrentEclipse <= -70, "Wrath"),
                                        Spell.CastSpell("Starfire", ret => !Me.IsMoving && !Buff.PlayerHasBuff("Nature's Grace") && Buff.PlayerHasBuff("Eclipse (Lunar)") && Buff.TargetHasDebuff("Moonfire") && Me.CurrentEclipse == 80, "Starfire"),
                                        Spell.CastSpell("Wrath", ret => !Me.IsMoving && !Buff.PlayerHasBuff("Eclipse (Lunar)") && Buff.TargetHasDebuff("Sunfire") && Me.CurrentEclipse <= 100 && Me.CurrentEclipse > -70, "Wrath"),
                                        Spell.CastSpell("Starfire", ret => !Me.IsMoving && !Buff.PlayerHasBuff("Eclipse (Solar)") && Buff.TargetHasDebuff("Moonfire") && Me.CurrentEclipse >= -100 && Me.CurrentEclipse < 80, "Starfire"),
                                        Spell.CastSpell("Moonfire", ret => Me.IsMoving && !Buff.TargetHasBuff("Moonfire"), "Moonfire (Moving)"),
                                        Spell.CastSpell("Sunfire", ret => Me.IsMoving && !Buff.TargetHasBuff("Sunfire"), "Sunfire (Moving)"),
                                        Spell.CastSpell("Starsurge", ret => Me.IsMoving && Buff.PlayerHasBuff("Shooting Stars"), "Starsurge"),
                                        Spell.CastSpell("Typhoon", ret => Me.IsMoving, "Typhoon (Moving)"),
                                        Spell.CastSpell("Moonfire", ret => Me.IsMoving && Buff.PlayerHasBuff("Eclipse (Lunar)"), "Moonfire (Moving)"),
                                        Spell.CastSpell("Sunfire", ret => Me.IsMoving && Buff.PlayerHasBuff("Eclipse (Solar)"), "Sunfire (Moving)"))))));
            }
        }

        public Composite burstRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                ));
            }
        }

        public Composite baseRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                #region Boomkin Form
new Decorator(ret => Buff.PlayerHasBuff("Boomkin Form"),
                            new PrioritySelector(
                    //new Action(a => { SysLog.Log("I am the start of public Composite baseRotation"); return RunStatus.Failure; }),
                    //PvP Utilities

                                //Rotation
                    //jade_serpent_potion,if=buff.bloodlust.react|target.time_to_die<=40|buff.celestial_alignment.up
                                new Decorator(ret => !Me.IsMoving,
                                    new PrioritySelector(
                    //starfall,if=!buff.starfall.up
                                        Spell.CastSpell("Starfall", ret => !Buff.PlayerHasBuff("Starfall"), "Starfall"),
                    //treants,if=talent.force_of_nature.enabled
                                        Spell.CastSpell("Force of Nature", ret => TalentManager.HasTalent(12), "Force of Nature"),
                    //berserking
                                        Racials.UseRacials(),
                    //wild_mushroom_detonate,moving=0,if=buff.wild_mushroom.stack>0&buff.solar_eclipse.up
                                        Spell.CastSpell("Wild Mushroom: Detonate", ret => !Me.IsMoving && MushroomCount > 0 && Buff.PlayerHasBuff("Eclipse (Solar)"), "Wild Mushroom: Detonate"),
                    //natures_swiftness,if=talent.dream_of_cenarius.enabled&talent.natures_swiftness.enabled
                                        Spell.CastSelfSpell("Nature's Swiftness", ret => TalentManager.HasTalent(17) && TalentManager.HasTalent(4), "Nature's Swiftness"),
                    //healing_touch,if=!buff.dream_of_cenarius_damage.up&talent.dream_of_cenarius.enabled
                                        Spell.CastHeal("Healing Touch", ret => !Buff.PlayerHasActiveBuff(108381) && TalentManager.HasTalent(17), "Healing Touch"),
                    //incarnation,if=talent.incarnation.enabled&(buff.lunar_eclipse.up|buff.solar_eclipse.up)
                                        Spell.CastSelfSpell("Incarnation", ret => TalentManager.HasTalent(11) && (Buff.PlayerHasBuff("Eclipse (Lunar)") || Buff.PlayerHasBuff("Eclipse (Solar)")), "Incarnation"),
                    //celestial_alignment,if=((eclipse_dir=-1&eclipse<=0)|(eclipse_dir=1&eclipse>=0))&(buff.chosen_of_elune.up|!talent.incarnation.enabled)
                                        Spell.CastSelfSpell("Celestial Alignment", ret => ((Common.eclipseDir() == -1 && Me.CurrentEclipse <= 0) || (Common.eclipseDir() == 1 && Me.CurrentEclipse >= 0)) && (Buff.PlayerHasActiveBuff("Incarnation: Chosen of Elune") || !TalentManager.HasTalent(11)), "Celestial Alignment"),
                    //natures_vigil,if=((talent.incarnation.enabled&buff.chosen_of_elune.up)|(!talent.incarnation.enabled&buff.celestial_alignment.up))&talent.natures_vigil.enabled
                                        Spell.CastSelfSpell("Nature's Vigil", ret => ((TalentManager.HasTalent(11) && Buff.PlayerHasActiveBuff("Incarnation: Chosen of Elune")) || (!TalentManager.HasTalent(11) && Buff.PlayerHasActiveBuff("Celestial Alignment"))) && TalentManager.HasTalent(18), "Nature's Vigil"),
                    //wrath,if=eclipse<=-70&eclipse_dir<=0
                                        Spell.CastSpell("Wrath", ret => Me.CurrentEclipse <= -70 && Common.eclipseDir() <= 0, "Wrath"),
                    //starfire,if=eclipse>=60&eclipse_dir>=0
                                        Spell.CastSpell("Starfire", ret => Me.CurrentEclipse >= 60 && Common.eclipseDir() >= 0, "Starfire"),
                    //J	15.60	moonfire,if=buff.lunar_eclipse.up&(dot.moonfire.remains<(buff.natures_grace.remains-2+2*set_bonus.tier14_4pc_caster))
                    //K	11.96	sunfire,if=buff.solar_eclipse.up&!buff.celestial_alignment.up&(dot.sunfire.remains<(buff.natures_grace.remains-2+2*set_bonus.tier14_4pc_caster))
                    //moonfire,if=!dot.moonfire.ticking&!buff.celestial_alignment.up&(buff.dream_of_cenarius_damage.up|!talent.dream_of_cenarius.enabled)
                                        Spell.CastSpell("Moonfire", ret => !Buff.TargetHasDebuff("Moonfire") && !Buff.PlayerHasActiveBuff("Celestial Alignment") && (Buff.PlayerHasActiveBuff(108381) || !TalentManager.HasTalent(17)), "Sunfire"),
                    //sunfire,if=!dot.sunfire.ticking&!buff.celestial_alignment.up&(buff.dream_of_cenarius_damage.up|!talent.dream_of_cenarius.enabled)
                                        Spell.CastSpell("Sunfire", ret => !Buff.TargetHasDebuff("Sunfire") && !Buff.PlayerHasActiveBuff("Celestial Alignment") && (Buff.PlayerHasActiveBuff(108381) || !TalentManager.HasTalent(17)), "Sunfire"),
                    //starsurge
                                        Spell.CastSpell("Starsurge", ret => true, "Starsurge"),
                    //starfire,if=buff.celestial_alignment.up&cast_time<buff.celestial_alignment.remains
                                        Spell.CastSpell("Starfire", ret => Buff.PlayerHasActiveBuff("Celestial Alignment") && SpellManager.Spells["Starfire"].CastTime < Buff.PlayerActiveBuffTimeLeft("Celestial Alignment").TotalSeconds, "Starfire"),
                    //wrath,if=buff.celestial_alignment.up&cast_time<buff.celestial_alignment.remains
                                        Spell.CastSpell("Wrath", ret => Buff.PlayerHasActiveBuff("Celestial Alignment") && SpellManager.Spells["Wrath"].CastTime < Buff.PlayerActiveBuffTimeLeft("Celestial Alignment").TotalSeconds, "Wrath"),
                    //starfire,if=eclipse_dir=1|(eclipse_dir=0&eclipse>0)
                                        Spell.CastSpell("Starfire", ret => Common.eclipseDir() == 1 || (Common.eclipseDir() == 0 && Me.CurrentEclipse > 0), "Starfire"),
                    //wrath,if=eclipse_dir=-1|(eclipse_dir=0&eclipse<=0)
                                        Spell.CastSpell("Wrath", ret => Common.eclipseDir() == -1 || (Common.eclipseDir() == 0 && Me.CurrentEclipse <= 0), "Wrath"))),
                                new Decorator(ret => Me.IsMoving,
                                    new PrioritySelector(
                    //moonfire,moving=1,if=!dot.sunfire.ticking
                                        Spell.CastSpell("Moonfire", ret => Me.IsMoving && !Buff.TargetHasDebuff("Sunfire"), "Moonfire"),
                    //sunfire,moving=1,if=!dot.moonfire.ticking
                                        Spell.CastSpell("Sunfire", ret => Me.IsMoving && !Buff.TargetHasDebuff("Moonfire"), "Sunfire"),
                    //U	0.00	wild_mushroom,moving=1,if=buff.wild_mushroom.stack<5
                    //starsurge,moving=1,if=buff.shooting_stars.react
                                        Spell.CastSpell("Starsurge", ret => Me.IsMoving && Buff.PlayerHasActiveBuff("Shooting Stars"), "Starsurge"),
                    //moonfire,moving=1,if=buff.lunar_eclipse.up
                                        Spell.CastSpell("Moonfire", ret => Me.IsMoving && Buff.PlayerHasBuff("Eclipse (Lunar)"), "Moonfire"),
                    //sunfire,moving=1
                                        Spell.CastSpell("Sunfire", ret => Me.IsMoving, "Sunfire")))
                        )),
                #endregion

                #region Bear Form
 new Decorator(ret => Buff.PlayerHasBuff("Bear Form"),
                            new PrioritySelector(
                        ))
                #endregion
));
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    Unit.EnsureTarget(),
                    Movement.CreateMoveToLosBehavior(),
                    Movement.CreateFaceTargetBehavior(),
                    Spell.WaitForCast(true),
		            Spell.CastSpell("Moonfire", ret => Buff.PlayerHasBuff("Eclipse (Lunar)"), "Moonfire"),
		            Spell.CastSpell("Sunfire", ret => true, "Sunfire"),
                    this.SingleRotation,
                    Movement.CreateMoveToTargetBehavior(true, 39f)
                    );
            }
        }

        public override Composite Medic
        {
            get
            {
                return new PrioritySelector(
                        Spell.CastSelfSpell("Innervate", ret => Me.ManaPercent < 50, "Innvervate"),
                        new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone"))));


            }
        }

        public override Composite PreCombat
        {
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                    //flask,type=warm_sun
                    //food,type=mogu_fish_stew
                    //mark_of_the_wild,if=!aura.str_agi_int.up
                            Buff.CastRaidBuff("Mark of the Wild", ret => true, "Mark of the Wild"),
                    //healing_touch,if=!buff.dream_of_cenarius_damage.up&talent.dream_of_cenarius.enabled
                            Spell.CastHeal("Healing Touch", ret => !Buff.PlayerHasActiveBuff(108381) && TalentManager.HasTalent(17), "Healing Touch"),
                    //moonkin_form
                            Spell.CastSelfSpell("Moonkin Form", ret => !Buff.PlayerHasBuff("Moonkin Form"), "Moonkin Form")
                    //jade_serpent_potion
                )));
            }
        }

        public override Composite Resting
        {
            get
            {
                return
                    new PrioritySelector(
                        Spell.CastSpell("Rejuvenation", ret => Me, ret => !Buff.PlayerHasBuff("Rejuvenation") && Me.HealthPercent < 75 && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.EnableMovement, "Rejuvenation on me"),
                        Base.Rest.CreateDefaultRestBehaviour());
            }
        }

        public override Composite PVPRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                    //new Action(a => { SysLog.Log("I am the start of public override Composite PVPRotation"); return RunStatus.Failure; }),
                        CrowdControl.freeMe(),
                        new Decorator(ret => Macro.Manual || BotChecker.BotBaseInUse("BGBuddy"),
                            new Decorator(ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                    Item.UseEngineerGloves(),
                                    new Action(delegate
                                    {
                                        Macro.isMultiCastMacroInUse();
                                        return RunStatus.Failure;
                                    }),
                                    new Decorator(ret => Macro.Burst, burstRotation),
                                    new Decorator(ret => !Macro.Burst || BotChecker.BotBaseInUse("BGBuddy"), baseRotation)))
                )));
            }
        }

        public override Composite PVERotation
        {
            get
            {
                return this.SingleRotation;
            }
        }
    }
}
