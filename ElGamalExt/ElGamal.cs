﻿/************************************************************************************
 This implementation of the ElGamal encryption scheme is based on the code from [1].
 It was changed and extended by Vasily Sidorov (http://bazzilic.me/).
 
 This code is provided as-is and is covered by the WTFPL 2.0 [2] (except for the
 parts that belong by O'Reilly - they are covered by [3]).
 
 
 [1] Adam Freeman & Allen Jones, Programming .NET Security: O'Reilly Media, 2003,
     ISBN 9780596552275 (http://books.google.com.sg/books?id=ykXCNVOIEuQC)
 
 [2] WTFPL – Do What the Fuck You Want to Public License, website,
     (http://wtfpl.net/)
 
 [3] Tim O'Reilly, O'Reilly Policy on Re-Use of Code Examples from Books: website,
     2001, (http://www.oreillynet.com/pub/a/oreilly/ask_tim/2001/codepolicy.html)
 ************************************************************************************/

using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace ElGamalExt
{
    public enum ElGamalPaddingMode : byte
    {
        ANSIX923,
        LeadingZeros,
        Zeros
    };

    public abstract class ElGamal : AsymmetricAlgorithm
    {
        public ElGamalPaddingMode Padding;

        public abstract void ImportParameters(ElGamalParameters p_parameters);
        public abstract ElGamalParameters ExportParameters(bool p_include_private_params);
        public abstract byte[] EncryptData(byte[] p_data);
        public abstract byte[] DecryptData(byte[] p_data);
        public abstract byte[] Sign(byte[] p_hashcode);
        public abstract bool VerifySignature(byte[] p_hashcode, byte[] p_signature);

        public abstract byte[] Multiply(byte[] p_first, byte[] p_second);

        public override string ToXmlString(bool p_include_private)
        {
            ElGamalParameters x_params = ExportParameters(p_include_private);
            // create a new string builder
            StringBuilder x_sb = new StringBuilder();
            // add the header
            x_sb.Append("<ElGamalKeyValue>");
            // add the public elements from the parameters
            x_sb.Append("<P>" + Convert.ToBase64String(x_params.P) + "</P>");
            x_sb.Append("<G>" + Convert.ToBase64String(x_params.G) + "</G>");
            x_sb.Append("<Y>" + Convert.ToBase64String(x_params.Y) + "</Y>");
            x_sb.Append("<Padding>" + Padding.ToString() + "</Padding>");
            if (p_include_private)
            {
                // we need to include X, which is the part of private key
                x_sb.Append("<X>" + Convert.ToBase64String(x_params.X) + "</X>");
            }
            // add the final element
            x_sb.Append("</ElGamalKeyValue>");
            return x_sb.ToString();
        }

        public override void FromXmlString(String p_string)
        {
            // create the params that we will use as the result
            ElGamalParameters x_params = new ElGamalParameters();
            // create a text reader using a string reader
            XmlTextReader x_reader =
                new XmlTextReader(new System.IO.StringReader(p_string));

            // run through the elements in the xml string
            while (x_reader.Read())
            {
                // we are only interested in processing start nodes
                if (true || x_reader.IsStartElement())
                {
                    switch (x_reader.Name)
                    {
                        case "P":
                            // set the value for P
                            x_params.P = Convert.FromBase64String(x_reader.ReadString());
                            break;
                        case "G":
                            // set the value for G
                            x_params.G = Convert.FromBase64String(x_reader.ReadString());
                            break;
                        case "Y":
                            // set the value for Y
                            x_params.Y = Convert.FromBase64String(x_reader.ReadString());
                            break;
                        case "Padding":
                            // set the padding mode
                            x_params.Padding = (ElGamalPaddingMode)Enum.Parse(typeof(ElGamalPaddingMode), x_reader.ReadString());
                            break;
                        case "X":
                            // set the value for X (this would not be found in a 
                            // string that was generated by excluding the private
                            // elements.
                            x_params.X = Convert.FromBase64String(x_reader.ReadString());
                            break;
                    }
                }
            }
            // Import the result
            ImportParameters(x_params);
        }
    }
}
