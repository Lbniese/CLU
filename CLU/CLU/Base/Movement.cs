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

#endregion Revision info

using System.Linq;
using CLU.Helpers;

namespace CLU.Base
{
    using System;
    using CommonBehaviors.Actions;
    using global::CLU.Settings;
    using Styx;
    using Styx.CommonBot;
    using Styx.Helpers;
    using Styx.Pathing;
    using Styx.TreeSharp;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using Action = Styx.TreeSharp.Action;

    public static class Movement
    {
        /* putting all the Movement logic here */

        public delegate WoWPoint LocationRetriever(object context);

        public delegate float DynamicRangeRetriever(object context);

        private const bool EnableBotTargetingOveride = false;

        private static bool IsFlyingUnit
        {
            get
            {
                return Me.CurrentTarget != null &&
                       (Me.CurrentTarget.IsFlying ||
                        Me.CurrentTarget.Distance2DSqr < 5 * 5 &&
                        Math.Abs(Me.Z - Me.CurrentTarget.Z) >= 5);
            }
        }

        private static LocalPlayer Me
        {
            get { return StyxWoW.Me; }
        }

        public static Composite CreateFaceTargetBehavior(float viewDegrees = 70f)
        {
            return CreateFaceTargetBehavior(ret => Me.CurrentTarget);
        }

        public static Composite CreateFaceTargetBehavior(CLU.UnitSelection toUnit, float viewDegrees = 70f)
        {
            return new Decorator(
                ret =>
                CLUSettings.Instance.EnableMovement && toUnit != null && toUnit(ret) != null &&
                !Me.IsMoving && !toUnit(ret).IsMe &&
                !Me.IsSafelyFacing(toUnit(ret), viewDegrees),
                new Action(ret =>
                {
                    Me.CurrentTarget.Face();
                    return RunStatus.Failure;
                }));
        }

        public static Composite CreateMoveToLosBehavior()
        {
            return CreateMoveToLosBehavior(ret => Me.CurrentTarget);
        }

        public static Composite CreateMoveToLosBehavior(CLU.UnitSelection toUnit)
        {
            return new Decorator(ret => CLUSettings.Instance.EnableMovement && toUnit != null && toUnit(ret) != null && toUnit(ret) != Me && !toUnit(ret).InLineOfSpellSight,
                new Action(ret => Navigator.MoveTo(toUnit(ret).Location)));
        }

        /// <summary>
        /// Movement Behaviour
        /// </summary>
        public static Composite MovingFacingBehavior()
        {
            return MovingFacingBehavior(ret => Me.CurrentTarget);
        }

        private static Composite MovingFacingBehavior(CLU.UnitSelection onUnit)

        // TODO: Check if we have an obstacle in our way and clear it ALSO need a mounted CHECK!!.
        {
            var badStuff = ObjectManager.GetObjectsOfType<WoWUnit>(false, false).Where(q => q.CreatedByUnitGuid != Me.Guid && q.FactionId == 14 && !q.Attackable && q.Distance < 8).OrderBy(u => u.DistanceSqr).FirstOrDefault();
            return new Sequence(

                // No Target?
                // Targeting Enabled?
                // Aquire Target
                       new DecoratorContinue(ret => (onUnit == null || onUnit(ret) == null || onUnit(ret).IsDead || onUnit(ret).IsFriendly) && CLUSettings.Instance.EnableTargeting,
                                             new PrioritySelector(
                                                ctx =>
                                                {
                                                    // Clear our current target if its Dead or is Friendly.
                                                    if (ctx != null && (onUnit(ctx).IsDead || onUnit(ctx).IsFriendly))
                                                    {
                                                        CLULogger.TroubleshootLog(" Target Appears to be dead or a Friendly. Clearing Current Target [" + CLULogger.SafeName((WoWUnit)ctx) + "]");
                                                        Me.ClearTarget();
                                                    }

                                                    // Aquires a target.
                                                    if (Unit.EnsureUnitTargeted != null)
                                                    {
                                                        // Clear our current target if its blacklisted.
                                                        if (Blacklist.Contains(Unit.EnsureUnitTargeted.Guid) || Unit.EnsureUnitTargeted.IsDead)
                                                        {
                                                            CLULogger.TroubleshootLog(" EnsureUnitTargeted Appears to be dead or Blacklisted. Clearing Current Target [" + CLULogger.SafeName(Unit.EnsureUnitTargeted) + "]");
                                                            Me.ClearTarget();
                                                        }

                                                        return Unit.EnsureUnitTargeted;
                                                    }

                                                    return null;
                                                },
                                                new Decorator(
                                                    ret => ret != null, //checks that the above ctx returned a valid target.
                                                    new Sequence(

                //new Action(ret => SysLog.DiagnosticLog(" CLU targeting activated. Targeting " + SysLog.SafeName((WoWUnit)ret))),
                // pending spells like mage blizard cause targeting to fail.
                //new DecoratorContinue(ctx => StyxWoW.Me.CurrentPendingCursorSpell != null,
                //        new Action(ctx => Lua.DoString("SpellStopTargeting()"))),
                                                        new Action(ret => ((WoWUnit)ret).Target()),
                                                        new WaitContinue(2, ret => onUnit(ret) != null && onUnit(ret) == (WoWUnit)ret, new ActionAlwaysSucceed()))))),

                        // Are we Facing the target?
                // Is the Target in line of site?
                // Face Target
                       new DecoratorContinue(ret => onUnit(ret) != null && !Me.IsSafelyFacing(onUnit(ret), 45f) && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => WoWMovement.Face(onUnit(ret).Guid)))),

