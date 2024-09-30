using System;
using MCGalaxy.Commands;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Maths;
using System.Collections.Generic;
namespace MCGalaxy
{
    public class DayNightCycle : Plugin
    {
        public override string name { get { return "DayNightCycle"; } }
        public override string MCGalaxy_Version { get { return "1.9.3.0"; } }
        public override string creator { get { return "Venk"; } }

        public static int timeOfDay = 0;
        public static SchedulerTask Task;

        public override void Load(bool startup)
        {
            Command.Register(new CmdSetTime());
			Command.Register(new CmdGetTime());
            Server.MainScheduler.QueueRepeat(DoDayNightCycle, null, TimeSpan.FromMilliseconds(100));
        }
		
        public override void Unload(bool shutdown)
        {
            Command.Unregister(Command.Find("SetTime"));
			Command.Unregister(Command.Find("GetTime"));
            Server.MainScheduler.Cancel(Task);
        }
		public static Dictionary<int, string> SkyColors = new Dictionary<int, string>()
        {
			{0,   "#709DED"},
			{10,  "#78A9FF"},
			{60,  "#78A9FF"},
			{80,  "#78A9FF"},
			{90,  "#78A9FF"},
			{95,  "#78A9FF"},
			{96,  "#78A9FF"},
			{97,  "#78A9FF"},
			{100, "#78A9FF"},
			{120, "#73a4fa"},
			{130, "#486599"},
			{135, "#3C547F"},
			{140, "#2D4061"},
			{145, "#212E46"},
			{180, "#000000"},
			{200, "#000000"},
			{223, "#304365"},
			{230, "#3C547F"},
			{232, "#3C547F"},
			{234, "#4C6BA2"},
			{238, "#6F9CEC"},
		};
		public static Dictionary<int, string> CloudColors = new Dictionary<int, string>()
        {
			{0,   "#FFFFFF"},
			{10,  "#FFFFFF"},
			{60,  "#FFFFFF"},
			{80,  "#FFFFFF"},
			{90,  "#FFFFFF"},
			{100, "#FFFFFF"},
			{120, "#FFFFFF"},
			{127, "#926864"},
			{130, "#2D4061"},
			{180, "#000000"},
			{223, "#212E46"},
			{230, "#D15F36"},
			{232, "#D15F36"},
			{234, "#D15F36"},
			{238, "#D15F36"},
		};
		public static Dictionary<int, string> LightingColors = new Dictionary<int, string>()
        {
			{0,   "#FFFFFF"},
			{10,  "#FFFFFF"},
			{60,  "#FFFFFF"},
			{80,  "#FFFFFF"},
			{90,  "#FFFFFF"},
			{100, "#FFFFFF"},
			{112, "#FFFFFF"},
			{120, "#FFFFFF"},
			{130, "#5c5c5c"},
			{180, "#333333"},
			{190, "#333333"},
			{200, "#333333"},
			{210, "#333333"},
			{220, "#333333"},
			{230, "#5c5c5c"},
			{234, "#fac3af"},
			{238, "#fac3af"},
		};
			public static Dictionary<int, string> ShadowColors = new Dictionary<int, string>()
        {
			{0,   "#9B9B9B"},
			{10,  "#9B9B9B"},
			{60,  "#9B9B9B"},
			{80,  "#9B9B9B"},
			{90,  "#9B9B9B"},
			{100, "#9B9B9B"},
			{112, "#9B9B9B"},
			{120, "#9B9B9B"},
			{130, "#2b2b2b"},
			{180, "#1a1a1a"},
			{190, "#1a1a1a"},
			{200, "#1a1a1a"},
			{210, "#1a1a1a"},
			{220, "#1a1a1a"},
			{230, "#2b2b2b"},
			{234, "#333333"},
			{238, "#757575"},
		};
		public static Dictionary<int, string> FogColors = new Dictionary<int, string>()
        {
			{0,   "#FFFFFF"},
			{10,  "#FFFFFF"},
			{60,  "#FFFFFF"},
			{70,  "#FFFFFF"},
			{80,  "#FFFFFF"},
			{90,  "#ffeae3"},
			{100, "#ffeae3"},
			{110, "#ffeae3"},
			{115, "#ffc0ab"},
			{120, "#d18469"},
			{130, "#3b3b3b"},
			{140, "#0f0f0f"},
			{150, "#0f0f0f"},
			{160, "#0f0f0f"},
			{170, "#0f0f0f"},
			{180, "#0f0f0f"},
			{223, "#0f0f0f"},
			{230, "#D15F36"},
			{232, "#D15F36"},
			{234, "#D15F36"},
			{238, "#D15F36"},
		};
		ColorDesc GetColor(Dictionary<int, string> list, float hour)
		{
			int dist = 1005;
			int oldColorKey = 0;
			int newColorKey = 0;
			foreach (var pair in list)
			{
				int d = (int)Math.Abs(pair.Key - hour);
				if (d < dist)
				{
					dist = d;
					oldColorKey = pair.Key;
				}
			}
			if (oldColorKey > 238)
			{
				newColorKey = 0;
			}
			else
			{
				foreach (var pair in list)
				{
					if (pair.Key > oldColorKey)
					{
						newColorKey = pair.Key;
						break;
					}
				}
			}
			
			ColorDesc oldColor = default(ColorDesc);
			ColorDesc newColor = default(ColorDesc);
		
			CommandParser.GetHex(Player.Console,list[oldColorKey] ,ref oldColor);
			if (true)
			{
				return oldColor;
			}
			if (list[oldColorKey] == list[newColorKey])
			{
				return oldColor;
			}
			CommandParser.GetHex(Player.Console,list[newColorKey] ,ref newColor);
			
			float progress = ((float)hour - (float)oldColorKey) / ((float)newColorKey - (float)oldColorKey);
			return LerpColor(oldColor, newColor, progress);
		}
		ColorDesc GetSkyColor(float hour)
		{
			return GetColor(SkyColors, hour);
		}
		ColorDesc GetCloudColor(float hour)
		{
			return GetColor(CloudColors, hour);
		}
		ColorDesc GetFogColor(float hour)
		{
			return GetColor(FogColors, hour);
		}
		ColorDesc GetSunColor(float hour)
		{
			return GetColor(LightingColors, hour);
		}
		ColorDesc GetShadowColor(float hour)
		{
			return GetColor(ShadowColors, hour);
		}
		float Lerp(float firstFloat, float secondFloat, float by)
		{
			 return firstFloat * (1 - by) + secondFloat * by;
		}
		byte LerpColourChannel(int col1, int col2, float t)
		{
			if (col1 == col2)
			{
				return (byte)col1;
			}
			int result = (int)Lerp((float)col1, (float)col2, t);//(int)((col2 -col1) * t + col1);
			if (result < 0)
			{
				result = 0;
			}
			if (result > 256)
			{
				result = 256;
			}
			return (byte)result;
		}
		ColorDesc LerpColor(ColorDesc col1, ColorDesc col2, float t)
		{
			return new ColorDesc(
			LerpColourChannel(col1.R,col2.R,t),
			LerpColourChannel(col1.G,col2.G,t),
			LerpColourChannel(col1.B,col2.B,t)
			);	
		}

