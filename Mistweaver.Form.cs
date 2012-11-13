using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperMonk
{
    public partial class MistweaverForm : Form
    {
        public MistweaverForm()
        {
            InitializeComponent();
        }

        private void MistweaverForm_Load(object sender, EventArgs e)
        {
            MistweaverSettings.Instance.Load();

            chk_RenewingMist.Checked = MistweaverSettings.Instance.RenewingMist;
            nud_RenewingMist.Value = MistweaverSettings.Instance.RenewingMistPercent;

            chk_SoothingMist.Checked = MistweaverSettings.Instance.SoothingMist;
            nud_SoothingMist.Value = MistweaverSettings.Instance.SoothingMistPercent;

            chk_SurgingMist.Checked = MistweaverSettings.Instance.SurgingMist;
            nud_SurgingMist.Value = MistweaverSettings.Instance.SurgingMistPercent;

            chk_EnvelopingMist.Checked = MistweaverSettings.Instance.EnvelopingMist;
            nud_EnvelopingMist.Value = MistweaverSettings.Instance.EnvelopingMistPercent;

            chk_Uplift.Checked = MistweaverSettings.Instance.Uplift;
            nud_UpliftCount.Value = MistweaverSettings.Instance.UpliftCount;
            nud_UpliftPercent.Value = MistweaverSettings.Instance.UpliftPercent;

            chk_SpinningCraneKick.Checked = MistweaverSettings.Instance.SpinningCraneKick;
            nud_SpinningCraneKickCount.Value = MistweaverSettings.Instance.SpinningCraneKickCount;
            nud_SpinningCraneKickPercent.Value = MistweaverSettings.Instance.SpinningCraneKickPercent;

            chk_RushingJadeWind.Checked = MistweaverSettings.Instance.RushingJadeWind;

            chk_ChiBurst.Checked = MistweaverSettings.Instance.ChiBurst;
            nud_ChiBurstCount.Value = MistweaverSettings.Instance.ChiBurstCount;
            nud_ChiBurstPercent.Value = MistweaverSettings.Instance.ChiBurstPercent;

            chk_Jab.Checked = MistweaverSettings.Instance.Jab;
            nud_JabCount.Value = MistweaverSettings.Instance.JabCount;

            chk_TigerPalm.Checked = MistweaverSettings.Instance.TigerPalm;
            nud_TigerPalmCount.Value = MistweaverSettings.Instance.TigerPalmCount;

            chk_BlackoutKick.Checked = MistweaverSettings.Instance.BlackoutKick;

            nud_IgnorePercent.Value = MistweaverSettings.Instance.IgnorePercent;
        }

        private void MistweaverForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MistweaverSettings.Instance.RenewingMist = chk_RenewingMist.Checked;
            MistweaverSettings.Instance.RenewingMistPercent = (int)nud_RenewingMist.Value;

            MistweaverSettings.Instance.SoothingMist = chk_SoothingMist.Checked;
            MistweaverSettings.Instance.SoothingMistPercent = (int)nud_SoothingMist.Value;

            MistweaverSettings.Instance.SurgingMist = chk_SurgingMist.Checked;
            MistweaverSettings.Instance.SurgingMistPercent = (int)nud_SurgingMist.Value;

            MistweaverSettings.Instance.EnvelopingMist = chk_EnvelopingMist.Checked;
            MistweaverSettings.Instance.EnvelopingMistPercent = (int)nud_EnvelopingMist.Value;

            MistweaverSettings.Instance.Uplift = chk_Uplift.Checked;
            MistweaverSettings.Instance.UpliftCount = (int)nud_UpliftCount.Value;
            MistweaverSettings.Instance.UpliftPercent = (int)nud_UpliftPercent.Value;

            MistweaverSettings.Instance.SpinningCraneKick = chk_SpinningCraneKick.Checked;
            MistweaverSettings.Instance.SpinningCraneKickCount = (int)nud_SpinningCraneKickCount.Value;
            MistweaverSettings.Instance.SpinningCraneKickPercent = (int)nud_SpinningCraneKickPercent.Value;

            MistweaverSettings.Instance.RushingJadeWind = chk_RushingJadeWind.Checked;

            MistweaverSettings.Instance.ChiBurst = chk_ChiBurst.Checked;
            MistweaverSettings.Instance.ChiBurstCount = (int)nud_ChiBurstCount.Value;
            MistweaverSettings.Instance.ChiBurstPercent = (int)nud_ChiBurstPercent.Value;

            MistweaverSettings.Instance.Jab = chk_Jab.Checked;
            MistweaverSettings.Instance.JabCount = (int)nud_JabCount.Value;

            MistweaverSettings.Instance.TigerPalm = chk_TigerPalm.Checked;
            MistweaverSettings.Instance.TigerPalmCount = (int)nud_TigerPalmCount.Value;

            MistweaverSettings.Instance.BlackoutKick = chk_BlackoutKick.Checked;

            MistweaverSettings.Instance.Save();
        }
    }
}
