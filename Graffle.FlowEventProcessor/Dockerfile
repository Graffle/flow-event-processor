FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /app

#copy csproj
COPY Graffle.FlowEventProcessor.csproj ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image, use buster-slim as alpine is missing some required libraries for grpc
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
ENV WebhookUrl=${WebhookUrl}
ENV EventId=${EventId}
ENV Verbose=False
ENV MaximumBlockScanRange=200
ENV FlowNode=${FlowNode}
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "Graffle.FlowEventProcessor.dll"]