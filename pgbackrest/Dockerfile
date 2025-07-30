FROM debian:bullseye

RUN apt-get update && \
    apt-get install -y curl gnupg software-properties-common && \
    echo "deb http://apt.postgresql.org/pub/repos/apt bullseye-pgdg main" > /etc/apt/sources.list.d/pgdg.list && \
    curl https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add - && \
    apt-get update && \
    apt-get install -y pgbackrest postgresql-client && \
    apt-get clean