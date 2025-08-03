FROM postgres:15

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    pgbackrest \
    && rm -rf /var/lib/apt/lists/*

COPY scripts /scripts
RUN chmod +x /scripts/*.sh