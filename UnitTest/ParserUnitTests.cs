using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TinyCSV;

namespace TinyJSON.UnitTests
{
	[TestClass()]
	public class ParserUnitTests
	{
		[TestMethod()]
		public void LoadUnitTest()
		{
			const string input = @"
name,model,animation,scale,radius,height,fly,speed.run,speed.walk,effects.birth,effects.visual,effects.death
1,Imports/Model/Player.prefab,,1,1,2,,,,,,
";
			Parser parser = new Parser(new Parser.Options {space = ',', title = 2, context = 3});
			List<DisplayDefine> list = new List<DisplayDefine>();
			Assert.IsTrue(parser.Load(input, new Handler_DisplayDefine(list)));
			Assert.IsTrue(list.Count == 1);
		}

		public class DisplayDefine
		{
			public int name;
			public string model;
			public string animation;
			public float scale;
			public float radius;
			public float height;
			public float fly;
			public Speed speed;
			public Effects effects;

			public struct Speed
			{
				public float run;
				public float walk;
			}

			public struct Effects
			{
				public string birth;
				public string visual;
				public string death;
			}

			public DisplayDefine()
			{
				speed.run = 4;
			}
		}

		public class Handler_DisplayDefine : Handler
		{
			public readonly List<DisplayDefine> list;
			public DisplayDefine define;

			public Handler_DisplayDefine(List<DisplayDefine> list)
			{
				this.list = list;
			}

			public void Start()
			{
				define = new DisplayDefine();
			}

			public void End()
			{
				list.Add(define);
			}

			public bool Value(string key, string value)
			{
				if (value == "")
					return true;
				switch (key)
				{
				case "animation":
					return TryParse(value, out define.animation);
				case "effects.birth":
					return TryParse(value, out define.effects.birth);
				case "effects.death":
					return TryParse(value, out define.effects.death);
				case "effects.visual":
					return TryParse(value, out define.effects.visual);
				case "fly":
					return TryParse(value, out define.fly);
				case "height":
					return TryParse(value, out define.height);
				case "model":
					return TryParse(value, out define.model);
				case "name":
					return TryParse(value, out define.name);
				case "radius":
					return TryParse(value, out define.radius);
				case "scale":
					return TryParse(value, out define.scale);
				case "speed.run":
					return TryParse(value, out define.speed.run);
				case "speed.walk":
					return TryParse(value, out define.speed.walk);
				}
				return true;
			}
		}

		private static bool TryParse(string value, out string s)
		{
			s = value;
			return true;
		}

		private static bool TryParse(string value, out byte b)
		{
			return byte.TryParse(value, out b);
		}

		private static bool TryParse(string value, out sbyte s)
		{
			return sbyte.TryParse(value, out s);
		}

		private static bool TryParse(string value, out short s)
		{
			return short.TryParse(value, out s);
		}

		private static bool TryParse(string value, out ushort u)
		{
			return ushort.TryParse(value, out u);
		}

		private static bool TryParse(string value, out int i)
		{
			return int.TryParse(value, out i);
		}

		private static bool TryParse(string value, out uint u)
		{
			return uint.TryParse(value, out u);
		}

		private static bool TryParse(string value, out long l)
		{
			return long.TryParse(value, out l);
		}

		private static bool TryParse(string value, out ulong u)
		{
			return ulong.TryParse(value, out u);
		}

		private static bool TryParse(string value, out float f)
		{
			return float.TryParse(value, out f);
		}

		private static bool TryParse(string value, out double d)
		{
			return double.TryParse(value, out d);
		}

		private static bool TryParse(string value, out bool b)
		{
			string str = value.Trim().ToUpper();
			if (str == "Y" || str == "T" || str == "YES" || str == "TRUE")
			{
				b = true;
			}
			else if (str == "N" || str == "F" || str == "NO" || str == "FALSE")
			{
				b = false;
			}
			else
			{
				int i;
				if (int.TryParse(value, out i))
				{
					b = i != 0;
				}
				else
				{
					b = false;
					return false;
				}
			}
			return true;
		}

		private static bool TryParse<T>(string value, out T t) where T : struct
		{
			Type type = typeof(T);
			if (type.IsEnum)
			{
				try
				{
					t = (T)Enum.Parse(type, value, false);
					return true;
				}
				catch (Exception)
				{
				}
			}
			t = default(T);
			return false;
		}
	}
}
