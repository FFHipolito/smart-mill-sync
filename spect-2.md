# SPEC-SMART-MILL-SYNC.md: Especificação Técnica e Roteiro de Execução para Agente de Código (Antigravity)

---

## 1. Contexto e Motivação Estratégica

### 1.1. O Cenário de Negócio
A Suzano opera uma cadeia integrada de celulose de alta complexidade. Dois momentos dessa cadeia geram atrito operacional:
1. **Tribo Florestal (MLPlan & CEM):** Realiza o planejamento logístico de colheita e transporte da madeira até a portaria da fábrica.
2. **Tribo Industrial (Portal Industrial: Programação GN, Mantis, PGR+, Qualitracker):** Recebe os caminhões no pátio e alimenta os digestores e caldeiras de recuperação.

### 1.2. O Problema Operacional Real
A madeira chega com variações expressivas de umidade base úmida (35% a 65%).
* Madeira excessivamente úmida (> 50%) reduz drasticamente o poder calorífico na queima e exige maior tempo de digestão.
* Para compensar a perda térmica e evitar a queda de pressão de vapor no digestor, a operação fabril é obrigada a queimar **Gás Natural (GN)** suplementar.
* A falta de sincronização em tempo real entre a balança de entrada da madeira e a esteira do **Programação GN** gera oscilações operacionais e elevação do custo variável.

### 1.3. O Propósito do Projeto
O **Smart Mill Sync** atua como a ponte tecnológica entre a logística florestal e a matriz energética industrial:
* Monitora as entradas de cargas no pátio com telemetria biométrica.
* Calcula o impacto estequeométrico em tempo real no consumo de GN.
* Notifica os operadores via WebSockets.
* **Objetivo de Carreira:** Demonstrar ao consultor técnico da Tribo Industrial proficiência em **C# / .NET 8**, arquitetura corporativa moderna (**Clean Architecture, DDD, CQRS com MediatR, SignalR, EF Core**), unificada com **Blazor WebAssembly** (100% C# cliente/servidor) e visão holística do negócio Suzano.

---

## 2. Stack Tecnológica e Decisões de Arquitetura

