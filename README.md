# Mikro Change


## Sobre o projeto

Aplicativo de console para Windows que automatiza a troca de senha de um usuário
específico em vários RouterBoards MikroTik de uma só vez, através da API nativa do
RouterOS (porta 8728).

Foi criado para eliminar o trabalho manual e repetitivo de entrar em cada roteador
individualmente (via Winbox, WebFig ou terminal) para alterar a senha de um usuário —
um cenário comum em rotinas de segurança, como troca periódica de credenciais em
toda uma rede de equipamentos.

Com o programa, a lista de dispositivos fica cadastrada
uma única vez, e a alteração em massa é feita com um único comando, com relatório de
sucesso/erro por dispositivo ao final.

O acesso ao programa é protegido por uma senha master, e todos os dados sensíveis
(credencial admin dos MikroTiks e lista de dispositivos) ficam armazenados
localmente de forma criptografada.


## Stack utilizada

| Tecnologia | Uso no projeto | Por que foi escolhida |
|---|---|---|
| **C# / .NET 10** | Linguagem e runtime da aplicação | Multiplataforma dentro do ecossistema Windows, forte suporte nativo a criptografia e sockets. |
| **PBKDF2-HMAC-SHA256** (`Rfc2898DeriveBytes`) | Hash da senha master | Algoritmo de derivação de chave recomendado pela OWASP para senhas; nativo do .NET, sem dependências externas; 600.000 iterações e comparação em tempo constante contra timing attacks |
| **DPAPI (Windows Data Protection API)** | Criptografia da credencial admin e da lista de dispositivos | Mecanismo nativo do Windows (o mesmo usado para senhas de Wi-Fi e credenciais salvas no navegador); os dados ficam atrelados à conta do usuário do Windows, sem precisar gerenciar chave de criptografia própria. |
| **API nativa do RouterOS (TCP/8728)** | Comunicação com os MikroTiks | Protocolo binário oficial do MikroTik, implementado do zero (sentenças, palavras, codificação de tamanho variável), com suporte aos dois esquemas de login (moderno e legado com desafio MD5). |


## Sobre as funcionalidades

- **Proteção com senha master:** o programa exige acesso mediante senha
  master, cadastrada na primeira vez que é executado, antes de liberar qualquer
  funcionalidade. A senha pode ser alterada posteriormente.
- **Gerenciamento de usuário admin:** cadastra (ou atualiza, mediante confirmação da
  credencial atual) o usuário e senha admin usados para autenticar em todos os
  MikroTiks cadastrados.
- **Cadastrar novo dispositivo:** adiciona um MikroTik à lista, a partir do IP e de
  um nome. Impede duplicidade de IP ou de nome.
- **Remover dispositivo:** remove um MikroTik da lista a partir do IP informado.
- **Listar dispositivos:** exibe todos os MikroTiks cadastrados, mostrando IP e nome
  lado a lado.
- **Alteração de senha em massa:** solicita o nome do usuário (dentro do MikroTik) que terá a
  senha alterada e a nova senha, pede confirmação final e então percorre todos os
  dispositivos cadastrados, alterando a senha em cada um e reportando sucesso ou
  erro individualmente.


## Como compilar

Pré-requisitos: SDK do .NET 10 instalado no Windows
(https://dotnet.microsoft.com/pt-br/download/dotnet/10.0).
 
```
cd MikroChange
dotnet build -c Release
```
 
Para gerar um único .exe (self-contained, não precisa de .NET instalado na máquina
de destino):
 
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
 
O executável final ficará em:
`bin\Release\net10.0-windows\win-x64\publish\MikroChange.exe`
 
## Como rodar
 
```
MikroChange.exe
```
 
Na primeira execução, o programa vai pedir para cadastrar a senha master.
Nas próximas execuções, ele vai pedir a senha master antes de liberar o menu.


## Onde os dados ficam salvos?
 
`%LocalAppData%\MikroChange\`
 
- `master.dat` — salt + hash PBKDF2-HMAC-SHA256 da senha master (não é criptografado
  porque já é um hash irreversível, mesmo princípio de um `/etc/shadow`).
- `admin.dat` — usuário/senha admin dos MikroTiks, protegido via DPAPI
  (Windows Data Protection API), atrelado à conta do Windows do usuário atual.
- `devices.dat` — lista de dispositivos (IP + nome), protegida via DPAPI.
Se você rodar o programa com outro usuário do Windows (ou copiar os arquivos para
outra máquina/perfil), a DPAPI não vai conseguir descriptografar `admin.dat` e
`devices.dat` — isso é esperado e proposital, é a proteção funcionando.


# Contribuições

Contribuições são sempre bem-vindas!\
Se você conhece uma maneira melhor de fazer algo, por favor, me avise!\
Ou sinta-se a vontade para enviar um novo PR!

At.te,\
<b>Felipe S. Carmo</b>.
