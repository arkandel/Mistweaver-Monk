using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Styx.CommonBot.Routines;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using Styx.CommonBot;
using CommonBehaviors.Actions;
using Styx.WoWInternals;
using Styx.Common;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace SuperMonk
{
    public class Mistweaver : CombatRoutine
    {
        private delegate bool SimpleBooleanDelegate(object context);
        private delegate WoWUnit UnitSelectionDelegate(object context);

        public override WoWClass Class
        {
            get { return WoWClass.Monk; }
        }

        public override string Name
        {
            get { return "SuperMonk - Mistweaver"; }
        }

        #region Overrides
        private Composite _healBehavior;
        public override Composite CombatBehavior
        {
            get { return _healBehavior ?? (_healBehavior = CreateHealBehavior()); }
        }
        public override Composite RestBehavior
        {
            get { return _healBehavior ?? (_healBehavior = CreateHealBehavior()); }
        }
        public override Composite HealBehavior
        {
            get { return _healBehavior ?? (_healBehavior = CreateHealBehavior()); }
        }
        public override Composite PullBehavior
        {
            get { return _healBehavior ?? (_healBehavior = CreateHealBehavior()); }
        }

        private Composite _combatBuffBehavior;
        public override Composite CombatBuffBehavior
        {
            get { return _combatBuffBehavior ?? (_combatBuffBehavior = CreateCombatBuffBehavior()); }
        }

        private Composite _preCombatBuffBehavior;
        public override Composite PreCombatBuffBehavior
        {
            get { return _preCombatBuffBehavior ?? (_preCombatBuffBehavior = CreatePreCombatBuffBehavior()); }
        }

        public override void Initialize()
        {
            Logging.Write("Loading settings...");
            MistweaverSettings.Instance.Load();
            Logging.Write("Settings loaded.");
        }

        public override void Pulse()
        {
            if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld)
                return;
            HealerManager.Instance.Pulse();
        }

        public override bool WantButton
        {
            get { return true; }
        }
        
        public override void OnButtonPress()
        {
            var form = new MistweaverForm();
            form.ShowDialog();
        }

        #endregion

        private Composite CreateHealBehavior()
        {
            return new PrioritySelector(
                // Choose best target
                ctx => ChooseBestHealTarget(HealerManager.Instance.FirstUnit),

                // Are we casting soothing mist on the wrong target?
                new Decorator(ret => ret != null && StyxWoW.Me.ChanneledCastingSpellId == SOOTHING_MIST && StyxWoW.Me.ChannelObject != null && StyxWoW.Me.ChannelObject.Guid != ((WoWObject)ret).Guid,
                    new Sequence(
                        new Styx.TreeSharp.Action(ret => SpellManager.StopCasting()),
                        new WaitContinue(TimeSpan.FromMilliseconds(250), ret => StyxWoW.Me.ChanneledCastingSpellId != SOOTHING_MIST, new ActionAlwaysSucceed())
                    )
                ),
                // Are we casting something other than soothing mist?
                new Decorator(ret => StyxWoW.Me.IsCasting && (StyxWoW.Me.ChanneledSpell == null || StyxWoW.Me.ChanneledSpell.Id != SOOTHING_MIST), new ActionAlwaysSucceed()),
                // Are we casting soothing mist but target is full health?
                new Decorator(ret => StyxWoW.Me.ChanneledCastingSpellId == SOOTHING_MIST && StyxWoW.Me.ChannelObject != null && StyxWoW.Me.ChannelObject.ToUnit().HealthPercent > MistweaverSettings.Instance.IgnorePercent,
                    new Sequence(
                        new Styx.TreeSharp.Action(ret => SpellManager.StopCasting()),
                        new WaitContinue(TimeSpan.FromMilliseconds(250), ret => StyxWoW.Me.ChanneledCastingSpellId != SOOTHING_MIST, new ActionAlwaysSucceed())
                    )
                ),

                // Don't lose mana
                SelfCast("Mana Tea", ret => StyxWoW.Me.ManaPercent < 85 && StyxWoW.Me.HasAura("Mana Tea", 2)),
                // Cast Renewing Mist, keep up the buff
                RenewingMist(),
                
                // Self Cast Expel Harm
                SelfCast("Expel Harm", ret => ((ret != null && ((WoWUnit)ret).Guid != StyxWoW.Me.Guid && ((WoWUnit)ret).HealthPercent > 50) || StyxWoW.Me.HealthPercent < 100) && StyxWoW.Me.CurrentChi < 4),

                // For Spinning Crane Kick
                Cast("Rushing Jade Wind", ret => MistweaverSettings.Instance.RushingJadeWind && !StyxWoW.Me.ActiveAuras.ContainsKey("Rushing Jade Wind") && MistweaverSettings.Instance.SpinningCraneKick && Unit.NearbyGroupMembersDistance(8f).Count(u => u.HealthPercent < MistweaverSettings.Instance.SpinningCraneKickPercent) >= MistweaverSettings.Instance.SpinningCraneKickCount && StyxWoW.Me.CurrentChi >= 2),

                // AoE, Monk's shouldn't be single target priority
                CastLikeMonk("Uplift", ret => StyxWoW.Me, ret => MistweaverSettings.Instance.Uplift && FetchRenewingMistTargets().Count(u => u.HealthPercent < MistweaverSettings.Instance.UpliftPercent) >= MistweaverSettings.Instance.UpliftCount && StyxWoW.Me.CurrentChi >= 2),
                
                // Chi Burst
                new PrioritySelector(ret => Clusters.GetBestUnitForCluster(GetChiBurstUnits(), ClusterType.Path, 10f),
                    new Decorator(ctx => MistweaverSettings.Instance.ChiBurst && Clusters.GetClusterCount((WoWUnit)ctx, GetChiBurstUnits(), ClusterType.Path, 10f) >= MistweaverSettings.Instance.ChiBurstCount && StyxWoW.Me.CurrentChi >= 2,
                        CastLikeMonk("Chi Burst", ret => (WoWUnit)ret, ret => true)
                    )
                ),
                                
                CastLikeMonk("Spinning Crane Kick", ret => StyxWoW.Me, ret => MistweaverSettings.Instance.SpinningCraneKick && Unit.NearbyGroupMembersDistance(8f).Count(u => u.HealthPercent < MistweaverSettings.Instance.SpinningCraneKickPercent) >= MistweaverSettings.Instance.SpinningCraneKickCount),
                
                // Single Target - only if heal list = 1 or cannot jab
                new Decorator(ret => !MistweaverSettings.Instance.Jab || HealerManager.Instance.HealList.Count(u => u.HealthPercent < MistweaverSettings.Instance.SoothingMistPercent) <= 1,
                    new PrioritySelector(
                        CastLikeMonk("Soothing Mist", ret => (WoWUnit)ret, ret => ret != null && MistweaverSettings.Instance.SoothingMist && ((WoWUnit)ret).HealthPercent < MistweaverSettings.Instance.SoothingMistPercent && StyxWoW.Me.ChanneledSpell == null),
                        CastLikeMonk("Enveloping Mist", ret => (WoWUnit)ret, ret => ret != null && MistweaverSettings.Instance.EnvelopingMist && ((WoWUnit)ret).HealthPercent < MistweaverSettings.Instance.EnvelopingMistPercent && StyxWoW.Me.CurrentChi >= 3 && !((WoWUnit)ret).HasMyAura("Enveloping Mist")),
                        CastLikeMonk("Surging Mist", ret => (WoWUnit)ret, ret => ret != null && MistweaverSettings.Instance.SurgingMist && ((WoWUnit)ret).HealthPercent < MistweaverSettings.Instance.SurgingMistPercent)
                    )
                ),

                // Melee                
                Cast("Jab", ret => MistweaverSettings.Instance.Jab && StyxWoW.Me.CurrentChi < MistweaverSettings.Instance.JabCount && StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.CurrentTarget.Attackable),
                Cast("Blackout Kick", ret => MistweaverSettings.Instance.BlackoutKick && StyxWoW.Me.CurrentChi >= 2 && (BuffTimeRemaining("Serpent's Zeal").TotalSeconds <= 5 || !HasBuffStacks("Serpent's Zeal", 2)) && StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.CurrentTarget.Attackable),
                Cast("Tiger Palm", ret => MistweaverSettings.Instance.TigerPalm && StyxWoW.Me.CurrentChi >= 1 && StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.CurrentTarget.Attackable && (BuffTimeRemaining("Tiger Power").TotalSeconds <= 5 || !HasBuffStacks("Tiger Power", MistweaverSettings.Instance.TigerPalmCount)))
            );
        }

        private Composite CreateCombatBuffBehavior()
        {
            return new PrioritySelector();
        }

        private Composite CreatePreCombatBuffBehavior()
        {
            return new PrioritySelector();
        }

        private TimeSpan BuffTimeRemaining(string buff)
        {
            return StyxWoW.Me.ActiveAuras.ContainsKey(buff) ? StyxWoW.Me.ActiveAuras[buff].TimeLeft : TimeSpan.Zero;
        }

        private bool HasBuffStacks(string buff, int stacks)
        {
            if (StyxWoW.Me.ActiveAuras.ContainsKey(buff))
            {
                return StyxWoW.Me.ActiveAuras[buff].StackCount >= stacks;
            }
            return false;
        }

        #region Cast
        private Composite Cast(string spell)
        {
            return Cast(spell, requirements => true);
        }

        private Composite Cast(string spell, SimpleBooleanDelegate requirements)
        {
            return Cast(spell, requirements, unit => StyxWoW.Me.CurrentTarget);
        }

        private Composite Cast(string spell, UnitSelectionDelegate unit)
        {
            return Cast(spell, requirements => true, unit);
        }

        private Composite SelfCast(string spell)
        {
            return SelfCast(spell, requirements => true);
        }

        private Composite SelfCast(string spell, SimpleBooleanDelegate requirements)
        {
            return Cast(spell, requirements, unit => StyxWoW.Me);
        }

        private Color MONK_COLOR = Color.FromRgb(0, 255, 150);
        private Composite Cast(string spell, SimpleBooleanDelegate requirements, UnitSelectionDelegate unit)
        {
            return new Decorator(ret => requirements != null && requirements(ret) && SpellManager.HasSpell(spell) && SpellManager.CanCast(spell, unit(ret), true, true),
                new Styx.TreeSharp.Action(ctx =>
                {
                    SpellManager.Cast(spell, unit(ctx));
                    Logging.Write(LogLevel.Normal, MONK_COLOR, "Casting Spell [{0}].", spell);
                    return RunStatus.Success;
                })
            );
        }

        private bool CanCastLikeMonk(string spellName, WoWUnit unit)
        {
            WoWSpell spell;
            if (!SpellManager.Spells.TryGetValue(spellName, out spell))
            {
                return false;
            }

            uint latency = StyxWoW.WoWClient.Latency * 2;
            TimeSpan cooldownLeft = spell.CooldownTimeLeft;
            if (cooldownLeft != TimeSpan.Zero && cooldownLeft.TotalMilliseconds >= latency)
                return false;

            if (spell.IsMeleeSpell)
            {
                if (!unit.IsWithinMeleeRange)
                {
                    Logging.Write(LogLevel.Diagnostic, MONK_COLOR, "CanCastSpell: cannot cast wowSpell {0} @ {1:F1} yds", spell.Name, unit.Distance);
                    return false;
                }
            }
            else if (spell.IsSelfOnlySpell)
            {
                ;
            }
            else if (spell.HasRange)
            {
                if (unit == null)
                {
                    return false;
                }

                if (unit.Distance < spell.MinRange)
                {
                    Logging.Write(LogLevel.Diagnostic, MONK_COLOR, "SpellCast: cannot cast wowSpell {0} @ {1:F1} yds - minimum range is {2:F1}", spell.Name, unit.Distance, spell.MinRange);
                    return false;
                }

                if (unit.Distance >= spell.MaxRange)
                {
                    Logging.Write(LogLevel.Diagnostic, MONK_COLOR, "SpellCast: cannot cast wowSpell {0} @ {1:F1} yds - maximum range is {2:F1}", spell.Name, unit.Distance, spell.MaxRange);
                    return false;
                }
            }

            if (StyxWoW.Me.CurrentPower < spell.PowerCost)
            {
                Logging.Write(LogLevel.Diagnostic, MONK_COLOR, "CanCastSpell: wowSpell {0} requires {1} power but only {2} available", spell.Name, spell.PowerCost, StyxWoW.Me.CurrentMana);
                return false;
            }

            if (StyxWoW.Me.IsMoving && spell.CastTime > 0)
            {
                Logging.Write(LogLevel.Diagnostic, MONK_COLOR, "CanCastSpell: wowSpell {0} is not instant ({1} ms cast time) and we are moving", spell.Name, spell.CastTime);
                return false;
            }

            return true;
        }

        private Composite CastLikeMonk(string spell, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements, bool face = false)
        {
            return new Decorator(
                ret => requirements != null && onUnit != null && requirements(ret) && onUnit(ret) != null && spell != null && CanCastLikeMonk(spell, onUnit(ret)),
                new Sequence(
                    new Styx.TreeSharp.Action(ctx =>
                        {
                            Logging.Write(LogLevel.Normal, MONK_COLOR, "{0} on {1} at {2:F1} yds at {3:F1}%", spell, onUnit(ctx).Name, onUnit(ctx).Distance, onUnit(ctx).HealthPercent);
                            if (face)
                                onUnit(ctx).Face();
                            SpellManager.Cast(spell, onUnit(ctx));
                        }
                    ),
                    new WaitContinue(TimeSpan.FromMilliseconds((int)StyxWoW.WoWClient.Latency << 1),
                        ctx => !(SpellManager.GlobalCooldown || StyxWoW.Me.IsCasting || StyxWoW.Me.ChanneledSpell != null),
                        new ActionAlwaysSucceed()
                    ),
                    new WaitContinue(TimeSpan.FromMilliseconds((int)StyxWoW.WoWClient.Latency << 1),
                        ctx => SpellManager.GlobalCooldown || StyxWoW.Me.IsCasting || StyxWoW.Me.ChanneledSpell != null,
                        new ActionAlwaysSucceed()
                    )
                )
            );
        }

        private Composite RenewingMist()
        {
            return new PrioritySelector(
                ctx => ChooseBestRenewingMistTarget(),
                new Decorator(ret => ret != null && MistweaverSettings.Instance.RenewingMist && ((WoWUnit)ret).HealthPercent <= MistweaverSettings.Instance.RenewingMistPercent,
                    CastLikeMonk("Renewing Mist", ctx => (WoWUnit)ctx, ret => true)
                ),
                new Decorator(ret => ret == null && MistweaverSettings.Instance.RenewingMist && StyxWoW.Me.HealthPercent <= MistweaverSettings.Instance.RenewingMistPercent,
                    CastLikeMonk("Renewing Mist", ctx => StyxWoW.Me, ret => true)
                )
            );
        }
        #endregion

        #region Heal Logic
        private const int RENEWING_MIST_BUFF = 119611;
        private IEnumerable<WoWUnit> FetchRenewingMistTargets()
        {
            return Unit.GroupMembers.Where(u =>
                !u.IsDead && !u.IsGhost &&
                u.HasMyAura("Renewing Mist")
            );
        }

        private const int SOOTHING_MIST = 115175;
        private static WoWUnit ChooseBestHealTarget(WoWUnit unit)
        {
            if (StyxWoW.Me.ChanneledCastingSpellId == SOOTHING_MIST && StyxWoW.Me.ChannelObject != null)
            {
                WoWUnit channelUnit = StyxWoW.Me.ChannelObject.ToUnit();
                if (channelUnit.HealthPercent < MistweaverSettings.Instance.SoothingMistPercent)
                    return channelUnit;
            }

            if (unit != null && unit.HealthPercent >= MistweaverSettings.Instance.IgnorePercent)
                unit = null;
            return unit;
        }

        private WoWUnit ChooseBestRenewingMistTarget()
        {
            if (!MistweaverSettings.Instance.RenewingMist)
                return null;
            var renewing_mist_percent = MistweaverSettings.Instance.RenewingMistPercent;
            return Unit.NearbyGroupMembers.Where(u =>
                u != null &&
                !u.IsDead && !u.IsGhost &&
                u.HealthPercent <= renewing_mist_percent).OrderBy(u => !u.HasMyAura("Renewing Mist")).ThenBy(u => u.HealthPercent).FirstOrDefault();
        }

        private IEnumerable<WoWUnit> GetChiBurstUnits()
        {
            var chi_burst_percent = MistweaverSettings.Instance.ChiBurstPercent;
            return Unit.NearbyGroupMembers.Where(u =>
                u != null &&
                !u.IsDead && !u.IsGhost &&
                u.HealthPercent <= chi_burst_percent);
        }
        #endregion
    }
}
