using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{
    public class ProtocolException : ApplicationException, IEncodable
    {
        private NameValueCollection _query = new NameValueCollection();

        public ProtocolException(NameValueCollection query, string text)
            : base(text)
        {
            _query = query;
        }

        public bool HasReturnTo
        {
            get
            {
                return (_query["openid.return_to"] != null);
            }
        }

        #region IEncodable Members

        public EncodingType WhichEncoding
        {
            get 
            {
                if (this.HasReturnTo)
                    return EncodingType.ENCODE_URL;

                string mode = _query.Get("openid.mode");
                if (mode != null)
                    if (mode != "checkid_setup" &&
                        mode != "checkid_immediate")
                        return EncodingType.ENCODE_KVFORM;

                // Notes from the original port
                //# According to the OpenID spec as of this writing, we are
                //# probably supposed to switch on request type here (GET
                //# versus POST) to figure out if we're supposed to print
                //# machine-readable or human-readable content at this
                //# point.  GET/POST seems like a pretty lousy way of making
                //# the distinction though, as it's just as possible that
                //# the user agent could have mistakenly been directed to
                //# post to the server URL.

                //# Basically, if your request was so broken that you didn't
                //# manage to include an openid.mode, I'm not going to worry
                //# too much about returning you something you can't parse.
                return EncodingType.ENCODE_NONE;
            }
        }

        public Uri EncodeToUrl()
        {
            string return_to = _query.Get("openid.return_to");
            if (return_to == null)
                throw new ApplicationException("return_to URL has not been set.");

            NameValueCollection q = new NameValueCollection();
            q.Add("openid.mode", "error");
            q.Add("openid.error", this.Message);

            UriBuilder builder = new UriBuilder(return_to);
            Util.AppendQueryArgs(ref builder, q);

            return new Uri(builder.ToString());
        }

        public byte[] EncodeToKVForm()
        {
            Hashtable d = new Hashtable();

            d.Add("mode", "error");
            d.Add("error", this.Message);

            return KVUtil.DictToKV((IDictionary)d);
        }

        #endregion
    }
}
