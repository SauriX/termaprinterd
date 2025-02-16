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
            components = new System.ComponentModel.Container(); // Asegura que no sea null
            btnSend = new Button();
            btnActualizar = new Button();
            listBoxImpresoras = new ListBox();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            salirToolStripMenuItem = new ToolStripMenuItem();
            SuspendLayout();
            this.btnToggleStartup = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnToggleStartup
            // 
            this.btnToggleStartup.Location = new System.Drawing.Point(50, 30);
            this.btnToggleStartup.Name = "btnToggleStartup";
            this.btnToggleStartup.Size = new System.Drawing.Size(200, 40);
            this.btnToggleStartup.TabIndex = 0;
            this.btnToggleStartup.Text = "Activar Inicio Automático";
            this.btnToggleStartup.UseVisualStyleBackColor = true;
            this.btnToggleStartup.Click += new System.EventHandler(this.btnToggleStartup_Click);

            // 
            // btnSend
            // 
            btnSend.Location = new Point(197, 28);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 0;
            btnSend.Text = "Enviar";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnActualizar
            // 
            btnActualizar.Location = new Point(37, 138);
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
            listBoxImpresoras.Location = new Point(30, 28);
            listBoxImpresoras.Name = "listBoxImpresoras";
            listBoxImpresoras.Size = new Size(120, 94);
            listBoxImpresoras.TabIndex = 1;

            // 
            // notifyIcon1
            // 
            notifyIcon1.Icon = new System.Drawing.Icon("assets/icon.ico"); // Asegúrate de que tienes un icono válido
            notifyIcon1.Text = "Mi Aplicación en Segundo Plano";
            notifyIcon1.Visible = false;
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;

            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { salirToolStripMenuItem });
            // 
            // salirToolStripMenuItem
            // 
            salirToolStripMenuItem.Name = "salirToolStripMenuItem";
            salirToolStripMenuItem.Size = new Size(100, 22);
            salirToolStripMenuItem.Text = "Salir";
            salirToolStripMenuItem.Click += salirToolStripMenuItem_Click;

            // 
            // Form1
            // 
            ClientSize = new Size(284, 261);
            Controls.Add(btnSend);
            Controls.Add(listBoxImpresoras);
            Controls.Add(btnActualizar);
            Name = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
