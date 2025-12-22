FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Notes.API/NotesApp.API.csproj Notes.API/

RUN dotnet restore Notes.API/NotesApp.API.csproj

COPY Notes.API/ Notes.API/
RUN dotnet publish Notes.API/NotesApp.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NotesApp.API.dll"]