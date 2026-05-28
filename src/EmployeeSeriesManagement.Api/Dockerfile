FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/EmployeeSeriesManagement.Domain/EmployeeSeriesManagement.Domain.csproj src/EmployeeSeriesManagement.Domain/
COPY src/EmployeeSeriesManagement.Application/EmployeeSeriesManagement.Application.csproj src/EmployeeSeriesManagement.Application/
COPY src/EmployeeSeriesManagement.Infrastructure/EmployeeSeriesManagement.Infrastructure.csproj src/EmployeeSeriesManagement.Infrastructure/
COPY src/EmployeeSeriesManagement.Api/EmployeeSeriesManagement.Api.csproj src/EmployeeSeriesManagement.Api/
RUN dotnet restore src/EmployeeSeriesManagement.Api/EmployeeSeriesManagement.Api.csproj

COPY src/ src/
RUN dotnet publish src/EmployeeSeriesManagement.Api/EmployeeSeriesManagement.Api.csproj \
    -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "EmployeeSeriesManagement.Api.dll"]
