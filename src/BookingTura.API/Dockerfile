# ==========================
# Build
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

COPY . .

RUN dotnet restore src/BookingTura.API/BookingTura.API.csproj

RUN dotnet publish src/BookingTura.API/BookingTura.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ==========================
# Runtime
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "BookingTura.API.dll"]