# SQL Server 2025 with Full-Text Search.
# The stock Linux image doesn't include full-text search, so this adds the mssql-server-fts package.
# SQL Server for Linux only ships amd64 packages: on Apple Silicon this runs under Rosetta (see readme).
FROM --platform=linux/amd64 mcr.microsoft.com/mssql/server:2025-latest

USER root

# The base image already has Microsoft's signing key (/usr/share/keyrings/microsoft-prod.gpg).
# Add the mssql-server-2025 repository for the image's Ubuntu release, then install full-text search.
RUN . /etc/os-release \
 && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/${VERSION_ID}/mssql-server-2025 ${VERSION_CODENAME} main" \
      > /etc/apt/sources.list.d/mssql-server-2025.list \
 && apt-get update \
 && ACCEPT_EULA=Y apt-get install -y --no-install-recommends mssql-server-fts \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

USER mssql
