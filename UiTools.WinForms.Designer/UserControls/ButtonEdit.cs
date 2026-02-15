using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing;

namespace UiTools.WinForms.Designer
{
    [Designer(typeof(FixedHeightUserControlDesigner))]
    public partial class ButtonEdit : UserControl
    {
        private class FixedHeightUserControlDesigner : ControlDesigner
        {
            public override SelectionRules SelectionRules
            {
                get { return (base.SelectionRules & ~(SelectionRules.BottomSizeable | SelectionRules.TopSizeable)); }
            }
        }

        public event EventHandler ButtonClick;
        
        [Browsable(true)]
        public new string Text
        {
            get { return textbox1.Text; }
            set { textbox1.Text = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string ButtonText
        {
            get { return button1.Text; }
            set { button1.Text = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Image ButtonImage
        {
            get { return button1.Image; }
            set { button1.Image = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string ButtonToolTipText
        {
            get { return toolTip1.GetToolTip(button1); }
            set { toolTip1.SetToolTip(button1, value); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Font TextFont
        {
            get { return textbox1.Font; }
            set { textbox1.Font = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Font ButtonFont
        {
            get { return button1.Font; }
            set { button1.Font = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool ButtonEnabled
        {
            get { return button1.Enabled; }
            set { button1.Enabled = value; }
        }

        public ButtonEdit()
        {
            InitializeComponent();
            Height = textbox1.Height;
            textbox1.TextChanged += textbox1_TextChanged;
            button1.Click += button1_Click;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ButtonClick?.Invoke(this, EventArgs.Empty);
        }

        private void textbox1_TextChanged(object sender, EventArgs e)
        {
            OnTextChanged(EventArgs.Empty);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, textbox1.Height + 1, specified);
        }
    }
}