                        // Target in Line of site?
                // We are not casting?
                // We are not channeling?
                // Move to Location
                       new DecoratorContinue(ret => onUnit(ret) != null && !onUnit(ret).InLineOfSight && !Me.IsCasting && !Spell.PlayerIsChanneling,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Target not in LoS. Moving closer.")),
                                                 new Action(ret => Navigator.MoveTo(onUnit(ret).Location)))),

                       // Blacklist targets TODO:  check this.
                       new DecoratorContinue(ret => onUnit(ret) != null && !Me.IsInInstance && Navigator.GeneratePath(Me.Location, onUnit(ret).Location).Length <= 0,
                                             new Action(delegate
                                                            {
                                                                Blacklist.Add(Me.CurrentTargetGuid, TimeSpan.FromDays(365));
                                                                CLULogger.MovementLog("[CLU] " + CLU.Version + ": Failed to generate path to: {0} blacklisted!", Me.CurrentTarget.Name);
                                                                return RunStatus.Success;
                                                            })),

                // Move away from bad stuff
                       new DecoratorContinue(ret => badStuff != null,
                                            new Sequence(
                                                new Action(ret => CLULogger.MovementLog("[CLU Movement] Movin out of bad Stuff.")),
                                                new Action(ret =>
                                                               {
                                                                   if (badStuff != null)
                                                                   {
                                                                       CLULogger.MovementLog("[CLU Movement] Movin out of {0}.", badStuff);
                                                                       Navigator.MoveTo(WoWMovement.CalculatePointFrom(badStuff.Location, 10));
                                                                   }
                                                               }))),

