FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /BallBoi
ENV APIKEY=ENTERAPIKEY
ENV GUILDID=ENTERGUILDID
#copy everything
COPY . ./ 
#restore as distinct layers
RUN dotnet restore 
#Build and publish a release
RUN dotnet publish -c Release -o out 
#build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /src
COPY --from=build-env /BallBoi/out .
ENTRYPOINT ["dotnet", "BallBoi.dll"]
