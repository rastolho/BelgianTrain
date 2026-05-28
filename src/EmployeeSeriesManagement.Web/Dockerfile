FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/EmployeeSeriesManagement.Domain/EmployeeSeriesManagement.Domain.csproj src/EmployeeSeriesManagement.Domain/
COPY src/EmployeeSeriesManagement.Application/EmployeeSeriesManagement.Application.csproj src/EmployeeSeriesManagement.Application/
COPY src/EmployeeSeriesManagement.Web/EmployeeSeriesManagement.Web.csproj src/EmployeeSeriesManagement.Web/
RUN dotnet restore src/EmployeeSeriesManagement.Web/EmployeeSeriesManagement.Web.csproj

COPY src/ src/
RUN printf '{"ApiBaseUrl":""}' > src/EmployeeSeriesManagement.Web/wwwroot/appsettings.json
RUN dotnet publish src/EmployeeSeriesManagement.Web/EmployeeSeriesManagement.Web.csproj \
    -c Release -o /app

FROM nginx:alpine AS runtime
COPY --from=build /app/wwwroot /usr/share/nginx/html
COPY src/EmployeeSeriesManagement.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
