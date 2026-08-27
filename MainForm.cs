using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AcordoDesktop
{
    /// <summary>
    /// Janela autônoma do Acordo. Não depende do Outlook nem de VSTO: é um
    /// programa Windows comum que abre a interface web do Acordo em uma
    /// janela própria. O usuário abre o programa quando quiser consultar o
    /// dossiê (alimentação manual), em vez de o painel aparecer sozinho
    /// dentro do Outlook.
    /// </summary>
    public sealed class MainForm : Form
    {
        private const string DossierUrl = "https://acordopanel-9uxdunud.manus.space";

        private readonly WebView2 _webView;
        private readonly ToolStrip _toolStrip;
        private readonly ToolStripButton _reloadButton;
        private readonly ToolStripButton _backButton;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly StatusStrip _statusStrip;
        private bool _initialized;

        public MainForm()
        {
            Text = "Acordo — Dossiê";
            Width = 1100;
            Height = 800;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 241, 232);

            _reloadButton = new ToolStripButton("Recarregar");
            _reloadButton.Click += (s, e) => _webView.CoreWebView2?.Reload();

            _backButton = new ToolStripButton("Voltar");
            _backButton.Click += (s, e) =>
            {
                if (_webView.CoreWebView2 != null && _webView.CoreWebView2.CanGoBack)
                {
                    _webView.CoreWebView2.GoBack();
                }
            };

            _toolStrip = new ToolStrip(_backButton, _reloadButton)
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
            };

            _statusLabel = new ToolStripStatusLabel("Carregando…");
            _statusStrip = new StatusStrip();
            _statusStrip.Items.Add(_statusLabel);

            _webView = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = Color.FromArgb(245, 241, 232),
            };

            Controls.Add(_webView);
            Controls.Add(_toolStrip);
            Controls.Add(_statusStrip);

            Load += MainForm_Load;
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            try
            {
                await _webView.EnsureCoreWebView2Async();
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                _webView.CoreWebView2.NavigationStarting += (s, args) => _statusLabel.Text = "Carregando…";
                _webView.CoreWebView2.NavigationCompleted += (s, args) =>
                    _statusLabel.Text = args.IsSuccess ? DossierUrl : "Falha ao carregar a página.";

                _webView.CoreWebView2.Navigate(DossierUrl);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                ShowRuntimeMissingMessage();
            }
            catch (Exception exception)
            {
                ShowGenericError(exception);
            }
        }

        private void ShowRuntimeMissingMessage()
        {
            Controls.Remove(_webView);
            _webView.Dispose();

            var message = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(30, 45, 52),
                Text = "Não foi possível abrir o painel Acordo.\r\n\r\n" +
                       "É necessário instalar o Microsoft Edge WebView2 Runtime neste computador " +
                       "(gratuito, da própria Microsoft) e abrir o programa novamente.\r\n\r\n" +
                       "Baixe em: https://developer.microsoft.com/microsoft-edge/webview2/",
            };
            Controls.Add(message);
            _statusLabel.Text = "WebView2 Runtime não encontrado.";
        }

        private void ShowGenericError(Exception exception)
        {
            Controls.Remove(_webView);
            _webView.Dispose();

            var message = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(30, 45, 52),
                Text = "Não foi possível abrir o painel Acordo.\r\n\r\n" +
                       "Confirme sua conexão com a internet e tente novamente.\r\n\r\n" +
                       "Detalhe técnico: " + exception.Message,
            };
            Controls.Add(message);
            _statusLabel.Text = "Erro ao carregar.";
        }
    }
}
