using System;
using System.IO;
using System.Text;

using MimeKit.Tnef;

namespace TnefTool
{
	public class TnefMessage
	{
		public Byte[] RawBinary;

		public DateTime DateSent;

		public String Subject = String.Empty;

		public String TextBody = String.Empty;

		public String HtmlBody = String.Empty;

		public TnefAddress Sender = null;

		public TnefAttachmentCollection Attachments = new TnefAttachmentCollection();

		public TnefMessage()
		{

		}

		public void Parse()
		{
			using (var reader = new TnefReader(new MemoryStream(this.RawBinary), 0, TnefComplianceMode.Loose))
			{
				while (reader.ReadNextAttribute())
				{
					if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
					{
						break;
					}

					TnefPropertyReader prop = reader.TnefPropertyReader;

					switch (reader.AttributeTag)
					{
						case TnefAttributeTag.RecipientTable:

							while (prop.ReadNextRow())
							{
								string name = null, addr = null;

								while (prop.ReadNextProperty())
								{
									switch (prop.PropertyTag.Id)
									{
										case TnefPropertyId.RecipientType:
											int recipientType = prop.ReadValueAsInt32();

											switch (recipientType)
											{
												case 1:

													Console.WriteLine("To:");

													break;

												case 2:

													Console.WriteLine("Cc:");

													break;

												case 3:

													Console.WriteLine("Bcc:");

													break;
											}

											break;

										case TnefPropertyId.TransmitableDisplayName:

											if (string.IsNullOrEmpty(name))
											{
												name = prop.ReadValueAsString();
											}
											else
											{
												
											}

											break;

										case TnefPropertyId.DisplayName:

											name = prop.ReadValueAsString();

											break;

										case TnefPropertyId.EmailAddress:

											if (string.IsNullOrEmpty(addr))
											{
												addr = prop.ReadValueAsString();
											}
											else
											{
												
											}

											break;

										case TnefPropertyId.SmtpAddress:
											// The SmtpAddress, if it exists, should take precedence over the EmailAddress
											// (since the SmtpAddress is meant to be used in the RCPT TO command).
											addr = prop.ReadValueAsString();

											break;

										default:

											break;
									}
								}
							}

							break;

						case TnefAttributeTag.MapiProperties:

							while (prop.ReadNextProperty())
							{
								switch (prop.PropertyTag.Id)
								{
									case TnefPropertyId.InternetMessageId:

										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode)
										{
											
										}
										else
										{
											
										}

										break;

									case TnefPropertyId.Subject:

										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode)
										{
											this.Subject = prop.ReadValueAsString();
										}
										else
										{
											
										}

										break;

									case TnefPropertyId.RtfCompressed:
										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary)
										{
											//var rtf = new TextPart("rtf");
											//rtf.ContentType.Name = "body.rtf";

											//var converter = new RtfCompressedToRtf();
											//var content = new MemoryStream();

											//using (var filtered = new FilteredStream(content))
											//{
											//	filtered.Add(converter);

											//	using (var compressed = prop.GetRawValueReadStream())
											//	{
											//		compressed.CopyTo(filtered, 4096);
											//		filtered.Flush();
											//	}
											//}

											//rtf.ContentObject = new ContentObject(content);
											//content.Position = 0;

											//builder.Attachments.Add(rtf);
										}
										else
										{
											
										}

										break;

									case TnefPropertyId.BodyHtml:

										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary)
										{
											this.HtmlBody = prop.ReadValueAsString();
										}
										else
										{
											
										}

										break;

									case TnefPropertyId.Body:

										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary)
										{
											this.TextBody = prop.ReadValueAsString();
										}
										else
										{
											
										}

										break;

									default:

										object val;

										try
										{
											val = prop.ReadValue();
										}
										catch
										{
											val = null;
										}

										String key = prop.PropertyTag.Id.ToString();

										switch (key)
										{
											case "SenderName":

												if (this.Sender == null)
												{
													this.Sender = new TnefAddress();
												}

												this.Sender.Display = val.ToString().Trim();

												break;

											case "SenderEmailAddress":

												if (this.Sender == null)
												{
													this.Sender = new TnefAddress();
												}

												this.Sender.Address = val.ToString().Trim();

												break;
										}

										break;
								}
							}

							break;

						case TnefAttributeTag.DateSent:

							this.DateSent = prop.ReadValueAsDateTime();

							break;

						case TnefAttributeTag.Body:

							break;

						case TnefAttributeTag.OemCodepage:

							int codepage = prop.ReadValueAsInt32();

							try
							{
								var encoding = Encoding.GetEncoding(codepage);
							}
							catch
							{
								
							}