        void DoDayNightCycle(SchedulerTask task)
        {
            if (timeOfDay > 23999) timeOfDay = 0;
            else timeOfDay += 1;

            Player[] players = PlayerInfo.Online.Items;

            foreach (Player pl in players)
            {
                if (!pl.level.Config.MOTD.Contains("daynightcycle=true")) continue;
				
				float hour = (timeOfDay / 100) ;
				
                ColorDesc sky    = GetSkyColor   (hour);
                ColorDesc cloud  = GetCloudColor (hour); 
                ColorDesc fog    = GetFogColor   (hour);
				ColorDesc sun    = GetSunColor   (hour);
				ColorDesc shadow = GetShadowColor(hour);
				
                pl.Send(Packet.EnvColor(0, sky.R, sky.G, sky.B));
				pl.Send(Packet.EnvColor(1, cloud.R, cloud.G, cloud.B));
				pl.Send(Packet.EnvColor(2, fog.R, fog.G, fog.B));
				pl.Send(Packet.EnvColor(3, shadow.R, shadow.G, shadow.B));
				pl.Send(Packet.EnvColor(4, sun.R, sun.G, sun.B));
            }

            Task = task;
        }
    }

    public sealed class CmdSetTime : Command2
    {
        public override string name { get { return "SetTime"; } }
        public override string shortcut { get { return "timeset"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0) { Help(p); return; }
            string[] args = message.SplitSpaces();

            DayNightCycle.timeOfDay = int.Parse(args[0]);
            p.Message("%STime set to: %b" + DayNightCycle.timeOfDay + "%S.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/SetTime [tick] - %HSets the day-night cycle time to [tick].");
        }
    }
	public sealed class CmdGetTime : Command2
    {
        public override string name { get { return "GetTime"; } }
        public override string shortcut { get { return "timeget"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data)
        {
            p.Message("%STime is: %b" + DayNightCycle.timeOfDay + "%S.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/GetTime - Gets the current time.");
        }
    }
}
