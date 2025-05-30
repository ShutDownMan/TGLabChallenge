# TGLabChallenge API

## 🚀 Como executar

### Visual Studio
1. Abra a solução no Visual Studio.
2. Clique com o botão direito no projeto `TGLabChallenge.API` e selecione **Definir como projeto de inicialização**.
3. Certifique-se de que o banco de dados está atualizado:
   - Abra o Console do Gerenciador de Pacotes e execute:
     ```
     Update-Database
     ```
4. Pressione **F5** para iniciar a aplicação.

#### Rodando Testes no Visual Studio
1. Abra a solução no Visual Studio.
2. No menu, vá em **Testar > Executar Todos os Testes** ou pressione **Ctrl + R, A**.
3. Você pode visualizar os resultados na janela **Test Explorer**.

### Dev
```bash
dotnet ef database update
dotnet run --project TGLabChallenge.API
```

### Prod
```bash
docker compose up -d
```

### Testes
```bash
dotnet test
```
