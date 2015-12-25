using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MimeTool
{
	public class MimeContentType
	{
		public String MimeType = String.Empty;

		public Encoding Charset = null;

		public String Name = String.Empty;

		public String Boundary = String.Empty;

		public MimeContentType(String data)
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

			this.MimeType = part;

			Int32 index = 1;

			while (index < parts.Length)
			{
				part = parts[index].Trim();

				Match find = Regex.Match(
					part,
					@"^(?<key>\w+?)\s*=\s*[""']{0,1}(?<val>[^""']+?)['""]{0,1}$",
					RegexOptions.IgnoreCase
				);

				if (find.Success)
				{
					String key = find.Groups["key"].Value.Trim().ToLower();

					String val = find.Groups["val"].Value.Trim();

					switch (key)
					{
						case "charset":

							this.Charset = Encoding.GetEncoding(Util.RefineCharset(val));

							break;

						case "boundary":

							this.Boundary = val;

							break;

						case "name":
						case "filename":

							this.Name = Regex.Replace(
								Util.DecodeString(val),
								String.Format(
									"[{0}]",
									Regex.Escape(new string(Path.GetInvalidFileNameChars()))
								),
								String.Empty

							);

							break;
					}
				}

				index++;
			}
		}
	}
}
