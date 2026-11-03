using System;
using System.Collections.Generic;
using Mopups.Pages;

namespace OWCE.Pages.Popup
{
    public partial class JumpstartAlert : PopupPage
    {
        private readonly Command _actionButtonCommand;
        public Command ActionButtonCommand => _actionButtonCommand;

        public JumpstartAlert(Command actionCommand)
        {
            _actionButtonCommand = actionCommand;

            InitializeComponent();

            BindingContext = this;
        }
    }
}
