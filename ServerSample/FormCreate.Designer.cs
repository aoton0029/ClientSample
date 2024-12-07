namespace ServerSample
{
    partial class FormCreate
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
            txt = new TextBox();
            btnCancel = new Button();
            btnOK = new Button();
            SuspendLayout();
            // 
            // txt
            // 
            txt.Font = new Font("メイリオ", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            txt.Location = new Point(12, 19);
            txt.Name = "txt";
            txt.Size = new Size(407, 31);
            txt.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("メイリオ", 12F);
            btnCancel.Location = new Point(94, 63);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(113, 41);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "キャンセル";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Font = new Font("メイリオ", 12F);
            btnOK.Location = new Point(224, 63);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(113, 41);
            btnOK.TabIndex = 2;
            btnOK.Text = "作成";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // FormCreate
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(431, 111);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(txt);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormCreate";
            Text = "新規作成";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txt;
        private Button btnCancel;
        private Button btnOK;
    }
}