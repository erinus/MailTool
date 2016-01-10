using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using MimeKit.Encodings;
using Ude;

namespace MimeTool
{
	public class Util
	{
		public static String RefineCharset(String name)
		{
			String result = name.ToLower();

			result = Regex.Replace(result, @"[\\]", String.Empty);

			if (result.Equals("utf8"))
			{
				result = "utf-8";
			}

			if (result.Equals("big-5"))
			{
				result = "big5";
			}

			if (result.Equals("iso-2022-cn"))
			{
				result = "x-cp50227";
			}

			if (result.Equals("cp-850"))
			{
				result = "ibm850";
			}

			if (result.Equals("cp932"))
			{
				result = "x-ms-cp932";
			}

			if (result.Equals("cp936"))
			{
				result = "gb2312";
			}

			if (result.Equals("cp1251"))
			{
				result = "Windows-1251";
			}

			if (result.Equals("cp1252"))
			{
				result = "Windows-1252";
			}

			if (result.Equals("cp1254"))
			{
				result = "Windows-1254";
			}

			if (result.Equals("binary"))
			{
				result = "ascii";
			}

			if (result.StartsWith("u"))
			{
				result = "utf-8";
			}

			// When MimeParser turns to stable status,
			// uncomment this segment to avoid unknown
			// codepage error.
			/*
			try
			{
				Encoding.GetEncoding(result);
			}
			catch (Exception e)
			{
				result = "ascii";
			}
			*/

			return result;
		}

		public static Encoding DetectCharset(Byte[] bytes)
		{
			Encoding detectedCharset = Encoding.ASCII;

			CharsetDetector detector = new CharsetDetector();

			detector.Feed(bytes);

			detector.DataEnd();

			if (detector.Confidence > 0.5)
			{
				detectedCharset = Encoding.GetEncoding(detector.Charset);
			}

			return detectedCharset;
		}

		public static Encoding DetectCharset(Byte[] bytes, Encoding defaultCharset)
		{
			Encoding detectedCharset = defaultCharset;

			if (detectedCharset == null)
			{
				detectedCharset = Encoding.ASCII;
			}

			CharsetDetector detector = new CharsetDetector();

			detector.Feed(bytes);

			detector.DataEnd();

			if (detector.Confidence > 0.7)
			{
				detectedCharset = Encoding.GetEncoding(
					Util.RefineCharset(detector.Charset)
				);
			}

			return detectedCharset;
		}

		public static String DecodeString(String data)
		{
			return Regex.Replace(
				data,
				@"=\?(?<charset>.+?)\?(?<encoding>.)\?(?<encoded>.+?)\?=",
				new MatchEvaluator(
					delegate(Match find)
					{
						String result = find.Groups[0].Value;

						String charset = find.Groups["charset"].Value.ToLower();

						String encoding = find.Groups["encoding"].Value.ToLower();

						String encoded = find.Groups["encoded"].Value;

						Byte[] decoded = new Byte[] { };

						charset = Util.RefineCharset(charset);

						switch (encoding)
						{
							case "b":

								decoded = Util.DecodeBase64(encoded);

								result = Util.DetectCharset(
									decoded,
									Encoding.GetEncoding(charset)
								).GetString(decoded);

								break;

							case "q":

								decoded = Util.DecodeQuoted(encoded);

								result = Util.DetectCharset(
									decoded,
									Encoding.GetEncoding(charset)
								).GetString(decoded).Replace("_", " ");

								break;
						}

						return result;
					}
				)
			);
		}

		public static Byte[] DecodeBase64(String data)
		{
			Byte[] buffer = Encoding.ASCII.GetBytes(data);

			Base64Decoder decoder = new Base64Decoder();

			Byte[] output = new Byte[decoder.EstimateOutputLength(buffer.Length)];

			Int32 length = decoder.Decode(buffer, 0, buffer.Length, output);

			MemoryStream stream = new MemoryStream(output, 0, length);

			return stream.ToArray();
		}

		public static Byte[] DecodeQuoted(String data)
		{
			Byte[] buffer = Encoding.ASCII.GetBytes(data);

			QuotedPrintableDecoder decoder = new QuotedPrintableDecoder();

			Byte[] output = new Byte[decoder.EstimateOutputLength(buffer.Length)];

			Int32 length = decoder.Decode(buffer, 0, buffer.Length, output);

			MemoryStream stream = new MemoryStream(output, 0, length);

			return stream.ToArray();
		}

		public static Byte[] DecodeBinary(String data)
		{
			Byte[] buffer = Encoding.ASCII.GetBytes(data);

			HexDecoder decoder = new HexDecoder();

			Byte[] output = new Byte[decoder.EstimateOutputLength(buffer.Length)];

			Int32 length = decoder.Decode(buffer, 0, buffer.Length, output);

			MemoryStream stream = new MemoryStream(output, 0, length);

			return stream.ToArray();
		}
	}
}
