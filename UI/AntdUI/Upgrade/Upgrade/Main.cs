namespace Upgrade
{
    public partial class Main : AntdUI.Window
    {
        Random random = new Random();
        public Main()
        {
            InitializeComponent();
            btn.Text = AntdUI.Localization.Get("btn", "立即更新");
            SetTheme();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AntdUI.Spin.open(this, AntdUI.Localization.Get("loading", "正在检查更新"), config =>
            {
                var info = CheckUpdate().Result;
                btn.Enabled = true;
                txt_note.Text = info.note;
                txt_nv.Prefix = AntdUI.Localization.Get("newVersion", "新版本");
                txt_nv.Suffix = info.size;
                txt_nv.Text = info.version;
                txt_nv.BadgeSvg = Properties.Resources.up;
            });
        }

        async Task<Model.VersionInfo> CheckUpdate()
        {
            return await Task.Run(() =>
            {
                Thread.Sleep(random.Next(10, 2000));
                return new Model.VersionInfo
                {
                    note = Properties.Resources.note,
                    size = "31 MB",
                    version = "9.0.1"
                };
            });
        }

        CancellationTokenSource? cts;
        async Task UpdateApp()
        {
            cts = new CancellationTokenSource();
            btn.Type = AntdUI.TTypeMini.Primary;
            btn.Loading = true;
            btn.Text = AntdUI.Localization.Get("download", "下载更新");
            btn_cancel.Visible = true;
            await Task.Run(() =>
            {
                Thread.Sleep(1000);

                if (DownLoad(cts.Token, prog =>
                {
                    btn.LoadingWaveValue = prog;
                    btn.Text = Math.Round(prog * 100.0, 1).ToString("N1") + "% " + AntdUI.Localization.Get("download", "下载更新");
                }))
                {
                    btn_cancel.Visible = false;
                    Thread.Sleep(1000);
                    btn.LoadingWaveValue = 0.9F;
                    btn.Text = "90.0% " + AntdUI.Localization.Get("verify", "校验文件");
                    Thread.Sleep(1000);
                    btn.LoadingWaveValue = 0.98F;
                    btn.Text = "98.0% " + AntdUI.Localization.Get("updating", "正在更新");
                    Thread.Sleep(1000);
                    btn.LoadingWaveValue = 0.99F;
                    btn.Text = "99.0% " + AntdUI.Localization.Get("updating", "正在更新");
                    if (random.Next(0, 9) > 2) UISuccess();
                    else
                    {
                        UIError(AntdUI.Localization.Get("incompleteFile", "文件不完整"));
                        return;
                    }
                }
                else
                {
                    UIError(AntdUI.Localization.Get("downloadFailed", "下载更新失败"));
                    return;
                }
            });
            btn_cancel.Visible = false;
        }

        bool DownLoad(CancellationToken token, Action<float> OnProgressChange)
        {
            long downMax = 100, downValue = 0;
            if (random.Next(0, 9) > 1)
            {
                for (int i = 0; i <= 100; i++)
                {
                    if (token.IsCancellationRequested) return false;
                    downValue = i;
                    float prog = (float)Math.Round(downValue * 1.0 / downMax, 3) * .9F;
                    OnProgressChange(prog);
                    Thread.Sleep(20);
                }
                return true;
            }
            else
            {
                int tmp = random.Next(10, 99);
                for (int i = 0; i <= tmp; i++)
                {
                    if (token.IsCancellationRequested) return false;
                    downValue = i;
                    float prog = (float)Math.Round(downValue * 1.0 / downMax, 3) * .9F;
                    OnProgressChange(prog);
                    Thread.Sleep(20);
                }
                Thread.Sleep(500);
                return false;
            }
        }

        async Task UpdateCompleted()
        {
            btn.Loading = true;
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
            });
            Invoke(Close);
        }

        private async void btn_Click(object sender, EventArgs e)
        {
            if (btn.Type == AntdUI.TTypeMini.Success)
            {
                btn.Loading = true;
                await UpdateCompleted();
                return;
            }
            await UpdateApp();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }

        #region UI

        void UISuccess()
        {
            btn.Loading = false;
            btn.LoadingWaveValue = 0;
            btn.Type = AntdUI.TTypeMini.Success;
            btn.Text = AntdUI.Localization.Get("downloadCompleted", "更新完成");
        }

        void UIError(string err)
        {
            btn.Loading = false;
            btn.LoadingWaveValue = 0;
            btn.Type = AntdUI.TTypeMini.Error;
            btn.Text = err;
        }

        #endregion

        /// <summary>
        /// 设置主题
        /// </summary>
        private void SetTheme()
        {
            Dark = AntdUI.Config.IsDark;
            btn_mode.Toggle = Dark;
            txt_nv.PrefixColor = AntdUI.Style.Db.TextSecondary;
            txt_nv.ForeColor = AntdUI.Style.Db.Text;
            txt_nv.SuffixColor = AntdUI.Style.Db.TextTertiary;
            if (Dark)
            {
                BackColor = Color.Black;
                ForeColor = Color.White;
                winBar.BackColor = Color.FromArgb(30, 255, 255, 255);
            }
            else
            {
                BackColor = Color.White;
                ForeColor = Color.Black;
                winBar.BackColor = Color.FromArgb(10, 0, 0, 0);
            }
        }

        private void btn_mode_Click(object sender, EventArgs e)
        {
            AntdUI.Config.IsDark = !AntdUI.Config.IsDark;
            SetTheme();
        }
        private void FrmMove(object sender, MouseEventArgs e) => DraggableMouseDown();
    }
}
