# POC sobre download de arquivos e zip de arquivos de um bucket do aws S3

Nessa POC eu estou baixando arquivos de um bucket do AWS s3, zipando e retornando o zip para download.

## Técnologias utilizadas

-  AWS S3
- :computer: Asp.net Core
- :whale: Docker / Docker-compose

## Como rodar o projeto


- Para rodar o projeto, crie um arquivo "**.env**" baseando se no arquivo "**environment-example.env**" no diretório principal, no mesmo nível do arquivo "**docker-compose.yml**". Nesse arquivo .env são salvas as configurações do S3, bem como informações importantes para acessar-lo, como se trata de informações sigilosas eu gerencio essas informações com váriaveis de ambiente no projeto.
    - Descrição das váriaveis de ambientes:

|  Várivel       | Descrição            | 
|---             |---                   |
|AWS_KEY_ID      |Chave publica do S3   |
|AWS_KEY_SECRET  |Chave privada do S3   |
|AWS_BUCKET_NAME |Nome do Bucket no S3  | 


 - Após criar o arquivo com as váriaveis de ambeite basta ter o docker instalado e rodar o seguinte comando no seu terminal:

```
docker-compose up 
```

- Após isso a api estará acessível através da porta 5001.
