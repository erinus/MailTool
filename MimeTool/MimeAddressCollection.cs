using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MimeTool
{
	public class MimeAddressCollection : List<MimeAddress>
	{
		public MimeAddressCollection()
		{

		}

		public MimeAddressCollection(String data)
		{
			data = data.Trim();

			List<Char> item = new List<Char>();

			Stack<Char> pair = new Stack<Char>();

			foreach (Char code in data)
			{
				if (pair.Count == 0)
				{
					if (code.Equals('"'))
					{
						pair.Push(code);
					}

					if (code.Equals(','))
					{
						this.Add(new MimeAddress(new String(item.ToArray())));

						item.Clear();

						continue;
					}
				}
				else
				{
					if (code.Equals('"'))
					{
						if (pair.Peek().Equals(code))
						{
							pair.Pop();
						}
						else
						{
							pair.Push(code);
						}
					}
				}

				item.Add(code);
			}

			if (item.Count != 0)
			{
				this.Add(new MimeAddress(new String(item.ToArray())));
			}
		}
	}
}
