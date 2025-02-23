namespace termalprinterd
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnToggleStartup;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem salirToolStripMenuItem;
        private System.Windows.Forms.Label lblWebSocket;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListBox listBoxImpresoras;
        private System.Windows.Forms.Button btnProbar;
        private System.Windows.Forms.Button btnUpdate;

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
            components = new System.ComponentModel.Container();
            listBoxImpresoras = new ListBox();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            salirToolStripMenuItem = new ToolStripMenuItem();
            btnToggleStartup = new Button();
            lblWebSocket = new Label();
            lblStatus = new Label();
            btnProbar = new Button();
            btnUpdate = new Button();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxImpresoras
            // 
            listBoxImpresoras.FormattingEnabled = true;
            listBoxImpresoras.ItemHeight = 15;
            listBoxImpresoras.Location = new Point(41, 50);
            listBoxImpresoras.Name = "listBoxImpresoras";
            listBoxImpresoras.Size = new Size(200, 94);
            listBoxImpresoras.TabIndex = 1;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Text = "Termal Printer Server";
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { salirToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(97, 26);
            // 
            // salirToolStripMenuItem
            // 
            salirToolStripMenuItem.Name = "salirToolStripMenuItem";
            salirToolStripMenuItem.Size = new Size(96, 22);
            salirToolStripMenuItem.Text = "Salir";
            salirToolStripMenuItem.Click += salirToolStripMenuItem_Click;
            // 
            // btnToggleStartup
            // 
            btnToggleStartup.Location = new Point(41, 186);
            btnToggleStartup.Name = "btnToggleStartup";
            btnToggleStartup.Size = new Size(200, 40);
            btnToggleStartup.TabIndex = 0;
            btnToggleStartup.Text = "Activar Inicio Automático";
            btnToggleStartup.UseVisualStyleBackColor = true;
            btnToggleStartup.Click += btnToggleStartup_Click;
            // 
            // lblWebSocket
            // 
            lblWebSocket.AutoSize = true;
            lblWebSocket.Location = new Point(56, 7);
            lblWebSocket.Name = "lblWebSocket";
            lblWebSocket.Size = new Size(174, 15);
            lblWebSocket.TabIndex = 2;
            lblWebSocket.Text = "WebSocket: ws://localhost:9090";
            
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(80, 32);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(123, 15);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Estado: Desconectado";
            // 
            // btnProbar
            // 
            btnProbar.Location = new Point(141, 150);
            btnProbar.Name = "btnProbar";
            btnProbar.Size = new Size(100, 30);
            btnProbar.TabIndex = 4;
            btnProbar.Text = "Probar";
            btnProbar.UseVisualStyleBackColor = true;
            btnProbar.Click += btnProbar_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(41, 150);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(100, 30);
            btnUpdate.TabIndex = 3;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnActualizar_Click;
            // 
            // Form1
            // 
            ClientSize = new Size(284, 261);
            Controls.Add(btnToggleStartup);
            Controls.Add(lblWebSocket);
            Controls.Add(listBoxImpresoras);
            Controls.Add(btnUpdate);
            Controls.Add(btnProbar);
            Controls.Add(lblStatus);
            Name = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
