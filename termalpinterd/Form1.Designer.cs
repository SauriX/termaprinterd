namespace termalprinterd
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnActualizar;
        private System.Windows.Forms.Button btnToggleStartup;
        private System.Windows.Forms.ListBox listBoxImpresoras;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem salirToolStripMenuItem;


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
            btnSend = new Button();
            btnActualizar = new Button();
            listBoxImpresoras = new ListBox();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            salirToolStripMenuItem = new ToolStripMenuItem();
            btnToggleStartup = new Button();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSend
            // 
            btnSend.Location = new Point(169, 96);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 0;
            btnSend.Text = "Probar";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnActualizar
            // 
            btnActualizar.Location = new Point(30, 169);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(75, 23);
            btnActualizar.TabIndex = 2;
            btnActualizar.Text = "update";
            btnActualizar.UseVisualStyleBackColor = true;
            btnActualizar.Click += btnActualizar_Click;
            // 
            // listBoxImpresoras
            // 
            listBoxImpresoras.ItemHeight = 15;
            listBoxImpresoras.Location = new Point(12, 69);
            listBoxImpresoras.Name = "listBoxImpresoras";
            listBoxImpresoras.Size = new Size(120, 94);
            listBoxImpresoras.TabIndex = 1;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Text = "Mi Aplicación en Segundo Plano";
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
            btnToggleStartup.Location = new Point(30, 198);
            btnToggleStartup.Name = "btnToggleStartup";
            btnToggleStartup.Size = new Size(200, 40);
            btnToggleStartup.TabIndex = 0;
            btnToggleStartup.Text = "Activar Inicio Automático";
            btnToggleStartup.UseVisualStyleBackColor = true;
            btnToggleStartup.Click += btnToggleStartup_Click;
            // 
            // Form1
            // 
            ClientSize = new Size(284, 261);
            Controls.Add(btnSend);
            Controls.Add(listBoxImpresoras);
            Controls.Add(btnActualizar);
            Controls.Add(btnToggleStartup);
            Name = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
