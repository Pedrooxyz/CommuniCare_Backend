# Grupo de Trabalho

| Nome Completo                                | Número de Aluno |
|----------------------------------------------|-----------------|
| Pedro Miguel Cardoso Ribeiro                 | 27960           |
| Carolina Macedo Branco                       | 27983           |
| Ricardo Filipe Esteves Fernandes             | 27961           |
| João Pedro Júnior Barbosa                    | 27964           |
| Rui Filipe Fontes Miranda                    | 21135           |


# CommuniCare

A aplicação **"CommuniCare"**, surgiu de uma ideia de um grupo de alunos, autodenominado de **TechInnovation**, do curso de Engenharia de Sistemas Informáticos do Instituto Politécnico do Cávado e do Ave, e consiste num software elaborado que visa a facilitação da interação entre vizinhos, promovendo o fortalecimento do espírito comunitário.

Acreditámos que com a **CommuniCare** é possível criar um ambiente onde os moradores de uma mesma localidade podem colaborar de forma eficiente e segura entre si. O objetivo é promover a solidariedade e a partilha de recursos, acreditando que a longo prazo haverá uma promoção da confiança mútua.

Através da utilização de um dispositivo com acesso a um navegador Web, a aplicação fomenta um ecossistema digital em que a entreajuda e o apoio à comunidade são os principais pilares. Uma das funcionalidades principais da plataforma permite que os utilizadores solicitem ajuda para tarefas diversas, estas solicitações vão desde pequenas reparações domésticas até apoio em atividades mais complexas, como mudanças ou jardinagem.

Pretendemos principalmente com a aplicação criar um espaço onde os vizinhos podem facilmente pedir auxílio, eliminando a necessidade de recorrer a serviços externos, promovendo a cooperação local. Acreditámos plenamente na frase **“Boas ações geram boas ações”** e exatamente por esse motivo, confiamos que o voluntariado entre os utilizadores surgirá naturalmente, impulsionado pelo simples fluir dos acontecimentos, onde cada ato de bondade inspira o próximo, criando uma corrente contínua de ajuda mútua.

Além disso, a **Communicare** irá integrar um sistema de pontos digitais dentro da aplicação, que permitirá aos utilizadores acumular **Cares** à medida que realizam ações de voluntariado. Os pontos servirão para reconhecer e valorizar o impacto das boas ações, podendo ser utilizados para obter recompensas dentro da plataforma. O objetivo deste sistema é incentivar a participação ativa e reforçar o espírito de solidariedade entre os voluntários.

Nos pedidos de ajuda, os utilizadores podem detalhar as suas necessidades, indicando o tipo de tarefa, a localização e o horário preferido, permitindo que outros membros da comunidade se ofereçam como voluntários. Este sistema de requisição de ajuda fortalece os laços entre os moradores, tornando a comunidade mais unida e resiliente, além de criar um ambiente de confiança no qual as pessoas podem contar umas com as outras.

Outra funcionalidade base da aplicação é a possibilidade de empréstimo de objetos, um mecanismo que favorece a economia circular dentro da comunidade e o rápido acesso a materiais que apenas serão necessários temporariamente. Temos plena consciência de que muitos dos itens que usamos no dia a dia, como ferramentas, eletrodomésticos ou objetos esporádicos, podem ser partilhados temporariamente com terceiros, reduzindo o desperdício e promovendo um consumo mais responsável.

Através da plataforma, os utilizadores podem registar os objetos que desejam emprestar, estabelecendo, se pretendido, um custo em pontos da aplicação. Os interessados podem solicitar o empréstimo e, caso o dono aceite, o objeto é bloqueado para uso durante o período acordado, sendo os pontos transferidos automaticamente. Este sistema funciona como um intermediador de empréstimos dentro da comunidade, permitindo que os utilizadores acumulem e utilizem os pontos da aplicação, anulando despesas e ao mesmo tempo beneficiando quem empresta através dos pontos que podem ser trocados por uma recompensa, promovendo a partilha e fortalecendo a confiança entre os membros.

Os utilizadores podem acumular pontos, as **Cares**, ao realizarem ações de voluntariado e utilizá-los dentro da plataforma. Além do sistema de empréstimo de objetos, que permite a troca de bens entre utilizadores com os pontos da aplicação.

Outra funcionalidade base para o funcionamento da aplicação, é a existência de um **marketplace exclusivo** onde estes pontos poderão ser trocados por diversas ofertas. Neste espaço, os utilizadores terão acesso a benefícios exclusivos, como vouchers promocionais, descontos em estabelecimentos parceiros e outras recompensas oferecidas por patrocinadores. Este modelo permite não só valorizar o voluntariado, como também criar um ecossistema sustentável dentro da própria plataforma, onde cada boa ação gera novas oportunidades e vantagens para os participantes.

---

# Contextualização

Atualmente, nas comunidades urbanas e suburbanas, devido principalmente à modernização do mundo, rotinas aceleradas e ao crescimento das tecnologias, observa-se uma crescente desconexão entre vizinhos. É exatamente através da tecnologia, com um software inovador, que pretendemos ser um auxílio na solução a esse problema.

"Poderíamos dizer que a **insegurança moderna**, em suas várias manifestações, é caracterizada pelo medo dos crimes e dos criminosos. Suspeitamos dos outros e de suas intenções, nos recusamos a confiar (ou não conseguimos fazê-lo) na constância e na regularidade da solidariedade humana. Castel atribui a culpa por esse estado de coisas ao **individualismo moderno**. Segundo ele, a sociedade moderna – substituindo as comunidades solidamente unidas e as corporações (que outrora definiam as regras de proteção e controlavam a aplicação dessas regras) pelo dever individual de cuidar de si próprio e de fazer por si mesmo – foi construída sobre a areia movediça da contingência: a insegurança e a ideia de que o perigo está em toda parte são inerentes a essa sociedade" (Bauman 2009:17) [1].

Acreditámos que a **Communicare** oferece uma solução para os desafios do individualismo e insegurança mencionados por Bauman, ao promover a interação entre vizinhos por meio de uma plataforma digital. Através de um sistema de voluntariado e pontos digitais, a aplicação incentiva a cooperação local e o fortalecimento dos laços comunitários, permitindo que os moradores confiem uns nos outros e contribuam para um ambiente mais solidário e unido.

---

# CommuniCare - Backend

API e lógica do servidor da aplicação **CommuniCare**. Funciona em conjunto com o frontend [CommuniCare_Frontend](https://github.com/Pedrooxyz/CommuniCare_Frontend).

## Instalação e Execução

1. **Clonar o repositório backend**
``` 
git clone https://github.com/Pedrooxyz/CommuniCare_Backend.git
cd CommuniCare_Backend
```

2. **Restaurar dependências e construir o projeto**  
``` 
dotnet restore
dotnet ef database update
dotnet build
```

3. **Executar a API localmente**  
``` 
dotnet run
``` 

# Bibliografia

[1] Bauman, Z. (2009). *Medo Líquido: A Insegurança na Era do Medo*. Rio de Janeiro: Zahar.
