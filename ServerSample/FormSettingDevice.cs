using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ServerSample
{
    public partial class FormSettingDevice : Form
    {
        private AppData _appData;

        public FormSettingDevice(AppData appdata)
        {
            InitializeComponent();
            _appData = appdata;
            gridResponse.AutoGenerateColumns = false;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            using (FormCreate f = new FormCreate())
            {
                f.StartPosition = FormStartPosition.CenterParent;
                if (f.ShowDialog() == DialogResult.OK)
                {
                    _appData.Devices.Add(new Device() { Name = f.DeviceName });
                }
            }
        }

        private void cmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cmb = (ComboBox)sender;
            var dev = cmb.SelectedItem as Device;
            if (dev != null)
            {
                gridResponse.DataSource = new BindingList<ResponseCommand>(dev.Commands);
            }
        }

        private void FormSettingDevice_Shown(object sender, EventArgs e)
        {
            cmbDevice.DisplayMember = "Name";
            cmbDevice.DataSource = new BindingList<Device>(_appData.Devices);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _appData.Write();
            this.DialogResult = DialogResult.OK;
        }
    }
}
