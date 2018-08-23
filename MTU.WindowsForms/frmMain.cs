using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MTU.WindowsForms
{
    using Args;
    using Enums;
    using Updater;

    public partial class frmMain : Form
    {
        MTUpdater updater;
        string failMessage, errorMessage;

        public frmMain()
        {
            InitializeComponent();
            failMessage = "Falha ao atualizar o sistema!" + Environment.NewLine;
            failMessage += "Clique em Sim para tentar novamente" + Environment.NewLine;
            failMessage += "Clique em Não para sair do atualizador";

            errorMessage = "Erro no atualizador!" + Environment.NewLine;
            errorMessage += "------------------------------------" + Environment.NewLine;
            errorMessage += "{0}" + Environment.NewLine;
            errorMessage += "------------------------------------" + Environment.NewLine;
            errorMessage += "{1}" + Environment.NewLine;
            errorMessage += "------------------------------------";

            updater = new MTUpdater("http://localhost/ver.xml");
            updater.Started += Updater_Started;
            updater.Failed += Updater_Failed;
            updater.Finished += Updater_Finished;
            updater.ErrorThrowed += Updater_ErrorThrowed;
            updater.StateChanged += Updater_StateChanged;
            updater.WorkerAppend += Updater_WorkerAppend;
            updater.WorkerProgress += Updater_WorkerProgress;
        }

        bool init = false;
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!init)
            {
                init = true;
                Refresh();
                Invalidate();

                updater.Update();
            }
        }

        private void Updater_StateChanged(object sender, StateEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action<object, StateEventArgs>(Updater_StateChanged), sender, e);
            else
            {
                var msg = string.Empty;
                switch (e.State)
                {
                    case UpdaterState.DownloadingVerification:
                        msg = "Baixando verificação...";
                        break;
                    case UpdaterState.CheckingFiles:
                        msg = "Verificando lista de arquivos...";
                        break;
                    case UpdaterState.Failed:
                        msg = "Falha no atualizador!";
                        break;
                }
                label1.Text = msg;
            }
        }

        private void Updater_WorkerAppend(object sender, WorkerAppendEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action<object, WorkerAppendEventArgs>(Updater_WorkerAppend), sender, e);
            else
            {
                var item = (e.Type == WorkerType.Update ? listView2.Items : listView1.Items).Add(e.Name);
                item.Name = e.Name;

                item.SubItems.Add("0%");
            }
        }

        private void Updater_WorkerProgress(object sender, ProgressEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action<object, ProgressEventArgs>(Updater_WorkerProgress), sender, e);
            else
            {
                var item = (e.Type == WorkerType.Update ? listView2.Items : listView1.Items)[e.Name];
                item.SubItems[1].Text = string.Format("{0}%", e.Progress);
            }
        }

        private void Updater_Started(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action<object, EventArgs>(Updater_Started), sender, e);
            else
                label1.Text = "Atualizador iniciado!";
        }

        private void Updater_Failed(object sender, RetryEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action<object, RetryEventArgs>(Updater_Failed), sender, e);
            else
                e.Retry = DialogResult.Yes == MessageBox.Show(this, failMessage, "Falha", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        private void Updater_ErrorThrowed(object sender, ErrorEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action<object, ErrorEventArgs>(Updater_ErrorThrowed), sender, e);
            else
                MessageBox.Show(this, string.Format(errorMessage, e.Error.Message, e.Error.StackTrace), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Updater_Finished(object sender, ResultEventArgs e)
        {
            Environment.Exit(e.Result ? 0 : -1);
        }
    }
}