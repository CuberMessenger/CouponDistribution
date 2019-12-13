FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["CouponDistribution/CouponDistribution.csproj", "CouponDistribution/"]
RUN dotnet restore "CouponDistribution/CouponDistribution.csproj"
COPY . .
WORKDIR "/src/CouponDistribution"
RUN dotnet build "CouponDistribution.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CouponDistribution.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CouponDistribution.dll"]