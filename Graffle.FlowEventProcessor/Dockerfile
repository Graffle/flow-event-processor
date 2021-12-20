FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

COPY ./Graffle.FlowEventProcessor/Graffle.FlowEventProcessor.csproj Graffle.FlowEventProcessor/

WORKDIR /app/Graffle.FlowEventProcessor
RUN dotnet restore Graffle.FlowEventProcessor.csproj
COPY ./Graffle.FlowEventProcessor .
RUN dotnet publish -c Release -o out
# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app/Graffle.FlowEventProcessor
COPY --from=build /app/Graffle.FlowEventProcessor/out ./
ENTRYPOINT ["dotnet", "Graffle.FlowEventProcessor.dll"]