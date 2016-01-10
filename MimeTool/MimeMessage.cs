using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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

		public MimePartCollection Attachments = new MimePartCollection();

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

		public void Save(Stream stream)
		{
			List<String> names = new List<String> {

				"date", "subject", "from", "to", "cc", "bcc", "content-type"

			};

			StreamWriter writer = new StreamWriter(stream);

			foreach (MimeHeader header in this.Headers)
			{
				if (names.Contains(header.Name.ToLower()))
				{
					continue;
				}

				writer.WriteLine(String.Format("{0}: {1}", header.Name, header.Data));
			}

			writer.WriteLine(

				String.Format(

					@"Date: {0}",

					this.Date.ToLocalTime().ToString(

						"ddd, dd MMM yyyy HH:mm:ss +0800",

						CultureInfo.CreateSpecificCulture("en-US")

					)

				)

			);

			writer.WriteLine(

				String.Format(

					@"Subject: =?UTF-8?B?{0}?=",

					Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Subject))

				)

			);

			List<String> addresses = new List<String>();

			addresses.Clear();

			foreach (MimeAddress address in this.From)
			{
				addresses.Add(

					String.Format(

						@"=?UTF-8?B?{0}?= <{1}>",

						Convert.ToBase64String(Encoding.UTF8.GetBytes(address.Display)),

						address.Address

					)

				);
			}

			writer.WriteLine(String.Format(@"From: {0}", String.Join(",\n ", addresses)));

			addresses.Clear();

			foreach (MimeAddress address in this.To)
			{
				addresses.Add(

					String.Format(

						@"=?UTF-8?B?{0}?= <{1}>",

						Convert.ToBase64String(Encoding.UTF8.GetBytes(address.Display)),

						address.Address

					)

				);
			}

			writer.WriteLine(String.Format(@"To: {0}", String.Join(",\n ", addresses)));

			addresses.Clear();

			foreach (MimeAddress address in this.Cc)
			{
				addresses.Add(

					String.Format(

						@"=?UTF-8?B?{0}?= <{1}>",

						Convert.ToBase64String(Encoding.UTF8.GetBytes(address.Display)),

						address.Address

					)

				);
			}

			writer.WriteLine(String.Format(@"Cc: {0}", String.Join(",\n ", addresses)));

			addresses.Clear();

			foreach (MimeAddress address in this.Bcc)
			{
				addresses.Add(

					String.Format(

						@"=?UTF-8?B?{0}?= <{1}>",

						Convert.ToBase64String(Encoding.UTF8.GetBytes(address.Display)),

						address.Address

					)

				);
			}

			writer.WriteLine(String.Format(@"Bcc: {0}", String.Join(",\n ", addresses)));

			String boundary = String.Format(

				@"====={0}=====",

				Guid.NewGuid().ToString().Replace("-", String.Empty)

			);

			writer.WriteLine(String.Format(@"Content-Type: multipart/mixed; boundary=""{0}""", boundary));

			writer.WriteLine(String.Empty);

			String base64 = String.Empty;

			Int32 offset = 0;

			Int32 length = 0;

			// BEG: TextBody

			writer.WriteLine(String.Format(@"--{0}", boundary));

			writer.WriteLine(@"Content-Type: text/plain; charset=""utf-8""");

			writer.WriteLine(@"Content-Transfer-Encoding: base64");

			writer.WriteLine(String.Empty);

			base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.TextBody));

			offset = 0;

			while (true)
			{
				if (offset < base64.Length)
				{
					length = 76;

					if (offset + length > base64.Length)
					{
						length = base64.Length - offset;
					}

					writer.WriteLine(base64.Substring(offset, length));

					offset += 76;

					continue;
				}

				break;
			}

			writer.WriteLine(String.Empty);

			// END: TextBody

			// BEG: HtmlBody

			writer.WriteLine(String.Format(@"--{0}", boundary));

			writer.WriteLine(@"Content-Type: text/html; charset=""utf-8""");

			writer.WriteLine(@"Content-Transfer-Encoding: base64");

			writer.WriteLine(String.Empty);

			base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.HtmlBody));

			offset = 0;

			while (true)
			{
				if (offset < base64.Length)
				{
					length = 76;

					if (offset + length > base64.Length)
					{
						length = base64.Length - offset;
					}

					writer.WriteLine(base64.Substring(offset, length));

					offset += 76;

					continue;
				}

				break;
			}

			writer.WriteLine(String.Empty);

			// END: HtmlBody

			Boolean text = false;

			Boolean html = false;

			foreach (MimePart part in this.Attachments)
			{
				writer.WriteLine(String.Format(@"--{0}", boundary));

				if (part.ContentType == null)
				{
					writer.WriteLine(@"Content-Type: application/octet-stream");
				}
				else
				{
					writer.WriteLine(

						String.Format(

							@"Content-Type: application/octet-stream; name=""{0}""",

							part.ContentType.Name

						)

					);
				}

				if (part.ContentDisposition == null)
				{
					writer.WriteLine(@"Content-Disposition: attachment");
				}
				else
				{
					writer.WriteLine(

						String.Format(

							@"Content-Disposition: attachment; filename=""=?utf-8?B?{0}?=""",

							Convert.ToBase64String(Encoding.UTF8.GetBytes(part.ContentDisposition.FileName))

						)

					);
				}

				writer.WriteLine(@"Content-Transfer-Encoding: base64");

				writer.WriteLine(String.Empty);

				if (part.IsText)
				{
					base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(part.StringContent));
				}
				else
				{
					base64 = Convert.ToBase64String(part.BinaryContent);
				}

				offset = 0;

				while (true)
				{
					if (offset < base64.Length)
					{
						length = 76;

						if (offset + length > base64.Length)
						{
							length = base64.Length - offset;
						}

						writer.WriteLine(base64.Substring(offset, length));

						offset += 76;

						continue;
					}

					break;
				}

				writer.WriteLine(String.Empty);
			}

			writer.WriteLine(String.Format(@"--{0}--", boundary));

			writer.Flush();
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

							continue;
						}

						if (String.IsNullOrEmpty(this.HtmlBody) &&
							part.ContentType.MimeType.Equals("text/html"))
						{
							this.HtmlBody = part.StringContent;

							continue;
						}
					}

					if (part.ContentType.MimeType.StartsWith("message"))
					{
						continue;
					}

					if (part.ContentType.MimeType.StartsWith("multipart"))
					{
						this.SearchBodies(part.PartContents);

						continue;
					}

					this.Attachments.Add(part);
				}
			}
		}
	}
}
