#!/bin/bash

exec sudo -u postgres /usr/bin/env bash - << eof
LOG_FILE="/var/log/pgbackrest/incremental.log"
DATE=\$(date '+%Y-%m-%d %H:%M:%S')

echo "[\$DATE] Starting incremental backup..." >> \$LOG_FILE

# Incremental backup
if pgbackrest --stanza=myfintrackstanza --type=incr backup; then
    echo "[\$DATE] Incremental backup completed successfully" >> \$LOG_FILE
    
    # Backup bilgilerini kaydet
    BACKUP_INFO=\$(pgbackrest --stanza=myfintrackstanza info --output=text | head -20)
    echo "[\$DATE] Backup info:" >> \$LOG_FILE
    echo "\$BACKUP_INFO" >> \$LOG_FILE
else
    echo "[\$DATE] ERROR: Incremental backup failed!" >> \$LOG_FILE
    exit 1
fi

echo "[\$DATE] Incremental backup process finished" >> \$LOG_FILE
eof