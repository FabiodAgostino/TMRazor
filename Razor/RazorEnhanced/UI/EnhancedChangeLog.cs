using System;
using System.Windows.Forms;

namespace RazorEnhanced.UI
{
    public partial class EnhancedChangeLog : Form
    {
        public EnhancedChangeLog()
        {
            InitializeComponent();
            LanguageHelper.TranslateForm(this);
        }

        private void EnhancedChangeLog_Load(object sender, EventArgs e)
        {

        }
    }
}
