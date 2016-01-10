using System;
using System.Text.RegularExpressions;

namespace MimeTool
{
	public class MimeAddress
	{
		public String Display = String.Empty;

		public String Address = String.Empty;

		public MimeAddress(String data)
		{
			data = data.Trim();

			String[] parts = data.Split(new String[] { "<" }, StringSplitOptions.None);

			// 如果無法切成二份，代表可能為以下格式
			// "xxx"
			// xxx
			// xxx@xxx

			if (parts.Length == 1)
			{
				Match find;

				Boolean address = false;

				Boolean dispaly = false;

				find = Regex.Match(parts[0], @"^[""']+(?<name>.+)['""]+$", RegexOptions.IgnoreCase);

				if (find.Success)
				{
					String text;

					text = find.Groups["name"].Value.Trim();
					
					this.Display = text;

					return;
				}

				find = Regex.Match(parts[0], @"^=\?(?<charset>.+?)\?(?<encoding>.)\?(?<encoded>.+)", RegexOptions.IgnoreCase);

				if (find.Success)
				{
					String text;

					text = find.Groups[0].Value.Trim();

					if (!text.EndsWith("?="))
					{
						text = String.Format("{0}?=", text);
					}

					this.Display = Util.DecodeString(text);

					return;
				}

				find = Regex.Match(parts[0], @"^<*.+@.+>*$", RegexOptions.IgnoreCase);

				if (find.Success)
				{
					String text;

					text = find.Groups[0].Value.Trim();

					this.Address = text;

					return;
				}
			}

			// 如果二部分，代表為以下格式
			// <xxx@xxx>
			// xxx<xxx@xxx>
			// "xxx"<xxx@xxx>

			if (parts.Length == 2)
			{
				String text;

				text = parts[0].Trim();

				text = Regex.Replace(text, @"^[""']*", String.Empty);

				text = Regex.Replace(text, @"['""]*$", String.Empty);

				text = Util.DecodeString(text);

				text = Regex.Replace(text, @"^[""']*", String.Empty);

				text = Regex.Replace(text, @"['""]*$", String.Empty);

				this.Display = text;

				text = parts[1].Trim();

				text = Regex.Replace(text, @"^<", String.Empty);

				text = Regex.Replace(text, @">$", String.Empty);

				this.Address = text;
			}
		}
	}
}
