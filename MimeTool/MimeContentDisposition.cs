using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MimeTool
{
	public class MimeContentDisposition
	{
		public String Type = String.Empty;

		public String FileName = String.Empty;

		public MimeContentDisposition(String data)
		{
			data = data.Trim();

			if (String.IsNullOrEmpty(data))
			{
				return;
			}

			String[] parts = data.Split(
				new String[] { ";" },
				StringSplitOptions.RemoveEmptyEntries
			);

			String part = String.Empty;

			part = parts[0].Trim();

			this.Type = part;

			List<String> filenames = new List<String>();

			Int32 index = 1;

			while (index < parts.Length)
			{
				part = parts[index].Trim();

				Match find = Regex.Match(
					part,
					@"^(?<key>.+?)\s*=\s*[""']{0,1}(?<val>.+?)['""]{0,1}$",
					RegexOptions.IgnoreCase
				);

				if (find.Success)
				{
					String key = find.Groups["key"].Value.Trim().ToLower();

					String val = find.Groups["val"].Value.Trim();

					if (key.Equals("name") ||
						key.Equals("filename"))
					{
						this.FileName = Regex.Replace(
							Util.DecodeString(val),
							String.Format(
								"[{0}]",
								Regex.Escape(new string(Path.GetInvalidFileNameChars()))
								),
							String.Empty
						);

						index++;

						continue;
					}

					if (key.StartsWith("name*") ||
						key.StartsWith("filename*"))
					{
						val = Regex.Replace(val, @"^[""']", String.Empty);

						val = Regex.Replace(val, @"['""]$", String.Empty);

						filenames.Add(val);

						index++;

						continue;
					}
				}

				index++;
			}

			if (filenames.Count != 0)
			{
				String text = String.Join(String.Empty, filenames);

				Match find = Regex.Match(
					text,
					@"^(?<charset>.+?)''(?<encoded>.+?)$"
				);

				if (find.Success)
				{
					String charset = find.Groups["charset"].Value;

					String encoded = find.Groups["encoded"].Value;

					Byte[] decoded = Util.DecodeBinary(encoded);

					this.FileName = Util.DetectCharset(
						decoded,
						Encoding.GetEncoding(
							Util.RefineCharset(charset)
						)
					).GetString(decoded);
				}
				else
				{
					this.FileName = String.Empty;
				}
			}
		}
	}
}
