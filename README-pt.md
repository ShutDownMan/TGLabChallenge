# TGLabChallenge API

## 📖 Sobre o Projeto

O TGLabChallenge API é uma aplicação desenvolvida em .NET 6 que implementa funcionalidades de gerenciamento de jogadores, apostas e carteiras. A aplicação utiliza Entity Framework Core para persistência de dados e SignalR para notificações em tempo real.

## 📊 Modelo Entidade-Relacionamento

O diagrama abaixo representa o modelo entidade-relacionamento (MER) da aplicação:

![Modelo Entidade-Relacionamento](MER.svg)

## 🚀 Como executar

### Pré-requisitos

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Docker](https://www.docker.com/)
- [Visual Studio](https://visualstudio.microsoft.com/) (opcional)

### Configuração do Banco de Dados

Certifique-se de configurar as variáveis de ambiente no arquivo `.env` para o banco de dados PostgreSQL. Para desenvolvimento local, o banco de dados SQLite será utilizado automaticamente.

### Migração do Banco de Dados

Antes de iniciar a aplicação, é necessário executar as migrações para garantir que o banco de dados esteja atualizado.

Você pode fazer isso primeiro **setando as váriaveis de ambiente necessárias** no arquivo `.env` e, em seguida, executando o seguinte comando:

```bash
dotnet ef database update --project Infrastructure
```

ou, se estiver utilizando Docker, execute o serviço de migrações:

```bash
docker compose run --rm migrations
```

## 🛠️ Modos de Execução

### Desenvolvimento

Execute a aplicação localmente com o seguinte comando:

```bash
dotnet run --project API
```

### Produção

Utilize Docker Compose para executar a aplicação em produção:

```bash
docker compose up -d
```

### Testes

Execute os testes automatizados com o seguinte comando:

```bash
dotnet test
```

### Visual Studio

1. Abra a solução no Visual Studio.
2. Clique com o botão direito no projeto `API` e selecione **Definir como projeto de inicialização**.
3. Pressione **CTRL + F5** para iniciar a aplicação.

#### Rodando Testes no Visual Studio

1. No menu, vá em **Testar > Executar Todos os Testes** ou pressione **Ctrl + R, A**.
2. Visualize os resultados na janela **Test Explorer**.

## 📦 Estrutura do Projeto

- **API**: Contém os controladores e configurações da aplicação.
- **Application**: Contém os serviços e modelos de negócio.
- **Domain**: Define as entidades e enums do domínio.
- **Infrastructure**: Implementa a persistência de dados e configurações do banco de dados.
- **Tests**: Contém os testes unitários e de integração.

### WebDashboard

O `WebDashboard` é uma aplicação React que serve como um painel para testar os endpoints da API. Ele permite que os usuários insiram um token JWT e interajam com os endpoints de autenticação, jogador e apostas.

#### Pré-requisitos

- [Node.js 20.x](https://nodejs.org/)
- [npm](https://www.npmjs.com/)

#### Como executar

1. Navegue até o diretório `WebDashboard`:
   ```bash
   cd WebDashboard
   ```

2. Instale as dependências:
   ```bash
   npm install
   ```

3. Inicie o servidor de desenvolvimento:
   ```bash
   npm start
   ```

4. Acesse o painel no navegador em `http://localhost:3000`.

#### Docker

O WebDashboard já está incluído na configuração do Docker Compose. Quando você executa:

```bash
docker compose up -d
```

O WebDashboard será automaticamente construído e implantado, disponível em `http://localhost:3000`.

## 🛡️ Segurança

A aplicação utiliza autenticação JWT para proteger as APIs. Certifique-se de configurar as chaves e parâmetros JWT no arquivo `appsettings.json`.

### Certificado HTTPS para Produção

Para testes de deploy em produção, o arquivo `localhost.pfx` é utilizado como certificado HTTPS. O caminho e a senha do certificado são configurados por meio de variáveis de ambiente no arquivo `.env`:

```env
CERTIFICATE_PATH=./localhost.pfx
CERTIFICATE_PASSWORD=123456Sete
```

Certifique-se de substituir essas variáveis por valores seguros antes de realizar o deploy em produção real. Além disso, configure o uso do certificado no pipeline da aplicação no arquivo `Program.cs`.

Certifique-se de armazenar o certificado e a senha em um local seguro e configurar variáveis de ambiente apropriadas para produção.

## 📚 Documentação da API

A documentação da API pode ser acessada em `/swagger` quando a aplicação estiver em execução (no modo de desenvolvimento).

A flag de desenvolvimento está presente no arquivo `launchSettings.json` e pode ser ativada para habilitar o Swagger:

```json
"profiles": {
  "API": {
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development"
    }
  }
}
```

## 🐳 Docker Compose

O arquivo `docker-compose.yml` configura os serviços necessários para execução em produção, incluindo:

- **API**: Serviço principal da aplicação.
- **Migrations**: Serviço para aplicar migrações no banco de dados.
- **PostgreSQL**: Banco de dados relacional.
- **WebDashboard**: Aplicação React para testar os endpoints da API.

Para iniciar todos os serviços com Docker Compose:

```bash
docker compose up -d
```

Após iniciar os serviços:
- A API estará disponível em `http://localhost:8080`
- O WebDashboard estará disponível em `http://localhost:3000`

### Configuração de Ambiente

A aplicação utiliza variáveis de ambiente definidas no arquivo `.env`:

```env
# Configurações do banco de dados
POSTGRES_USER=prod-user
POSTGRES_PASSWORD=prod-password
POSTGRES_DB=prod-db
POSTGRES_HOST=postgres-db
POSTGRES_PORT=5432

# Configurações da API
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTPS_PORTS=8081
ASPNETCORE_HTTP_PORTS=8080

# Configurações do certificado
CERTIFICATE_PATH=./localhost.pfx
CERTIFICATE_PASSWORD=123456Sete
```

Certifique-se de modificar esses valores para seu ambiente de produção.

## 📝 Licença

Este projeto está sob a licença MIT. Consulte o arquivo `LICENSE` para mais detalhes.
