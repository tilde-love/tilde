// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using Tilde.Cli.Verbs;

namespace Tilde.Cli
{
    public static class Commands
    {       
        public static IEnumerable<Type> Verbs => new[]
        {
            typeof(ListVerb),
            typeof(NewVerb),
            typeof(DeleteVerb),
            typeof(StartVerb),
            typeof(StopVerb),
            typeof(PullVerb),
            typeof(PushVerb),
            typeof(WatchVerb),
            typeof(LogsVerb)
        };

        public static void Parse(ParserResult<object> parserResult)
        {
            parserResult.WithParsed((ListVerb opts) => ListVerb.List(opts));
            parserResult.WithParsed((NewVerb opts) => NewVerb.New(opts));
            parserResult.WithParsed((DeleteVerb opts) => DeleteVerb.Delete(opts));
            parserResult.WithParsed((StartVerb opts) => StartVerb.Start(opts));
            parserResult.WithParsed((StopVerb opts) => StopVerb.Stop(opts));
            parserResult.WithParsed((PullVerb opts) => PullVerb.Pull(opts));
            parserResult.WithParsed((PushVerb opts) => PushVerb.Push(opts));
            parserResult.WithParsed((WatchVerb opts) => WatchVerb.Watch(opts));
            parserResult.WithParsed((LogsVerb opts) => LogsVerb.Logs(opts));
        }

        public static void ParseErrors(ParserResult<object> parserResult)
        {
            parserResult.WithNotParsed(
                errs =>
                {
                    string helpText = string.Empty;

                    List<ErrorType> errorTypes = errs.Select(e => e.Tag)
                        .ToList();

                    if (parserResult.TypeInfo.Current == typeof(NullInstance))
                    {
                        if (errorTypes.Contains(ErrorType.NoVerbSelectedError))
                        {
                            helpText = HelpText.AutoBuild(
                                    parserResult,
                                    h =>
                                    {
                                        // Configure HelpText here  or create your own and return it 
                                        h.AdditionalNewLineAfterOption = true;
                                        return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                                    },
                                    e => e
                                )
                                .ToString();
                        }
                        else
                        {
                            helpText = HelpText.AutoBuild(
                                    parserResult,
                                    h =>
                                    {
                                        // Configure HelpText here  or create your own and return it 
                                        h.AdditionalNewLineAfterOption = true;
                                        return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                                    },
                                    e => e
                                )
                                .ToString();
                        }
                    }
                    else
                    {
                        helpText = HelpText.AutoBuild(
                                parserResult,
                                h =>
                                {
                                    // Configure HelpText here  or create your own and return it 
                                    h.AdditionalNewLineAfterOption = true;
                                    return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                                },
                                e => e
                            )
                            .ToString();
                    }

//                    string[] lines = helpText
//                        .Split(
//                            new[] {Environment.NewLine},
//                            StringSplitOptions.RemoveEmptyEntries
//                        );

                    Console.Error.Write(helpText); // string.Join(Environment.NewLine, lines));
                }
            );
        }
    }
}