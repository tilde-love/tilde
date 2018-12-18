// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;

namespace Tilde.Cli
{
    public class Logo
    {
        public static void PrintLogo()
        {
            PrintLogo(0, 6); 
        }

        public static void PrintLogo(int logo)
        {
            PrintLogo(logo, logo + 1); 
        }

        public static void PrintLogo(int min, int max)
        {
            Random random = new Random(unchecked((int) (DateTime.UtcNow.Ticks & 0xFFFFFFFF)));

            int logo = random.Next(min, max);            

            switch (logo)
            {
                case 0:
                    Console.WriteLine("                                ");
                    Console.WriteLine("                                ");
                    Console.WriteLine("      ..od88bo..                ");
                    Console.WriteLine("   .d8888888888888b.    .d888b. ");
                    Console.WriteLine("  `\"8888P'   `88888888888888888'");
                    Console.WriteLine("                 `\"d8888b''     ");
                    Console.WriteLine("                                ");
                    Console.WriteLine("                                ");

                    break;

                case 1:
                    Console.WriteLine("          ....     ....     ");
                    Console.WriteLine("       .od8888b. .d8888bo.  ");
                    Console.WriteLine("      d888888888o888888888b ");
                    Console.WriteLine("      888888888888888888888 ");
                    Console.WriteLine("      `8888888888888888888' ");
                    Console.WriteLine("       `88888888888888888'  ");
                    Console.WriteLine("         `8888888888888'    ");
                    Console.WriteLine("           \"888888888P      ");
                    Console.WriteLine("              `888'         ");
                    Console.WriteLine("                \"           ");

                    break;
                
                case 2:
                    Console.WriteLine("                                     ....     ....     ");
                    Console.WriteLine("                                  .od8888b. .d8888bo.  ");
                    Console.WriteLine("      ..od88bo..                 d888888888o888888888b ");
                    Console.WriteLine("   .d8888888888888b.    .d888b.  888888888888888888888 ");
                    Console.WriteLine("  `\"8888P'   `88888888888888888' `8888888888888888888' ");
                    Console.WriteLine("                 `\"d8888b''       `88888888888888888'  ");
                    Console.WriteLine("                                    `8888888888888'    ");
                    Console.WriteLine("                                      \"888888888P      ");
                    Console.WriteLine("                                         `888'         ");
                    Console.WriteLine("                                           \"           ");

                    break;

                case 3:
                    Console.WriteLine("  ");
                    Console.WriteLine("                                 dP                           ");
                    Console.WriteLine("      ..od88bo..                 88                           ");
                    Console.WriteLine("   .d8888888888888b.    .d888b.  88 .d8888b. dP   .dP .d8888b.");
                    Console.WriteLine("  `\"8888P'   `88888888888888888' 88 88'  `88 88   d8' 88ooood8");
                    Console.WriteLine("                 `\"d8888b''      88 88.  .88 88 .88'  88.  ...");
                    Console.WriteLine("                                 dP `88888P' 8888P'   `88888P'");
                    Console.WriteLine("  ");

                    break;

                case 4:
                    Console.WriteLine("                                     ....     ....     ");
                    Console.WriteLine("    dP   oo dP       dP           .od8888b. .d8888bo.  ");
                    Console.WriteLine("    88      88       88          d888888888o888888888b ");
                    Console.WriteLine("  d8888P dP 88 .d888b88 .d8888b. 888888888888888888888 ");
                    Console.WriteLine("    88   88 88 88'  `88 88ooood8 `8888888888888888888' ");
                    Console.WriteLine("    88   88 88 88.  .88 88.  ...  `88888888888888888'  ");
                    Console.WriteLine("    dP   dP dP `88888P8 `88888P'    `8888888888888'    ");
                    Console.WriteLine("                                      \"888888888P      ");
                    Console.WriteLine("                                         `888'         ");
                    Console.WriteLine("                                           \"           ");

                    break;

                case 5:
                    Console.WriteLine("");
                    Console.WriteLine("    dP   oo dP       dP          dP                           ");
                    Console.WriteLine("    88      88       88          88                           ");
                    Console.WriteLine("  d8888P dP 88 .d888b88 .d8888b. 88 .d8888b. dP   .dP .d8888b.");
                    Console.WriteLine("    88   88 88 88'  `88 88ooood8 88 88'  `88 88   d8' 88ooood8");
                    Console.WriteLine("    88   88 88 88.  .88 88.  ... 88 88.  .88 88 .88'  88.  ...");
                    Console.WriteLine("    dP   dP dP `88888P8 `88888P' dP `88888P' 8888P'   `88888P'");
                    Console.WriteLine("");

                    break;            
            }
        }
    }
}