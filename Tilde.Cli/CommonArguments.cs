// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.CommandLine;

namespace Tilde.Cli
{
    public static class CommonArguments
    {
        public static Argument<Uri> ProjectNameArgument()
        {
            return new Argument<Uri>
            {
                Arity = ArgumentArity.ExactlyOne,
                Name = "project",
                Description = "Project name."
            };
        }
        
        public static Argument<Uri> ProjectPathArgument()
        {
            return new Argument<Uri>
            {
                Arity = ArgumentArity.ExactlyOne,
                Name = "project-path",
                Description = "Path within a project. <project>/<file>"
            };
        }

        public static Option ServerUriOption()
        {
            return new Option(
                new[]
                {
                    "--server-uri",
                    "-s"
                },
                "The uri of the tilde server.",
                new Argument<Uri>(new Uri("http://localhost:5678", UriKind.RelativeOrAbsolute))
                {
                    Name = "uri"
                }
            );
        }
//        
//        public static Option ServerUriOption()
//        {
//            return new Option(
//                new[]
//                {
//                    "--server-uri",
//                    "-s"
//                },
//                "The uri the server should use to listen on.",
//                new Argument<Uri>(new Uri("http://localhost:5678", UriKind.RelativeOrAbsolute))
//                {
//                    Name = "uri"
//                }
//            );
//        }
    }
}