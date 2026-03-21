# K8s Notifications API

API REST responsável por gerenciar e enviar **notificações** dentro de uma arquitetura baseada em **microserviços**, preparada para execução em **containers Docker** e orquestração com **Kubernetes (K8s)**.

O projeto demonstra na prática conceitos fundamentais de **Cloud-Native Applications**, incluindo:

* Containerização da aplicação
* Deploy declarativo em Kubernetes
* Gerenciamento de configuração com ConfigMap
* Armazenamento seguro de credenciais com Secrets
* Orquestração e escalabilidade de containers

Este repositório faz parte de um conjunto de serviços demonstrando como aplicações podem ser executadas em um **cluster Kubernetes utilizando manifests YAML**.

---

# Arquitetura da Aplicação

A arquitetura segue o padrão comum para workloads executados em Kubernetes.

```
      Client
        │
        ▼
   Kubernetes Service
        │
        ▼
    Deployment
        │
        ▼
       Pods
(Notifications API)
        │
        ├── ConfigMap
        └── Secret
```

Componentes principais:

| Componente | Função                     |
| ---------- | -------------------------- |
| Deployment | Gerenciamento dos pods     |
| Service    | Comunicação entre serviços |
| ConfigMap  | Configuração da aplicação  |
| Secret     | Dados sensíveis            |

---

# Tecnologias Utilizadas

Backend

* .NET
* ASP.NET Core
* REST API

Infraestrutura

* Docker
* Kubernetes
* YAML Manifests
* kubectl

---

# Estrutura do Repositório

```
k8s-notifications-api
│
├── src/
│   └── NotificationsAPI
│
├── k8s/
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── deployment.yaml
│   └── service.yaml
│
├── Dockerfile
├── .dockerignore
└── README.md
```

---

# Pré-requisitos

Para executar o projeto é necessário possuir instalado:

* .NET SDK
* Docker
* Kubernetes Cluster
* kubectl

Exemplos de clusters locais que podem ser utilizados:

* Minikube
* Kind
* Docker Desktop Kubernetes

---

# Executando a Aplicação Localmente

Clone o repositório:

```bash
git clone https://github.com/techGabrielBr/k8s-notifications-api.git
cd k8s-notifications-api
```

Executar a aplicação:

```bash
dotnet restore
dotnet build
dotnet run --project src/NotificationsAPI
```

A API estará disponível em:

```
http://localhost:5000
```

---

# Containerização com Docker

Construir a imagem da aplicação:

```bash
docker build -t notifications-api .
```

Executar o container:

```bash
docker run -p 5000:5000 notifications-api
```

A API ficará disponível em:

```
http://localhost:5000
```

---

# Deploy no Kubernetes

Os manifests Kubernetes estão localizados na pasta:

```
k8s/
```

Aplicar todos os recursos no cluster:

```bash
kubectl apply -f k8s/
```

---

# Verificando os Recursos

Listar recursos criados:

```bash
kubectl get all
```

Ver pods em execução:

```bash
kubectl get pods
```

Ver services:

```bash
kubectl get svc
```

Ver logs de um pod:

```bash
kubectl logs <nome-do-pod>
```

---

# Descrição dos Manifests Kubernetes

## ConfigMap

Arquivo:

```
k8s/configmap.yaml
```

Responsável por armazenar **configurações não sensíveis** da aplicação.

Exemplos:

* parâmetros da aplicação
* URLs de serviços externos
* feature flags

Essas configurações são injetadas nos containers como variáveis de ambiente.

---

## Secret

Arquivo:

```
k8s/secret.yaml
```

Armazena **dados sensíveis** utilizados pela aplicação.

Exemplos:

* connection strings
* tokens
* credenciais

Secrets são armazenados em **base64** e podem ser consumidos pelos containers como:

* variáveis de ambiente
* volumes montados

---

## Deployment

Arquivo:

```
k8s/deployment.yaml
```

Define o Deployment da aplicação.

Responsabilidades:

* criar e gerenciar pods
* manter pods ativos
* recriar pods em caso de falha
* permitir atualizações da aplicação

Configurações típicas:

* imagem Docker
* número de réplicas
* variáveis de ambiente
* portas expostas

---

## Service

Arquivo:

```
k8s/service.yaml
```

Cria um Service para permitir comunicação entre os pods da aplicação e outros serviços do cluster.

Funções:

* endpoint estável
* balanceamento de carga entre pods
* abstração sobre IPs dinâmicos dos pods

---

# Testando a API

Exemplo de requisição:

```bash
curl http://localhost:5000/notifications
```

Exemplo de criação de notificação:

```bash
curl -X POST http://localhost:5000/catalog/place-order \
-H "Content-Type: application/json" \
-d '{
    "gameId": "6B29FC40-CA47-1067-B31D-00DD010662DA",
    "price": 40.0
}'
```

---

# Objetivo do Projeto

Este projeto foi criado com o objetivo de demonstrar:

* deploy de APIs em Kubernetes
* arquitetura baseada em containers
* separação de configuração e credenciais
* boas práticas de infraestrutura cloud-native

Ele também pode ser utilizado como **base para estudo de microserviços e Kubernetes**.
