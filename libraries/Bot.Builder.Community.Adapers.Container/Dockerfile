FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["AdapterBot.csproj", "./AdapterBot/"]

RUN dotnet restore "AdapterBot/AdapterBot.csproj"
COPY . ./AdapterBot/
WORKDIR "/src/AdapterBot"
RUN dotnet build "AdapterBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdapterBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdapterBot.dll"]