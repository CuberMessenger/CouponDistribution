#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
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

COPY --from=build /src/CouponDistribution/dummywebconfig .

RUN cp dummywebconfig web.config

ENTRYPOINT ["dotnet", "CouponDistribution.dll"]