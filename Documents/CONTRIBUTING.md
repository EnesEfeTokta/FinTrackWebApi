# FinTrack Projesine Katkıda Bulunma Rehberi

Öncelikle, FinTrack projesine katkıda bulunmayı düşündüğünüz için teşekkür ederiz! Bu proje, topluluk katkılarıyla daha da güçlenecektir. Bu rehber, katkıda bulunma sürecini olabildiğince kolay ve şeffaf hale getirmek için hazırlanmıştır.

## İçindekiler
- [Davranış Kuralları (Code of Conduct)](#davranış-kuralları-code-of-conduct)
- [Nasıl Katkıda Bulunabilirim?](#nasıl-katkıda-bulunabilirim)
  - [Hata Bildirimi](#hata-bildirimi)
  - [Yeni Özellik veya İyileştirme Önerme](#yeni-özellik-veya-iyileştirme-önerme)
  - [İlk Katkınız](#ilk-katkınız)
- [Geliştirme Ortamı Kurulumu](#geliştirme-ortamı-kurulumu)
- [Katkı Süreci ve Git Akışı](#katkı-süreci-ve-git-akışı)
- [Kodlama Standartları](#kodlama-standartları)
  - [Genel Kurallar](#genel-kurallar)
  - [C# (.NET) için Standartlar](#c-net-için-standartlar)
  - [Python için Standartlar](#python-için-standartlar)
- [Commit Mesajı Standartları](#commit-mesajı-standartları)
- [Pull Request (PR) Süreci](#pull-request-pr-süreci)

## Davranış Kuralları (Code of Conduct)

Bu projenin tüm katılımcılarının, projemizin `CODE_OF_CONDUCT.md` dosyasında belirtilen davranış kurallarına uyması beklenmektedir. Lütfen herkes için samimi ve kapsayıcı bir ortam sağlamak adına bu kuralları okuyun ve uyun.

## Nasıl Katkıda Bulunabilirim?

### Hata Bildirimi
Eğer bir hata bulduysanız, lütfen GitHub "Issues" bölümünde yeni bir "issue" oluşturun. Hatanızı tarif ederken şu bilgileri eklemeye çalışın:
- Hatanın ne olduğunu açık ve kısa bir şekilde özetleyin.
- Hatayı yeniden oluşturmak için gereken adımları listeleyin.
- Beklediğiniz davranışın ne olduğunu açıklayın.
- Gördüğünüz gerçek davranışın ne olduğunu açıklayın.
- Mümkünse ekran görüntüleri ekleyin.

### Yeni Özellik veya İyileştirme Önerme
Harika bir fikriniz mi var? GitHub "Issues" bölümünde yeni bir "issue" oluşturarak fikrinizi bizimle paylaşın. Önerinizi detaylandırarak neden faydalı olacağını açıklayın.

### İlk Katkınız
Eğer projeye ilk defa katkıda bulunacaksanız, "Issues" bölümünde `good first issue` veya `help wanted` etiketli konulara göz atabilirsiniz. Bunlar, projeye başlamak için genellikle daha uygun konulardır.

## Geliştirme Ortamı Kurulumu

Proje, geliştirme ortamını standartlaştırmak ve kolaylaştırmak için tamamen **Docker** üzerine kuruludur.

**Gereksinimler:**
- [Git](https://git-scm.com/)
- [Docker](https://www.docker.com/products/docker-desktop/) ve Docker Compose

**Kurulum Adımları:**
1.  Bu repoyu **fork'layın** ve fork'ladığınız repoyu yerel makinenize klonlayın:
    ```bash
    git clone https://github.com/SENIN_KULLANICI_ADIN/FinTrack.git
    cd FinTrack
    ```
2.  Projenin ana dizininde, tüm servisleri (API'ler ve veritabanı) başlatmak için aşağıdaki komutu çalıştırın:
    ```bash
    docker-compose up --build -d
    ```
    - `--build` parametresi, kodda yaptığınız değişikliklerin yansıtılması için imajların yeniden oluşturulmasını sağlar.
    - `-d` parametresi, servisleri arka planda çalıştırır.
3.  Hepsi bu kadar! Servisleriniz artık çalışıyor.
    - **Ana API:** `http://localhost:5000`
    - **Swagger UI:** `http://localhost:5000/swagger`
    - **Bot API:** `http://localhost:5001`

Servisleri durdurmak için `docker-compose down` komutunu kullanabilirsiniz.

## Katkı Süreci ve Git Akışı

1.  Yukarıda anlatıldığı gibi repoyu fork'layıp klonlayın.
2.  Ana (`main` veya `develop`) branch'inden yola çıkarak yeni bir branch oluşturun. Branch isminiz yaptığınız işi özetlemelidir.
    ```bash
    # Örnekler:
    git checkout -b feature/add-user-profile-endpoint
    git checkout -b fix/login-validation-bug
    ```
3.  Değişikliklerinizi yapın ve kodlama standartlarına uygun olduğundan emin olun.
4.  Değişikliklerinizi anlamlı commit mesajları ile kaydedin.
5.  Oluşturduğunuz branch'i kendi fork'unuza push'layın:
    ```bash
    git push origin feature/add-user-profile-endpoint
    ```
6.  GitHub üzerinden orijinal FinTrack reposuna bir **Pull Request (PR)** açın.

## Kodlama Standartları

### Genel Kurallar
- Tüm kod, yorum ve dokümantasyon **İngilizce** olmalıdır.
- Kodunuzu anlaşılır ve temiz tutun. Gereksiz karmaşıklıktan kaçının.

### C# (.NET) için Standartlar
- [Microsoft'un C# Kodlama Standartları](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)'nı takip edin.
- Hata ayıklama için `Console.WriteLine` yerine projedeki `ILogger` yapısını kullanın.
- Asenkron operasyonlar için `async/await`'i doğru şekilde kullanın.
- Tüm public metotlar ve sınıflar için XML yorumları ekleyin. Bu, Swagger dokümantasyonu için de gereklidir.

### Python için Standartlar
- [PEP 8 Style Guide](https://www.python.org/dev/peps/pep-0008/)'a uyun.
- Kodunuzu göndermeden önce `black` gibi bir formatlayıcı ile formatlamanız tavsiye edilir.

## Commit Mesajı Standartları

Projemiz, commit mesajları için [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) standardını kullanır. Bu, hem Git geçmişini okunabilir kılar hem de versiyonlama sürecini otomatikleştirir.

**Format:** `<tür>(<kapsam>): <açıklama>`

**Yaygın Türler:**
- `feat`: Yeni bir özellik eklerken.
- `fix`: Bir hatayı düzeltirken.
- `docs`: Sadece dokümantasyonu değiştirirken.
- `style`: Kodun anlamını etkilemeyen formatlama değişiklikleri (boşluk, noktalı virgül vb.).
- `refactor`: Hata düzeltmeyen veya özellik eklemeyen kod yeniden yapılandırmaları.
- `test`: Eksik testleri eklerken veya mevcut testleri düzeltirken.
- `chore`: Build sürecini, yardımcı araçları veya kütüphaneleri etkileyen değişiklikler.

**Örnekler:**
```
feat(auth): Add password reset functionality
fix(account): Correctly calculate account balance with negative transactions
docs(readme): Update setup instructions for Docker
```

## Pull Request (PR) Süreci

1.  PR'ınızın başlığı, yaptığınız değişikliği net bir şekilde özetlemelidir. (Örn: `feat(categories): Add support for sub-categories`)
2.  PR açıklamasında şu soruları yanıtlayın:
    - **Bu değişiklik neden gerekli?**
    - **Ne yapıyor?**
    - **İlgili "Issue" numarası var mı?** (Örn: `Closes #123`)
3.  PR'ınızın tek bir amaca hizmet ettiğinden emin olun. Birden fazla alakasız değişikliği tek bir PR'da birleştirmeyin.
4.  Gönderdiğiniz kodun projeyi "build" ettiğinden ve tüm testlerin geçtiğinden emin olun.
5.  PR'ınız gözden geçirildikten sonra istenen değişiklikleri yapmaya ve geri bildirimlere yanıt vermeye hazır olun.

FinTrack'i daha iyi bir yer haline getirmeye yardımcı olduğunuz için tekrar teşekkürler!