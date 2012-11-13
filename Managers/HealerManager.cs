using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Styx;
using Styx.Common.Helpers;
using Styx.CommonBot;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace SuperMonk
{
    internal class HealerManager : Targeting
    {
        private static readonly WaitTimer _tankReset = WaitTimer.ThirtySeconds;

        private static ulong _tankGuid;

        static HealerManager()
        {
            Instance = new HealerManager();
        }

        public new static HealerManager Instance { get; private set; }

        public List<WoWPlayer> HealList { get { return ObjectList.ConvertAll(o => o.ToPlayer()); } }

        protected override List<WoWObject> GetInitialObjectList()
        {
            return ObjectManager.ObjectList.Where(o => o is WoWPlayer).ToList();
        }

        protected override void DefaultIncludeTargetsFilter(List<WoWObject> incomingUnits, HashSet<WoWObject> outgoingUnits)
        {
            foreach (WoWObject incomingUnit in incomingUnits)
            {
                outgoingUnits.Add(incomingUnit);
            }
        }

        protected override void DefaultRemoveTargetsFilter(List<WoWObject> units)
        {
            bool isHorde = StyxWoW.Me.IsHorde;
            var mistweaver_settings_ignore_percent = MistweaverSettings.Instance.IgnorePercent;
            for (int i = units.Count - 1; i >= 0; i--)
            {
                WoWObject o = units[i];
                if (!(o is WoWPlayer))
                {
                    units.RemoveAt(i);
                    continue;
                }

                WoWPlayer p = o.ToPlayer();

                if (p.IsDead || p.IsGhost)
                {
                    units.RemoveAt(i);
                    continue;
                }

                if (p.IsHostile)
                {
                    units.RemoveAt(i);
                    continue;
                }

                if (!p.IsInMyPartyOrRaid)
                {
                    units.RemoveAt(i);
                    continue;
                }

                if (p.HealthPercent > mistweaver_settings_ignore_percent)
                {
                    units.RemoveAt(i);
                    continue;
                }

                if (p.DistanceSqr > 40 * 40)
                {
                    units.RemoveAt(i);
                    continue;
                }
            }

            if (!units.Any(o => o.IsMe) && StyxWoW.Me.HealthPercent < mistweaver_settings_ignore_percent)
            {
                units.Add(StyxWoW.Me);
            }
        }

        protected override void DefaultTargetWeight(List<TargetPriority> units)
        {
            var tanks = GetMainTankGuids();
            var mistweaver_settings_ignore_percent = MistweaverSettings.Instance.IgnorePercent;
            foreach (TargetPriority prio in units)
            {
                prio.Score = 500f;
                WoWPlayer p = prio.Object.ToPlayer();
                prio.Score -= p.HealthPercent * 5;
                if (!p.InLineOfSpellSight)
                {
                    prio.Score -= 100f;
                }
                if (p.HasMyAura("Enveloping Mist"))
                {
                    prio.Score -= 50f;
                }
                if (tanks.Contains(p.Guid) && p.HealthPercent < mistweaver_settings_ignore_percent)
                {
                    prio.Score += 100f;
                }
            }
        }

        private static HashSet<ulong> GetMainTankGuids()
        {
            var infos = StyxWoW.Me.GroupInfo.RaidMembers;
            return new HashSet<ulong>(
                from pi in infos
                where (pi.Role & WoWPartyMember.GroupRole.Tank) != 0
                select pi.Guid);
        }
    }
}
