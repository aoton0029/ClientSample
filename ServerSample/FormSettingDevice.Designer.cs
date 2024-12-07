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
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            gridResponse = new DataGridView();
            ColumnCommand = new DataGridViewTextBoxColumn();
            ColumnResponse = new DataGridViewTextBoxColumn();
            ColumnDeleteButton = new DataGridViewButtonColumn();
            btnCreate = new Button();
            cmbDevice = new ComboBox();
            label1 = new Label();
            btnSave = new Button();
            ((System.ComponentModel.ISupportInitialize)gridResponse).BeginInit();
            SuspendLayout();
            // 
            // gridResponse
            // 
            gridResponse.AllowUserToResizeRows = false;
            gridResponse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = SystemColors.Control;
            dataGridViewCellStyle5.Font = new Font("メイリオ", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle5.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.True;
            gridResponse.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            gridResponse.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridResponse.Columns.AddRange(new DataGridViewColumn[] { ColumnCommand, ColumnResponse, ColumnDeleteButton });
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = SystemColors.Window;
            dataGridViewCellStyle6.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle6.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
            gridResponse.DefaultCellStyle = dataGridViewCellStyle6;
            gridResponse.Location = new Point(10, 65);
            gridResponse.Name = "gridResponse";
            gridResponse.RowHeadersWidth = 20;
            gridResponse.Size = new Size(649, 497);
            gridResponse.TabIndex = 10;
            // 
            // ColumnCommand
            // 
            ColumnCommand.HeaderText = "コマンド";
            ColumnCommand.Name = "ColumnCommand";
            ColumnCommand.Width = 200;
            // 
            // ColumnResponse
            // 
            ColumnResponse.HeaderText = "返信内容";
            ColumnResponse.Name = "ColumnResponse";
            ColumnResponse.Width = 320;
            // 
            // ColumnDeleteButton
            // 
            ColumnDeleteButton.HeaderText = "";
            ColumnDeleteButton.Name = "ColumnDeleteButton";
            ColumnDeleteButton.Text = "削除";
            ColumnDeleteButton.UseColumnTextForButtonValue = true;
            ColumnDeleteButton.Width = 80;
            // 
            // btnCreate
            // 
            btnCreate.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnCreate.Location = new Point(540, 12);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(119, 47);
            btnCreate.TabIndex = 11;
            btnCreate.Text = "新規作成";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // cmbDevice
            // 
            cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevice.FlatStyle = FlatStyle.Flat;
            cmbDevice.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(60, 19);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(349, 32);
            cmbDevice.TabIndex = 12;
            cmbDevice.SelectedIndexChanged += cmbDevice_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            label1.Location = new Point(12, 25);
            label1.Name = "label1";
            label1.Size = new Size(42, 24);
            label1.TabIndex = 13;
            label1.Text = "機器";
            // 
            // btnSave
            // 
            btnSave.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnSave.Location = new Point(415, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(119, 47);
            btnSave.TabIndex = 14;
            btnSave.Text = "保存";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // FormSettingDevice
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(671, 572);
            Controls.Add(btnSave);
            Controls.Add(label1);
            Controls.Add(cmbDevice);
            Controls.Add(btnCreate);
            Controls.Add(gridResponse);
            Name = "FormSettingDevice";
            Text = "コマンド設定";
            Shown += FormSettingDevice_Shown;
            ((System.ComponentModel.ISupportInitialize)gridResponse).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView gridResponse;
        private DataGridViewTextBoxColumn ColumnCommand;
        private DataGridViewTextBoxColumn ColumnResponse;
        private DataGridViewButtonColumn ColumnDeleteButton;
        private Button btnCreate;
        private ComboBox cmbDevice;
        private Label label1;
        private Button btnSave;
    }
}