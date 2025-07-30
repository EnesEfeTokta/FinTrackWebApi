LOG_FILE="/var/log/pgbackrest/incremental.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Starting incremental backup..." >> $LOG_FILE

# Incremental backup
if pgbackrest --stanza=myfintrackstanza --type=incr backup; then
    echo "[$DATE] Incremental backup completed successfully" >> $LOG_FILE
    
    # Backup bilgilerini kaydet
    BACKUP_INFO=$(pgbackrest --stanza=myfintrackstanza info --output=text | head -20)
    echo "[$DATE] Backup info:" >> $LOG_FILE
    echo "$BACKUP_INFO" >> $LOG_FILE
    
    # Başarı bildirimi için webhook (Şimdlik dursun)
    # curl -X POST -H 'Content-type: application/json' \
    #   --data '{"text":"✅ FinTrack DB Incremental Backup Successful"}' \
    #   YOUR_SLACK_WEBHOOK_URL
else
    echo "[$DATE] ERROR: Incremental backup failed!" >> $LOG_FILE
    
    # Hata bildirimi için webhook (Şimdilik dursun)
    # curl -X POST -H 'Content-type: application/json' \
    #   --data '{"text":"❌ FinTrack DB Incremental Backup FAILED!"}' \
    #   YOUR_SLACK_WEBHOOK_URL
    
    exit 1
fi

echo "[$DATE] Incremental backup process finished" >> $LOG_FILE