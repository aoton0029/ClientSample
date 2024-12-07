using System.ComponentModel;

namespace ServerSample
{
    public partial class Form1 : Form
    {
        private AppData _appData = new AppData();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {

        }

        private void btnSettingDevice_Click(object sender, EventArgs e)
        {
            using (FormSettingDevice f = new FormSettingDevice(_appData))
            {
                f.StartPosition = FormStartPosition.CenterParent;
                if(f.ShowDialog() == DialogResult.OK)
                {                    
                    cmbDevice.DisplayMember = "Name";
                    cmbDevice.DataSource = new BindingList<Device>(_appData.Devices);
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
    }
}
