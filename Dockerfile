# Build image stage
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
RUN apk update && apk add bash && apk add curl

WORKDIR /api

# nuget restore
# Install Credential Provider and set env variables to enable Nuget restore with auth
ARG PAT
RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS "{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/ntguiri/_packaging/ntguiri/nuget/v3/index.json\", \"password\":\"${PAT}\"}]}"
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER 0

COPY . .
RUN dotnet restore -s "https://pkgs.dev.azure.com/ntguiri/_packaging/ntguiri/nuget/v3/index.json" -s "https://api.nuget.org/v3/index.json" "NG.Auth.sln"

# dotnet build and test
RUN dotnet build -c Release --no-restore
RUN dotnet test --filter FullyQualifiedName~UnitTest -c Release --logger "trx;LogFileName=result.trx" --no-build --no-restore -r /publish/test
# dotnet publish
RUN dotnet publish -c Release --no-build -o /publish

# Runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /publish

COPY --from=build /publish .

ENTRYPOINT ["dotnet", "NG.Auth.Presentation.WebAPI.dll"]