using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace riseofthepython.Core
{
	internal class PythonManager : Assemblyhelper
	{
		public PythonManager()
		{
			Load();
		}

		private static void Load()
		{
			menu.AddSubMenu(new Menu("Python Manager", "Menu Python"));
			menu.SubMenu("MenuPython").AddItem(new MenuItem("PythonActive", "Active").SetValue(true));

			foreach (
				var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
			{
				menu.SubMenu("MenuPython")
					.AddItem(
						new MenuItem("Python" + enemy.ChampionName, enemy.ChampionName).SetValue(
							TargetSelector.GetPriority(enemy) > 4));
			}


			menu.SubMenu("MenuPython")
				.AddItem(
					new MenuItem("PythonSelectOption", "Set: ").SetValue(
						new StringList(new[] { "Single Select", "Multi Select" })));
			menu.SubMenu("MenuPython")
				.AddItem(new MenuItem("PythonSetClick", "Add/Remove with click").SetValue(true));
			menu.SubMenu("Menu")
				.AddItem(
					new MenuItem("Reset", "Reset List").SetValue(
						new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

			menu.SubMenu("Menu").AddSubMenu(new Menu("Draw:", "Draw"));

			menu.SubMenu("Menu")
				.SubMenu("Draw")
				.AddItem(new MenuItem("DrawSearch", "Search Range").SetValue(new Circle(true, Color.GreenYellow)));
			menu.SubMenu("Menu")
				.SubMenu("Draw")
				.AddItem(new MenuItem("DrawActive", "Active Enemy").SetValue(new Circle(true, Color.GreenYellow)));
			menu.SubMenu("Menu")
				.SubMenu("Draw")
				.AddItem(new MenuItem("DrawNearest", "Nearest Enemy").SetValue(new Circle(true, Color.DarkSeaGreen)));
			menu.SubMenu("Menu")
				.SubMenu("Draw")
				.AddItem(new MenuItem("DrawStatus", "Show Status").SetValue(true));


			menu.SubMenu("Menu")
				.AddItem(new MenuItem("SearchRange", "Search Range"))
				.SetValue(new Slider(1000, 2000));

			Game.OnUpdate += OnGameUpdate;
			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnWndProc += Game_OnWndProc;
		}

		private static void ClearList()
		{
			foreach (
				var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
			{
				menu.Item("" + enemy.ChampionName).SetValue(false);
			}
		}

		private static void OnGameUpdate(EventArgs args) {}

		private static void Game_OnWndProc(WndEventArgs args)
		{
			if (menu.Item("Reset").GetValue<KeyBind>().Active && args.Msg == 257)
			{
				ClearList();
				Notifications.AddNotification(" List is resetted.", 5);
			}

			if (args.Msg != (uint) WindowsMessages.WM_LBUTTONDOWN)
			{
				return;
			}

			if (menu.Item("SetClick").GetValue<bool>())
			{
				foreach (var objAiHero in from hero in ObjectManager.Get<Obj_AI_Hero>()
					where hero.IsValidTarget()
					select hero
					into h
					orderby h.Distance(Game.CursorPos) descending
					select h
					into enemy
					where enemy.Distance(Game.CursorPos) < 150f
					select enemy)
				{
					if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
					{
						var xSelect = menu.Item("SelectOption").GetValue<StringList>().SelectedIndex;

						switch (xSelect)
						{
						case 0:
							ClearList();
							menu.Item("" + objAiHero.ChampionName).SetValue(true);
							Notifications.AddNotification(
								"Added " + objAiHero.ChampionName + " to  List", 5);
							break;
						case 1:
							var menuStatus = menu.Item("" + objAiHero.ChampionName).GetValue<bool>();
							menu.Item("" + objAiHero.ChampionName).SetValue(!menuStatus);

							//Notifications.AddNotification("Removed " + objAiHero.ChampionName + " to  List", 5);
							Game.PrintChat(
								string.Format(
									"<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
									!menuStatus ? "#FFFFFF" : "#FF8877",
									!menuStatus ? "Added to  List:" : "Removed from  List:",
									objAiHero.Name, objAiHero.ChampionName));
							break;
						}
					}
				}
			}
		}

		private static void Drawing_OnDraw(EventArgs args)
		{
			if (!menu.Item("Active").GetValue<bool>())
			{
				return;
			}

			if (menu.Item("DrawStatus").GetValue<bool>())
			{
				var enemies = ObjectManager.Get<Obj_AI_Hero>().Where(xEnemy => xEnemy.IsEnemy);
				var objAiHeroes = enemies as Obj_AI_Hero[] ?? enemies.ToArray();
				Drawing.DrawText(Drawing.Width * 0.89f, Drawing.Height * 0.58f, Color.GreenYellow, " Status");
				Drawing.DrawText(Drawing.Width * 0.89f, Drawing.Height * 0.58f, Color.GhostWhite, "_____________");
				for (int i = 0; i < objAiHeroes.Count(); i++)
				{
					var xCaption = objAiHeroes[i].ChampionName;
					var xWidth = Drawing.Width * 0.90f;
					if (menu.Item("" + objAiHeroes[i].ChampionName).GetValue<bool>())
					{
						xCaption = "+ " + xCaption;
						xWidth = Drawing.Width * 0.8910f;
					}
					Drawing.DrawText(xWidth, Drawing.Height * 0.58f + (float) (i + 1) * 15, Color.Gainsboro, xCaption);
				}
			}

			var drawSearch = menu.Item("DrawSearch").GetValue<Circle>();
			var drawActive = menu.Item("DrawActive").GetValue<Circle>();
			var drawNearest = menu.Item("DrawNearest").GetValue<Circle>();

			var drawSearchRange = menu.Item("SearchRange").GetValue<Slider>().Value;
			if (drawSearch.Active)
			{
				Render.Circle.DrawCircle(ObjectManager.Player.Position, drawSearchRange, drawSearch.Color);
			}

			foreach (var enemy in
				ObjectManager.Get<Obj_AI_Hero>()
				.Where(enemy => enemy.Team != ObjectManager.Player.Team)
				.Where(
					enemy =>
					enemy.IsVisible && menu.Item("" + enemy.ChampionName) != null &&
					!enemy.IsDead)
				.Where(enemy => menu.Item("" + enemy.ChampionName).GetValue<bool>()))
			{
				if (ObjectManager.Player.Distance(enemy) < drawSearchRange)
				{
					if (drawActive.Active)
					{
						Render.Circle.DrawCircle(enemy.Position, 85f, drawActive.Color);
					}
				}
				else if (ObjectManager.Player.Distance(enemy) > drawSearchRange &&
					ObjectManager.Player.Distance(enemy) < drawSearchRange + 400)
				{
					if (drawNearest.Active)
					{
						Render.Circle.DrawCircle(enemy.Position, 85f, drawNearest.Color);
					}
				}
			}
		}
	}
}

