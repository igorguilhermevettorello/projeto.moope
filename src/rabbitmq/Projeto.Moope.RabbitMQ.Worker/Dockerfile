FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Projeto.Moope.RabbitMQ.Worker.csproj"
RUN dotnet publish "src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Projeto.Moope.RabbitMQ.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Projeto.Moope.RabbitMQ.Worker.dll"]

