using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using MimeTool;
using TnefTool;

namespace Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			/*
			TnefMessage tnef = new TnefMessage();

			tnef.RawBinary = File.ReadAllBytes(
				//@"E:\Projects\MailTool\MailTool\bin\eml\USA-CHN_USER_2_993458_1436503346.mbx\001.eml - winmail.dat"
				@"E:\Projects\MailTool\MailTool\bin\eml\USA-CHN_USER_2_993458_1436504872.mbx\001.eml - winmail.dat"
			);

			tnef.Debug();

			Console.WriteLine(String.Format("DateSent: {0}", tnef.DateSent));

			Console.WriteLine(String.Format("Subject: {0}", tnef.Subject));

			Console.WriteLine(String.Format("Sender: {0} <{1}>", tnef.Sender.Display, tnef.Sender.Address));

			foreach (TnefAttachment attachment in tnef.Attachments)
			{
				Console.WriteLine(String.Format("Attachment: {0}", attachment.FileName));
			}

			return;
			*/

			String text = String.Empty;

			/*
			DirectoryInfo dmbx = new DirectoryInfo(@".\mbx");

			foreach (FileInfo fmbx in dmbx.GetFiles("*.mbx"))
			{
				text = String.Format(
					"{0}\n\n",
					fmbx.Name
				);

				Console.Write(text);

				Byte[] bmbx = File.ReadAllBytes(fmbx.FullName);

				MimeFolder folder = new MimeFolder();

				folder.RawBinary = bmbx;

				Int32 item = 0;

				foreach (Byte[] beml in folder.ExtractBinaries())
				{
					item++;

					String psub = String.Format(
						@".\eml\{0}",
						fmbx.Name
					);

					if (!Directory.Exists(psub))
					{
						Directory.CreateDirectory(psub);
					}

					File.WriteAllBytes(
						String.Format(@"{0}\{1:000}.eml", psub, item),
						beml
					);

					text = String.Format(
						"\t{0:0000}\n\n",
						item
					);

					Console.Write(text);
				}
			}

			return;
			*/

			DirectoryInfo deml = new DirectoryInfo(@".\eml");

			foreach (DirectoryInfo dsub in deml.GetDirectories())
			{
				foreach (FileInfo feml in dsub.GetFiles("*.eml"))
				{
					Byte[] beml = File.ReadAllBytes(feml.FullName);

					MimeMessage mime = new MimeMessage();

					mime.RawString = Util.DetectCharset(beml).GetString(beml);

					//File.WriteAllText(
					//	String.Format(
					//		@"{0}.raw.txt",
					//		file.FullName
					//	),
					//	message.RawString,
					//	Encoding.UTF8
					//);

					mime.Parse();

					text = String.Format(
						"{0}\n\n",
						feml.FullName
					);

					Console.Write(text);

					File.WriteAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						String.Empty,
						Encoding.UTF8
					);

					if (!mime.Success)
					{
						text = "\tfailed.\n\n";

						Console.Write(text);

						File.AppendAllText(
							String.Format(
								@"{0}.txt",
								feml.FullName
							),
							text,
							Encoding.UTF8
						);

						continue;
					}

					/*
					#region MimeMessage.RawHead
					text = String.Empty;

					foreach (String rawhead in message.RawHead)
					{
						text += String.Format(
							"{0}\n",
							rawhead
						);
					}

					File.WriteAllText(
						String.Format(
							@"{0}\{1}.rawhead.txt",
							Path.GetDirectoryName(feml.FullName),
							Path.GetFileNameWithoutExtension(feml.Name)
						),
						text,
						Encoding.UTF8
					);
					#endregion
					*/

					/*
					#region MimeMessage.RawBody
					text = String.Empty;

					foreach (String rawbody in message.RawBody)
					{
						text += String.Format(
							"{0}\n",
							rawbody
						);
					}

					File.WriteAllText(
						String.Format(
							@"{0}.rawbody.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);
					#endregion
					*/

					/*
					#region MimeMessage.Head
					text = String.Empty;

					foreach (String head in message.Head)
					{
						text += String.Format(
							"{0}\n",
							head
						);
					}

					File.WriteAllText(
						String.Format(
							@"{0}\{1}.head.txt",
							Path.GetDirectoryName(feml.FullName),
							Path.GetFileNameWithoutExtension(feml.Name)
						),
						text,
						Encoding.UTF8
					);
					#endregion
					*/

					/*
					#region MimeMessage.Body
					text = String.Empty;

					foreach (String body in message.Body)
					{
						text += String.Format(
							"{0}\n",
							body
						);
					}

					File.WriteAllText(
						String.Format(
							@"{0}\{1}.body.txt",
							Path.GetDirectoryName(feml.FullName),
							Path.GetFileNameWithoutExtension(feml.Name)
						),
						text,
						Encoding.UTF8
					);
					#endregion
					*/

					/*
					#region MimeMessage.Headers
					text = String.Empty;

					foreach (MimeHeader header in message.Headers)
					{
						text += String.Format(
							"{0}: {1}\n",
							header.Name,
							header.Data
						);
					}

					File.WriteAllText(
						String.Format(
							@"{0}\{1}.headers.txt",
							Path.GetDirectoryName(feml.FullName),
							Path.GetFileNameWithoutExtension(feml.Name)
						),
						text,
						Encoding.UTF8
					);
					#endregion
					*/

					text = String.Format(
						"Date: {0}\n" +
						"Subject: {1}\n" +
						"Content-Type.MimeType: {2}\n" +
						"Content-Type.Charset: {3}\n" +
						"Content-Type.Name: {4}\n" +
						"Content-Type.Boundary: {5}\n" +
						"Content-Disposition.Type: {6}\n" +
						"Content-Disposition.FileName: {7}\n" +
						"Content-Transfer-Encoding: {8}\n",
						mime.Date.ToString("yyyy-MM-dd HH:mm:ss"),
						mime.Subject,
						mime.ContentType == null
							? String.Empty
							: mime.ContentType.MimeType,
						mime.ContentType == null
							? String.Empty
							: mime.ContentType.Charset == null
								? String.Empty
								: mime.ContentType.Charset.BodyName,
						mime.ContentType == null
							? String.Empty
							: String.IsNullOrEmpty(mime.ContentType.Name)
								? String.Empty
								: mime.ContentType.Name,
						mime.ContentType == null
							? String.Empty
							: String.IsNullOrEmpty(mime.ContentType.Boundary)
								? String.Empty
								: mime.ContentType.Boundary,
						mime.ContentDisposition == null
							? String.Empty
								: mime.ContentDisposition.Type,
						mime.ContentDisposition == null
							? String.Empty
								: mime.ContentDisposition.FileName,
						mime.ContentTransferEncoding
					);

					//Console.Write(text);

					File.AppendAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);

					List<String> list = new List<String>();

					text = String.Empty;

					list.Clear();

					if (mime.From != null)
					{
						foreach (MimeAddress address in mime.From)
						{
							list.Add(
								String.Format(
									"'{0}' <{1}>",
									address.Display,
									address.Address
								)
							);
						}
					}

					text += "From: " + String.Join("\n      ", list) + "\n";

					File.AppendAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);

					text = String.Empty;

					list.Clear();

					if (mime.To != null)
					{
						foreach (MimeAddress address in mime.To)
						{
							list.Add(
								String.Format(
									"'{0}' <{1}>",
									address.Display,
									address.Address
								)
							);
						}
					}

					text += "To:   " + String.Join("\n      ", list) + "\n";

					File.AppendAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);

					text = String.Empty;

					list.Clear();

					if (mime.Cc != null)
					{
						foreach (MimeAddress address in mime.Cc)
						{
							list.Add(
								String.Format(
									"'{0}' <{1}>",
									address.Display,
									address.Address
								)
							);
						}
					}

					text += "Cc:   " + String.Join("\n      ", list) + "\n";

					File.AppendAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);

					text = String.Empty;

					list.Clear();

					if (mime.Bcc != null)
					{
						foreach (MimeAddress address in mime.Bcc)
						{
							list.Add(
								String.Format(
									"'{0}' <{1}>",
									address.Display,
									address.Address
								)
							);
						}
					}

					text += "Bcc:  " + String.Join("\n      ", list) + "\n";

					File.AppendAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);

					text = "\n";

					File.AppendAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);

					text = String.Format(
						"TextBody:\n" +
						"{0}\n\n" +
						"HtmlBody:\n" +
						"{1}\n\n",
						mime.TextBody,
						mime.HtmlBody
					);

					//Console.Write(text);

					File.AppendAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);

					text = "Parts:\n\n";

					//Console.Write(text);

					File.AppendAllText(
						String.Format(
							@"{0}.txt",
							feml.FullName
						),
						text,
						Encoding.UTF8
					);

					GetPartContents(feml, mime, "\t");

					//Console.WriteLine(
					//	String.Format(
					//		"RawHead.Count: {0}",
					//		message.RawHead.Count
					//	)
					//);

					//Console.WriteLine(
					//	String.Format(
					//		"RawBody.Count: {0}",
					//		message.RawBody.Count
					//	)
					//);

					using (FileStream steam = new FileStream(String.Format(@"{0}\{1}.new.eml", dsub.FullName, Path.GetFileNameWithoutExtension(feml.Name)), FileMode.OpenOrCreate))
					{
						mime.Save(steam);
					}
				}
			}
		}

		static void GetPartContents(FileInfo feml, MimeEntity entity, String tabs)
		{
			String text = String.Empty;

			foreach (MimePart part in entity.PartContents)
			{
				text = String.Format(
					"{0}Part:\n" +
					"{0}Part.IsText: {1}\n" +
					"{0}Part.IsFile: {2}\n" +
					"{0}Part.IsMultipart: {3}\n" +
					"{0}Content-Type.MimeType: {4}\n" +
					"{0}Content-Type.Charset: {5}\n" +
					"{0}Content-Type.Name: {6}\n" +
					"{0}Content-Type.Boundary: {7}\n" +
					"{0}Content-Disposition.Type: {8}\n" +
					"{0}Content-Disposition.FileName: {9}\n" +
					"{0}Part.Content-Transfer-Encoding: {10}\n" +
					"{0}Part.Content-ID: {11}\n\n",
					tabs,
					part.IsText,
					part.IsFile,
					part.IsMultipart,
					part.ContentType == null
						? String.Empty
						: part.ContentType.MimeType,
					part.ContentType == null
						? String.Empty
						: part.ContentType.Charset == null
							? String.Empty
							: part.ContentType.Charset.BodyName,
					part.ContentType == null
						? String.Empty
						: String.IsNullOrEmpty(part.ContentType.Name)
							? String.Empty
							: part.ContentType.Name,
					part.ContentType == null
						? String.Empty
						: String.IsNullOrEmpty(part.ContentType.Boundary)
							? String.Empty
							: part.ContentType.Boundary,
					part.ContentDisposition == null
						? String.Empty
						: part.ContentDisposition.Type,
					part.ContentDisposition == null
						? String.Empty
						: part.ContentDisposition.FileName,
					part.ContentTransferEncoding,
					part.ContentId
				);

				Console.Write(text);

				File.AppendAllText(
					String.Format(
						@"{0}.txt",
						feml.FullName
					),
					text,
					Encoding.UTF8
				);

				/*
				#region MimeMessage.Headers
				String text = String.Empty;

				foreach (String line in part.Body)
				{
					text += String.Format(
						"{0}\n",
						line
					);
				}

				File.AppendAllText(
					String.Format(
						@"{0}\{1}.parts.txt",
						Path.GetDirectoryName(feml.FullName),
						Path.GetFileNameWithoutExtension(feml.Name)
					),
					"Part.Body:\n" +
					text +
					"\n\n\n",
					Encoding.UTF8
				);
				#endregion
				*/

				if (part.IsFile)
				{
					String filename = String.Empty;

					if (part.ContentDisposition != null && !String.IsNullOrEmpty(part.ContentDisposition.FileName))
					{
						filename = part.ContentDisposition.FileName;
					}

					if (part.ContentType != null && !String.IsNullOrEmpty(part.ContentType.Name))
					{
						filename = part.ContentType.Name;
					}

					if (!String.IsNullOrEmpty(filename))
					{
						File.WriteAllBytes(
							String.Format(
								@"{0}\{1} - {2}",
								Path.GetDirectoryName(feml.FullName),
								feml.Name,
								filename
							),
							part.BinaryContent
						);
					}
				}

				GetPartContents(feml, part, tabs + "\t");
			}
		}
	}
}
