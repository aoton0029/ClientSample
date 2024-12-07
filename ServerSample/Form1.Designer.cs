namespace ServerSample
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle16 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle17 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle18 = new DataGridViewCellStyle();
            label1 = new Label();
            lblStatus = new Label();
            groupBox1 = new GroupBox();
            btnOpen = new Button();
            btnClose = new Button();
            txtIP = new TextBox();
            txtPort = new TextBox();
            StatusColumnConnectIP = new DataGridViewTextBoxColumn();
            gridConnectClient = new DataGridView();
            label3 = new Label();
            txtRecvLog = new TextBox();
            btnClear = new Button();
            cmbDevice = new ComboBox();
            gridResponse = new DataGridView();
            btnSettingDevice = new Button();
            btnSettingServer = new Button();
            ColumnCommand = new DataGridViewTextBoxColumn();
            ColumnResponse = new DataGridViewTextBoxColumn();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            button1 = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridConnectClient).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridResponse).BeginInit();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("メイリオ", 9.75F);
            label1.Location = new Point(340, 26);
            label1.Name = "label1";
            label1.Size = new Size(22, 20);
            label1.TabIndex = 1;
            label1.Text = "IP";
            // 
            // lblStatus
            // 
            lblStatus.BackColor = SystemColors.ActiveCaption;
            lblStatus.Font = new Font("メイリオ", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            lblStatus.Location = new Point(12, 26);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(152, 103);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "停止中";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnSettingServer);
            groupBox1.Controls.Add(txtPort);
            groupBox1.Controls.Add(lblStatus);
            groupBox1.Controls.Add(gridConnectClient);
            groupBox1.Controls.Add(btnClose);
            groupBox1.Controls.Add(btnOpen);
            groupBox1.Controls.Add(txtIP);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(label3);
            groupBox1.Font = new Font("メイリオ", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            groupBox1.Location = new Point(7, 4);
            groupBox1.Margin = new Padding(0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1168, 139);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "サーバーステータス";
            // 
            // btnOpen
            // 
            btnOpen.Font = new Font("メイリオ", 15.75F);
            btnOpen.Location = new Point(170, 27);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(158, 51);
            btnOpen.TabIndex = 3;
            btnOpen.Text = "開始";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click;
            // 
            // btnClose
            // 
            btnClose.Font = new Font("メイリオ", 15.75F);
            btnClose.Location = new Point(170, 78);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(158, 51);
            btnClose.TabIndex = 4;
            btnClose.Text = "停止";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // txtIP
            // 
            txtIP.Font = new Font("メイリオ", 12F);
            txtIP.Location = new Point(343, 46);
            txtIP.MaxLength = 15;
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(148, 31);
            txtIP.TabIndex = 0;
            txtIP.Text = "999.999.999.999";
            txtIP.TextAlign = HorizontalAlignment.Center;
            // 
            // txtPort
            // 
            txtPort.Font = new Font("メイリオ", 12F);
            txtPort.ImeMode = ImeMode.Disable;
            txtPort.Location = new Point(497, 46);
            txtPort.MaxLength = 5;
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(64, 31);
            txtPort.TabIndex = 6;
            txtPort.Text = "8080";
            txtPort.TextAlign = HorizontalAlignment.Center;
            // 
            // StatusColumnConnectIP
            // 
            StatusColumnConnectIP.HeaderText = "接続中クライアント";
            StatusColumnConnectIP.Name = "StatusColumnConnectIP";
            StatusColumnConnectIP.Width = 250;
            // 
            // gridConnectClient
            // 
            gridConnectClient.AllowUserToAddRows = false;
            gridConnectClient.AllowUserToDeleteRows = false;
            gridConnectClient.AllowUserToResizeRows = false;
            dataGridViewCellStyle16.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle16.BackColor = SystemColors.Control;
            dataGridViewCellStyle16.Font = new Font("メイリオ", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle16.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle16.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle16.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle16.WrapMode = DataGridViewTriState.True;
            gridConnectClient.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle16;
            gridConnectClient.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridConnectClient.Columns.AddRange(new DataGridViewColumn[] { StatusColumnConnectIP });
            dataGridViewCellStyle17.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle17.BackColor = SystemColors.Window;
            dataGridViewCellStyle17.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle17.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle17.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle17.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle17.WrapMode = DataGridViewTriState.False;
            gridConnectClient.DefaultCellStyle = dataGridViewCellStyle17;
            gridConnectClient.Location = new Point(575, 21);
            gridConnectClient.Name = "gridConnectClient";
            gridConnectClient.RowHeadersWidth = 20;
            gridConnectClient.RowTemplate.Height = 24;
            gridConnectClient.Size = new Size(291, 108);
            gridConnectClient.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("メイリオ", 9.75F);
            label3.Location = new Point(494, 25);
            label3.Name = "label3";
            label3.Size = new Size(48, 20);
            label3.TabIndex = 7;
            label3.Text = "ポート";
            // 
            // txtRecvLog
            // 
            txtRecvLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtRecvLog.Font = new Font("メイリオ", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            txtRecvLog.Location = new Point(5, 56);
            txtRecvLog.Multiline = true;
            txtRecvLog.Name = "txtRecvLog";
            txtRecvLog.Size = new Size(569, 545);
            txtRecvLog.TabIndex = 4;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Font = new Font("メイリオ", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnClear.Location = new Point(471, 18);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(104, 36);
            btnClear.TabIndex = 6;
            btnClear.Text = "クリア";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // cmbDevice
            // 
            cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevice.FlatStyle = FlatStyle.Flat;
            cmbDevice.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(9, 22);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(413, 32);
            cmbDevice.TabIndex = 7;
            cmbDevice.SelectedIndexChanged += cmbDevice_SelectedIndexChanged;
            // 
            // gridResponse
            // 
            gridResponse.AllowUserToAddRows = false;
            gridResponse.AllowUserToDeleteRows = false;
            gridResponse.AllowUserToResizeRows = false;
            gridResponse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridResponse.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridResponse.Columns.AddRange(new DataGridViewColumn[] { ColumnCommand, ColumnResponse });
            dataGridViewCellStyle18.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle18.BackColor = SystemColors.Window;
            dataGridViewCellStyle18.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle18.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle18.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = DataGridViewTriState.False;
            gridResponse.DefaultCellStyle = dataGridViewCellStyle18;
            gridResponse.Location = new Point(8, 57);
            gridResponse.Name = "gridResponse";
            gridResponse.RowHeadersWidth = 20;
            gridResponse.Size = new Size(566, 544);
            gridResponse.TabIndex = 9;
            // 
            // btnSettingDevice
            // 
            btnSettingDevice.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSettingDevice.Font = new Font("メイリオ", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnSettingDevice.Location = new Point(478, 20);
            btnSettingDevice.Name = "btnSettingDevice";
            btnSettingDevice.Size = new Size(96, 36);
            btnSettingDevice.TabIndex = 10;
            btnSettingDevice.Text = "設定";
            btnSettingDevice.UseVisualStyleBackColor = true;
            btnSettingDevice.Click += btnSettingDevice_Click;
            // 
            // btnSettingServer
            // 
            btnSettingServer.Font = new Font("メイリオ", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            btnSettingServer.Location = new Point(1038, 91);
            btnSettingServer.Name = "btnSettingServer";
            btnSettingServer.Size = new Size(124, 42);
            btnSettingServer.TabIndex = 11;
            btnSettingServer.Text = "サーバー設定";
            btnSettingServer.UseVisualStyleBackColor = true;
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
            // groupBox2
            // 
            groupBox2.Controls.Add(button1);
            groupBox2.Controls.Add(txtRecvLog);
            groupBox2.Controls.Add(btnClear);
            groupBox2.Font = new Font("メイリオ", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            groupBox2.Location = new Point(7, 146);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(581, 610);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "受信ログ";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cmbDevice);
            groupBox3.Controls.Add(btnSettingDevice);
            groupBox3.Controls.Add(gridResponse);
            groupBox3.Font = new Font("メイリオ", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            groupBox3.Location = new Point(595, 146);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(580, 610);
            groupBox3.TabIndex = 12;
            groupBox3.TabStop = false;
            groupBox3.Text = "送信データ";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Font = new Font("メイリオ", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            button1.Location = new Point(361, 18);
            button1.Name = "button1";
            button1.Size = new Size(104, 36);
            button1.TabIndex = 7;
            button1.Text = "クリア";
            button1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 761);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "Form1";
            Text = "Form1";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridConnectClient).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridResponse).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Label label1;
        private Label lblStatus;
        private GroupBox groupBox1;
        private Button btnClose;
        private Button btnOpen;
        private TextBox txtPort;
        private DataGridView gridConnectClient;
        private DataGridViewTextBoxColumn StatusColumnConnectIP;
        private TextBox txtIP;
        private Label label3;
        private TextBox txtRecvLog;
        private Button btnClear;
        private ComboBox cmbDevice;
        private DataGridView gridResponse;
        private Button btnSettingServer;
        private Button btnSettingDevice;
        private DataGridViewTextBoxColumn ColumnCommand;
        private DataGridViewTextBoxColumn ColumnResponse;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Button button1;
    }
}
