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
        private BindingList<ResponseCommand> _bindingCommands = new BindingList<ResponseCommand>();
        private BindingList<Device> _devices;

        public FormSettingDevice()
        {
            InitializeComponent();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            using (FormCreate f = new FormCreate())
            {
                f.StartPosition = FormStartPosition.CenterParent;
                if (f.ShowDialog() == DialogResult.OK)
                {
                    _devices.Add(new Device() { Name = f.DeviceName });
                    FileUtils.WriteToJsonFile("data.json", _devices, true);
                }
            }
        }

        private void cmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cmb = (ComboBox)sender;
            var dev = cmb.SelectedItem as Device;
            if (dev != null)
            {
                _bindingCommands = new BindingList<ResponseCommand>(dev.Commands);
            }
        }

        private void FormSettingDevice_Shown(object sender, EventArgs e)
        {
            var list = FileUtils.ReadFromJsonFile<List<Device>>("data.json");
            _devices = new BindingList<Device>(list);

            cmbDevice.DisplayMember = "Name";
            cmbDevice.DataSource = _devices;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            FileUtils.WriteToJsonFile("data.json", _devices, true);
            this.DialogResult = DialogResult.OK;
        }
    }
}
