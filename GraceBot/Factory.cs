﻿using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using GraceBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;

namespace GraceBot
{
    internal class Factory : IFactory
    {
        private static IFactory _factoryInstance;
        private static IApp _appInstance;
        private static ILuisManager _luisManagerInstance;
        private static ISlackManager _slackManagerInstance;
        private static IDbManager _dbManagerInstance;
        private static IBotManager _botManagerInstance;
        private static ICommandManager _commandManagerInstance;
        private static Dictionary<string, object> _dialogs;

        // disable default constructor
        private Factory()
        { }

        // a static constructor
        internal static IFactory GetFactory()
        {
            _factoryInstance = _factoryInstance ?? new Factory();
            return _factoryInstance;
        }
        public IApp GetApp()
        {
            _appInstance = _appInstance ?? new App(GetFactory());
            return _appInstance;
        }

        // Return a new 
        public ILuisManager GetLuisManager()
        {
            _luisManagerInstance= _luisManagerInstance??new LuisManager();
            return _luisManagerInstance;
        }

        // Return a new 
        public ISlackManager GetSlackManager()
        {
            _slackManagerInstance= _slackManagerInstance?? new SlackManager();
            return _slackManagerInstance;
        }

        public IDbManager GetDbManager()
        {
            _dbManagerInstance= _dbManagerInstance??new DbManager(new Models.GraceBotContext());
            return _dbManagerInstance;
        }

        public IBotManager GetBotManager()
        {
            _botManagerInstance= _botManagerInstance??new BotManager();
            return _botManagerInstance;
        }

        public ICommandManager GetCommandManager()
        {
            _commandManagerInstance = _commandManagerInstance ?? new CommandManager(GetFactory());
            return _commandManagerInstance;
        }

        public IFilter GetActivityFilter()
        {
            var sep = Path.DirectorySeparatorChar;
            return new ActivityFilter(File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + $"{sep}BadWords{sep}en"));
        }

        public IDefinition GetActivityDefinition()
        {
            var sep = Path.DirectorySeparatorChar;
            using (var reader =
                new JsonTextReader(
                new StreamReader(AppDomain.CurrentDomain.BaseDirectory + $"{sep}Words{sep}dictionary.json"))
            )
            {
                var definitions = new JsonSerializer().Deserialize<Dictionary<string, string>>(reader);
                return new ActivityDefinition(definitions);
            }
        }

        public IDialog<T> GetGraceDialog<T>(string dialogName)
        {
            if (_dialogs == null)
                InitialDialog();
            object dialog = null;
            if (_dialogs.TryGetValue(dialogName, out dialog))
            {
                if (dialog is IDialog<T>)
                    return (IDialog<T>)dialog;
            }
            return null;
        }

        public Dictionary<string, List<string>> GetResponseData(string contextOrDialogName)
        {
            return new Dictionary<string, List<string>>();
        }

        private void InitialDialog()
        {
            _dialogs = new Dictionary<string, object>();
            _dialogs.Add(HomeDialog.Name, new HomeDialog(this));
        }
    }
}
