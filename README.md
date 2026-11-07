# Mikro Change - 1.0.1


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

O acesso ao programa é protegido por uma senha master, e a lista de dispositivos fica
armazenada localmente de forma criptografada. A credencial admin dos MikroTiks nunca é
gravada em disco: ela é solicitada a cada operação que altera dispositivos, mantida
apenas em memória e descartada ao final da execução.


## Stack utilizada

| Tecnologia | Uso no projeto | Por que foi escolhida |
|---|---|---|
| **C# / .NET 10** | Linguagem e runtime da aplicação | Multiplataforma dentro do ecossistema Windows, forte suporte nativo a criptografia e sockets. |
| **PBKDF2-HMAC-SHA256** (`Rfc2898DeriveBytes`) | Hash da senha master | Algoritmo de derivação de chave recomendado pela OWASP para senhas; nativo do .NET, sem dependências externas; 600.000 iterações e comparação em tempo constante contra timing attacks |
| **API nativa do RouterOS (TCP/8728)** | Comunicação com os MikroTiks | Protocolo binário oficial do MikroTik, implementado do zero (sentenças, palavras, codificação de tamanho variável), com suporte aos dois esquemas de login (moderno e legado com desafio MD5). |


## Sobre as funcionalidades

- **Proteção com senha master:** o programa exige acesso mediante senha
  master, cadastrada na primeira vez que é executado, antes de liberar qualquer
  funcionalidade. A senha pode ser alterada posteriormente.
- **Cadastrar novo dispositivo:** adiciona um MikroTik à lista, a partir do IP e de
  um nome. Impede duplicidade de IP ou de nome.
- **Remover dispositivo:** remove um MikroTik da lista a partir do IP informado.
- **Listar dispositivos:** exibe todos os MikroTiks cadastrados, mostrando IP e nome
  lado a lado.
- **Alteração de senha em massa:** solicita o nome do usuário (dentro do MikroTik) que terá a
  senha alterada e a nova senha, pede confirmação final, solicita o usuário e a senha
  admin (usados somente nesta execução) e então percorre todos os dispositivos
  cadastrados, alterando a senha em cada um e reportando sucesso ou erro individualmente.
- **Habilitar/desabilitar usuário em massa:** pergunta se o usuário será habilitado ou
  desabilitado e qual é o usuário (dentro do MikroTik), pede confirmação final, solicita
  o usuário e a senha admin (usados somente nesta execução) e então percorre todos os
  dispositivos cadastrados, aplicando a ação em cada um, com relatório individual de
  sucesso/erro. Por segurança, o próprio usuário admin informado não pode ser habilitado
  nem desabilitado por este fluxo.


## Credenciais de administrador (somente em memória)

O usuário e a senha admin usados para entrar nos MikroTiks **não são guardados em lugar
nenhum**: nem em arquivo, nem entre uma operação e outra. Eles existem apenas na memória
do programa, enquanto uma operação está sendo executada.


### Quando o usuário deve informá-las

Somente nas operações que alteram dispositivos:

- **Executar alteração de senha em massa**
- **Habilitar/Desabilitar usuário em massa**

Em ambas, o programa primeiro pergunta os dados da operação (usuário-alvo e nova senha, ou
ação e usuário-alvo) e pede a confirmação final. Só depois disso solicita o **usuário admin**
e a **senha admin** (digitada de forma mascarada) e, em seguida, percorre os dispositivos.

- Cadastrar, remover e listar dispositivos e alterar a senha master **não** pedem credenciais admin.
- Se a operação for cancelada antes da confirmação final, as credenciais admin nem chegam a ser digitadas.
- Senha admin vazia cancela a operação.
- Ao terminar o lote (com sucesso, com falhas ou por erro) as credenciais são descartadas.
  Na próxima operação, o programa pede usuário e senha admin novamente.


### Por que foi feito assim

- **Não há nada em disco para vazar.** A credencial admin dá controle sobre todos os
  roteadores cadastrados. Um arquivo que a contenha, mesmo criptografado, pode ser copiado,
  entrar em backups ou ser sincronizado com a nuvem. Sem arquivo, não há o que roubar do disco.
- **A proteção em disco não valia contra quem usa a mesma conta do Windows.** O arquivo ficava
  atrelado apenas à conta do Windows: qualquer processo executando com essa conta (inclusive
  um malware) podia lê-lo e descriptografá-lo sem precisar da senha master, que só controla o
  acesso à interface do programa.
- **Programa aberto não significa poder de alteração.** Mesmo com o programa aberto e a senha
  master já digitada, ninguém consegue alterar os MikroTiks sem conhecer a credencial admin,
  que precisa ser digitada a cada operação.
- **Janela de exposição mínima.** A credencial só existe na memória enquanto a operação roda,
  em vez de ficar disponível durante todo o tempo em que o programa está aberto.


### Limitações

- **Descartar não é o mesmo que zerar a memória.** Em .NET, strings são imutáveis: o programa
  solta as referências à credencial para que o coletor de lixo possa liberar a memória, mas não
  consegue sobrescrever os bytes imediatamente, e a senha passa por cópias internas durante o
  login. O que se garante é que nada é gravado em disco e que nenhuma referência à credencial
  sobrevive de uma operação para a outra.
- **A API usada (porta 8728) não tem TLS.** No login moderno (RouterOS 6.43 ou mais novo), a
  senha é enviada em texto claro pela rede até o dispositivo. Execute o programa a partir de
  uma rede de gerenciamento confiável.


## Como compilar

Pré-requisitos: SDK do .NET 10 instalado no Windows
(https://dotnet.microsoft.com/pt-br/download/dotnet/10.0).
 
 
Para gerar um único .exe (self-contained, não precisa de .NET instalado na máquina
de destino):

```
dotnet publish MikroChange.UI/MikroChange.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none
```

O executável final ficará em:
`MikroChange\MikroChange.UI\bin\Release\net10.0\win-x64\publish\MikroChange.exe`


## Onde os dados ficam salvos?
 
`%LocalAppData%\MikroChange\`
 
- `master.dat` — salt + hash PBKDF2-HMAC-SHA256 da senha master (não é criptografado
  porque já é um hash irreversível, mesmo princípio de um `/etc/shadow`).
- `devices.dat` — lista de dispositivos (IP + nome), criptografada e atrelada à conta do
  Windows do usuário atual (não abre em outro usuário ou em outra máquina).

A credencial admin dos MikroTiks **não** é salva em nenhum arquivo (veja a seção
"Credenciais de administrador"). Se existir um `admin.dat` de versões anteriores,
ele é removido automaticamente ao abrir o programa.


# Contribuições

Contribuições são sempre bem-vindas!\
Se você conhece uma maneira melhor de fazer algo, por favor, me avise!\
Ou sinta-se a vontade para enviar um novo PR!

At.te,\
<b>Felipe S. Carmo</b>.
