# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./

# BlazorEnableCompression=false ensures all _framework/ files are present
# as plain uncompressed files that nginx can serve directly.
# Without this flag, dotnet publish only outputs .br/.gz compressed versions
# and nginx returns index.html as a fallback for every missing file.
RUN dotnet publish -c Release -o /app/publish -p:BlazorEnableCompression=false

# Stage 2: Serve with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot ./
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
