#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif __IOS__ || __TVOS__
using Foundation;
using UIKit;
#endif
#endregion
using System.Runtime.Serialization.Formatters.Binary;
namespace ahjas
{
#if __IOS__ || __TVOS__
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
#else
	static class Program
#endif
	{
		private static Game1 game;
		private static Game2 menu;
		private static Game3 winLoss;
		public static bool win;
		public static bool NewGame;
		internal static void RunGame()
		{
			game = new Game1(!NewGame);
			game.Run();
#if !__IOS__ && !__TVOS__
			game.Dispose();
#endif
		}
		internal static void RunGame3()
		{
			winLoss = new Game3(win);
			winLoss.Run();
#if !__IOS__ && !__TVOS__
			winLoss.Dispose();
#endif
		}
		internal static void RunGame2()
		{
			menu = new Game2();
			menu.Run();
#if !__IOS__ && !__TVOS__
			menu.Dispose();
#endif
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
#if !MONOMAC && !__IOS__ && !__TVOS__
		[STAThread]
#endif
		static void Main(string[] args)
		{
#if MONOMAC
            NSApplication.Init ();

            using (var p = new NSAutoreleasePool ()) {
                NSApplication.SharedApplication.Delegate = new AppDelegate();
                NSApplication.Main(args);
            }
#elif __IOS__ || __TVOS__
            UIApplication.Main(args, null, "AppDelegate");
#else
			RunGame2();
			RunGame();
			RunGame3();
#endif
		}

#if __IOS__ || __TVOS__
        public override void FinishedLaunching(UIApplication app)
        {
            RunGame();
        }
#endif
	}

#if MONOMAC
    class AppDelegate : NSApplicationDelegate
    {
        public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs a) =>  {
                if (a.Name.StartsWith("MonoMac")) {
                    return typeof(MonoMac.AppKit.AppKitFramework).Assembly;
                }
                return null;
            };
            Program.RunGame();
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
        {
            return true;
        }
    }  
#endif
}
