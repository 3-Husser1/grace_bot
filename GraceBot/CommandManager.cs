﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GraceBot.Models;

namespace GraceBot
{
    internal class CommandManager:ICommandManager
    {
        internal  Dictionary<string, ICommand> Commands { get; } = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
        {
            { CommandString.GET_UNPROCESSED_QUESTIONS, new CommandGetQuestion() },
            { CommandString.REPLYING_TO_QUESTION, new CommandReplyQuestion() }
        };

        public  ICommand GetCommand(string cmd,UserRole userRole)
        {
            if (userRole.Equals(UserRole.Ranger))
            {
                return Commands[cmd];

            }
            else
            {
                // TO DO ....
                return null;
            }
        }


    }
}