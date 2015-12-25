using System;
using System.Text.RegularExpressions;

namespace MimeTool
{
	public class MimeHeader
	{
		public String Name = String.Empty;

		public String Data = String.Empty;

		public MimeHeader(String data)
		{
			Match find = Regex.Match(data, @"^(?<name>.+?)\s*:\s*(?<data>.*)$", RegexOptions.Singleline);

			this.Name = find.Groups["name"].Value.Trim();

			this.Data = find.Groups["data"].Value.Trim();
		}
	}
}
