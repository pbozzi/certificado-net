using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace CertificadoNet
{
    public sealed class CertificadoDigital
    {
        private const string ICP_BRASIL = "Autoridade Certificadora Raiz Brasileira";

        private const string OID_NOME_ALTERNATIVO_SUJEITO = "2.5.29.17";
        private const string OID_PF_DADOS_TITULAR =         "2.16.76.1.3.1";
        private const string OID_PJ_NOME_RESPONSAVEL =      "2.16.76.1.3.2";
        private const string OID_PJ_CNPJ =                  "2.16.76.1.3.3";
        private const string OID_PJ_DADOS_RESPONSAVEL =     "2.16.76.1.3.4";
        private const string OID_PF_ELEITORAL =             "2.16.76.1.3.5";
        private const string OID_PF_INSS =                  "2.16.76.1.3.6";
        private const string OID_PJ_INSS =                  "2.16.76.1.3.7";
        private const string OID_PJ_NOME_EMPRESARIAL =      "2.16.76.1.3.8";

        public Dictionary<int, string> ErrosValidacaoCadeia { get; private set; }

        public TipoCertificadoEnum TipoCertificado { get; private set; }
        public DateTime ValidoAPartir { get; private set; }
        public DateTime ValidoAte { get; private set; }
        public bool IcpBrasil { get; private set; }
        public bool CadeiaValida { get; private set; }
        public bool PeriodoValido { get; private set; }
        public PessoaFisica PessoaFisica { get; private set; }
        public PessoaJuridica PessoaJuridica { get; private set; }

        public CertificadoDigital(byte[] buffer, bool validaCadeia = true) : this(new X509Certificate2(buffer), validaCadeia) { }
        public CertificadoDigital(string nomeArquivo, bool validaCadeia = true) : this(new X509Certificate2(nomeArquivo), validaCadeia) { }
        public CertificadoDigital(X509Certificate2 certificado, bool validaCadeia = true)
        {
            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                var raizesIcpBrasil = store.Certificates.Find(X509FindType.FindByIssuerName, ICP_BRASIL, true);

                ValidoAPartir = certificado.NotBefore;
                ValidoAte = certificado.NotAfter;
                PeriodoValido = DateTime.Now > certificado.NotBefore && DateTime.Now < certificado.NotAfter;

                var chain = new X509Chain();
                if (validaCadeia)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 30);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

                    if (chain.Build(certificado))
                        CadeiaValida = true;
                    else
                    {
                        ErrosValidacaoCadeia = new Dictionary<int, string>();
                        chain.ChainStatus.ToList().ForEach(x => ErrosValidacaoCadeia.Add((int)x.Status, x.StatusInformation));
                    }
                }
                else
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                var certificadoRaiz = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
                IcpBrasil = raizesIcpBrasil.Contains(certificadoRaiz);

                TipoCertificado = ObterTipo(certificado);
                if (TipoCertificado == TipoCertificadoEnum.eCPF)
                    PessoaFisica = ObterDadosPessoaFisica(certificado);
                else if (TipoCertificado == TipoCertificadoEnum.eCNPJ)
                    PessoaJuridica = ObterDadosPessoaJuridica(certificado);
            }
            catch (Exception ex)
            {
                throw new CertificadoNetException("Erro na leitura do certificado", ex);
            }
        }

        private TipoCertificadoEnum ObterTipo(X509Certificate2 certificado)
        {
            foreach (X509Extension extension in certificado.Extensions)
            {
                if (extension.Oid.Value == OID_NOME_ALTERNATIVO_SUJEITO)
                {
                    if (extension.Format(false).Contains(OID_PF_DADOS_TITULAR))
                        return TipoCertificadoEnum.eCPF;
                    else if (extension.Format(false).Contains(OID_PJ_DADOS_RESPONSAVEL))
                        return TipoCertificadoEnum.eCNPJ;
                }
            }

            return TipoCertificadoEnum.Outro;
        }
        private PessoaFisica ObterDadosPessoaFisica(X509Certificate2 certificado)
        {
            try
            {
                string oid;
                byte[] extensao;
                ASN1Helper helper;

                foreach (var ext in certificado.Extensions)
                {
                    if (ext.Oid.Value == OID_NOME_ALTERNATIVO_SUJEITO)
                    {
                        if (ext.Format(false).Contains(OID_PF_DADOS_TITULAR))
                        {
                            extensao = ext.RawData;
                            helper = new ASN1Helper(ref extensao);

                            for (int i = 0; i < helper.TagList.Count; i++)
                            {
                                if (helper.TagList[i].TagId == TagID.OBJECT_IDENTIFIER)
                                {
                                    oid = helper.TagList[i].Format(extensao);
                                    if (oid == OID_PF_DADOS_TITULAR)
                                    {
                                        for (i++; (i < helper.TagList.Count) && ((helper.TagList[i].TagId != TagID.OCTET_STRING) && (helper.TagList[i].TagId != TagID.UTF8_STRING) && (helper.TagList[i].TagId != TagID.PrintableString)); i++) ;
                                        if (i < helper.TagList.Count)
                                        {
                                            var dadosTitular = helper.TagList[i].Format(extensao);

                                            int ini = certificado.Subject.IndexOf("CN=") + 3;
                                            int meio = certificado.Subject.IndexOf(":", ini);
                                            string nomeTitular;
                                            if (meio != -1)
                                                nomeTitular = certificado.Subject.Substring(ini, meio - ini);
                                            else
                                            {
                                                int fim = certificado.Subject.IndexOf(", ", ini) - 1;
                                                nomeTitular = certificado.Subject.Substring(ini, fim - ini + 1);
                                            }

                                            var pessoaFisica = new PessoaFisica(nomeTitular, dadosTitular);
                                            return pessoaFisica;
                                        }
                                    }
                                }
                            }
                        }
                        else
                            throw new CertificadoNetException(1);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new CertificadoNetException(5, ex);
            }
        }
        private PessoaJuridica ObterDadosPessoaJuridica(X509Certificate2 certificado)
        {
            try
            {
                byte[] extensao;
                ASN1Helper helper;
                string cnpj = null;
                string inss = null;
                string razaoSocial = null;
                string nomeReponsavel = null;
                string dadosResponsavel = null;
                string oid;

                foreach (var ext in certificado.Extensions)
                {
                    if (ext.Oid.Value == OID_NOME_ALTERNATIVO_SUJEITO)
                    {
                        if (ext.Format(false).Contains(OID_PJ_DADOS_RESPONSAVEL))
                        {
                            extensao = ext.RawData;
                            helper = new ASN1Helper(ref extensao);
                            for (int i = 0; i < helper.TagList.Count; i++)
                            {
                                if (helper.TagList[i].TagId == TagID.OBJECT_IDENTIFIER)
                                {
                                    oid = helper.TagList[i].Format(extensao);
                                    for (i++; (i < helper.TagList.Count) && ((helper.TagList[i].TagId != TagID.OCTET_STRING) && (helper.TagList[i].TagId != TagID.UTF8_STRING) && (helper.TagList[i].TagId != TagID.PrintableString)); i++) ;

                                    if (oid.Equals(OID_PJ_CNPJ))
                                        cnpj = helper.TagList[i].Format(extensao);
                                    else if (oid.Equals(OID_PJ_INSS))
                                        inss = helper.TagList[i].Format(extensao);
                                    else if (oid.Equals(OID_PJ_NOME_EMPRESARIAL))
                                        razaoSocial = helper.TagList[i].Format(extensao);
                                    else if (oid.Equals(OID_PJ_NOME_RESPONSAVEL))
                                        nomeReponsavel = helper.TagList[i].Format(extensao);
                                    else if (oid.Equals(OID_PJ_DADOS_RESPONSAVEL))
                                        dadosResponsavel = helper.TagList[i].Format(extensao);

                                    var pessoaJuridica = new PessoaJuridica(cnpj, inss, razaoSocial, nomeReponsavel, dadosResponsavel);
                                    return pessoaJuridica;
                                }
                            }
                        }
                        else
                            throw new CertificadoNetException(2);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new CertificadoNetException(6, ex);
            }
        }
    }
}
