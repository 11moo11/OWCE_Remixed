using System;
using System.Collections.Generic;
using Mopups.Pages;
using Mopups.Services;

namespace OWCE.Pages.Popup
{
    public partial class Alert : PopupPage
    {
        public string SuperTitleText { get; set; } = String.Empty;
        public string TitleText { get; set; } = String.Empty;
        public string MessageText { get; set; } = String.Empty;
        public string ButtonText { get; set; } = "OK";

        //public Action ActionButtonClicked { get; set; } = null;

        private readonly Command _actionButtonCommand;
        public Command ActionButtonCommand => _actionButtonCommand;

        public Alert(string title, string message, Command command = null)
        {
            BindingContext = this;

            _actionButtonCommand = command ?? new Command(async () =>
            {
                await MopupService.Instance.RemovePageAsync(this);
            });

            TitleText = title;
            MessageText = message;
            InitializeComponent();
        }
    }
}
