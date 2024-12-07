namespace ServerSample
{
    partial class FormSettingDevice
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            gridResponse = new DataGridView();
            btnCreate = new Button();
            cmbDevice = new ComboBox();
            label1 = new Label();
            btnSave = new Button();
            ColumnCommand = new DataGridViewTextBoxColumn();
            ColumnResponse = new DataGridViewTextBoxColumn();
            ColumnDeleteButton = new DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)gridResponse).BeginInit();
            SuspendLayout();
            // 
            // gridResponse
            // 
            gridResponse.AllowUserToResizeRows = false;
            gridResponse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Meiryo", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            gridResponse.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            gridResponse.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridResponse.Columns.AddRange(new DataGridViewColumn[] { ColumnCommand, ColumnResponse, ColumnDeleteButton });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Meiryo", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            gridResponse.DefaultCellStyle = dataGridViewCellStyle2;
            gridResponse.Location = new Point(11, 87);
            gridResponse.Margin = new Padding(3, 4, 3, 4);
            gridResponse.Name = "gridResponse";
            gridResponse.RowHeadersWidth = 20;
            gridResponse.Size = new Size(742, 663);
            gridResponse.TabIndex = 10;
            // 
            // btnCreate
            // 
            btnCreate.Font = new Font("Meiryo", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnCreate.Location = new Point(617, 16);
            btnCreate.Margin = new Padding(3, 4, 3, 4);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(136, 63);
            btnCreate.TabIndex = 11;
            btnCreate.Text = "新規作成";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // cmbDevice
            // 
            cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevice.FlatStyle = FlatStyle.Flat;
            cmbDevice.Font = new Font("Meiryo", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(69, 25);
            cmbDevice.Margin = new Padding(3, 4, 3, 4);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(398, 38);
            cmbDevice.TabIndex = 12;
            cmbDevice.SelectedIndexChanged += cmbDevice_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Meiryo", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label1.Location = new Point(14, 33);
            label1.Name = "label1";
            label1.Size = new Size(53, 30);
            label1.TabIndex = 13;
            label1.Text = "機器";
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Meiryo", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnSave.Location = new Point(474, 16);
            btnSave.Margin = new Padding(3, 4, 3, 4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(136, 63);
            btnSave.TabIndex = 14;
            btnSave.Text = "保存";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // ColumnCommand
            // 
            ColumnCommand.DataPropertyName = "Command";
            ColumnCommand.HeaderText = "コマンド";
            ColumnCommand.MinimumWidth = 6;
            ColumnCommand.Name = "ColumnCommand";
            ColumnCommand.Width = 200;
            // 
            // ColumnResponse
            // 
            ColumnResponse.DataPropertyName = "Value";
            ColumnResponse.HeaderText = "返信内容";
            ColumnResponse.MinimumWidth = 6;
            ColumnResponse.Name = "ColumnResponse";
            ColumnResponse.Width = 320;
            // 
            // ColumnDeleteButton
            // 
            ColumnDeleteButton.HeaderText = "";
            ColumnDeleteButton.MinimumWidth = 6;
            ColumnDeleteButton.Name = "ColumnDeleteButton";
            ColumnDeleteButton.Text = "削除";
            ColumnDeleteButton.UseColumnTextForButtonValue = true;
            ColumnDeleteButton.Width = 80;
            // 
            // FormSettingDevice
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(767, 763);
            Controls.Add(btnSave);
            Controls.Add(label1);
            Controls.Add(cmbDevice);
            Controls.Add(btnCreate);
            Controls.Add(gridResponse);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormSettingDevice";
            Text = "コマンド設定";
            Shown += FormSettingDevice_Shown;
            ((System.ComponentModel.ISupportInitialize)gridResponse).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView gridResponse;
        private Button btnCreate;
        private ComboBox cmbDevice;
        private Label label1;
        private Button btnSave;
        private DataGridViewTextBoxColumn ColumnCommand;
        private DataGridViewTextBoxColumn ColumnResponse;
        private DataGridViewButtonColumn ColumnDeleteButton;
    }
}