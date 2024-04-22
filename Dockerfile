FROM mcr.microsoft.com/dotnet/sdk:8.0.203-bookworm-slim-arm64v8 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore --use-current-runtime
# Build and publish a release
RUN dotnet publish -c Release -o out --use-current-runtime --self-contained false --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0.3-bookworm-slim-arm64v8
#####################
#PUPPETEER RECIPE
#####################
RUN apt-get update && apt-get -f install && apt-get -y install wget gnupg2 apt-utils
RUN wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add -
RUN echo 'deb [arch=arm64] http://dl.google.com/linux/chrome/deb/ stable main' >> /etc/apt/sources.list
RUN apt-get update \
&& apt-get install -y chromium --no-install-recommends --allow-downgrades fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf
######################
#END PUPPETEER RECIPE
######################
ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/chromium"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Lom.dll"]