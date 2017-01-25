﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace GraceBot.Dialogs
{
    internal class HomeDialog : GraceDialog<object>
    {
        private Dictionary<string, List<string>> _responses;

        public const string NAME = "Home";

        public HomeDialog(IFactory factory, params object[] dialogVariables) : base(factory, dialogVariables)
        {
            _responses = _factory.GetResponseData(NAME);
        }

        public override async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            throw new NotImplementedException();
        }


    }
}