* **Linguagem & Runtime:** C# / .NET 8 SDK
* **Solução:** `SmartMillSync.sln` com arquitetura limpa em camadas:
  * `SmartMillSync.Domain`: Entidades ricas, regras invariantes, Enums (dependência zero).
  * `SmartMillSync.Application`: Casos de uso, CQRS (MediatR), validações fluentes (FluentValidation), interfaces de repositórios.
  * `SmartMillSync.Infrastructure`: Entity Framework Core (Npgsql / PostgreSQL), mapeamentos Fluent API, migrations, `BackgroundService` em C#.
  * `SmartMillSync.Shared`: DTOs, Requests/Responses e enums compartilhados diretamente entre API e front-end.
  * `SmartMillSync.Api`: ASP.NET Core Web API, Controllers REST, SignalR Hub, middlewares de logging e exception handling.
  * `SmartMillSync.Client`: Blazor WebAssembly (SPA em C# nativo rodando via WebAssembly no navegador).
* **Banco de Dados:** PostgreSQL (Render / Supabase / Local via Docker).
* **Mensageria/Real-Time:** ASP.NET Core SignalR.

---

## 3. Regras de Negócio e Cálculos Estequeométricos

### 3.1. Entidade `WoodDelivery` (Pátio de Madeira)
* **Atributos:**
  * `Id` (Guid): Identificador único imutável.
  * `TruckPlate` (string): Formato Mercosul ou clássico, sempre em uppercase sem traços.
  * `ForestOrigin` (string): Ex.: "Módulo Florestal Mucuri-04", "Horto Aracruz-12".
  * `WoodSpecies` (string): Ex.: "Eucalyptus Urograndis", "Eucalyptus Grandis".
  * `GrossWeight` (decimal, toneladas): Peso bruto na balança de entrada.
  * `TareWeight` (decimal, toneladas): Tara na balança de saída.
  * `NetWeight` (decimal, calculado): `GrossWeight - TareWeight`.
  * `MoisturePercentage` (decimal, %): Teor de umidade medido por amostragem.
  * `DryWeightTons` (decimal, calculado): `NetWeight * (1 - (MoisturePercentage / 100))`.
  * `Status` (Enum: `InTransit`, `ArrivedAtGate`, `Weighed`, `Unloading`, `Completed`, `Rejected`).
  * `RequiresThermalCompensation` (bool): `MoisturePercentage > 50.0m`.

### 3.2. Balanço Térmico e Demanda de Gás Natural (`GasConsumptionSimulation`)
* Linha Base de Consumo: 1 tonelada de madeira com umidade base (45%) demanda zero gás adicional.
* Cada 1% de umidade acima de 50% exige **3.85 Nm³ de Gás Natural por tonelada líquida processada** para manter o rendimento da caldeira.
* Cálculo de Compensação:
  `ExtraGasVolume (Nm³) = (MoisturePercentage - 50.0) * 3.85 * NetWeight`
* Se `ExtraGasVolume > 150 Nm³`, classificar como `AlertLevel.High` e disparar evento SignalR prioritário.

---

## 4. Passo a Passo Detalhado para Execução pelo Agente (Antigravity)

### Fase 1: Camada de Domínio (`SmartMillSync.Domain`)
1. Garantir que as entidades criadas não possuam setters públicos desprotegidos.
2. Implementar `Enums/DeliveryStatus.cs` (`InTransit = 1`, `ArrivedAtGate = 2`, `Weighed = 3`, `Unloading = 4`, `Completed = 5`, `Rejected = 6`).
3. Implementar `Entities/WoodDelivery.cs` conforme modelado, com validação de consistência no construtor.
4. Criar a entidade `Entities/GasTelemetryRecord.cs` para registrar históricos de vazão (Nm³/h), temperatura e desvio térmico.

### Fase 2: Camada Compartilhada (`SmartMillSync.Shared`)
1. Implementar records imutáveis em `DTOs/WoodDeliveryDtos.cs`:
   - `CreateWoodDeliveryRequest`: Dados de entrada da portaria.
   - `WoodDeliveryResponse`: Payload estruturado para consumo no Blazor e na API.
   - `EnergyBalanceSummaryDto`: Métricas consolidadas (Total de Cargas, Biomassa Seca Acumulada, Gás Adicional Estimado).

### Fase 3: Camada de Aplicação (`SmartMillSync.Application`)
1. **MediatR Commands:**
   - `RegisterWoodDeliveryCommand(CreateWoodDeliveryRequest Request) : IRequest<WoodDeliveryResponse>`
   - `RegisterWoodDeliveryCommandHandler`: Instancia o domínio, persiste via repositório e retorna a resposta mapeada.
2. **MediatR Queries:**
   - `GetActiveDeliveriesQuery : IRequest<IReadOnlyList<WoodDeliveryResponse>>`
   - `GetMillEnergyBalanceQuery : IRequest<EnergyBalanceSummaryDto>`
3. **FluentValidation:**
   - Criar `RegisterWoodDeliveryValidator` aplicando validações para placa, limites de pesagem e faixa de umidade válida (10% a 70%).
4. **Interfaces:**
   - `IWoodDeliveryRepository`
   - `ISignalRNotificationService`

### Fase 4: Camada de Infraestrutura (`SmartMillSync.Infrastructure`)
1. Implementar `SmartMillDbContext : DbContext`:
   - Configurar tabelas usando Fluent API (`IEntityTypeConfiguration<WoodDelivery>`).
   - Mapear precisão decimal explícita (`HasPrecision(18, 2)`).
2. Implementar o repositório `WoodDeliveryRepository : IWoodDeliveryRepository`.
3. Implementar o worker de fundo `Workers/ThermalBalanceWorker : BackgroundService`:
   - Executar a cada 30 segundos usando `PeriodicTimer`.
   - Calcular se há cargas acumuladas com excesso de umidade e enviar notificações broadcast via SignalR.

### Fase 5: Camada de API (`SmartMillSync.Api`)
1. Configurar `Program.cs`:
   - Injeção de dependência das camadas.
   - Configuração de CORS aberto para o cliente Blazor.
   - Registro do endpoint do SignalR: `app.MapHub<MillSyncHub>("/hubs/mill-sync")`.
2. Criar `Controllers/WoodDeliveriesController.cs`:
   - `POST /api/v1/deliveries` (executa command do MediatR).
   - `GET /api/v1/deliveries` (executa query do MediatR).
   - `GET /api/v1/energy-balance` (executa query de consolidação).
3. Criar `Hubs/MillSyncHub.cs` herdando de `Hub`.

### Fase 6: Camada de Front-End Blazor (`SmartMillSync.Client`)
1. Configurar injeção de `HttpClient` e cliente SignalR em `Program.cs`.
2. Criar layout industrial escuro (inspirado no padrão do Portal Industrial da Suzano).
3. **Página Principal (`Pages/YardOverview.razor`):**
   - Painel superior com KPIs em tempo real: Toneladas de Madeira no Dia, Teor Médio de Umidade, Compensação GN Necessária (Nm³).
   - Tabela de chegadas com badge dinâmico (Verde: Umidade Normal; Âmbar: Umidade Moderada; Vermelho: Alerta Crítico GN).
   - Formulário modal para simulação de chegada de novo caminhão.
   - Escuta ativa via SignalR para atualizar a lista sem refresh de página.

---

## 5. Roteiro de Verificação e Argumentação Técnica
Ao concluir, o Antigravity deve garantir que:
1. O comando `dotnet build` passe com zero warnings e zero erros.
2. Todas as entidades estejam 100% isoladas de frameworks de banco de dados na camada Domain.
3. Não haja strings mágicas nem lógica de negócio embutida diretamente nos Controllers HTTP.