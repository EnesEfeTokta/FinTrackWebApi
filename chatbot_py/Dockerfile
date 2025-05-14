FROM python:3.13-slim

WORKDIR /app

COPY chatbot_py/requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

# Geri kalan chatbot uygulama dosyalarını kopyala
# Dockerfile proje kökündeyse:
COPY chatbot_py/. .
# Dockerfile chatbot_py klasöründeyse:
# COPY . .

# Chatbot'un bir sunucu çalıştırıyorsa (örn: Flask, FastAPI) portunu açın
# EXPOSE 5000

# .env dosyasındaki değişkenleri runtime'da veya build argümanları ile geçmek daha iyidir
# CMD ["python", "main.py", "--env-file", ".env"] # .env dosyasını kullanıyorsa

# Uygulamayı başlat
CMD ["python", "main.py"]