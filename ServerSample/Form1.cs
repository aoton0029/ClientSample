using System.ComponentModel;

namespace ServerSample
{
    public partial class Form1 : Form
    {
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
            using (FormSettingDevice f = new FormSettingDevice())
            {
                f.StartPosition = FormStartPosition.CenterParent;
                if(f.ShowDialog() == DialogResult.OK)
                {
                    var list = FileUtils.ReadFromJsonFile<List<Device>>("data.obj");
                    cmbDevice.DisplayMember = "Name";
                    cmbDevice.DataSource = list;
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
    }
}
