using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using riseofthepython.Champions;
using riseofthepython.Core;

namespace riseofthepython
{    
	public class Program
	{
		public static Obj_AI_Hero player { get { return ObjectManager.Player; } }
		public static Orbwalking.Orbwalker orbwalker;
		public static Spell Q, Q2, W, W2, E, E2, R;
		public static Menu menu;
		public static SpellSlot Ignite;
		static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}

		static void Game_OnGameLoad(EventArgs args)
		{
			menu = new Menu("ROP - " + player.ChampionName, player.ChampionName, true);
			Menu OrbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
			orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);
			menu.AddSubMenu(OrbwalkerMenu);
			Menu TargetSelectorMenu = new Menu ("Target Selector", "TargetSelector");
			menu.AddSubMenu(TargetSelectorMenu);
			TargetSelector.AddToMenu(TargetSelectorMenu);
		
			switch(player.ChampionName)
			{
			case "Cassiopeia":
				new Cassiopeia();
				break;
			}

		}

	}
}
