using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Styx;
using Styx.Helpers;

namespace SuperMonk
{
    public class MistweaverSettings : Settings
    {
        public static readonly MistweaverSettings Instance = new MistweaverSettings();
        private MistweaverSettings()
            : base(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(@"Routines/Super Monk/Mistweaver-{0}.xml", StyxWoW.Me.Name)))
        {
        }

        [Setting, DefaultValue(true)]
        public bool RenewingMist { get; set; }

        [Setting, DefaultValue(100)]
        public int RenewingMistPercent { get; set; }

        [Setting, DefaultValue(true)]
        public bool SoothingMist { get; set; }

        [Setting, DefaultValue(55)]
        public int SoothingMistPercent { get; set; }

        [Setting, DefaultValue(true)]
        public bool SurgingMist { get; set; }

        [Setting, DefaultValue(35)]
        public int SurgingMistPercent { get; set; }

        [Setting, DefaultValue(true)]
        public bool EnvelopingMist { get; set; }

        [Setting, DefaultValue(45)]
        public int EnvelopingMistPercent { get; set; }

        [Setting, DefaultValue(true)]
        public bool Jab { get; set; }

        [Setting, DefaultValue(2)]
        public int JabCount { get; set; }

        [Setting, DefaultValue(true)]
        public bool BlackoutKick { get; set; }

        [Setting, DefaultValue(true)]
        public bool Uplift { get; set; }

        [Setting, DefaultValue(95)]
        public int UpliftPercent { get; set; }

        [Setting, DefaultValue(5)]
        public int UpliftCount { get; set; }

        [Setting, DefaultValue(true)]
        public bool SpinningCraneKick { get; set; }

        [Setting, DefaultValue(85)]
        public int SpinningCraneKickPercent { get; set; }

        [Setting, DefaultValue(7)]
        public int SpinningCraneKickCount { get; set; }

        [Setting, DefaultValue(true)]
        public bool RushingJadeWind { get; set; }

        [Setting, DefaultValue(true)]
        public bool ChiBurst { get; set; }

        [Setting, DefaultValue(90)]
        public int ChiBurstPercent { get; set; }

        [Setting, DefaultValue(3)]
        public int ChiBurstCount { get; set; }

        [Setting, DefaultValue(true)]
        public bool TigerPalm { get; set; }

        [Setting, DefaultValue(3)]
        public int TigerPalmCount { get; set; }

        [Setting, DefaultValue(95)]
        public int IgnorePercent { get; set; }
    }
}
