#!/bin/bash
exec sudo -u postgres /usr/bin/env bash - << eof
LOG_FILE="/var/log/pgbackrest/health.log"
DATE=\$(date '+%Y-%m-%d %H:%M:%S')
ERROR_COUNT=0

# Eski log dosyasını temizle
> \$LOG_FILE

echo "[\$DATE] Starting health check..." >> \$LOG_FILE

echo "[\$DATE] Checking database connection..." >> \$LOG_FILE
# -h db_postgres kaldırıldı, yerel bağlantı varsayılır
if pg_isready -U postgres; then
    echo "[\$DATE] ✅ Database connection: OK" >> \$LOG_FILE
else
    echo "[\$DATE] ❌ Database connection: FAILED" >> \$LOG_FILE
    ERROR_COUNT=\$((ERROR_COUNT + 1))
fi

echo "[\$DATE] Checking pgBackRest status..." >> \$LOG_FILE
if pgbackrest --stanza=myfintrackstanza info > /dev/null 2>&1; then
    echo "[\$DATE] ✅ pgBackRest status: OK" >> \$LOG_FILE

    LAST_BACKUP=\$(pgbackrest --stanza=myfintrackstanza info --output=text | grep -i "full backup" | tail -1)
    echo "[\$DATE] Last backup info: \$LAST_BACKUP" >> \$LOG_FILE
else
    echo "[\$DATE] ❌ pgBackRest status: FAILED" >> \$LOG_FILE
    ERROR_COUNT=\$((ERROR_COUNT + 1))
fi

echo "[\$DATE] Checking disk usage..." >> \$LOG_FILE
if [ -d "/backup" ]; then
    DISK_USAGE=\$(df /backup | tail -1 | awk '{print \$5}' | sed 's/%//')
    if [ "\$DISK_USAGE" -gt 90 ]; then
        echo "[\$DATE] 🚨 CRITICAL: Backup disk usage is \${DISK_USAGE}% (>90%)" >> \$LOG_FILE
        ERROR_COUNT=\$((ERROR_COUNT + 1))
    elif [ "\$DISK_USAGE" -gt 85 ]; then
        echo "[\$DATE] ⚠️  WARNING: Backup disk usage is \${DISK_USAGE}% (>85%)" >> \$LOG_FILE
    else
        echo "[\$DATE] ✅ Backup disk usage: \${DISK_USAGE}% (OK)" >> \$LOG_FILE
    fi
else
    echo "[\$DATE] ❌ Backup directory not found: /backup" >> \$LOG_FILE
    ERROR_COUNT=\$((ERROR_COUNT + 1))
fi

echo "[\$DATE] Checking WAL archiving..." >> \$LOG_FILE
# -h db_postgres kaldırıldı
WAL_STATUS=\$(psql -U postgres -d myfintrackdb -t -c "SELECT archived_count, failed_count FROM pg_stat_archiver;" 2>/dev/null || echo "Connection failed")
if [[ "\$WAL_STATUS" != "Connection failed" ]]; then
    echo "[\$DATE] ✅ WAL archiving status: \$WAL_STATUS" >> \$LOG_FILE
else
    echo "[\$DATE] ❌ Failed to check WAL archiving status" >> \$LOG_FILE
    ERROR_COUNT=\$((ERROR_COUNT + 1))
fi

echo "[\$DATE] Checking backup repository size..." >> \$LOG_FILE
if [ -d "/backup" ]; then
    REPO_SIZE=\$(du -sh /backup 2>/dev/null | cut -f1)
    echo "[\$DATE] ✅ Backup repository total size: \$REPO_SIZE" >> \$LOG_FILE
else
    echo "[\$DATE] ❌ Cannot determine backup repository size" >> \$LOG_FILE
    ERROR_COUNT=\$((ERROR_COUNT + 1))
fi

# Sonuç özeti
if [ \$ERROR_COUNT -eq 0 ]; then
    echo "[\$DATE] ✅ Health check completed successfully - All systems OK" >> \$LOG_FILE
    exit 0
else
    echo "[\$DATE] ❌ Health check completed with \$ERROR_COUNT error(s)" >> \$LOG_FILE
    exit 1
fi
eof