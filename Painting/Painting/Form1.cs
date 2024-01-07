using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

namespace Painting
{
    // This class represents the main form of your painting application.
    public partial class Form1 : Form
    {
        private CommandParser commandParser;
        static List<Form1> listofForms = new List<Form1>();
        /// <summary>
        /// public picturebox to show canvas
        /// </summary>
        public PictureBox pbDrawingArea;
        /// <summary>
        /// constructor to instantiate form1 and add it to list of forms
        /// </summary>
        public Form1()
        {
            // This call is necessary for WinForms designer support.
            InitializeComponent();
            if (pbDrawingArea == null)
            {
                pbDrawingArea = new PictureBox();
                pbDrawingArea.Name = "pbDrawingArea";
                pbDrawingArea.Location = new Point(507, 20);
                pbDrawingArea.Size = new Size(707, 310);
                pbDrawingArea.BackColor = Color.LightGray;
            }
            
            Controls.Add(pbDrawingArea);

            // Initialize the CommandParser with a PictureBox control and a delegate to Invalidate that control.
            // The PictureBox is likely where the drawing will occur, and Invalidate will cause it to repaint.
            commandParser = new CommandParser(pbDrawingArea, () => pbDrawingArea.Invalidate());

            // This sets the window state of the form to maximized when the application starts.
            this.WindowState = FormWindowState.Maximized;
            listofForms.Add(this);
           
        }
        /// <summary>
        /// run the commands on all forms
        /// </summary>
        /// <param name="text"></param>
        public void run(string text)
        {
            string command = text;
            // Retrieves the command input text from a TextBox control.

            //command = txtCommandInput.Text;
            try
            {
                // Tries to execute the command using the CommandParser instance.
                commandParser.Interpreter(command);
            }
            catch (Exception ex)
            {
                // If an exception occurs, it is caught and displayed in a message box.
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Event handler for the 'Run' button click event.
        private void btnRun_Click(object sender, EventArgs e)
        {
            pbDrawingArea.Visible = true;
            lblError.Visible = false;
            foreach (Form1 obj in listofForms)
            {
                obj.run(txtCommandInput.Text);
            }
            
        }

        // Event handler for the 'Check Syntax' button click event.
        private void btnCheckSyntax_Click(object sender, EventArgs e)
        {
            // Retrieves the command input text from a TextBox control.
            var command = txtCommandInput.Text;
            try
            {
                // Tries to check the syntax of the command using the CommandParser instance.
                string result=commandParser.CheckSyntax(command);
                if (result == "")
                {
                    MessageBox.Show("Syntax is correct.", "Syntax Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    pbDrawingArea.Visible = true;
                    lblError.Visible = false;
                }
                else
                {
                    lblError.Text = result;
                    pbDrawingArea.Visible = false;
                    lblError.Visible = true;
                }
                // If no exception is thrown, a message box indicates the syntax is correct.
              
            }
            catch (Exception ex)
            {
                // If a syntax error occurs, it is caught and displayed in a message box.
                MessageBox.Show(ex.Message, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler for the 'Exit' button click event.
        private void btn_Exit_Click(object sender, EventArgs e)
        {
            // This method call terminates the application.
            Application.Exit();
        }

        // Event handler for a click event on the PictureBox (presumably the drawing area).
        // Currently, this event handler does nothing.
        private void pbDrawingArea_Click(object sender, EventArgs e)
        {
          
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text=commandParser.OpenFile();
            if (text == null)
            {
                MessageBox.Show("Error Opening File");
            }
            else
            {
                txtCommandInput.Text = text;
            }
            
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data=txtCommandInput.Text;
            commandParser.SaveIPLFile(data);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtCommandInput.Text = "";
        }

        private void openNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a copy of the MainForm
            Form1 copyForm = new Form1();

            // Open the copy in a new thread
            Thread formThread = new Thread(() => Application.Run(copyForm));
            formThread.SetApartmentState(ApartmentState.STA);
            formThread.Start();
        }

        private void btnClearCanvas_Click(object sender, EventArgs e)
        {
            foreach (Form1 obj in listofForms)
            {
                obj.pbDrawingArea.Image=null;
            }
        }
    }
}
