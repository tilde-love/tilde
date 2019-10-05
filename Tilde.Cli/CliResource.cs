// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Tilde.Cli
{
    public class CliResource
    {
        public List<string> Aliases { get; set; } = new List<string>();

        public string Description { get; set; }

        public List<Command> Commands { get; set; } = new List<Command>();

        public string Name { get; set; }
        
        public Dictionary<string, Command> VerbCommands { get; set; } = new Dictionary<string, Command>();

        protected Command CreateNounCommand(
            string description,
            IReadOnlyCollection<Symbol> symbols = null,
            Argument argument = null,
            ICommandHandler handler = null)
        {
            Command command = new Command(Name, description ?? Description, symbols, argument, handler: handler);

            foreach (string alias in Aliases)
            {
                command.AddAlias(alias);
            }

            return command;
        }
    }
}