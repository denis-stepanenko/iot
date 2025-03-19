FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine
WORKDIR /app
COPY . .
ENV ASPNETCORE_URLS http://*:5000
RUN dotnet restore
RUN dotnet publish -c Release -o out
WORKDIR /app/out

EXPOSE 5000

CMD ["dotnet", "IOTAPI.dll"]