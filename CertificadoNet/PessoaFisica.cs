using System;
using System.Globalization;

namespace CertificadoNet
{
    public sealed class PessoaFisica
    {
        public string Nome { get; private set; }
        public DateTime DataNascimento { get; private set; }
        public string CPF { get; private set; }
        public string NIS { get; private set; }
        public string RG { get; private set; }
        public string OrgaoExpedidor { get; private set; }
        public string Inss { get; private set; }
        public string TituloEleitor { get; private set; }
        public string ZonaEleitoral { get; private set; }
        public string Secao { get; private set; }
        public string Municipio { get; private set; }

        public PessoaFisica(string nome, string dados)
        {
            try
            {
                if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(dados))
                    throw new CertificadoNetException(3);

                Nome = nome;
                if (DateTime.TryParseExact(dados.Substring(0, 8), "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataNascimento))
                    DataNascimento = dataNascimento;
                CPF = dados.Substring(8, 11);
                NIS = dados.Substring(19, 11);
                RG = dados.Substring(30, 15);
                if (!RG.Equals("0".PadRight(15, '0')))
                    OrgaoExpedidor = dados.Substring(45);
            }
            catch (Exception ex)
            {
                throw new CertificadoNetException(3, ex);
            }
        }
    }
}
