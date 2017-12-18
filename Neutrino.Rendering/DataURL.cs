using System;
using System.Text;

namespace Neutrino
{
    public class DataURL
    {
        public string MediaType { get; set; }
        public string CharSet { get; set; }
        public byte[] Data { get; set; }

        public static bool FromUri(string uri, out DataURL output)
        {
            // https://tools.ietf.org/html/rfc2397
            // BASE64 
            // data:[< mediatype >][; base64],< data >
            //
            // dataurl:= "data:"[mediatype][";base64"] "," data
            // mediatype  := [type "/" subtype] * (";" parameter )
            // data:= *urlchar
            // parameter:= attribute "=" value

            const string DATA_PREFIX = "data:";
            if (!uri.StartsWith(DATA_PREFIX))
            {
                output = null;
                return false;
            }

            var endOfPrefix = uri.IndexOf(',');
            if (endOfPrefix <= -1)
            {
                output = null;
                return false;
            }

            // DON'T include the comma on end
            var mediaTypeToken = uri.Substring(DATA_PREFIX.Length, endOfPrefix - DATA_PREFIX.Length);
            var parameters = mediaTypeToken.Split(new char[] { ';' }, StringSplitOptions.None);

            const string PLAIN_TEXT = "text/plain";
            var mediaType = (parameters.Length > 0 && string.IsNullOrEmpty(parameters[0]))
                ? PLAIN_TEXT
                : parameters[0];

            const string BASE_64_FLAG = "base64";
            var useBase64 = (parameters.Length > 0 && parameters[parameters.Length - 1] == BASE_64_FLAG);

            // LOOK FOR CHARSET
            string textCharsetString = "US-ASCII";
            const string CHARSET_TOKEN = "charset";
            for (var i = 1; i < parameters.Length; i += 1)
            {
                if (parameters[i].Contains("="))
                {
                    var paramTokens = parameters[i].Split(new[] { '=' }, StringSplitOptions.None);
                    if (paramTokens.Length == 2 && paramTokens[0] == CHARSET_TOKEN)
                    {
                        textCharsetString = paramTokens[1];
                    }
                }
            }

            // DEFAULT is text/plain;charset=US-ASCII

            var dataToken = uri.Substring(endOfPrefix + 1);
            if (useBase64)
            {
                output = new DataURL
                {
                    MediaType = mediaType,
                    CharSet = textCharsetString,
                    Data = Convert.FromBase64String(dataToken),
                };
                return true;
            }
            else
            {
                var encoder = Encoding.GetEncoding(textCharsetString);
                output = new DataURL
                {
                    MediaType = mediaType,
                    CharSet = textCharsetString,
                    Data = encoder.GetBytes(dataToken),
                };
                return true;
            }
        }
    }    
}
