using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CertificadoNet
{
    [Serializable]
    public class CertificadoNetException : Exception
    {
        private static readonly IDictionary<int, string> hash = new Dictionary<int, string>();
        public int CodigoErro { get; private set; }

        static CertificadoNetException()
        {
            hash.Add(new KeyValuePair<int, string>(1, "O certificado informado não é um e-CPF"));
            hash.Add(new KeyValuePair<int, string>(2, "O certificado informado não é um e-CNPJ"));
            hash.Add(new KeyValuePair<int, string>(3, "Pessoa Física inválida"));
            hash.Add(new KeyValuePair<int, string>(4, "Pessoa Jurídica inválida"));
            hash.Add(new KeyValuePair<int, string>(5, "Erro ao obter dados da Pessoa Física"));
            hash.Add(new KeyValuePair<int, string>(6, "Erro ao Obter dados da Pessoa Jurídica"));
        }

        public CertificadoNetException() { }

        public CertificadoNetException(string message) : base(message) { }

        public CertificadoNetException(string message, Exception inner) : base(message, inner) { }

        public CertificadoNetException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public CertificadoNetException(int codigoErro) : base(hash[codigoErro])
        {
            CodigoErro = codigoErro;
        }

        public CertificadoNetException(int codigoErro, Exception ex) : base(hash[codigoErro], ex)
        {
            CodigoErro = codigoErro;
        }
    }
}
