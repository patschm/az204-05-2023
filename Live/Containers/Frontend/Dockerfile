FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80
ENV CON_STR=""

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/Frontend
RUN dotnet restore "Frontend.csproj"
RUN dotnet build "Frontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Frontend.csproj" -c Release -o /dist /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /dist .
ENTRYPOINT ["dotnet", "Frontend.dll"]
