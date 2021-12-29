FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

RUN mkdir -p /root/src/MarketAnalysis  
WORKDIR /root/src/MarketAnalysis  
COPY MarketAnalysis     MarketAnalysis  
WORKDIR /root/src/MarketAnalysis/MarketAnalysis

RUN dotnet restore ./MarketAnalysis.csproj  
RUN dotnet publish -c release -o published -r linux-arm

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim-arm32v7

WORKDIR /root/  
COPY --from=builder /root/src/MarketAnalysis/MarketAnalysis/published .

CMD ["dotnet", "./MarketAnalysis.dll"]