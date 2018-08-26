namespace MTU.WindowsForms
{
    partial class frmMain
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnPlay = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.gbVerification = new System.Windows.Forms.GroupBox();
            this.lvWorkers = new System.Windows.Forms.ListView();
            this.chVerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chVerProgress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.gbDownload = new System.Windows.Forms.GroupBox();
            this.lvUpdates = new System.Windows.Forms.ListView();
            this.chUpdateName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chUpdateProgress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblUpdateStatus = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.gbVerification.SuspendLayout();
            this.gbDownload.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblUpdateStatus, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnPlay, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblStatus, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gbVerification, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.gbDownload, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(903, 457);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnPlay
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.btnPlay, 2);
            this.btnPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPlay.Enabled = false;
            this.btnPlay.Location = new System.Drawing.Point(5, 415);
            this.btnPlay.Margin = new System.Windows.Forms.Padding(5);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(893, 37);
            this.btnPlay.TabIndex = 0;
            this.btnPlay.Text = "Jogar!";
            this.btnPlay.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Location = new System.Drawing.Point(5, 5);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(5);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(441, 35);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Inicializando...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gbVerification
            // 
            this.gbVerification.Controls.Add(this.lvWorkers);
            this.gbVerification.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbVerification.Location = new System.Drawing.Point(5, 50);
            this.gbVerification.Margin = new System.Windows.Forms.Padding(5);
            this.gbVerification.Name = "gbVerification";
            this.gbVerification.Size = new System.Drawing.Size(441, 355);
            this.gbVerification.TabIndex = 2;
            this.gbVerification.TabStop = false;
            this.gbVerification.Text = "Fila de verificação";
            // 
            // lvWorkers
            // 
            this.lvWorkers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chVerName,
            this.chVerProgress});
            this.lvWorkers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvWorkers.FullRowSelect = true;
            this.lvWorkers.GridLines = true;
            this.lvWorkers.Location = new System.Drawing.Point(3, 16);
            this.lvWorkers.Margin = new System.Windows.Forms.Padding(5);
            this.lvWorkers.Name = "lvWorkers";
            this.lvWorkers.Size = new System.Drawing.Size(435, 336);
            this.lvWorkers.TabIndex = 0;
            this.lvWorkers.UseCompatibleStateImageBehavior = false;
            this.lvWorkers.View = System.Windows.Forms.View.Details;
            // 
            // chVerName
            // 
            this.chVerName.Text = "Nome";
            this.chVerName.Width = 250;
            // 
            // chVerProgress
            // 
            this.chVerProgress.Text = "Progresso";
            this.chVerProgress.Width = 100;
            // 
            // gbDownload
            // 
            this.gbDownload.Controls.Add(this.lvUpdates);
            this.gbDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDownload.Location = new System.Drawing.Point(456, 50);
            this.gbDownload.Margin = new System.Windows.Forms.Padding(5);
            this.gbDownload.Name = "gbDownload";
            this.gbDownload.Size = new System.Drawing.Size(442, 355);
            this.gbDownload.TabIndex = 3;
            this.gbDownload.TabStop = false;
            this.gbDownload.Text = "Fila de atualização";
            // 
            // lvUpdates
            // 
            this.lvUpdates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chUpdateName,
            this.chUpdateProgress});
            this.lvUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvUpdates.FullRowSelect = true;
            this.lvUpdates.GridLines = true;
            this.lvUpdates.Location = new System.Drawing.Point(3, 16);
            this.lvUpdates.Margin = new System.Windows.Forms.Padding(5);
            this.lvUpdates.Name = "lvUpdates";
            this.lvUpdates.Size = new System.Drawing.Size(436, 336);
            this.lvUpdates.TabIndex = 0;
            this.lvUpdates.UseCompatibleStateImageBehavior = false;
            this.lvUpdates.View = System.Windows.Forms.View.Details;
            // 
            // chUpdateName
            // 
            this.chUpdateName.Text = "Nome";
            this.chUpdateName.Width = 250;
            // 
            // chUpdateProgress
            // 
            this.chUpdateProgress.Text = "Progresso";
            this.chUpdateProgress.Width = 100;
            // 
            // lblUpdateStatus
            // 
            this.lblUpdateStatus.AutoSize = true;
            this.lblUpdateStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUpdateStatus.Location = new System.Drawing.Point(456, 5);
            this.lblUpdateStatus.Margin = new System.Windows.Forms.Padding(5);
            this.lblUpdateStatus.Name = "lblUpdateStatus";
            this.lblUpdateStatus.Size = new System.Drawing.Size(442, 35);
            this.lblUpdateStatus.TabIndex = 4;
            this.lblUpdateStatus.Tag = "{0} arquivos atualizados de {1} atualizações";
            this.lblUpdateStatus.Text = "{0} arquivos atualizados de {1} atualizações";
            this.lblUpdateStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(903, 457);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Multi-threading Updater";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.gbVerification.ResumeLayout(false);
            this.gbDownload.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox gbVerification;
        private System.Windows.Forms.ListView lvWorkers;
        private System.Windows.Forms.ColumnHeader chVerName;
        private System.Windows.Forms.ColumnHeader chVerProgress;
        private System.Windows.Forms.GroupBox gbDownload;
        private System.Windows.Forms.ListView lvUpdates;
        private System.Windows.Forms.ColumnHeader chUpdateName;
        private System.Windows.Forms.ColumnHeader chUpdateProgress;
        private System.Windows.Forms.Label lblUpdateStatus;
    }
}

