#!/bin/bash
set -e

if [ ! -f "${PGDATA}/PG_VERSION" ]; then
    echo "Initializing database..."
    initdb -D ${PGDATA} --wal-segsize=16
fi

echo "Configuring postgresql.conf for archiving..."
echo "listen_addresses = '*'" >> ${PGDATA}/postgresql.conf
echo "wal_level = replica" >> ${PGDATA}/postgresql.conf
echo "archive_mode = on" >> ${PGDATA}/postgresql.conf
echo "archive_command = 'pgbackrest --stanza=myfintrackstanza archive-push %p'" >> ${PGDATA}/postgresql.conf
echo "max_wal_senders = 3" >> ${PGDATA}/postgresql.conf

echo "Starting PostgreSQL in background..."
postgres -D ${PGDATA} &

PG_PID=$!

until pg_isready -h localhost -p 5432; do
  echo 'Waiting for postgres...' && sleep 2;
done

echo 'Creating pgBackRest stanza...'
pgbackrest --stanza=myfintrackstanza stanza-create || echo 'Stanza creation failed or already exists.'

echo 'Starting initial backup...'
pgbackrest --stanza=myfintrackstanza --type=full backup || echo 'Initial backup failed.'

echo "Setup complete. PostgreSQL is running."

wait ${PG_PID}