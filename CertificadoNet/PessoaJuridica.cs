using System;

namespace CertificadoNet
{
    public sealed class PessoaJuridica
    {
        public string CNPJ { get; set; }
        public string RazaoSocial { get; set; }
        public string INSS { get; set; }
        public PessoaFisica Responsavel { get; set; }

        public PessoaJuridica(string cnpj, string inss, string razaoSocial, string nomeResponsavel, string dadosResponsavel)
        {
            try
            {
                if (string.IsNullOrEmpty(cnpj) || string.IsNullOrEmpty(razaoSocial) || string.IsNullOrEmpty(nomeResponsavel) || string.IsNullOrEmpty(dadosResponsavel))
                    throw new CertificadoNetException(4);

                CNPJ = cnpj;
                INSS = inss;
                RazaoSocial = razaoSocial;
                Responsavel = new PessoaFisica(nomeResponsavel, dadosResponsavel);
            }
            catch (Exception ex)
            {
                throw new CertificadoNetException(4, ex);
            }
        }
    }
}
