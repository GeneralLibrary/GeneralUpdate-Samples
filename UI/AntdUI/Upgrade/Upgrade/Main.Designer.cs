using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Upgrade
{
    partial class Main
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
            winBar = new AntdUI.PageHeader();
            btn_mode = new AntdUI.Button();
            txt_note = new AntdUI.Input();
            txt_nv = new AntdUI.Label();
            btn = new AntdUI.Button();
            progress1 = new AntdUI.Progress();
            panel_btns = new AntdUI.Panel();
            btn_cancel = new AntdUI.Button();
            winBar.SuspendLayout();
            panel_btns.SuspendLayout();
            SuspendLayout();
            // 
            // winBar
            // 
            winBar.BackColor = Color.FromArgb(10, 0, 0, 0);
            winBar.Controls.Add(btn_mode);
            winBar.DividerShow = true;
            winBar.Dock = DockStyle.Top;
            winBar.Icon = Properties.Resources.GeneralUpdate;
            winBar.LocalizationText = "title";
            winBar.Location = new Point(0, 0);
            winBar.Name = "winBar";
            winBar.ShowButton = true;
            winBar.ShowIcon = true;
            winBar.Size = new Size(581, 40);
            winBar.SubText = "0.0.1";
            winBar.TabIndex = 0;
            winBar.Text = "General Update 客户端";
            // 
            // btn_mode
            // 
            btn_mode.Dock = DockStyle.Right;
            btn_mode.Ghost = true;
            btn_mode.IconSvg = "SunOutlined";
            btn_mode.Location = new Point(387, 0);
            btn_mode.Name = "btn_mode";
            btn_mode.Radius = 0;
            btn_mode.Size = new Size(50, 40);
            btn_mode.TabIndex = 6;
            btn_mode.ToggleIconSvg = "MoonOutlined";
            btn_mode.WaveSize = 0;
            btn_mode.Click += btn_mode_Click;
            // 
            // txt_note
            // 
            txt_note.AutoScroll = true;
            txt_note.BorderWidth = 0F;
            txt_note.Dock = DockStyle.Fill;
            txt_note.LocalizationPlaceholderText = "note";
            txt_note.Location = new Point(0, 97);
            txt_note.Multiline = true;
            txt_note.Name = "txt_note";
            txt_note.Padding = new Padding(8, 0, 8, 0);
            txt_note.PlaceholderText = "修复了一些已知的问题";
            txt_note.ReadOnly = true;
            txt_note.Size = new Size(581, 264);
            txt_note.TabIndex = 1;
            // 
            // txt_nv
            // 
            txt_nv.BadgeOffsetX = 100;
            txt_nv.BadgeSize = 1F;
            txt_nv.Dock = DockStyle.Top;
            txt_nv.Font = new Font("Microsoft YaHei UI", 20F);
            txt_nv.Location = new Point(0, 40);
            txt_nv.Name = "txt_nv";
            txt_nv.Padding = new Padding(0, 8, 0, 0);
            txt_nv.Prefix = "检查更新";
            txt_nv.Size = new Size(581, 57);
            txt_nv.TabIndex = 0;
            txt_nv.TextAlign = ContentAlignment.MiddleCenter;
            txt_nv.TextMultiLine = false;
            txt_nv.MouseDown += FrmMove;
            // 
            // btn
            // 
            btn.Dock = DockStyle.Fill;
            btn.Enabled = false;
            btn.Location = new Point(6, 6);
            btn.Name = "btn";
            btn.Size = new Size(533, 53);
            btn.TabIndex = 0;
            btn.Type = AntdUI.TTypeMini.Primary;
            btn.Click += btn_Click;
            // 
            // progress1
            // 
            progress1.BackgroundImage = Properties.Resources.GeneralUpdate;
            progress1.BackgroundImageLayout = ImageLayout.Zoom;
            progress1.ContainerControl = this;
            progress1.ForeColor = Color.White;
            progress1.Location = new Point(-240, 109);
            progress1.Name = "progress1";
            progress1.Radius = 12;
            progress1.Shape = AntdUI.TShapeProgress.Circle;
            progress1.Size = new Size(200, 200);
            progress1.TabIndex = 3;
            progress1.UseSystemText = true;
            progress1.Value = 0.4F;
            progress1.Visible = false;
            // 
            // panel_btns
            // 
            panel_btns.Controls.Add(btn);
            panel_btns.Controls.Add(btn_cancel);
            panel_btns.Dock = DockStyle.Bottom;
            panel_btns.Location = new Point(0, 361);
            panel_btns.Name = "panel_btns";
            panel_btns.Padding = new Padding(6);
            panel_btns.Radius = 0;
            panel_btns.Size = new Size(581, 65);
            panel_btns.TabIndex = 2;
            // 
            // btn_cancel
            // 
            btn_cancel.Dock = DockStyle.Right;
            btn_cancel.Ghost = true;
            btn_cancel.IconSvg = "CloseCircleFilled";
            btn_cancel.Location = new Point(539, 6);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Padding = new Padding(0, 4, 0, 4);
            btn_cancel.Size = new Size(36, 53);
            btn_cancel.TabIndex = 1;
            btn_cancel.Visible = false;
            btn_cancel.WaveSize = 0;
            btn_cancel.Click += btn_cancel_Click;
            // 
            // Main
            // 
            BackColor = Color.White;
            ClientSize = new Size(581, 426);
            Controls.Add(txt_note);
            Controls.Add(progress1);
            Controls.Add(txt_nv);
            Controls.Add(winBar);
            Controls.Add(panel_btns);
            Font = new Font("Microsoft YaHei UI", 12F);
            ForeColor = Color.Black;
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "General Update 客户端";
            winBar.ResumeLayout(false);
            panel_btns.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader winBar;
        private AntdUI.Button btn_mode;
        private AntdUI.Input txt_note;
        private AntdUI.Label txt_nv;
        private AntdUI.Button btn;
        private AntdUI.Progress progress1;
        private AntdUI.Panel panel_btns;
        private AntdUI.Button btn_cancel;
    }
}
