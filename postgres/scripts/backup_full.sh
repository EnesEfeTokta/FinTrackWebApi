#!/bin/bash

exec sudo -u postgres /usr/bin/env bash - << eof
LOG_FILE="/var/log/pgbackrest/full.log"
DATE=\$(date '+%Y-%m-%d %H:%M:%S')

echo "[\$DATE] Starting full backup..." >> \$LOG_FILE

if pgbackrest --stanza=myfintrackstanza --type=full backup; then
    echo "[\$DATE] Full backup completed successfully" >> \$LOG_FILE
    
    # expire komutu yedeklemelerin saklama politikasına göre silinmesini sağlar
    if pgbackrest --stanza=myfintrackstanza expire; then
        echo "[\$DATE] Old backups expired according to retention policy" >> \$LOG_FILE
    else
        echo "[\$DATE] WARNING: Failed to expire old backups" >> \$LOG_FILE
    fi

    BACKUP_INFO=\$(pgbackrest --stanza=myfintrackstanza info --output=text)
    echo "[\$DATE] Current backup status:" >> \$LOG_FILE
    echo "\$BACKUP_INFO" >> \$LOG_FILE
    
    # Disk kullanım bilgisi
    DISK_USAGE=\$(df -h /backup | tail -1)
    echo "[\$DATE] Backup repository disk usage: \$DISK_USAGE" >> \$LOG_FILE
else
    echo "[\$DATE] ERROR: Full backup failed!" >> \$LOG_FILE
    exit 1
fi

echo "[\$DATE] Full backup process finished successfully" >> \$LOG_FILE
eof