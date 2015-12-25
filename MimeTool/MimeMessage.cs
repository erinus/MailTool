using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MimeTool
{
	public class MimeMessage : MimeEntity
	{
		public DateTime Date;

		public String Subject = String.Empty;

		public String TextBody = String.Empty;

		public String HtmlBody = String.Empty;

		public MimeAddressCollection From = new MimeAddressCollection();

		public MimeAddressCollection To = new MimeAddressCollection();

		public MimeAddressCollection Cc = new MimeAddressCollection();

		public MimeAddressCollection Bcc = new MimeAddressCollection();

		public MimeMessage()
		{

		}

		public override void Parse()
		{
			base.Parse();

			if (!this.Success)
			{
				return;
			}

			this.Success = false;

			foreach (MimeHeader header in this.Headers)
			{
				String name = header.Name.ToLower();

				String data = header.Data;

				if (name.Equals("date"))
				{
					String text = header.Data;

					text = text.Trim();

					Match find = Regex.Match(text, @"(?<datetime>[0-9]{2} [a-zA-Z]{3} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2})( (?<timezone>[+-]*[0-9]+))*");

					if (find.Success)
					{
						String datetime = find.Groups["datetime"].Value;

						String timezone = find.Groups["timezone"].Value;

						if (Regex.IsMatch(timezone, @"^[+-]"))
						{
							if (timezone.Length != 5)
							{
								timezone = String.Empty;
							}
						}
						else
						{
							if (timezone.Length != 4)
							{
								timezone = String.Empty;
							}
						}

						text = String.Format("{0} {1}", datetime, timezone);

						this.Date = DateTime.Parse(
							text,
							CultureInfo.InvariantCulture,
							DateTimeStyles.None
						);
					}
				}

				if (name.Equals("subject"))
				{
					this.Subject = Util.DecodeString(header.Data);
				}

				if (name.Equals("from"))
				{
					this.From.AddRange(new MimeAddressCollection(data));
				}

				if (name.Equals("to"))
				{
					this.To.AddRange(new MimeAddressCollection(data));
				}

				if (name.Equals("cc"))
				{
					this.Cc.AddRange(new MimeAddressCollection(data));
				}

				if (name.Equals("bcc"))
				{
					this.Bcc.AddRange(new MimeAddressCollection(data));
				}
			}

			if (this.ContentType == null)
			{
				this.TextBody = String.Join("\n", this.RawBody);
			}
			else
			{
				if (this.ContentType.MimeType.StartsWith("text"))
				{
					if (this.ContentType.MimeType.Equals("text/plain"))
					{
						this.TextBody = this.StringContent;
					}

					if (this.ContentType.MimeType.Equals("text/html"))
					{
						this.HtmlBody = this.StringContent;
					}
				}

				if (this.ContentType.MimeType.StartsWith("multipart"))
				{
					this.SearchBodies(this.PartContents);
				}
			}

			this.Success = true;
		}

		private void SearchBodies(List<MimePart> parts)
		{
			foreach (MimePart part in parts)
			{
				if (part.ContentType != null)
				{
					if (part.ContentType.MimeType.StartsWith("text"))
					{
						if (String.IsNullOrEmpty(this.TextBody) &&
							part.ContentType.MimeType.Equals("text/plain"))
						{
							this.TextBody = part.StringContent;
						}

						if (String.IsNullOrEmpty(this.HtmlBody) &&
							part.ContentType.MimeType.Equals("text/html"))
						{
							this.HtmlBody = part.StringContent;
						}
					}

					if (part.ContentType.MimeType.StartsWith("message"))
					{

					}

					if (part.ContentType.MimeType.StartsWith("multipart"))
					{
						this.SearchBodies(part.PartContents);
					}
				}
			}
		}
	}
}
