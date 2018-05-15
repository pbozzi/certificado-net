using System.Collections.Generic;
using System.Text;

namespace CertificadoNet
{
    class ASN1Helper
    {
        private byte[] _rawdata;
        public List<TAG> TagList;

        public ASN1Helper(ref byte[] rawdata)
        {
            _rawdata = rawdata;
            TagList = new List<TAG>();
            LoadTagList();
        }

        private bool LoadTagList()
        {
            TAG newtag;
            bool bretval = false;
            for (int offset = 0; offset < _rawdata.Length;)
            {
                if ((newtag = new TAG(ref _rawdata, ref offset)) != null)
                    TagList.Add(newtag);
            }
            return bretval;
        }
    }

    enum TagID : int
    {
        RFC822NAME = 1,
        INTEGER = 2,
        BIT_STRING = 3,
        OCTET_STRING = 4,
        NULL = 5,
        OBJECT_IDENTIFIER = 6,
        UTF8_STRING = 12,
        SEQUENCE = 16,
        SET = 17,
        PrintableString = 19,
        T61String = 20,
        IA5String = 22,
        UTCTime = 23
    }

    enum TagClass : int
    {
        Universal = 0x00,
        Application = 0x40,
        Context_specific = 0x80,
        Private = 0xc0
    }

    class TAG
    {
        protected TagID _tagid;
        protected TagClass _tagclass;
        protected int _start_offset;
        protected int _lenght_octets;
        protected int _start_contents;

        public TagID TagId
        {
            get { return _tagid; }
        }

        public TagClass TagClassValue
        {
            get { return _tagclass; }
        }

        public int StartOffset
        {
            get { return _start_offset; }
        }

        public int LengthOctets
        {
            get { return _lenght_octets; }
        }

        public int StartContents
        {
            get { return _start_contents; }
        }

        public TAG(ref byte[] rawdata, ref int offset)
        {
            _start_offset = offset;
            _lenght_octets = 0;

            _tagclass = (TagClass)(rawdata[offset] & 0xc0);

            // verifica se a tag está em formato Short ou Long
            if ((rawdata[offset] & 0x1f) == 0x1f)
            { // formato Long, não usado nos certificados da ICP-Brasil
                _tagid = (TagID)CalculaBase128(rawdata, ref offset);
            }
            else
            { // formato Short
                _tagid = (TagID)(rawdata[offset] & 0x1f);
                offset++;
            }

            // Octetos de tamanho
            if ((rawdata[offset] & 0x80) == 0x00)
            { // Formato Short: tamanho de até 127 bytes
                _lenght_octets = (int)(rawdata[offset++] & 0x7f);
            }
            else
            { // Formato Long: tamanho em 2 até 127 octetos
                int lenoctetos = (int)rawdata[offset++] & 0x7f;
                _lenght_octets = CalculaBase256(rawdata, ref offset, lenoctetos);
            }
            _start_contents = offset;

            switch (_tagid)
            {
                case TagID.OBJECT_IDENTIFIER:
                case TagID.PrintableString:
                case TagID.OCTET_STRING:
                case TagID.UTF8_STRING:
                case TagID.BIT_STRING:
                case TagID.IA5String:
                case TagID.INTEGER:
                case TagID.RFC822NAME:
                case TagID.T61String:
                case TagID.UTCTime:
                    offset += _lenght_octets;
                    break;
                case TagID.NULL:
                case TagID.SEQUENCE:
                case TagID.SET:
                    break;
                default:
                    break;
            }
        }

        public string Format(byte[] rawdata)
        {
            string retval = string.Empty;

            switch (_tagid)
            {
                case TagID.OBJECT_IDENTIFIER:
                    retval = CalculaOID(rawdata, _start_contents, _lenght_octets);
                    break;
                case TagID.IA5String:
                case TagID.T61String:
                case TagID.PrintableString:
                case TagID.UTCTime:
                case TagID.OCTET_STRING:
                case TagID.UTF8_STRING:
                case TagID.RFC822NAME:
                    retval = (new ASCIIEncoding()).GetString(rawdata, _start_contents, _lenght_octets);
                    break;
                default:
                    retval = (string)_tagid.ToString();
                    break;
            }

            //case TagID.BIT_STRING:
            //case TagID.INTEGER:
            //case TagID.UTCTime:
            //case TagID.NULL:
            //case TagID.SEQUENCE:
            //case TagID.SET:
            //    break;

            return retval;
        }

        public static int CalculaBase256(byte[] rawdata, ref int offset, int length)
        {
            int tamanho = 0;

            if ((rawdata != null) && (rawdata.Length >= offset + length))
            {
                tamanho = (int)rawdata[offset++];

                for (int i = 1; i < length; i++)
                {
                    tamanho <<= 8;
                    tamanho += (int)rawdata[offset++];
                }
            }

            return tamanho;
        }

        public static int CalculaBase128(byte[] rawdata, ref int offset)
        {
            int tamanho = 0;

            if ((rawdata != null) && (rawdata.Length > offset))
            {
                tamanho = (int)(rawdata[offset++] & 0x7f);

                do
                {
                    tamanho <<= 7;
                    tamanho += (int)(rawdata[offset++] & 0x7f);
                } while ((rawdata.Length > offset) && (rawdata[offset++] & 0x80) == 0x80);
            }

            return tamanho;
        }

        public static string CalculaOID(byte[] rawdata, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}.{1}", ((int)rawdata[offset] / 40), ((int)rawdata[offset] % 40));
            offset++;
            length--;

            for (int i = offset; i < (offset + length); i++)
            {
                int auxvalue = (int)rawdata[i] & 0x7f;

                if (((rawdata[i] & 0x80) == 0x80) && (i < offset + length))
                {
                    auxvalue = ((int)rawdata[i++] & 0x7f) << 7;
                    int auxvalue2 = ((int)rawdata[i] & 0x7f);

                    while (((rawdata[i] & 0x80) == 0x80) && (i < offset + length))
                    {
                        auxvalue += auxvalue2;
                        auxvalue <<= 7;
                        auxvalue2 = ((int)rawdata[++i] & 0x7f);
                    }
                    sb.AppendFormat(".{0}", auxvalue + auxvalue2);
                }
                else
                    sb.AppendFormat(".{0}", auxvalue);
            }
            return sb.ToString();
        }
    }
}
