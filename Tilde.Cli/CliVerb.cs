// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Tilde.Cli
{
    public class CliVerb
    {
        public List<string> Aliases { get; set; } = new List<string>();

        public string Description { get; set; }
        public string Name { get; set; }

        public CliVerb()
        {
        }

        public CliVerb(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        public CliVerb(IEnumerable<string> names, string description = "")
        {
            IEnumerable<string> enumerable = names as string[] ?? names.ToArray();

            Name = enumerable.FirstOrDefault();
            Aliases = enumerable.Skip(1)
                .ToList();

            Description = description;
        }
    }
}