							break;
					}
				}

				if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
				{
					TnefPropertyReader propertyReader = reader.TnefPropertyReader;

					TnefAttachMethod attachMethod = TnefAttachMethod.ByValue;

					TnefAttachFlags attachFlags;

					TnefAttachment attachment = null;

					do
					{
						if (reader.AttributeLevel != TnefAttributeLevel.Attachment)
						{

						}

						switch (reader.AttributeTag)
						{
							case TnefAttributeTag.AttachRenderData:

								attachMethod = TnefAttachMethod.ByValue;

								attachment = new TnefAttachment();

								break;

							case TnefAttributeTag.Attachment:

								if (attachment == null)
								{
									break;
								}

								while (propertyReader.ReadNextProperty())
								{
									switch (propertyReader.PropertyTag.Id)
									{
										case TnefPropertyId.AttachLongFilename:

											attachment.FileName = propertyReader.ReadValueAsString();

											break;

										case TnefPropertyId.AttachFilename:

											if (String.IsNullOrEmpty(attachment.FileName))
											{
												attachment.FileName = propertyReader.ReadValueAsString();
											}
											else
											{

											}

											break;

										case TnefPropertyId.AttachContentLocation:

											break;

										case TnefPropertyId.AttachContentBase:

											break;

										case TnefPropertyId.AttachContentId:

											break;

										case TnefPropertyId.AttachDisposition:

											break;

										case TnefPropertyId.AttachMethod:

											attachMethod = (TnefAttachMethod)propertyReader.ReadValueAsInt32();

											break;

										case TnefPropertyId.AttachMimeTag:

											attachment.MimeType = propertyReader.ReadValueAsString();

											break;

										case TnefPropertyId.AttachFlags:

											attachFlags = (TnefAttachFlags)propertyReader.ReadValueAsInt32();

											break;

										case TnefPropertyId.AttachData:

											Stream stream = propertyReader.GetRawValueReadStream();

											byte[] guid = new byte[16];

											stream.Read(guid, 0, 16);

											break;

										case TnefPropertyId.DisplayName:

											break;

										case TnefPropertyId.AttachSize:

											break;

										default:

											break;
									}
								}

								break;

							case TnefAttributeTag.AttachData:

								if (attachment == null || attachMethod != TnefAttachMethod.ByValue)
								{
									break;
								}

								attachment.BinaryContent = propertyReader.ReadValueAsBytes();

								this.Attachments.Add(attachment);

								break;

							case TnefAttributeTag.AttachCreateDate:

								break;

							case TnefAttributeTag.AttachModifyDate:

								break;

							case TnefAttributeTag.AttachTitle:

								break;

							default:

								break;
						}
					}
					while (reader.ReadNextAttribute());
				}
				else
				{

				}
			}
		}

		public void Debug()
		{
			using (var reader = new TnefReader(new MemoryStream(this.RawBinary), 0, TnefComplianceMode.Loose))
			{
				while (reader.ReadNextAttribute())
				{
					if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
					{
						break;
					}

					TnefPropertyReader prop = reader.TnefPropertyReader;

					switch (reader.AttributeTag)
					{
						case TnefAttributeTag.RecipientTable:

							while (prop.ReadNextRow())
							{
								string name = null, addr = null;

								while (prop.ReadNextProperty())
								{
									switch (prop.PropertyTag.Id)
									{
										case TnefPropertyId.RecipientType:
											int recipientType = prop.ReadValueAsInt32();

											switch (recipientType)
											{
												case 1:

													Console.WriteLine("To:");

													break;

												case 2:

													Console.WriteLine("Cc:");

													break;

												case 3:

													Console.WriteLine("Bcc:");

													break;
											}

											Console.WriteLine("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, recipientType);

											break;

										case TnefPropertyId.TransmitableDisplayName:

											if (string.IsNullOrEmpty(name))
											{
												name = prop.ReadValueAsString();

												Console.WriteLine("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, name);
											}
											else
											{
												Console.WriteLine("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());
											}

											break;

										case TnefPropertyId.DisplayName:

											name = prop.ReadValueAsString();

											Console.WriteLine("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, name);

											break;

										case TnefPropertyId.EmailAddress:

											if (string.IsNullOrEmpty(addr))
											{
												addr = prop.ReadValueAsString();

												Console.WriteLine("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, addr);
											}
											else
											{
												Console.WriteLine("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());
											}

											break;

										case TnefPropertyId.SmtpAddress:
											// The SmtpAddress, if it exists, should take precedence over the EmailAddress
											// (since the SmtpAddress is meant to be used in the RCPT TO command).
											addr = prop.ReadValueAsString();

											Console.WriteLine("RecipientTable Property: {0} = {1}", prop.PropertyTag.Id, addr);

											break;

										default:

											Console.WriteLine("RecipientTable Property (unhandled): {0} = {1}", prop.PropertyTag.Id, prop.ReadValue());

											break;
									}
								}
							}

							break;

						case TnefAttributeTag.MapiProperties:

							while (prop.ReadNextProperty())
							{
								switch (prop.PropertyTag.Id)
								{
									case TnefPropertyId.InternetMessageId:

										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode)
										{
											Console.WriteLine("Message Property (InternetMessageId): {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());
										}
										else
										{
											Console.WriteLine("Unknown property type for Message-Id: {0}", prop.PropertyTag.ValueTnefType);
										}

										break;

									case TnefPropertyId.Subject:

										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode)
										{
											this.Subject = prop.ReadValueAsString();

											Console.WriteLine("Message Property (Subject): {0} = {1}", prop.PropertyTag.Id, this.Subject);
										}
										else
										{
											Console.WriteLine("Unknown property type for Subject: {0}", prop.PropertyTag.ValueTnefType);
										}

										break;

									case TnefPropertyId.RtfCompressed:
										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary)
										{
											//var rtf = new TextPart("rtf");
											//rtf.ContentType.Name = "body.rtf";

											//var converter = new RtfCompressedToRtf();
											//var content = new MemoryStream();

											//using (var filtered = new FilteredStream(content))
											//{
											//	filtered.Add(converter);

											//	using (var compressed = prop.GetRawValueReadStream())
											//	{
											//		compressed.CopyTo(filtered, 4096);
											//		filtered.Flush();
											//	}
											//}

											//rtf.ContentObject = new ContentObject(content);
											//content.Position = 0;

											//builder.Attachments.Add(rtf);

											Console.WriteLine("Message Property (RtfCompressed): {0} = <compressed rtf data>", prop.PropertyTag.Id);
										}
										else
										{
											Console.WriteLine("Unknown property type for {0}: {1}", prop.PropertyTag.Id, prop.PropertyTag.ValueTnefType);
										}

										break;

									case TnefPropertyId.BodyHtml:

										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary)
										{
											this.HtmlBody = prop.ReadValueAsString();

											Console.WriteLine("Message Property (BodyHtml): {0} = {1}", prop.PropertyTag.Id, this.HtmlBody);
										}
										else
										{
											Console.WriteLine("Unknown property type for {0}: {1}", prop.PropertyTag.Id, prop.PropertyTag.ValueTnefType);
										}

										break;

									case TnefPropertyId.Body:

										if (prop.PropertyTag.ValueTnefType == TnefPropertyType.String8 ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Unicode ||
											prop.PropertyTag.ValueTnefType == TnefPropertyType.Binary)
										{
											this.TextBody = prop.ReadValueAsString();

											Console.WriteLine("Message Property (Body): {0} = {1}", prop.PropertyTag.Id, this.TextBody);
										}
										else
										{
											Console.WriteLine("Unknown property type for {0}: {1}", prop.PropertyTag.Id, prop.PropertyTag.ValueTnefType);
										}

										break;

									default:

										object val;

										try
										{
											val = prop.ReadValue();
										}
										catch
										{
											val = null;
										}

										String key = prop.PropertyTag.Id.ToString();

										switch (key)
										{
											case "SenderName":

												if (this.Sender == null)
												{
													this.Sender = new TnefAddress();
												}

												this.Sender.Display = val.ToString().Trim();

												break;

											case "SenderEmailAddress":

												if (this.Sender == null)
												{
													this.Sender = new TnefAddress();
												}

												this.Sender.Address = val.ToString().Trim();

												break;
										}

										Console.WriteLine("Message Property (unhandled): {0} = {1}", prop.PropertyTag.Id, val);

										break;
								}
							}

							break;

						case TnefAttributeTag.DateSent:

							this.DateSent = prop.ReadValueAsDateTime();

							Console.WriteLine("Message Attribute (DateSent): {0} = {1}", reader.AttributeTag, this.DateSent);

							break;

						case TnefAttributeTag.Body:

							Console.WriteLine("Message Attribute (Body): {0} = {1}", reader.AttributeTag, prop.ReadValueAsString());

							break;

						case TnefAttributeTag.OemCodepage:

							int codepage = prop.ReadValueAsInt32();

							try
							{
								var encoding = Encoding.GetEncoding(codepage);

								Console.WriteLine("Message Attribute: OemCodepage = {0}", encoding.HeaderName);
							}
							catch
							{
								Console.WriteLine("Message Attribute: OemCodepage = {0}", codepage);
							}

							break;
					}
				}

				if (reader.AttributeLevel == TnefAttributeLevel.Attachment)
				{
					Console.WriteLine("attachments found");

					TnefPropertyReader prop = reader.TnefPropertyReader;

					TnefAttachMethod attachMethod = TnefAttachMethod.ByValue;

					TnefAttachFlags flags;

					TnefAttachment attachment = null;

					Console.WriteLine("Extracting attachments...");

					do
					{
						if (reader.AttributeLevel != TnefAttributeLevel.Attachment)
						{
							Console.WriteLine("Expected attachment attribute level: {0}", reader.AttributeLevel);
						}

						switch (reader.AttributeTag)
						{
							case TnefAttributeTag.AttachRenderData:

								Console.WriteLine("Attachment Attribute: {0}", reader.AttributeTag);

								attachMethod = TnefAttachMethod.ByValue;

								attachment = new TnefAttachment();

								break;

							case TnefAttributeTag.Attachment:

								Console.WriteLine("Attachment Attribute: {0}", reader.AttributeTag);

								if (attachment == null)
								{
									break;
								}

								while (prop.ReadNextProperty())
								{
									switch (prop.PropertyTag.Id)
									{
										case TnefPropertyId.AttachLongFilename:

											attachment.FileName = prop.ReadValueAsString();

											Console.WriteLine("Attachment Property (AttachLongFilename): {0} = {1}", prop.PropertyTag.Id, attachment.FileName);

											break;

										case TnefPropertyId.AttachFilename:

											if (attachment.FileName == null)
											{
												attachment.FileName = prop.ReadValueAsString();

												Console.WriteLine("Attachment Property: {0} = {1}", prop.PropertyTag.Id, attachment.FileName);
											}
											else
											{
												Console.WriteLine("Attachment Property: {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());
											}

											break;

										case TnefPropertyId.AttachContentLocation:

											Console.WriteLine("Attachment Property (AttachContentLocation): {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());

											break;

										case TnefPropertyId.AttachContentBase:

											Console.WriteLine("Attachment Property (AttachContentBase): {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());

											break;

										case TnefPropertyId.AttachContentId:

											Console.WriteLine("Attachment Property (AttachContentId): {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());

											break;

										case TnefPropertyId.AttachDisposition:

											Console.WriteLine("Attachment Property (AttachDisposition): {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());

											break;

										case TnefPropertyId.AttachMethod:

											attachMethod = (TnefAttachMethod)prop.ReadValueAsInt32();

											Console.WriteLine("Attachment Property (AttachMethod): {0} = {1}", prop.PropertyTag.Id, attachMethod);

											break;

										case TnefPropertyId.AttachMimeTag:

											attachment.MimeType = prop.ReadValueAsString();

											Console.WriteLine("Attachment Property (AttachMimeTag): {0} = {1}", prop.PropertyTag.Id, attachment.MimeType);

											break;

										case TnefPropertyId.AttachFlags:

											flags = (TnefAttachFlags)prop.ReadValueAsInt32();

											Console.WriteLine("Attachment Property (AttachFlags): {0} = {1}", prop.PropertyTag.Id, flags);

											break;

										case TnefPropertyId.AttachData:

											Stream stream = prop.GetRawValueReadStream();

											byte[] guid = new byte[16];

											stream.Read(guid, 0, 16);

											Console.WriteLine("Attachment Property (AttachData): {0} has GUID {1}", prop.PropertyTag.Id, new Guid(guid));

											break;

										case TnefPropertyId.DisplayName:

											Console.WriteLine("Attachment Property (DisplayName): {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsString());

											break;

										case TnefPropertyId.AttachSize:

											Console.WriteLine("Attachment Property (AttachSize): {0} = {1}", prop.PropertyTag.Id, prop.ReadValueAsInt64());

											break;

										default:

											Console.WriteLine("Attachment Property (unhandled): {0} = {1}", prop.PropertyTag.Id, prop.ReadValue());

											break;
									}
								}

								break;

							case TnefAttributeTag.AttachData:

								if (attachment == null || attachMethod != TnefAttachMethod.ByValue)
								{
									break;
								}

								attachment.BinaryContent = prop.ReadValueAsBytes();

								this.Attachments.Add(attachment);

								Console.WriteLine("Attachment Attribute (AttachData): {0}", reader.AttributeTag);

								break;

							case TnefAttributeTag.AttachCreateDate:

								Console.WriteLine("Attachment Attribute (AttachCreateDate): {0} = {1}", reader.AttributeTag, prop.ReadValueAsDateTime());

								break;

							case TnefAttributeTag.AttachModifyDate:

								Console.WriteLine("Attachment Attribute (AttachModifyDate): {0} = {1}", reader.AttributeTag, prop.ReadValueAsDateTime());

								break;

							case TnefAttributeTag.AttachTitle:

								Console.WriteLine("Attachment Attribute (AttachTitle): {0} = {1}", reader.AttributeTag, prop.ReadValueAsString());

								break;

							default:

								Console.WriteLine("Attachment Attribute (unhandled): {0} = {1}", reader.AttributeTag, prop.ReadValue());

								break;
						}
					}
					while (reader.ReadNextAttribute());
				}
				else
				{
					Console.WriteLine("no attachments");
				}
			}
		}
	}
}
