# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Serve with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot ./
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
