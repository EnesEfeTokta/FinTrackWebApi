FROM postgres:15

# pgBackRest, cron ve gerekli diğer araçları kur
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    gnupg \
    procps \
    pgbackrest \
    cron \
    && rm -rf /var/lib/apt/lists/*

COPY ./scripts /scripts
RUN chmod +x /scripts/*.sh