                // Move Behind Target enabled?
                // Target is Alive?
                // Target is not Moving?
                // Are we behind the target?
                // We are not casting?
                // We are not the tank?
                // We are not channeling?
                // Move Behind Target
                       new DecoratorContinue(ret => CLUSettings.Instance.EnableMoveBehindTarget && onUnit(ret) != null && onUnit(ret) != Me && // && !onUnit(ret).IsMoving
                                             onUnit(ret).InLineOfSight && onUnit(ret).IsAlive && !onUnit(ret).MeIsBehind && !Me.IsCasting && !Spell.PlayerIsChanneling &&
                                             Unit.DistanceToTargetBoundingBox() >= 10,// && (Unit.Tanks != null && Unit.Tanks.Any(x => x.Guid != Me.Guid))
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Not behind the target. Moving behind target.")),
                                                 new Action(ret => Navigator.MoveTo(CalculatePointBehindTarget())))),

                        // Target is greater than CombatMinDistance?
                // Target is Moving?
                // We are not moving Forward?
                // Move Forward to   wards target
                       new DecoratorContinue(ret => onUnit(ret) != null && Unit.DistanceToTargetBoundingBox() >= CLU.Instance.ActiveRotation.CombatMinDistance &&
                                             onUnit(ret).IsMoving && !Me.MovementInfo.MovingForward && onUnit(ret).InLineOfSight && !IsFlyingUnit,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Too far away from moving target (T[{0}] >= P[{1}]). Moving forward.", Unit.DistanceToTargetBoundingBox(), CLU.Instance.ActiveRotation.CombatMinDistance)),
                                                 new Action(ret => WoWMovement.Move(WoWMovement.MovementDirection.Forward)))),

                        // Target is less than CombatMinDistance?
                // Target is Moving?
                // We are moving Forward
                // Stop Moving Forward
                       new DecoratorContinue(ret => onUnit(ret) != null && Unit.DistanceToTargetBoundingBox() < CLU.Instance.ActiveRotation.CombatMinDistance &&
                                             onUnit(ret).IsMoving && Me.MovementInfo.MovingForward && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Too close to target (T[{0}] < P[{1}]). Movement Stopped.", Unit.DistanceToTargetBoundingBox(), CLU.Instance.ActiveRotation.CombatMinDistance)),
                                                 new Action(ret => WoWMovement.MoveStop()))),

                        // Target is not Moving?
                // Target is greater than CombatMaxDistance?
                // We are not Moving?
                // Move Forward
                       new DecoratorContinue(ret => onUnit(ret) != null && !onUnit(ret).IsMoving &&
                                             Unit.DistanceToTargetBoundingBox() >= CLU.Instance.ActiveRotation.CombatMaxDistance && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Too far away from non moving target (T[{0}] >= P[{1}]). Moving forward.", Unit.DistanceToTargetBoundingBox(), CLU.Instance.ActiveRotation.CombatMaxDistance)),
                                                 new Action(ret => WoWMovement.Move(WoWMovement.MovementDirection.Forward, new TimeSpan(99, 99, 99))))),

                        // Target is less than CombatMaxDistance?
                // We are Moving?
                // We are moving Forward?
                // Stop Moving
                       new DecoratorContinue(ret => onUnit(ret) != null && Unit.DistanceToTargetBoundingBox() < CLU.Instance.ActiveRotation.CombatMaxDistance &&
                                             Me.IsMoving && Me.MovementInfo.MovingForward && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Too close to target  (T[{0}] < P[{1}]). Movement Stopped", Unit.DistanceToTargetBoundingBox(), CLU.Instance.ActiveRotation.CombatMaxDistance)),
                                                 new Action(ret => WoWMovement.MoveStop()))));
        }

        /// <summary>
        /// Calculates the point to move behind the targets location
        /// </summary>
        /// <returns>the WoWpoint location to move to</returns>
        private static WoWPoint CalculatePointBehindTarget()
        {
            return
                Me.CurrentTarget.Location.RayCast(
                    Me.CurrentTarget.Rotation + WoWMathHelper.DegreesToRadians(150), Spell.MeleeRange - 2f);
        }

        /// <summary>
        /// Will stop the player if we are moving.
        /// </summary>
        /// <returns>Runstatus.Success if stopped</returns>
        public static Composite EnsureMovementStoppedBehavior()
        {
            return new Decorator(
                ret => CLUSettings.Instance.EnableMovement && Me.IsMoving,
                new Action(ret => Navigator.PlayerMover.MoveStop()));
        }

        /// <summary>
        ///   Creates a move to melee range behavior. Will return RunStatus.Success if it has reached the location, or stopped in range. Best used at the end of a rotation.
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <param name = "stopInRange">true to stop in range.</param>
        /// <param name = "range">The range.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveToMeleeBehavior(bool stopInRange)
        {
            return CreateMoveToMeleeBehavior(ret => Me.CurrentTarget.Location, stopInRange);
        }

        public static Composite CreateMoveToMeleeBehavior(LocationRetriever location, bool stopInRange)
        {
            return
                new Decorator(
                    ret => !Me.IsCasting,
                    CreateMoveToLocationBehavior(location, stopInRange, ret => Me.CurrentTarget.IsPlayer ? 2f : Spell.MeleeRange));
        }

        /// <summary>
        ///   Creates a move to location behavior. Will return RunStatus.Success if it has reached the location, or stopped in range. Best used at the end of a rotation.
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <param name = "location">The location.</param>
        /// <param name = "stopInRange">true to stop in range.</param>
        /// <param name = "range">The range.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveToLocationBehavior(LocationRetriever location, bool stopInRange, DynamicRangeRetriever range)
        {
            // Do not fuck with this. It will ensure we stop in range if we're supposed to.
            // Otherwise it'll stick to the targets ass like flies on dog shit.
            // Specifying a range of, 2 or so, will ensure we're constantly running to the target. Specifying 0 will cause us to spin in circles around the target
            // or chase it down like mad. (PVP oriented behavior)
            return
                new Decorator(

                // Don't run if the movement is disabled.
                    ret => CLUSettings.Instance.EnableMovement,
                    new PrioritySelector(
                        new Decorator(

                // Give it a little more than 1/2 a yard buffer to get it right. CTM is never 'exact' on where we land. So don't expect it to be.
                            ret => stopInRange && Me.Location.Distance(location(ret)) < range(ret),
                            new PrioritySelector(
                                EnsureMovementStoppedBehavior(),

                // In short; if we're not moving, just 'succeed' here, so we break the tree.
                                new Action(ret => RunStatus.Success)
                                )
                            ),
                        new Action(ret => Navigator.MoveTo(location(ret)))
                        ));
        }

        /// <summary>
        ///   Creates a move to target behavior. Will return RunStatus.Success if it has reached the location, or stopped in range. Best used at the end of a rotation.
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <param name = "stopInRange">true to stop in range.</param>
        /// <param name = "range">The range.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveToTargetBehavior(bool stopInRange, float range)
        {
            return CreateMoveToTargetBehavior(stopInRange, range, ret => StyxWoW.Me.CurrentTarget);
        }

        /// <summary>
        ///   Creates a move to target behavior. Will return RunStatus.Success if it has reached the location, or stopped in range. Best used at the end of a rotation.
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <param name = "stopInRange">true to stop in range.</param>
        /// <param name = "range">The range.</param>
        /// <param name="onUnit">The unit to move to.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveToTargetBehavior(bool stopInRange, float range, CLU.UnitSelection onUnit)
        {
            return
                new Decorator(ret => onUnit != null && onUnit(ret) != null && onUnit(ret) != StyxWoW.Me && !StyxWoW.Me.IsCasting,
                    CreateMoveToLocationBehavior(ret => onUnit(ret).Location, stopInRange, ret => range));
        }
    }
}