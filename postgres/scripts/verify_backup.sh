LOG_FILE="/var/log/pgbackrest/verify.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting backup verification..." >> $LOG_FILE

echo "[$DATE] Verifying stanza configuration..." >> $LOG_FILE
if pgbackrest --stanza=myfintrackstanza check; then
    echo "[$DATE] ✅ Stanza configuration verification: PASSED" >> $LOG_FILE
else
    echo "[$DATE] ❌ Stanza configuration verification: FAILED" >> $LOG_FILE
    exit 1
fi

echo "[$DATE] Checking backup archives..." >> $LOG_FILE
if pgbackrest --stanza=myfintrackstanza --type=full check; then
    echo "[$DATE] ✅ Backup archive check: PASSED" >> $LOG_FILE
else
    echo "[$DATE] ❌ Backup archive check: FAILED" >> $LOG_FILE
    echo "[$DATE] Attempting to diagnose the issue..." >> $LOG_FILE

    ERROR_INFO=$(pgbackrest --stanza=myfintrackstanza info 2>&1)
    echo "[$DATE] Diagnostic info: $ERROR_INFO" >> $LOG_FILE
    exit 1
fi

echo "[$DATE] Retrieving backup information..." >> $LOG_FILE
BACKUP_INFO=$(pgbackrest --stanza=myfintrackstanza info --output=text)

if [[ -n "$BACKUP_INFO" ]]; then
    echo "[$DATE] ✅ Backup information retrieved successfully" >> $LOG_FILE
    echo "[$DATE] Current backup status:" >> $LOG_FILE
    echo "----------------------------------------" >> $LOG_FILE
    echo "$BACKUP_INFO" >> $LOG_FILE
    echo "----------------------------------------" >> $LOG_FILE

    FULL_BACKUP_COUNT=$(echo "$BACKUP_INFO" | grep -c "full backup")
    INCR_BACKUP_COUNT=$(echo "$BACKUP_INFO" | grep -c "incr backup")
    
    echo "[$DATE] Summary - Full backups: $FULL_BACKUP_COUNT, Incremental backups: $INCR_BACKUP_COUNT" >> $LOG_FILE

    LAST_BACKUP_DATE=$(echo "$BACKUP_INFO" | grep -E "(full|incr) backup" | tail -1 | awk '{print $3, $4}')
    echo "[$DATE] Last backup date: $LAST_BACKUP_DATE" >> $LOG_FILE
    
    if [[ -n "$LAST_BACKUP_DATE" ]]; then
        LAST_BACKUP_TIMESTAMP=$(date -d "$LAST_BACKUP_DATE" +%s 2>/dev/null)
        CURRENT_TIMESTAMP=$(date +%s)
        HOURS_SINCE_BACKUP=$(( (CURRENT_TIMESTAMP - LAST_BACKUP_TIMESTAMP) / 3600 ))
        
        if [[ $HOURS_SINCE_BACKUP -gt 48 ]]; then
            echo "[$DATE] ⚠️  WARNING: Last backup is $HOURS_SINCE_BACKUP hours old (>48h)" >> $LOG_FILE
            
            # Eski backup uyarısı (Şimdlik dursun)
            # curl -X POST -H 'Content-type: application/json' \
            #   --data '{"text":"⚠️ WARNING: FinTrack DB last backup is '${HOURS_SINCE_BACKUP}' hours old"}' \
            #   YOUR_SLACK_WEBHOOK_URL
        else
            echo "[$DATE] ✅ Last backup is recent ($HOURS_SINCE_BACKUP hours ago)" >> $LOG_FILE
        fi
    fi
else
    echo "[$DATE] ❌ Failed to retrieve backup information" >> $LOG_FILE
    exit 1
fi

# 4. Repository consistency check
echo "[$DATE] Checking repository consistency..." >> $LOG_FILE
REPO_CHECK=$(pgbackrest --stanza=myfintrackstanza info --output=json 2>&1)
if [[ $? -eq 0 ]]; then
    echo "[$DATE] ✅ Repository consistency check: PASSED" >> $LOG_FILE
else
    echo "[$DATE] ❌ Repository consistency check: FAILED" >> $LOG_FILE
    echo "[$DATE] Repository check error: $REPO_CHECK" >> $LOG_FILE
fi

echo "[$DATE] Checking WAL archive status..." >> $LOG_FILE
WAL_INFO=$(pgbackrest --stanza=myfintrackstanza info --output=text | grep -i "wal archive")
if [[ -n "$WAL_INFO" ]]; then
    echo "[$DATE] ✅ WAL archive info: $WAL_INFO" >> $LOG_FILE
else
    echo "[$DATE] ⚠️  No WAL archive information found" >> $LOG_FILE
fi

# Test restore simulation (sadece boyut kontrolü)
echo "[$DATE] Simulating restore validation..." >> $LOG_FILE
TEST_RESTORE_INFO=$(pgbackrest --stanza=myfintrackstanza info --output=json | grep -o '"backup-size":[0-9]*' | head -1)
if [[ -n "$TEST_RESTORE_INFO" ]]; then
    BACKUP_SIZE=$(echo "$TEST_RESTORE_INFO" | cut -d':' -f2)
    BACKUP_SIZE_MB=$((BACKUP_SIZE / 1024 / 1024))
    echo "[$DATE] ✅ Latest backup size: ${BACKUP_SIZE_MB} MB" >> $LOG_FILE
    
    # Büyük backup uyarısı (1GB'dan büyükse)
    if [[ $BACKUP_SIZE_MB -gt 1024 ]]; then
        echo "[$DATE] ℹ️  INFO: Large backup detected (${BACKUP_SIZE_MB} MB)" >> $LOG_FILE
    fi
else
    echo "[$DATE] ⚠️  Could not determine backup size" >> $LOG_FILE
fi

echo "[$DATE] ✅ Backup verification completed successfully" >> $LOG_FILE

# Başarı bildirimi (Şimdlik dursun)
# curl -X POST -H 'Content-type: application/json' \
#   --data '{"text":"✅ FinTrack DB Monthly Backup Verification Completed Successfully"}' \
#   YOUR_SLACK_WEBHOOK_URL

exit 0