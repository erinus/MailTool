using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MimeTool
{
	public abstract class MimeEntity
	{
		public Boolean Success = false;

		public String RawString = String.Empty;

		public List<String> RawHead = new List<String>();

		public List<String> Head = new List<String>();

		public List<String> RawBody = new List<String>();

		public List<String> Body = new List<String>();

		public MimeHeaderCollection Headers = new MimeHeaderCollection();

		public MimeContentType ContentType = null;

		public MimeContentDisposition ContentDisposition = null;

		public String ContentTransferEncoding = String.Empty;

		public String ContentId = String.Empty;

		public Boolean IsMultipart = false;

		public Boolean IsFile = false;

		public Boolean IsText = false;

		public Byte[] BinaryContent = new Byte[0];

		public String StringContent = String.Empty;

		public List<MimePart> PartContents = new List<MimePart>();

		public virtual void Parse()
		{
			String[] rawLines = this.RawString.Replace("\r\n", "\n").Split(new String[] { "\n" }, StringSplitOptions.None);

			this.DivideHeadAndBody(rawLines);

			this.RefineHead();

			this.DetectHead();

			this.RefineBody();

			this.DetectBody();

			this.Success = true;
		}

		private void DivideHeadAndBody(String[] rawLines)
		{
			Boolean head = true;

			Int32 index = 0;

			while (index < rawLines.Length)
			{
				String rawLine = rawLines[index];

				if (head)
				{
					if (String.IsNullOrEmpty(rawLine))
					{
						if (this is MimeMessage)
						{
							Boolean still = false;

							Int32 cache = index;

							index++;

							while (index < rawLines.Length)
							{
								String tmpLine = rawLines[index];

								if (String.IsNullOrEmpty(tmpLine))
								{
									break;
								}

								if (tmpLine.ToLower().StartsWith("subject:"))
								{
									still = true;

									break;
								}

								index++;
							}

							if (!still)
							{
								index = cache;

								head = false;
							}
						}
						else
						{
							index++;

							head = false;
						}

						continue;
					}
				}

				if (head)
				{
					this.RawHead.Add(rawLine);
				}
				else
				{
					this.RawBody.Add(rawLine);
				}

				index++;
			}
		}

		private void RefineHead()
		{
			List<String> lines = new List<String>();

			Int32 index = 0;

			while (index < this.RawHead.Count)
			{
				String line = this.RawHead[index];

				if (Regex.IsMatch(line.ToLower(), @"^[^\s]+?:", RegexOptions.None))
				{
					lines.Add(line.Trim());

					index++;

					while (index < this.RawHead.Count)
					{
						line = this.RawHead[index];

						Match find = Regex.Match(line, @"^\s(.+)$", RegexOptions.None);

						if (find.Success)
						{
							line = find.Groups[1].Value;

							line = Regex.Replace(line, @"^\s+", " ");

							line = Regex.Replace(line, @"\s+$", String.Empty);

							lines.Add(line);

							index++;

							continue;
						}

						break;
					}

					this.Head.Add(String.Join(String.Empty, lines));

					lines.Clear();

					continue;
				}

				index++;
			}
		}

		private void DetectHead()
		{
			foreach (String line in this.Head)
			{
				this.Headers.Add(new MimeHeader(line));
			}

			foreach (MimeHeader header in this.Headers)
			{
				String name = header.Name.ToLower();

				String data = header.Data;

				switch (name)
				{
					case "content-type":

						this.ContentType = new MimeContentType(data);

						break;

					case "content-disposition":

						this.ContentDisposition = new MimeContentDisposition(data);

						break;

					case "content-transfer-encoding":

						data = data.ToLower();

						if (data.Contains("7bit"))
						{
							this.ContentTransferEncoding = "7bit";
						}

						if (data.Contains("8bit"))
						{
							this.ContentTransferEncoding = "8bit";
						}

						if (data.Contains("base64"))
						{
							this.ContentTransferEncoding = "base64";
						}

						if (data.Contains("quot") || data.Contains("print"))
						{
							this.ContentTransferEncoding = "quoted-printable";
						}

						break;

					case "content-id":

						data = Regex.Replace(data, @"^<", String.Empty);

						data = Regex.Replace(data, @">$", String.Empty);

						this.ContentId = data;

						break;
				}
			}
		}

		private void RefineBody()
		{
			String type = String.Empty;

			if (this.ContentType != null)
			{
				type = this.ContentType.MimeType.ToLower();
			}

			Int32 index = 0;

			switch (type)
			{
				case "multipart/alternative":
				case "multipart/related":
				case "multipart/mixed":

					Boolean startFound = false;

					String multiPartStart = String.Format("--{0}", this.ContentType.Boundary);

					Boolean closeFound = false;

					String multiPartClose = String.Format("--{0}--", this.ContentType.Boundary);

					while (index < this.RawBody.Count)
					{
						String line = this.RawBody[index];

						if (line.Equals(multiPartStart))
						{
							startFound = true;
						}

						if (startFound)
						{
							this.Body.Add(line);
						}

						if (line.Equals(multiPartClose))
						{
							startFound = false;

							closeFound = true;
						}

						index++;
					}

					if (!closeFound)
					{
						this.Body.Add(multiPartClose);
					}

					break;

				case "text/plain":
				case "text/html":
				case "text/calendar":

					while (index < this.RawBody.Count)
					{
						String line = this.RawBody[index];

						this.Body.Add(line);

						index++;
					}

					break;

				default:

					while (index < this.RawBody.Count)
					{
						String line = this.RawBody[index];

						this.Body.Add(line);

						index++;
					}

					break;
			}
		}

		private void DetectBody()
		{
			String type = String.Empty;

			if (this.ContentType != null)
			{
				type = this.ContentType.MimeType.ToLower();
			}

			List<String> lines = new List<String>();

			Byte[] data;

			Int32 mode = 0;

			if (type.StartsWith("text") || type.StartsWith("message"))
			{
				mode = 1;
			}

			if (type.StartsWith("multipart"))
			{
				mode = 2;
			}

			switch (mode)
			{
				case 0:

					this.IsFile = true;

					foreach (String line in this.Body)
					{
						switch (this.ContentTransferEncoding)
						{
							case "base64":

								lines.Add(line);

								break;

							case "quoted-printable":

								if (line.EndsWith("="))
								{
									lines.Add(line.Remove(line.Length - 1));
								}
								else
								{
									lines.Add(line);
								}

								break;
						}
					}

					switch (this.ContentTransferEncoding)
					{
						case "7bit":

							this.BinaryContent = Encoding.UTF7.GetBytes(String.Join("\n", lines));

							break;

						case "8bit":

							this.BinaryContent = Encoding.UTF8.GetBytes(String.Join("\n", lines));

							break;

						case "base64":

							data = Util.DecodeBase64(String.Join(String.Empty, lines));

							this.BinaryContent = data;

							break;

						case "quoted-printable":

							data = Util.DecodeQuoted(String.Join(String.Empty, lines));

							this.BinaryContent = data;

							break;
					}

					break;

				case 1:

					this.IsText = true;

					foreach (String line in this.Body)
					{
						switch (this.ContentTransferEncoding)
						{
							case "7bit":

								lines.Add(line);

								break;

							case "8bit":

								lines.Add(line);

								break;

							case "base64":

								lines.Add(line);

								break;

							case "quoted-printable":

								if (line.EndsWith("="))
								{
									lines.Add(line.Remove(line.Length - 1));
								}
								else
								{
									lines.Add(line);
								}

								break;

							default:

								lines.Add(line);

								break;
						}
					}

					switch (this.ContentTransferEncoding)
					{
						case "7bit":

							this.StringContent = String.Join("\n", lines);

							break;

						case "8bit":

							this.StringContent = String.Join("\n", lines);

							break;

						case "base64":

							data = Util.DecodeBase64(String.Join(String.Empty, lines));

							if (data.Length == 0)
							{
								this.StringContent = String.Empty;
							}
							else
							{
								this.StringContent = Util.DetectCharset(data, this.ContentType.Charset).GetString(data);
							}

							break;

						case "quoted-printable":

							data = Util.DecodeQuoted(String.Join(String.Empty, lines));

							if (data.Length == 0)
							{
								this.StringContent = String.Empty;
							}
							else
							{
								this.StringContent = Util.DetectCharset(data, this.ContentType.Charset).GetString(data).Replace("_", " ");
							}

							break;

						default:

							this.StringContent = String.Join("\n", lines);

							break;
					}

					break;

				case 2:

					this.IsMultipart = true;

					String partStart = String.Format("--{0}", this.ContentType.Boundary);

					String partClose = String.Format("--{0}--", this.ContentType.Boundary);

					foreach (String line in this.Body)
					{
						if (line.Equals(partStart))
						{
							if (lines.Count != 0)
							{
								MimePart part = new MimePart();

								part.RawString = String.Join("\n", lines);

								part.Parse();

								this.PartContents.Add(part);
							}

							lines.Clear();

							continue;
						}

						if (line.Equals(partClose))
						{
							MimePart part = new MimePart();

							part.RawString = String.Join("\n", lines);

							part.Parse();

							this.PartContents.Add(part);

							break;
						}

						lines.Add(line);
					}

					break;
			}
		}
	}
}
