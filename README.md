[![Build Status](https://travis-ci.org/pbozzi/certificado-net.svg?branch=master)](https://travis-ci.org/pbozzi/certificado-net)
[![Total Downloads](https://img.shields.io/nuget/dt/CertificadoNet.svg)](https://www.nuget.org/packages/CertificadoNet)
[![Latest Stable Version](https://img.shields.io/nuget/v/CertificadoNet.svg)](https://www.nuget.org/packages/CertificadoNet)

# certificado-net
Biblioteca para leitura de certificados digitais padrão ICP-Brasil em .NET. Retorna os dados dos certificados tipos eCNPJ e eCPF.

## Instalação

```sh
$ Install-Package CertificadoNet
$ dotnet add package CertificadoNet
```

## Utilização

```csharp
var cert = new CertificadoDigital("cert.cer");
```

## Propriedades

* TipoCertificado: eCNPJ, eCPF ou Outro.
* ValidoAPartir: data e hora início da validade (não é possível utilizar o certificado antes dessa data).
* ValidoAte: data e hora fim da validade (não é possível utilizar o certificado após essa data).
* PeriodoValido: informa se o certificado não está vencido.
* IcpBrasil: informa se o certificado digital possui a ICP-Brasil como AC raiz.
* CadeiaValida: informa se o certificado possui uma cadeia válida.
* PessoaFisica: quando tipo eCPF, contém as informações do titular do certificado.
* PessoaJuridica: quando tipo eCNPJ, contém as informações do titular do certificado.

Atributos da Pessoa Física:
* Nome
* Data de Nascimento
* CPF
* NIS
* RG
* Órgão expedidor
* INSS
* Título de eleitor
* Zona eleitoral
* Seção
* Município

Atributos da Pessoa Jurídica:
* CNPJ
* Razão social
* INSS
* Responsável (atributos da Pessoa Física)

## Requisitos

- .NET Standard >= 2.0.0

## Package

https://www.nuget.org/packages/CertificadoNet

## Licença

MIT License

Copyright (c) 2018 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
