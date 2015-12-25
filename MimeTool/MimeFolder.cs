using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MimeTool
{
	public class MimeFolder
	{
		public Byte[] RawBinary;

		public MimeFolder()
		{

		}

		public List<Byte[]> ExtractBinaries()
		{
			List<Byte[]> result = new List<Byte[]>();

			Int32 item = 0;

			List<Byte> line = new List<Byte>();

			List<Byte> data = new List<Byte>();

			using (MemoryStream smbx = new MemoryStream(this.RawBinary))
			{
				while (true)
				{
					Int32 read = smbx.ReadByte();

					if (read == -1)
					{
						if (data.Count == 0)
						{
							break;
						}

						Byte[] beml = data.ToArray();

						result.Add(beml);

						data.Clear();

						break;
					}

					line.Add((Byte)read);

					if (read.Equals('\r'))
					{
						continue;
					}

					if (read.Equals('\n'))
					{
						String text;

						text = Encoding.ASCII.GetString(line.ToArray()).Trim();

						Match find = Regex.Match(
							text,
							@"^From - (?<datetime>\w{3} \w{3} \d{2} \d{2}:\d{2}:\d{2} \d{4}$)",
							RegexOptions.IgnoreCase
						);

						if (find.Success)
						{
							text = find.Groups["datetime"].Value;

							DateTime time;

							if (DateTime.TryParseExact(
								text,
								"ddd MMM dd HH:mm:ss yyyy",
								CultureInfo.InvariantCulture,
								DateTimeStyles.None, out time))
							{
								if (item == 0)
								{
									data.Clear();
								}
								else
								{
									Byte[] beml = data.ToArray();

									result.Add(beml);

									data.Clear();
								}

								item++;
							}
						}

						data.AddRange(line);

						line.Clear();
					}
				}
			}

			return result;
		}

		public List<MimeMessage> ExtractMessages()
		{
			List<MimeMessage> result = new List<MimeMessage>();

			Int32 item = 0;

			List<Byte> line = new List<Byte>();

			List<Byte> data = new List<Byte>();

			using (MemoryStream smbx = new MemoryStream(this.RawBinary))
			{
				while (true)
				{
					Int32 read = smbx.ReadByte();

					if (read == -1)
					{
						if (data.Count == 0)
						{
							break;
						}

						Byte[] beml = data.ToArray();

						MimeMessage message = new MimeMessage();

						message.RawString = Util.DetectCharset(beml).GetString(beml);

						message.Parse();

						result.Add(message);

						data.Clear();

						break;
					}

					line.Add((Byte)read);

					if (read.Equals('\r'))
					{
						continue;
					}

					if (read.Equals('\n'))
					{
						String text;

						text = Encoding.ASCII.GetString(line.ToArray()).Trim();

						Match find = Regex.Match(
							text,
							@"^From - (?<datetime>\w{3} \w{3} \d{2} \d{2}:\d{2}:\d{2} \d{4}$)",
							RegexOptions.IgnoreCase
						);

						if (find.Success)
						{
							text = find.Groups["datetime"].Value;

							DateTime time;

							if (DateTime.TryParseExact(
								text,
								"ddd MMM dd HH:mm:ss yyyy",
								CultureInfo.InvariantCulture,
								DateTimeStyles.None, out time))
							{
								if (item == 0)
								{
									data.Clear();
								}
								else
								{
									Byte[] beml = data.ToArray();

									MimeMessage message = new MimeMessage();

									message.RawString = Util.DetectCharset(beml).GetString(beml);

									message.Parse();

									result.Add(message);

									data.Clear();
								}

								item++;
							}
						}

						data.AddRange(line);

						line.Clear();
					}
				}
			}

			return result;
		}
	}
}
