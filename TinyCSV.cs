using System.Collections.Generic;
using System.Text;

namespace TinyCSV
{
	#region 自定义事件接口
	public interface Handler
	{
		void Start();
		void End();
		bool Value(string key, string value);
	}
	#endregion

	public class Parser
	{
		private readonly StringBuilder builder;
		private readonly List<string> titles;
		private bool loading;
		private int index;
		private int length;
		private int row;
		private int column;

		#region 解析选项
		public struct Options
		{
			public char space;
			public int title;
			public int context;
		}

		private readonly Options options;

		public Parser() : this(new Options {space = ',', title = 1, context = 2}) { }
		public Parser(char space) : this(new Options {space = space, title = 1, context = 2}) { }
		public Parser(int title, int context) : this(new Options {space = ',', title = title, context = context}) { }

		public Parser(char space, int title, int context) : this(new Options {
			space = space,
			title = title,
			context = context
		}) { }

		public Parser(Options options)
		{
			this.options = options;
			this.builder = new StringBuilder();
			this.titles = new List<string>();
			this.loading = false;
		}
		#endregion

		#region 内部实现
		private void ReadUnescape(string input, StringBuilder builder)
		{
			while (index < length)
			{
				char c = input[index];
				if (c == options.space || c == '\r' || c == '\n')
					break;
				if (builder != null)
					builder.Append(c);
				++index;
			}
		}

		private bool ReadEscape(string input, ref int index, StringBuilder builder)
		{
			++index;
			bool quote = false;
			while (index < length)
			{
				char c = input[index];
				if (quote)
				{
					if (c == options.space || c == '\r' || c == '\n')
						break;
					if (c != '"')
						return false;
					if (builder != null)
						builder.Append('"');
					quote = false;
				}
				else
				{
					if (c == '"')
					{
						quote = true;
					}
					else
					{
						if (builder != null)
							builder.Append(c);
					}
				}
				++index;
			}
			return true;
		}

		private void Read(string input, StringBuilder builder)
		{
			if (input[index] == '"')
			{
				int i = index;
				if (ReadEscape(input, ref i, builder))
				{
					index = i;
					return;
				}
				if (builder != null)
					builder.Length = 0;
			}
			ReadUnescape(input, builder);
		}

		private void SkipRow(string input)
		{
			column = 0;
			while (true)
			{
				if (index >= length)
					break;
				Read(input, null);
				++column;
				if (index >= length)
					break;
				if (input[index] == options.space)
				{
					++index;
					continue;
				}
				if (input[index] == '\r')
					++index;
				if (index < length && input[index] == '\n')
					++index;
				break;
			}
		}
		#endregion

		#region 对外解析接口
		public bool Load(string input, Handler handler)
		{
			if (loading)
				return false;
			loading = true;
			builder.Length = 0;
			titles.Clear();
			index = 0;
			row = 1;
			length = input.Length;
			while (row < options.title)
			{
				SkipRow(input);
				++row;
			}
			column = 0;
			while (true)
			{
				if (index >= length)
					break;
				Read(input, builder);
				++column;
				titles.Add(builder.ToString());
				builder.Length = 0;
				if (index >= length)
					break;
				if (input[index] == options.space)
				{
					++index;
					continue;
				}
				if (input[index] == '\r')
					++index;
				if (index < length && input[index] == '\n')
					++index;
				break;
			}
			++row;
			while (row < options.context)
			{
				SkipRow(input);
				++row;
			}
			while (index < length)
			{
				column = 0;
				while (true)
				{
					if (index >= length)
						break;
					if (column < titles.Count)
					{
						Read(input, builder);
						string value = builder.ToString();
						builder.Length = 0;
						if (column == 0)
						{
							if (value.Length == 0 && index >= length)
								break;
							handler.Start();
						}
						if (!handler.Value(titles[column], value))
						{
							loading = false;
							return false;
						}
						++column;
					}
					else
					{
						Read(input, null);
					}
					if (index >= length)
						break;
					if (input[index] == options.space)
					{
						++index;
						continue;
					}
					if (input[index] == '\r')
						++index;
					if (index < length && input[index] == '\n')
						++index;
					++row;
					break;
				}
				if (column > 0)
					handler.End();
			}
			loading = false;
			return true;
		}

		public bool Load(byte[] input, Handler handler)
		{
			int start = 0;
			if (input.Length >= 3 && input[0] == 0xEF && input[1] == 0xBB && input[2] == 0xBF)
			{
				start += 3;
			}
			return Load(input, start, handler);
		}

		public bool Load(byte[] input, int start, Handler handler)
		{
			return Load(input, start, input.Length - start, handler);
		}

		public bool Load(byte[] input, int start, int count, Handler handler)
		{
			return Load(Encoding.UTF8.GetString(input, start, count), handler);
		}
		#endregion

		public int Row
		{
			get { return row; }
		}

		public int Column
		{
			get { return column + 1; }
		}
	}
}